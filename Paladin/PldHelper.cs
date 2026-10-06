using AEAssist;
using AEAssist.Define;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text;
using ECommons.DalamudServices;
using AEAssist.CombatRoutine.View.JobView;
using LittleDart.Paladin.Mitigation;
using LittleDart.Paladin.Setting;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin;

public class PldHelper
{

    public static bool HasTankStance()
    {
        return Core.Me.HasAura(PldSkills.钢铁信念buff, 0);
    }

    public static bool IsInDuty()
    {
        return Core.Resolve<MemApiDuty>().IsBoundByDuty();
    }

    public string GetDutyState()
    {
        if (IsInDuty() && PartyHelper.Party is { Count: > 0 })
        {
            switch (PartyHelper.Party.Count)
            {
                case 4: return "四人副本";
                case 8: return "八人副本";
            }
        }

        return "副本外";
    }

    public static IBattleChara? GetCurrentTarget()
    {
        return Core.Me.GetCurrTarget();
    }

    public static double GetAuraTimeLeft(uint auraId)
    {
        if (Core.Resolve<MemApiBuff>().GetTimeSpanLeft(Core.Me, auraId, out var left))
        {
            return left.TotalSeconds;
        }

        return 0;
    }

    public static double GetComboTimeLeftSeconds()
    {
        return Core.Resolve<MemApiSpell>().GetComboTimeLeft().TotalSeconds;
    }

    public static uint GetLastComboSpellId()
    {
        return Core.Resolve<MemApiSpell>().GetLastComboSpellId();
    }

    public static bool HasAura(uint auraId, int timeLeft = 0)
    {
        return Core.Me.HasAura(auraId, timeLeft);
    }

    public const int GcdReentryGuardMs = 1500;

    public static bool IsRecentlyCast(uint spellId, int ms = GcdReentryGuardMs)
    {
        return SpellExtension.RecentlyUsed(spellId, ms);
    }

    public static int GetAuraStack(uint auraId)
    {
        return Core.Me.GetAuraStack(auraId);
    }

    public static bool HasCombo()
    {
        return GetLastComboSpellId() != 0;
    }

    public static bool IsAtRoyalAuthorityStep()
    {
        return GetLastComboSpellId() == PldSkills.暴乱剑;
    }

    public static bool HasDivineMight()
    {
        return HasAura(PldSkills.神圣魔法效果提高);
    }

    public static bool CanUseEmpoweredHolySpirit()
    {
        if (!HasDivineMight())
        {
            return false;
        }

        return IsAtRoyalAuthorityStep();
    }

    public static int GetAtonementChainStep()
    {
        if (HasAura(PldSkills.葬送剑预备)) return 3;
        if (HasAura(PldSkills.祈告剑预备)) return 2;
        if (HasAura(PldSkills.赎罪剑预备)) return 1;
        return 0;
    }

    public static bool IsAtonementChainRunning()
    {
        return GetAtonementChainStep() != 0;
    }

    public static bool HasValidTarget()
    {
        return GetCurrentTarget() != null;
    }

    public static bool IsInMeleeRange()
    {
        var target = GetCurrentTarget();
        if (target == null)
        {
            return false;
        }

        float attackRange = SettingMgr.GetSetting<GeneralSettings>().AttackRange;

        return Core.Me.Distance(target) <= attackRange;
    }

    public static bool IsTargetFar()
    {
        if (!HasValidTarget())
        {
            return false;
        }

        return !IsInMeleeRange();
    }

    public static float GetDistanceToTarget()
    {
        var target = GetCurrentTarget();

        return target == null ? float.MaxValue : Core.Me.Distance(target);
    }

    public static float GetDistanceToTargetCircle()
    {
        var target = GetCurrentTarget();

        return target == null
            ? float.MaxValue
            : Core.Me.Distance(
                target,
                DistanceMode.IgnoreTargetHitbox | DistanceMode.IgnoreHeight);
    }

