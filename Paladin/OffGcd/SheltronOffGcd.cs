using AEAssist.CombatRoutine.Module;
using LittleDart.Paladin.Mitigation;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.OffGcd;

public class SheltronOffGcd : PldSlotResolverBase
{
    private static readonly MitigationSkillDef? Def = MitigationConfig.Find(PldSkills.盾阵);

    protected override string ResolverDescription => "盾阵（短减，圣盾阵的低级形态）（调度器）";

    protected override int CheckReal() => MitigationResolverSupport.Check(Def);

    public override void Build(Slot slot) => MitigationResolverSupport.Cast(slot, Def);
}
