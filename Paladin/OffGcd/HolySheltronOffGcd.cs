using AEAssist.CombatRoutine.Module;
using LittleDart.Paladin.Mitigation;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.OffGcd;

public class HolySheltronOffGcd : PldSlotResolverBase
{
    private static readonly MitigationSkillDef? Def = MitigationConfig.Find(PldSkills.圣盾阵);

    protected override string ResolverDescription => "圣盾阵（短减，消耗 50 忠义）（调度器）";

    protected override int CheckReal() => MitigationResolverSupport.Check(Def);

    public override void Build(Slot slot) => MitigationResolverSupport.Cast(slot, Def);
}
