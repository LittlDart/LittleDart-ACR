using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using LittleDart.Paladin.Setting;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.GCD;

internal static class AoeQueue
{
    public static bool ShouldUseAoe()
    {
        if (!LittleDartRotationEntry.QT.GetQt(PldQt.UseAoe))
        {
            return false;
        }

        if (!PldHelper.HasValidTarget())
        {
            return false;
        }

        var settings = PldSettings.Instance;
        int enemyCount = PldHelper.GetNearbyEnemyCount((int)settings.AoeRadius);

        return enemyCount >= settings.AoeEnemyCount;
    }
}

public class TotalEclipseGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "群体：全蚀斩";

    protected override int CheckReal()
    {
        if (!AoeQueue.ShouldUseAoe())
        {
            return -1;
        }

        if (!SpellExtension.IsUnlock(PldSkills.全蚀斩))
        {
            return -1;
        }

        if (!PldHelper.IsInMeleeRange())
        {
            return -1;
        }

        if (PldHelper.GetLastComboSpellId() == PldSkills.全蚀斩)
        {
            return -1;
        }

        if (!SpellExtension.IsReadyWithCanCast(PldSkills.全蚀斩.GetSpell()))
        {
            return -1;
        }

        return 0;
    }

    public override void Build(Slot slot)
    {
        slot.Add(PldSkills.全蚀斩.GetSpell());
    }
}

public class ProminenceGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "群体：日珥斩";

    protected override int CheckReal()
    {
        if (!AoeQueue.ShouldUseAoe())
        {
            return -1;
        }

        if (!SpellExtension.IsUnlock(PldSkills.日珥斩))
        {
            return -1;
        }

        if (!PldHelper.IsInMeleeRange())
        {
            return -1;
        }

        if (PldHelper.GetLastComboSpellId() != PldSkills.全蚀斩)
        {
            return -1;
        }

        if (!SpellExtension.IsReadyWithCanCast(PldSkills.日珥斩.GetSpell()))
        {
            return -1;
        }

        return 0;
    }

    public override void Build(Slot slot)
    {
        slot.Add(PldSkills.日珥斩.GetSpell());
    }
}

public class HolyCircleGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "群体：圣环（需强化 buff，权重低于悔罪连）";

    protected override int CheckReal()
    {
        if (!AoeQueue.ShouldUseAoe())
        {
            return -1;
        }

        if (!SpellExtension.IsUnlock(PldSkills.圣环))
        {
            return -1;
        }

        if (!PldHelper.IsInMeleeRange())
        {
            return -1;
        }

        if (!PldHelper.HasDivineMight())
        {
            return -1;
        }

        if (!SpellExtension.IsReadyWithCanCast(PldSkills.圣环.GetSpell()))
        {
            return -1;
        }

        return 0;
    }

    public override void Build(Slot slot)
    {
        slot.Add(PldSkills.圣环.GetSpell());
    }
}

public class FofHolyCircleRequiescatGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "群体战逃内：圣环（安魂祈祷状态中）";

    protected override int CheckReal()
    {
        if (!AoeQueue.ShouldUseAoe())
        {
            return -1;
        }

        if (!PldHelper.IsFofInside())
        {
            return -1;
        }

        if (!SpellExtension.IsUnlock(PldSkills.圣环))
        {
            return -1;
        }

        if (!PldHelper.IsInMeleeRange())
        {
            return -1;
        }

        if (!PldHelper.HasAura(PldSkills.安魂祈祷))
        {
            return -1;
        }

        if (PldHelper.IsRecentlyCast(PldSkills.圣环))
        {
            return -1;
        }

        if (!SpellExtension.IsReadyWithCanCast(PldSkills.圣环.GetSpell()))
        {
            return -1;
        }

        return 0;
    }

    public override void Build(Slot slot)
    {
        slot.Add(PldSkills.圣环.GetSpell());
    }
}

public class FofAoeConfiteorGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "群体战逃内：悔罪（悔罪连·链头）";

    protected override int CheckReal()
    {
        if (!PldHelper.IsFofInside())
        {
            return -1;
        }

        if (!AoeQueue.ShouldUseAoe())
        {
            return -1;
        }

        if (!PldHelper.IsFofActive())
        {
            return -1;
        }

        if (!PldHelper.HasAura(PldSkills.安魂祈祷))
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

