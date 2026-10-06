using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using LittleDart.Paladin.Setting;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.OffGcd;

internal static class TwinDamage
{

    internal static bool IsRequiescatCastable()
    {
        if (!LittleDartRotationEntry.QT.GetQt(PldQt.UseRequiescat))
        {
            return false;
        }

        if (!PldHelper.IsFofActive())
        {
            return false;
        }

        if (SpellExtension.IsUnlock(PldSkills.绝对统治)
            && SpellExtension.IsReadyWithCanCast(PldSkills.绝对统治.GetSpell()))
        {
            return true;
        }

        return SpellExtension.IsUnlock(PldSkills.安魂祈祷技能)
            && SpellExtension.IsReadyWithCanCast(PldSkills.安魂祈祷技能.GetSpell());
    }

    public static bool ShouldUse()
    {
        if (!PldHelper.HasValidTarget())
        {
            return false;
        }

        if (!PldHelper.IsInMeleeRange())
        {
            return false;
        }

        if (LittleDartRotationEntry.QT.GetQt(PldQt.TwinDamageDirect))
        {
            return true;
        }

        {
            double fofCd = PldHelper.GetFofCooldownSeconds();
            if (fofCd < PldSettings.Instance.TwinDamageFofLockMin
                || fofCd > PldSettings.Instance.TwinDamageFofLockMax)
            {
                return false;
            }
        }

        if (SpellExtension.IsUnlock(PldSkills.绝对统治)
            || SpellExtension.IsUnlock(PldSkills.安魂祈祷技能))
        {
            double reqCd = PldHelper.GetBurstStarterCooldownSeconds();
            if (reqCd < PldSettings.Instance.TwinDamageReqLockMin
                || reqCd > PldSettings.Instance.TwinDamageReqLockMax)
            {
                return false;
            }
        }

        return true;
    }

    private static long _lastDiagTime;

    internal static void Diag(
        bool shouldUse,
        bool scornUnlock, bool scornReady,
        bool expiUnlock, bool expiReady)
    {
        if (!Diagnostics.Enabled)
        {
            return;
        }

        long now = AEAssist.Helper.TimeHelper.Now();
        if (now - _lastDiagTime < 1000)
        {
            return;
        }

        _lastDiagTime = now;

        double fofCd = PldHelper.GetFofCooldownSeconds();
        bool lock1Ok = fofCd >= PldSettings.Instance.TwinDamageFofLockMin
                    && fofCd <= PldSettings.Instance.TwinDamageFofLockMax;

        bool reqUnlocked = SpellExtension.IsUnlock(PldSkills.绝对统治)
                        || SpellExtension.IsUnlock(PldSkills.安魂祈祷技能);
        double reqCd = PldHelper.GetBurstStarterCooldownSeconds();
        bool lock2Ok = !reqUnlocked
                    || (reqCd >= PldSettings.Instance.TwinDamageReqLockMin
                     && reqCd <= PldSettings.Instance.TwinDamageReqLockMax);

        PldHelper.LogTagged(
            $"[LittleDart-PLD/双伤] shouldUse={shouldUse} " +
            $"| 锁1(fofCd={fofCd:F1}s)={lock1Ok} " +
            $"锁2(reqCd={reqCd:F1}s unlocked={reqUnlocked})={lock2Ok} " +
            $"| 厄运流转 unlock={scornUnlock} ready={scornReady} " +
            $"| 偿赎剑 unlock={expiUnlock} ready={expiReady} " +
            $"| 起手技 ready={SpellExtension.IsReadyWithCanCast(PldHelper.GetBurstStarterSkill().GetSpell())} " +
            $"| fof={PldHelper.IsFofActive()} " +
            $"安魂buff={PldHelper.HasAura(PldSkills.安魂祈祷)} " +
            $"近战={PldHelper.IsInMeleeRange()}");
    }
}

public class CircleOfScornOffGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "oGCD：厄运流转（双伤 1）";

    protected override int CheckReal()
    {
        bool unlock = SpellExtension.IsUnlock(PldSkills.厄运流转);
        bool shouldUse = TwinDamage.ShouldUse();
        bool ready = SpellExtension.IsReadyWithCanCast(PldSkills.厄运流转.GetSpell());

        TwinDamage.Diag(
            shouldUse,
            unlock, ready,
            SpellExtension.IsUnlock(PldSkills.偿赎剑),
            SpellExtension.IsReadyWithCanCast(PldSkills.偿赎剑.GetSpell()));

        if (!unlock)
        {
            return -1;
        }

        if (!shouldUse)
        {
            return -1;
        }

        if (!ready)
        {
            return -1;
        }

        return 0;
    }

    public override void Build(Slot slot)
    {
        slot.Add(PldSkills.厄运流转.GetSpell());
    }
}

public class ExpiacionOffGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "oGCD：偿赎剑（双伤 2）";

    protected override int CheckReal()
    {
        if (!SpellExtension.IsUnlock(PldSkills.偿赎剑))
        {
            return -1;
        }

        if (!TwinDamage.ShouldUse())
        {
            return -1;
        }

        if (!SpellExtension.IsReadyWithCanCast(PldSkills.偿赎剑.GetSpell()))
        {
            return -1;
        }

        return 0;
    }

    public override void Build(Slot slot)
    {
        slot.Add(PldSkills.偿赎剑.GetSpell());
    }
}

public class SpiritsWithinOffGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "oGCD：深奥之灵（双伤 2 · 低等级）";

    protected override int CheckReal()
    {
        if (SpellExtension.IsUnlock(PldSkills.偿赎剑))
        {
            return -1;
        }

        if (!SpellExtension.IsUnlock(PldSkills.深奥之灵))
        {
            return -1;
        }

        if (!TwinDamage.ShouldUse())
        {
            return -1;
        }

        if (!SpellExtension.IsReadyWithCanCast(PldSkills.深奥之灵.GetSpell()))
        {
            return -1;
        }

        return 0;
    }

    public override void Build(Slot slot)
    {
        slot.Add(PldSkills.深奥之灵.GetSpell());
    }
}