    public static bool PassesRangedHolySpiritGate()
    {
        return PassesRangedGate(PldSettings.Instance.RangedHolySpiritMinDistance);
    }

    public static bool PassesRangedShieldLobGate()
    {
        return PassesRangedGate(PldSettings.Instance.RangedShieldLobMinDistance);
    }

    public static bool IsDailyMode()
    {
        PldSettings? s = PldSettings.Instance;
        if (s != null && s.Mode == MitigationConfig.DailyModeValue)
        {
            return true;
        }

        JobViewWindow? qt = LittleDartRotationEntry.QT;
        return qt != null && qt.GetQt(PldQt.DailyMode);
    }

    private static bool PassesRangedGate(int minDistance)
    {
        if (IsDailyMode())
        {
            return true;
        }

        if (!HasValidTarget())
        {
            return false;
        }

        return GetDistanceToTarget() >= minDistance;
    }

    public static bool IsMoving()
    {
        return Core.Resolve<MemApiMove>().IsMoving();
    }

    public static double GetFofCooldownSeconds()
    {
        return Core.Resolve<MemApiSpell>().GetCooldown(PldSkills.战逃反应).TotalSeconds;
    }

    public static uint GetBurstStarterSkill()
    {
        return SpellExtension.IsUnlock(PldSkills.绝对统治)
            ? PldSkills.绝对统治
            : PldSkills.安魂祈祷技能;
    }

    public static double GetBurstStarterCooldownSeconds()
    {
        return Core.Resolve<MemApiSpell>().GetCooldown(GetBurstStarterSkill()).TotalSeconds;
    }

    public static bool IsFofActive()
    {
        return HasAura(PldSkills.战逃反应buff);
    }

    public const int FofRecentCastWindowMs = 1500;

    public static bool IsFofRecentCast()
    {
        return PldSkills.战逃反应.RecentlyUsed(FofRecentCastWindowMs);
    }

    public static bool IsFofInside()
    {
        return IsFofActive() || IsFofRecentCast();
    }

    public static bool IsFofOutside()
    {
        return !IsFofInside();
    }

    public static int GetNearbyEnemyCount(int range)
    {
        return TargetHelper.GetNearbyEnemyCount(range);
    }

    public static double GetTargetAuraTimeLeft(uint auraId)
    {
        var target = GetCurrentTarget();
        if (target == null)
        {
            return 0;
        }

        if (Core.Resolve<MemApiBuff>().GetTimeSpanLeft(target, auraId, out var left))
        {
            return left.TotalSeconds;
        }

        return 0;
    }

    public static IBattleChara? GetAnotherTank()
    {
        return PartyHelper.GetAnotherTank(Core.Me);
    }

    public static IBattleChara? GetLowestHpPartner(float maxDistance = 30f)
    {
        IBattleChara? result = null;
        float lowest = float.MaxValue;

        foreach (var member in PartyHelper.CastableAlliesWithin30)
        {
            if (member.GameObjectId == Core.Me.GameObjectId)
            {
                continue;
            }

            float hpPercent = member.CurrentHpPercent();
            if (hpPercent < lowest)
            {
                lowest = hpPercent;
                result = member;
            }
        }

        _ = maxDistance;
        return result;
    }

    public enum SupportTargetKind
    {
        Partner,

        Healer,

        LowestHp,
    }

    public static IBattleChara? GetSupportTarget(SupportTargetKind kind) => kind switch
    {
        SupportTargetKind.Partner => GetAnotherTank(),
        SupportTargetKind.Healer => GetLowestHpHealer(),
        SupportTargetKind.LowestHp => GetLowestAbsoluteHpPartner(),
        _ => null,
    };

    public static IBattleChara? GetLowestAbsoluteHpPartner()
    {
        IBattleChara? result = null;
        uint lowest = uint.MaxValue;

        foreach (var member in PartyHelper.Party)
        {
            if (!IsSupportCandidate(member))
            {
                continue;
            }

            if (member.CurrentHp < lowest)
            {
                lowest = member.CurrentHp;
                result = member;
            }
        }

        return result;
    }

