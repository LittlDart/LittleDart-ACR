using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.GCD;

public class SpiritOutsideGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "战逃外：强化圣灵（留到 buff 将被覆盖）";

    protected override int CheckReal()
    {
        if (!PldHelper.IsFofOutside())
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

        if (!PldHelper.HasDivineMight())
        {
            return -1;
        }

        if (!PldHelper.IsAtRoyalAuthorityStep())
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

public class RoyalAuthorityGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "基础连招：王权剑";

    protected override int CheckReal()
    {
        if (!PldHelper.HasValidTarget())
        {
            return -1;
        }

        if (!SpellExtension.IsUnlock(PldSkills.王权剑))
        {
            return -1;
        }

        if (!PldHelper.IsAtRoyalAuthorityStep())
        {
            return -1;
        }

        if (PldHelper.IsRecentlyCast(PldSkills.王权剑))
        {
            return -1;
        }

        if (!SpellExtension.IsReadyWithCanCast(PldSkills.王权剑.GetSpell()))
        {
            return -1;
        }

        return 0;
    }

    public override void Build(Slot slot)
    {
        slot.Add(PldSkills.王权剑.GetSpell());
    }
}

public class RiotBladeGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "基础连招：暴乱剑";

    protected override int CheckReal()
    {
        if (!PldHelper.HasValidTarget())
        {
            return -1;
        }

        if (!SpellExtension.IsUnlock(PldSkills.暴乱剑))
        {
            return -1;
        }

        if (PldHelper.GetLastComboSpellId() != PldSkills.先锋剑)
        {
            return -1;
        }

        if (!SpellExtension.IsReadyWithCanCast(PldSkills.暴乱剑.GetSpell()))
        {
            return -1;
        }

        return 0;
    }

    public override void Build(Slot slot)
    {
        slot.Add(PldSkills.暴乱剑.GetSpell());
    }
}

public class FastBladeGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "基础连招：先锋剑（兜底起手）";

    protected override int CheckReal()
    {
        if (!PldHelper.HasValidTarget())
        {
            return -1;
        }

        if (!SpellExtension.IsUnlock(PldSkills.先锋剑))
        {
            return -1;
        }

        uint combo = PldHelper.GetLastComboSpellId();
        if (combo == PldSkills.先锋剑 || combo == PldSkills.暴乱剑)
        {
            return -1;
        }

        if (!PldHelper.IsInMeleeRange())
        {
            return -1;
        }

        return 0;
    }

    public override void Build(Slot slot)
    {
        slot.Add(PldSkills.先锋剑.GetSpell());
    }
}
