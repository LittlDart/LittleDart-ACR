using Dalamud.Game.ClientState.Conditions;
using ECommons.DalamudServices;
using LittleDart.Paladin.Mitigation.Calc;
using LittleDart.Paladin.Setting;

namespace LittleDart.Paladin.Mitigation;

public sealed class MitigationState
{
    public HpTracker Hp { get; } = new();

    private readonly List<double> _pullIntervals = new();
    private readonly Dictionary<uint, long> _lastCastMs = new();

    private long _lastEnemySampleMs;
    private long _lastInCombatMs;
    private long _lastWaveStartMs;
    private bool _waveEndedFlag;

    private long _pullMomentMs;
    private int _prevEnemyCount;

    public bool WaveActive { get; private set; }

    public long WaveStartMs { get; private set; }

    public int EnemyCount { get; private set; }

    public int EnemiesIn5m { get; private set; }

    public int AggroOnMe { get; private set; }

    public long NowMs => Environment.TickCount64;

    public long WaveElapsedMs => WaveActive ? NowMs - WaveStartMs : 0;

    public long SincePullMomentMs => WaveActive ? NowMs - Math.Max(_pullMomentMs, WaveStartMs) : 0;

    public IReadOnlyList<double> PullIntervals => _pullIntervals;

    public bool ResourceTight { get; set; }

    public float LastWaveDamagePercent { get; set; }

    public void Tick()
    {
        long now = NowMs;
        Hp.Tick(now, MitigationConfig.HpSampleIntervalMs, CurrentDamageWindowMs);
        SampleEnemies(now);

        bool inCombat = IsInCombat();

        if (inCombat)
        {
            _lastInCombatMs = now;

            if (!WaveActive)
            {
                WaveActive = true;
                WaveStartMs = now;
                _pullMomentMs = now;
                _prevEnemyCount = 0;

                if (_lastWaveStartMs > 0)
                {
                    double interval = (now - _lastWaveStartMs) / 1000d;
                    if (interval >= MitigationConfig.MinPullIntervalSeconds
                        && interval <= MitigationConfig.MaxPullIntervalSeconds)
                    {
                        _pullIntervals.Add(interval);
                        while (_pullIntervals.Count > 10)
                        {
                            _pullIntervals.RemoveAt(0);
                        }
                    }
                }

                _lastWaveStartMs = now;
            }
        }
        else if (WaveActive && now - _lastInCombatMs >= CurrentWaveEndDebounceMs)
        {
            WaveActive = false;
            _waveEndedFlag = true;
            LastWaveDamagePercent = DamageTakenPercent(CurrentDamageWindowMs);
        }
    }

    public bool ConsumeWaveEnded()
    {
        bool flag = _waveEndedFlag;
        _waveEndedFlag = false;
        return flag;
    }

    public float DamageTakenPercent(int windowMs) => Hp.SelfDamageTakenPercent(windowMs);

    public void RecordCast(uint skillId) => _lastCastMs[skillId] = NowMs;

    public long LastCastMs(uint skillId) => _lastCastMs.TryGetValue(skillId, out long t) ? t : 0;

    public double EstimateSecondsToNextPull()
    {
        var settings = PldSettings.Instance;
        double estimate = settings?.MitigationDefaultNextPullSeconds ?? MitigationConfig.DefaultNextPullSeconds;

        if (_pullIntervals.Count > 0)
        {
            int take = Math.Min(MitigationConfig.IntervalHistoryCount, _pullIntervals.Count);
            double sum = 0;
            for (int i = _pullIntervals.Count - take; i < _pullIntervals.Count; i++)
            {
                sum += _pullIntervals[i];
            }

            estimate = sum / take;
        }

        return Math.Max(0d, estimate);
    }

    public void Reset()
    {
        Hp.Reset();
        _pullIntervals.Clear();
        _lastCastMs.Clear();
        _lastEnemySampleMs = 0;
        _lastInCombatMs = 0;
        _lastWaveStartMs = 0;
        _waveEndedFlag = false;
        _pullMomentMs = 0;
        _prevEnemyCount = 0;
        WaveActive = false;
        WaveStartMs = 0;
        EnemyCount = 0;
        EnemiesIn5m = 0;
        AggroOnMe = 0;
        ResourceTight = false;
        LastWaveDamagePercent = 0f;
        MitigationSignals.ClearManual();
    }

    private void SampleEnemies(long now)
    {
        if (now - _lastEnemySampleMs < MitigationConfig.EnemySampleIntervalMs)
        {
            return;
        }

        _lastEnemySampleMs = now;
        EnemyCount = EnemyCounter.CountWithin(CurrentEnemyRadius);
        EnemiesIn5m = EnemyCounter.CountWithin(5f);
        AggroOnMe = EnemyCounter.CountAggroOnMe();

        if (EnemyCount > _prevEnemyCount)
        {
            _pullMomentMs = now;
        }

        _prevEnemyCount = EnemyCount;
    }

    public bool IsInCombat()
    {
        try
        {
            return Svc.Condition[ConditionFlag.InCombat];
        }
        catch
        {
            return EnemyCount > 0;
        }
    }

    private static int CurrentDamageWindowMs
        => PldSettings.Instance?.MitigationDamageWindowMs ?? MitigationConfig.DamageWindowMs;

    private static float CurrentEnemyRadius
        => PldSettings.Instance?.MitigationEnemyCountRadius ?? MitigationConfig.EnemyCountRadius;

    private static int CurrentWaveEndDebounceMs
        => PldSettings.Instance?.MitigationWaveEndDebounceMs ?? MitigationConfig.WaveEndDebounceMs;
}
