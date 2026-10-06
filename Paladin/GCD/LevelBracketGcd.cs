using AEAssist;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.GCD;

public abstract class PldBracketQueueGcd : PldSlotResolverBase
{
    protected abstract int BracketLevel { get; }

    protected abstract ISlotResolver[] CreateQueue();

    private ISlotResolver[]? _queue;

    private ISlotResolver[] Queue => _queue ??= CreateQueue();

    protected override int CheckReal()
    {
        if (Core.Me.Level != BracketLevel)
        {
            return -1;
        }

        foreach (ISlotResolver resolver in Queue)
        {
            if (resolver.Check() >= 0)
            {
                return 0;
            }
        }

        return -1;
    }

    public override void Build(Slot slot)
    {
        foreach (ISlotResolver resolver in Queue)
        {
            if (resolver.Check() >= 0)
            {
                resolver.Build(slot);
                return;
            }
        }
    }
}

public class FofConfiteorSoloGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "80档战逃内：悔罪（悔罪连只有这一段）";

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

public class PldLevel70Gcd : PldBracketQueueGcd
{
    protected override string ResolverDescription => "等级档 70：整条 GCD 队列";

    protected override int BracketLevel => 70;

    protected override ISlotResolver[] CreateQueue() =>
    [
        new FofGoringBladeGcd(),
        new FofSpiritRequiescatGcd(),
        new FofSpiritEmpoweredGcd(),
        new FofRoyalAuthorityGcd(),
        new FofRiotBladeGcd(),
        new FofFastBladeGcd(),
        new FofFillerSpiritGcd(),

        new FofRequiescatSpiritRangedGcd(),
        new FofRangedSpiritGcd(),
        new FofSlideCastSpiritGcd(),
        new FofStillRangedSpiritGcd(),
        new FofShieldLobGcd(),

        new RiotBladeGcd(),
        new BaseComboAfter64Gcd(),
        new RoyalAuthorityGcd(),
        new FastBladeGcd(),

        new HolySpiritRangedGcd(),
        new SlideCastHolySpiritGcd(),
        new ReadCastHolySpiritRangedGcd(),
        new ShieldLobGcd(),
    ];
}

public class PldLevel80Gcd : PldBracketQueueGcd
{
    protected override string ResolverDescription => "等级档 80：整条 GCD 队列";

    protected override int BracketLevel => 80;

    protected override ISlotResolver[] CreateQueue() =>
    [
        new FofGoringBladeGcd(),
        new FofConfiteorSoloGcd(),
        new FofSpiritRequiescatGcd(),
        new FofAtonementGcd(),
        new BaseComboAfter76SupplicationGcd(),
        new BaseComboAfter76SepulchreGcd(),
        new FofSpiritEmpoweredGcd(),
        new FofRoyalAuthorityGcd(),
        new FofRiotBladeGcd(),
        new FofFastBladeGcd(),
        new FofFillerSpiritGcd(),

        new FofRequiescatSpiritRangedGcd(),
        new FofRangedSpiritGcd(),
        new FofSlideCastSpiritGcd(),
        new FofStillRangedSpiritGcd(),
        new FofShieldLobGcd(),

        new BaseComboAfter76AtonementGcd(),
        new RiotBladeGcd(),
        new BaseComboAfter64Gcd(),
        new RoyalAuthorityGcd(),
        new FastBladeGcd(),

        new HolySpiritRangedGcd(),
        new SlideCastHolySpiritGcd(),
        new ReadCastHolySpiritRangedGcd(),
        new ShieldLobGcd(),
    ];
}

public class PldLevel90Gcd : PldBracketQueueGcd
{
    protected override string ResolverDescription => "等级档 90：整条 GCD 队列";

    protected override int BracketLevel => 90;

    protected override ISlotResolver[] CreateQueue() =>
    [
        new FofBladeOfFaithGcd(),
        new FofBladeOfTruthGcd(),
        new FofBladeOfValorGcd(),

        new FofGoringBladeGcd(),
        new FofConfiteorGcd(),
        new FofSpiritRequiescatGcd(),
        new FofAtonementGcd(),
        new BaseComboAfter76SupplicationGcd(),
        new BaseComboAfter76SepulchreGcd(),
        new FofSpiritEmpoweredGcd(),
        new FofRoyalAuthorityGcd(),
        new FofRiotBladeGcd(),
        new FofFastBladeGcd(),
        new FofFillerSpiritGcd(),

        new FofRequiescatSpiritRangedGcd(),
        new FofRangedSpiritGcd(),
        new FofSlideCastSpiritGcd(),
        new FofStillRangedSpiritGcd(),
        new FofShieldLobGcd(),

        new BaseComboAfter76AtonementGcd(),
        new RiotBladeGcd(),
        new BaseComboAfter64Gcd(),
        new RoyalAuthorityGcd(),
        new FastBladeGcd(),

        new HolySpiritRangedGcd(),
        new SlideCastHolySpiritGcd(),
        new ReadCastHolySpiritRangedGcd(),
        new ShieldLobGcd(),
    ];
}
