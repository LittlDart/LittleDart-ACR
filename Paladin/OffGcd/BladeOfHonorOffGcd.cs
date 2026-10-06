using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.OffGcd;

public class BladeOfHonorOffGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "oGCD：荣耀之剑（悔罪连收尾）";

    protected override int CheckReal()
    {
        if (!PldHelper.HasValidTarget())
        {
            return -1;
        }

        if (!SpellExtension.IsUnlock(PldSkills.荣耀之剑))
        {
            return -1;
        }

        if (!PldHelper.HasAura(PldSkills.荣耀之剑预备, 1))
        {
            return -1;
        }

        if (!SpellExtension.IsReadyWithCanCast(PldSkills.荣耀之剑.GetSpell()))
        {
            return -1;
        }

        return 0;
    }

    public override void Build(Slot slot)
    {
        slot.Add(PldSkills.荣耀之剑.GetSpell());
    }
}
