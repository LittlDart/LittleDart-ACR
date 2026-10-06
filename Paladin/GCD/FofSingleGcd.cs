using AEAssist;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using LittleDart.Paladin.Skill;
using LittleDart.Paladin.Setting;

namespace LittleDart.Paladin.GCD;

public class FofGoringBladeGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "战逃内：沥血剑";

    protected override int CheckReal()
    {
        if (!PldHelper.IsFofInside())
        {
            return -1;
        }

        if (!PldHelper.HasValidTarget())
        {
            return -1;
        }

        if (!SpellExtension.IsUnlock(PldSkills.沥血剑))
        {
            return -1;
        }

        if (!PldHelper.HasAura(PldSkills.沥血剑预备))
        {
            return -1;
        }

        if (!SpellExtension.IsReadyWithCanCast(PldSkills.沥血剑.GetSpell()))
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
        slot.Add(PldSkills.沥血剑.GetSpell());
    }
}

public class FofConfiteorGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "战逃内：悔罪（悔罪连·链头）";

    protected override int CheckReal()
    {
        if (!PldHelper.IsFofInside())
        {
            return -1;
        }

        if (!PldHelper.HasValidTarget())
        {
            return -1;
        }

        if (!PldHelper.HasAura(PldSkills.安魂祈祷)
            && !PldHelper.HasAura(PldSkills.悔罪预备))
        {
            return -1;
        }

        if (!SpellExtension.IsUnlock(PldSkills.悔罪))
        {
            return -1;
        }

        if (PldHelper.IsRecentlyCast(PldSkills.悔罪))
        {
            return -1;
        }

        if (ConfiteorDelay.IsDelayed())
        {
            return -1;
        }

        if (!SpellExtension.IsReadyWithCanCast(PldSkills.悔罪.GetSpell()))
        {
            return -1;
        }

        return 0;
    }

    public override void Build(Slot slot)
    {
        slot.Add(PldSkills.悔罪.GetSpell());
    }
}

public class FofBladeOfFaithGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "悔罪连：信念之剑（第 2 段）";

    protected override int CheckReal()
    {
        if (!PldHelper.HasValidTarget())
        {
            return -1;
        }

        if (!SpellExtension.IsUnlock(PldSkills.信念之剑))
        {
            return -1;
        }

        return ConfiteorChain.CanFollow(PldSkills.悔罪, PldSkills.信念之剑) ? 0 : -1;
    }

    public override void Build(Slot slot)
    {
        slot.Add(PldSkills.信念之剑.GetSpell());
    }
}

public class FofBladeOfTruthGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "悔罪连：真理之剑（第 3 段）";

    protected override int CheckReal()
    {
        if (!PldHelper.HasValidTarget())
        {
            return -1;
        }

        if (!SpellExtension.IsUnlock(PldSkills.真理之剑))
        {
            return -1;
        }

        return ConfiteorChain.CanFollow(PldSkills.信念之剑, PldSkills.真理之剑) ? 0 : -1;
    }

    public override void Build(Slot slot)
    {
        slot.Add(PldSkills.真理之剑.GetSpell());
    }
}

public class FofBladeOfValorGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "悔罪连：英勇之剑（第 4 段）";

    protected override int CheckReal()
    {
        if (!PldHelper.HasValidTarget())
        {
            return -1;
        }

        if (!SpellExtension.IsUnlock(PldSkills.英勇之剑))
        {
            return -1;
        }

        return ConfiteorChain.CanFollow(PldSkills.真理之剑, PldSkills.英勇之剑) ? 0 : -1;
    }

    public override void Build(Slot slot)
    {
        slot.Add(PldSkills.英勇之剑.GetSpell());
    }
}

public class FofSpiritRequiescatGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "战逃内：圣灵（安魂祈祷状态中）";

    protected override int CheckReal()
    {
        if (!PldHelper.IsFofInside())
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

        if (!SpellExtension.IsUnlock(PldSkills.安魂祈祷技能)
            && !SpellExtension.IsUnlock(PldSkills.绝对统治))
        {
            return -1;
        }

        if (!PldHelper.HasAura(PldSkills.安魂祈祷))
        {
            return -1;
        }

        if (!PldHelper.IsInMeleeRange())
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

public class FofRequiescatSpiritRangedGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "战逃内远离：圣灵（安魂祈祷状态中）";

    protected override int CheckReal()
    {
        if (!PldHelper.IsFofInside())
        {
            return -1;
        }

        if (!PldHelper.HasValidTarget())
        {
            return -1;
        }

        if (!SpellExtension.IsUnlock(PldSkills.安魂祈祷技能)
            && !SpellExtension.IsUnlock(PldSkills.绝对统治))
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

        if (!PldHelper.HasAura(PldSkills.安魂祈祷))
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

public class FofSpiritEmpoweredGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "战逃内：圣灵（神圣魔法效果提高中）";

    protected override int CheckReal()
    {
        if (!PldHelper.IsFofInside())
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

        if (!PldHelper.IsInMeleeRange())
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

public class FofRoyalAuthorityGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "战逃内：王权剑";

    protected override int CheckReal()
    {
        if (!PldHelper.IsFofInside())
        {
            return -1;
        }

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

        if (!PldHelper.IsInMeleeRange())
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

public class FofAtonementGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "战逃内：赎罪剑（只判预备 buff）";

    protected override int CheckReal()
    {
        if (!PldHelper.IsFofInside())
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

        if (!PldHelper.IsInMeleeRange())
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

public class FofRiotBladeGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "战逃内：暴乱剑";

    protected override int CheckReal()
    {
        if (!PldHelper.IsFofInside())
        {
            return -1;
        }

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

        if (!PldHelper.IsInMeleeRange())
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

public class FofFastBladeGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "战逃内：先锋剑";

    protected override int CheckReal()
    {
        if (!PldHelper.IsFofInside())
        {
            return -1;
        }

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

public class FofRangedSpiritGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "战逃内远离：圣灵（强化）";

    protected override int CheckReal()
    {
        if (!PldHelper.IsFofInside())
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

        if (PldHelper.IsInMeleeRange())
        {
            return -1;
        }

        if (!PldHelper.PassesRangedHolySpiritGate())
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

public class FofSlideCastSpiritGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "战逃内远离：圣灵（滑步：锁移动后读条）";

    protected override int CheckReal()
    {
        if (!PldHelper.IsFofInside())
        {
            return -1;
        }

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

public class FofStillRangedSpiritGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "战逃内远离：圣灵（站定）";

    protected override int CheckReal()
    {
        if (!PldHelper.IsFofInside())
        {
            return -1;
        }

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

        if (PldHelper.IsInMeleeRange())
        {
            return -1;
        }

        if (!PldHelper.PassesRangedHolySpiritGate())
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

public class FofShieldLobGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "战逃内远离：投盾";

    protected override int CheckReal()
    {
        if (!PldHelper.IsFofInside())
        {
            return -1;
        }

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

        if (PldHelper.IsInMeleeRange())
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
