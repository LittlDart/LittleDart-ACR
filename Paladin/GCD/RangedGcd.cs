using AEAssist;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using LittleDart.Paladin.Setting;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.GCD;

public class HolySpiritRangedGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "远离：强化圣灵";

    protected override int CheckReal()
    {
        if (!PldHelper.HasValidTarget())
        {
            return -1;
        }

        if (!SpellExtension.IsUnlock(PldSkills.圣灵))
        {
            return -1;
        }

        if (!PldHelper.IsTargetFar())
        {
            return -1;
        }

        if (!PldHelper.PassesRangedHolySpiritGate())
        {
            return -1;
        }

        if (!PldHelper.HasDivineMight())
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

public class SlideCastHolySpiritGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "远离：滑步圣灵（锁移动后读条）";

    protected override int CheckReal()
    {
        if (!LittleDartRotationEntry.QT.GetQt(PldQt.SlideCast))
        {
            return -1;
        }

        if (PldHelper.IsDailyMode())
        {
            return -1;
        }

        if (!PldHelper.HasValidTarget())
        {
            return -1;
        }

        if (!SpellExtension.IsUnlock(PldSkills.圣灵))
        {
            return -1;
        }

        if (PldHelper.HasDivineMight())
        {
            return -1;
        }

        if (!PldHelper.IsTargetFar())
        {
            return -1;
        }

        if (!PldHelper.PassesRangedHolySpiritGate())
        {
            return -1;
        }

        if (Core.Me.CurrentMp < PldSettings.Instance.HolySpiritMinMp)
        {
            return -1;
        }

        PldSlideCast.RequestCast();

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

public class ReadCastHolySpiritRangedGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "远离：读条圣灵（站定才放）";

    protected override int CheckReal()
    {
        if (!LittleDartRotationEntry.QT.GetQt(PldQt.HolySpiritRanged))
        {
            return -1;
        }

        if (!PldHelper.HasValidTarget())
        {
            return -1;
        }

        if (!SpellExtension.IsUnlock(PldSkills.圣灵))
        {
            return -1;
        }

        if (!PldHelper.IsTargetFar())
        {
            return -1;
        }

        if (!PldHelper.PassesRangedHolySpiritGate())
        {
            return -1;
        }

        if (PldHelper.HasDivineMight())
        {
            return -1;
        }

        if (PldHelper.IsMoving())
        {
            return -1;
        }

        if (Core.Me.CurrentMp < PldSettings.Instance.HolySpiritMinMp)
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

public class ShieldLobGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "远离：投盾";

    protected override int CheckReal()
    {
        if (!LittleDartRotationEntry.QT.GetQt(PldQt.ShieldLobRanged))
        {
            return -1;
        }

        if (!PldHelper.HasValidTarget())
        {
            return -1;
        }

        if (!SpellExtension.IsUnlock(PldSkills.投盾))
        {
            return -1;
        }

        if (!PldHelper.IsTargetFar())
        {
            return -1;
        }

        if (!PldHelper.PassesRangedShieldLobGate())
        {
            return -1;
        }

        if (!SpellExtension.IsReadyWithCanCast(PldSkills.投盾.GetSpell()))
        {
            return -1;
        }

        return 0;
    }

    public override void Build(Slot slot)
    {
        slot.Add(PldSkills.投盾.GetSpell());
    }
}
