using AEAssist;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using Dalamud.Game.ClientState.Conditions;
using ECommons.DalamudServices;
using LittleDart.Paladin.Setting;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin;

public static class PldSlideCast
{

    private enum LockState
    {
        Idle,

        Pending,

        Casting,

        Released,
    }

    private static LockState _state = LockState.Idle;

    private static long _requestTime;

    private static long _lockStartTime;

    private static long _releaseTime;

    private static float _releaseRemainMs;

    private static bool _releasedByPrediction;

    private static bool _clientFreeLogged;

    public static bool IsLocked { get; private set; }

    private const int PendingTimeoutMs = 1000;

    private const int MaxLockMs = 5000;

    private const int InterruptToleranceMs = 150;

    private const string LogTag = "[LittleDart-PLD/滑步]";

    public static void RequestCast()
    {
        if (_state == LockState.Idle)
        {
            _state = LockState.Pending;
            _requestTime = TimeHelper.Now();
        }
    }

    public static void Tick()
    {
        long now = TimeHelper.Now();

        if (IsLocked && now - _lockStartTime >= MaxLockMs)
        {
            Log($"强制解锁 锁满={MaxLockMs}ms");
            Reset();
            return;
        }

        if (!IsEnabled())
        {
            if (IsLocked || _state != LockState.Idle)
            {
                Log("解锁 原因=QT关或切日常");
            }

            Reset();
            return;
        }

        bool castingHoly = IsCastingHolySpirit();
        bool clientFree = ClientSaysFree();

        switch (_state)
        {
            case LockState.Pending:
                if (now - _requestTime > PendingTimeoutMs)
                {
                    Log($"放弃请求 超时={PendingTimeoutMs}ms");
                    Reset();
                    return;
                }

                if (castingHoly)
                {
                    _state = LockState.Casting;
                    Log("圣灵读条=True");
                }

                break;

            case LockState.Casting:
                if (!castingHoly)
                {
                    LogCastEnd(now);
                    Reset();
                    return;
                }

                if (clientFree)
                {
                    _state = LockState.Released;
                    NoteRelease(now, byPrediction: false);
                    Log($"放行 源=客户端标志 剩余={RemainingMs():F0}ms");
                }
                else if (RemainingMs() <= ReleaseThresholdMs())
                {
                    _state = LockState.Released;
                    NoteRelease(now, byPrediction: true);
                    Log($"放行 源=预测 剩余={RemainingMs():F0}ms 阈值={ReleaseThresholdMs()}ms");
                }

                break;

            case LockState.Released:
                if (clientFree && !_clientFreeLogged)
                {
                    _clientFreeLogged = true;
                    Log(_releasedByPrediction
                        ? $"客户端标志落下 源=预测 滞后={now - _releaseTime}ms"
                        : "客户端标志落下 源=标志");
                }

                if (!castingHoly)
                {
                    LogCastEnd(now);
                    Reset();
                }

                break;

            case LockState.Idle:
            default:
                break;
        }

        ApplyLock(_state is LockState.Pending or LockState.Casting, now);

        Diag(_state, castingHoly, clientFree);
    }

    public static void OnDispose()
    {
        if (IsLocked)
        {
            Log("Dispose 解锁");
        }

        Reset();
    }

    private static void Reset()
    {
        _state = LockState.Idle;
        _releaseTime = 0;
        _clientFreeLogged = false;
        ApplyLock(false, TimeHelper.Now());
    }

    private static void NoteRelease(long now, bool byPrediction)
    {
        _releaseTime = now;
        _releaseRemainMs = RemainingMs();
        _releasedByPrediction = byPrediction;
        _clientFreeLogged = false;
    }

    private static void LogCastEnd(long now)
    {
        if (_releaseTime == 0)
        {
            return;
        }

        long afterRelease = now - _releaseTime;
        bool interrupted = afterRelease + InterruptToleranceMs < _releaseRemainMs;

        Log(interrupted
            ? $"读条结束 放行剩余={_releaseRemainMs:F0}ms 放行到结束={afterRelease}ms 打断=True 提前={_releaseRemainMs - afterRelease:F0}ms"
            : $"读条结束 放行剩余={_releaseRemainMs:F0}ms 放行到结束={afterRelease}ms 打断=False");
    }

    private static void ApplyLock(bool shouldLock, long now)
    {
        if (shouldLock == IsLocked)
        {
            return;
        }

        if (shouldLock)
        {
            AI.Instance.LockPos = true;
            _lockStartTime = now;
            Log("LockPos=True");
        }
        else
        {
            AI.Instance.LockPos = false;
            Log("LockPos=False");
        }

        IsLocked = shouldLock;
    }

    private static bool IsEnabled()
    {
        if (PldSettings.Instance == null)
        {
            return false;
        }

        if (LittleDartRotationEntry.QT == null || !LittleDartRotationEntry.QT.GetQt(PldQt.SlideCast))
        {
            return false;
        }

        return !PldHelper.IsDailyMode();
    }

    private static bool IsCastingHolySpirit()
    {
        return Core.Me.IsCasting && Core.Me.CastActionId == PldSkills.圣灵;
    }

    private static bool ClientSaysFree()
    {
        return !Svc.Condition[ConditionFlag.Casting]
            && !Svc.Condition[ConditionFlag.OccupiedInEvent];
    }

    private static float RemainingMs()
    {
        if (!Core.Me.IsCasting)
        {
            return 0f;
        }

        return MathF.Max(0f, Core.Me.TotalCastTime - Core.Me.CurrentCastTime) * 1000f;
    }

    private static float ReleaseThresholdMs()
    {
        int raw = PldSettings.Instance.SlideCastReleaseMs;
        return Math.Clamp(raw, 0, 1000);
    }

    private static void Log(string message)
    {
        if (!Diagnostics.Enabled)
        {
            return;
        }

        PldHelper.LogTagged($"{LogTag} {message}");
    }

    private static void Diag(LockState state, bool castingHoly, bool clientFree)
    {
        if (!Diagnostics.Enabled)
        {
            return;
        }

        if (state == _lastDiagState && IsLocked == _lastDiagLocked)
        {
            return;
        }

        _lastDiagState = state;
        _lastDiagLocked = IsLocked;

        PldHelper.LogTagged(
            $"{LogTag} 状态={StateName(state)} 锁定={IsLocked} " +
            $"| 圣灵读条={castingHoly} 剩余={RemainingMs():F0}ms " +
            $"| 客户端自由={clientFree} 阈值={ReleaseThresholdMs():F0}ms");
    }

    private static LockState _lastDiagState = LockState.Idle;
    private static bool _lastDiagLocked;

    private static string StateName(LockState state) => state switch
    {
        LockState.Idle => "空闲",
        LockState.Pending => "待起读条",
        LockState.Casting => "读条中",
        LockState.Released => "已放行",
        _ => state.ToString(),
    };
}
