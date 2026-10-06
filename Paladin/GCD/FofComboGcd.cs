using AEAssist;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using LittleDart.Paladin.Setting;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.GCD;

internal static class FofQueue
{
    public static bool IsInFofWindow()
    {
        return PldHelper.HasValidTarget()
            && PldHelper.HasCombo()
            && PldHelper.IsFofActive();
    }
}

internal static class ConfiteorChain
{
    public const int ChainWindowMs = 5000;

    public static bool CanFollow(uint previous, uint current)
    {
        if (!SpellExtension.IsUnlock(current))
        {
            return false;
        }

        if (!SpellExtension.RecentlyUsed(previous, ChainWindowMs))
        {
            return false;
        }

        if (!SpellExtension.IsReadyWithCanCast(current.GetSpell()))
        {
            return false;
        }

        return true;
    }
}

public class FofFillerSpiritGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "战逃内：兜底圣灵";

    protected override int CheckReal()
    {
        if (!PldHelper.IsFofInside())
        {
            return -1;
        }

        if (!SpellExtension.IsUnlock(PldSkills.圣灵))
        {
            return -1;
        }

        if (!PldHelper.IsInMeleeRange())
        {
            return -1;
        }

        if (Core.Me.CurrentMp < PldSettings.Instance.HolySpiritMinMp)
        {
            return -1;
        }

        if (PldHelper.IsMoving())
        {
            return -1;
        }

        if (!SpellExtension.IsReadyWithCanCast(PldSkills.圣灵.GetSpell()))
        {
            return -1;
        }

        return 0;
    }

    public override void Build(Slot slot)
    {
        slot.Add(PldSkills.圣灵.GetSpell());
    }
}

public static class ConfiteorDelay
{
    private static int _remainingGcds;

    public static bool IsDelayed()
    {
        if (!LittleDartRotationEntry.QT.GetQt(PldQt.DelayConfiteor))
        {
            return false;
        }

        return _remainingGcds > 0;
    }

    public static void StartDelay()
    {
        if (!LittleDartRotationEntry.QT.GetQt(PldQt.DelayConfiteor))
        {
            return;
        }

        _remainingGcds = 2;
    }

    public static void Tick()
    {
        if (_remainingGcds > 0)
        {
            _remainingGcds--;
        }
    }

    public static void Reset()
    {
        _remainingGcds = 0;
    }
}