    public static IBattleChara? GetLowestHpHealer()
    {
        IBattleChara? result = null;
        uint lowest = uint.MaxValue;

        foreach (var member in PartyHelper.CastableHealers)
        {
            if (!IsSupportCandidate(member))
            {
                continue;
            }

            if (member.CurrentHp < lowest)
            {
                lowest = member.CurrentHp;
                result = member;
            }
        }

        return result;
    }

    private static bool IsSupportCandidate(IBattleChara? member)
    {
        if (member is null)
        {
            return false;
        }

        if (member.GameObjectId == Core.Me.GameObjectId)
        {
            return false;
        }

        return member.CurrentHp > 0;
    }

    public static float GetDistanceTo(IBattleChara? target)
    {
        return target is null
            ? float.MaxValue
            : Core.Me.Distance(target, DistanceMode.IgnoreTargetHitbox | DistanceMode.IgnoreHeight);
    }

    public static float GetActionRange(uint actionId)
    {
        try
        {
            var action = Svc.Data?.Excel?.GetSheet<Lumina.Excel.Sheets.Action>()?.GetRowOrDefault(actionId);
            return action is null ? 0f : action.Value.Range;
        }
        catch
        {
            return 0f;
        }
    }

    public static bool ShouldUseAoe()
    {
        if (!LittleDartRotationEntry.QT.GetQt(PldQt.UseAoe))
        {
            return false;
        }

        return TargetHelper.GetNearbyEnemyCount(5) >= PldSettings.Instance.AoeEnemyCount;
    }

    public static void SetTankStance(bool on)
    {
        uint spellId = on ? PldSkills.钢铁信念 : PldSkills.解除钢铁信念;

        Core.Resolve<MemApiSpell>().Cast(spellId, Core.Me.Position);
    }

    public static void ClearHighPriorityQueues()
    {
        AI.Instance.BattleData.HighPrioritySlots_GCD.Clear();
        AI.Instance.BattleData.HighPrioritySlots_OffGCD.Clear();

        Log("已清扫高优先级技能队列（GCD + oGCD）");

        Toast("已清扫高优先级技能队列", 1200);
    }

    public static void Log(string message)
    {
        LogTagged($"{PldLogTag.Plain} {message}");
    }

    public static void LogTagged(string taggedMessage)
    {
        LogHelper.Info(taggedMessage);

        if (IsChatOutputDisabled())
        {
            return;
        }

        if (TryPrintColoredToChat(taggedMessage))
        {
            return;
        }

        PrintPlainToChat(taggedMessage);
    }

    private static bool IsChatOutputDisabled()
    {
        try
        {
            return !SettingMgr.GetSetting<GeneralSettings>().Print;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryPrintColoredToChat(string taggedMessage)
    {
        try
        {
            if (Plugin.Config is null)
            {
                return false;
            }

            XivChatType chatType = Plugin.Config.MessageChatType;
            if (chatType == XivChatType.None)
            {
                return false;
            }

            Svc.Chat.Print(new XivChatEntry
            {
                Type = chatType,
                Message = PldLogTag.BuildChatMessage(taggedMessage),
            });

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void PrintPlainToChat(string taggedMessage)
    {
        try
        {
            Core.Resolve<MemApiChatMessage>()?.PrintPluginMessage(taggedMessage);
        }
        catch
        {
        }
    }

    public const int ToastStyleInfo = 1;

    public const int ToastStyleWarn = 2;

    public static void Toast(string message, int time = 2000, int style = ToastStyleInfo)
    {
        if (!PldSettings.Instance.UseToast)
        {
            return;
        }

        Core.Resolve<MemApiChatMessage>().Toast2(message, style, time);
    }

    public static void ToastModeBanner(bool daily)
    {
        Toast($"当前模式：{(daily ? "日常" : "高难")}",
              time: 1000,
              style: daily ? ToastStyleInfo : ToastStyleWarn);
    }
}
