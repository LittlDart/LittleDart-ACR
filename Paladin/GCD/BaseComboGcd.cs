using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.GCD;

public class BaseComboBefore64Gcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "基础连招(64级前)：王权剑";

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

        if (PldHelper.IsRecentlyCast(PldSkills.王权剑))
        {
            return -1;
        }

        if (SpellExtension.IsUnlock(PldSkills.圣灵))
        {
            return -1;
        }

        if (!PldHelper.IsAtRoyalAuthorityStep())
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

public class BaseComboHaloneGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "基础连招(60级前)：战女神之怒";

    protected override int CheckReal()
    {
        if (!PldHelper.HasValidTarget())
        {
            return -1;
        }

        if (SpellExtension.IsUnlock(PldSkills.王权剑))
        {
            return -1;
        }

        if (!SpellExtension.IsUnlock(PldSkills.战女神之怒))
        {
            return -1;
        }

        if (!PldHelper.IsAtRoyalAuthorityStep())
        {
            return -1;
        }

        if (!SpellExtension.IsReadyWithCanCast(PldSkills.战女神之怒.GetSpell()))
        {
            return -1;
        }

        return 0;
    }

    public override void Build(Slot slot)
    {
        slot.Add(PldSkills.战女神之怒.GetSpell());
    }
}

public class BaseComboAfter64Gcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "基础连招(64级后)：圣灵";

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

        if (!PldHelper.CanUseEmpoweredHolySpirit())
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

public class BaseComboAfter76AtonementGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "基础连招(76级后)：赎罪剑";

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

        if (!SpellExtension.IsUnlock(PldSkills.赎罪剑))
        {
            return -1;
        }

        if (!PldHelper.HasAura(PldSkills.赎罪剑预备))
        {
            return -1;
        }

        if (PldHelper.IsRecentlyCast(PldSkills.赎罪剑))
        {
            return -1;
        }

        if (!PldHelper.IsAtRoyalAuthorityStep())
        {
            return -1;
        }

        if (!SpellExtension.IsReadyWithCanCast(PldSkills.赎罪剑.GetSpell()))
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

public class BaseComboAfter76SupplicationGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "基础连招(76级后)：祈告剑";

    protected override int CheckReal()
    {
        if (!PldHelper.HasValidTarget())
        {
            return -1;
        }

        if (!SpellExtension.IsUnlock(PldSkills.祈告剑))
        {
            return -1;
        }

        if (!PldHelper.HasAura(PldSkills.祈告剑预备))
        {
            return -1;
        }

        if (!SpellExtension.IsReadyWithCanCast(PldSkills.祈告剑.GetSpell()))
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

public class BaseComboAfter76SepulchreGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "基础连招(76级后)：葬送剑";

    protected override int CheckReal()
    {
        if (!PldHelper.HasValidTarget())
        {
            return -1;
        }

        if (!SpellExtension.IsUnlock(PldSkills.葬送剑))
        {
            return -1;
        }

        if (!PldHelper.HasAura(PldSkills.葬送剑预备))
        {
            return -1;
        }

        if (!SpellExtension.IsReadyWithCanCast(PldSkills.葬送剑.GetSpell()))
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
