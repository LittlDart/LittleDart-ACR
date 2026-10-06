using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using LittleDart.Paladin.Setting;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.GCD;

internal static class DumpQueue
{
    public static bool IsEnabled()
    {
        return LittleDartRotationEntry.QT.GetQt(PldQt.DumpResource)
            && PldHelper.HasValidTarget();
    }

    public static bool HasMpForHolySpirit()
    {
        return Core.Me.CurrentMp >= PldSettings.Instance.HolySpiritMinMp;
    }
}

public class DumpAtonementGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "倾泻资源：赎罪剑";

    protected override int CheckReal()
    {
        if (!DumpQueue.IsEnabled())
        {
            return -1;
        }

        if (!SpellExtension.IsUnlock(PldSkills.赎罪剑))
        {
            return -1;
        }

        if (PldHelper.GetAtonementChainStep() != 1)
        {
            return -1;
        }

        return 0;
    }

    public override void Build(Slot slot)
    {
        slot.Add(PldSkills.赎罪剑.GetSpell());
    }
}

public class DumpSupplicationGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "倾泻资源：祈告剑";

    protected override int CheckReal()
    {
        if (!DumpQueue.IsEnabled())
        {
            return -1;
        }

        if (!SpellExtension.IsUnlock(PldSkills.祈告剑))
        {
            return -1;
        }

        if (PldHelper.GetAtonementChainStep() != 2)
        {
            return -1;
        }

        return 0;
    }

    public override void Build(Slot slot)
    {
        slot.Add(PldSkills.祈告剑.GetSpell());
    }
}

public class DumpSepulchreGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "倾泻资源：葬送剑";

    protected override int CheckReal()
    {
        if (!DumpQueue.IsEnabled())
        {
            return -1;
        }

        if (!SpellExtension.IsUnlock(PldSkills.葬送剑))
        {
            return -1;
        }

        if (PldHelper.GetAtonementChainStep() != 3)
        {
            return -1;
        }

        return 0;
    }

    public override void Build(Slot slot)
    {
        slot.Add(PldSkills.葬送剑.GetSpell());
    }
}

public class DumpHolySpiritGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "倾泻资源：圣灵（强化/读条兜底）";

    protected override int CheckReal()
    {
        if (!DumpQueue.IsEnabled())
        {
            return -1;
        }

        if (!SpellExtension.IsUnlock(PldSkills.圣灵))
        {
            return -1;
        }

        if (PldHelper.IsAtonementChainRunning())
        {
            return -1;
        }

        if (!PldSettings.Instance.DumpHardCastHolySpirit)
        {
            return -1;
        }

        if (!DumpQueue.HasMpForHolySpirit())
        {
            return -1;
        }

        if (PldHelper.IsMoving())
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
