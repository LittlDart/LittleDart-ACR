using AEAssist.CombatRoutine.Module;
using LittleDart.Paladin.Mitigation;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.OffGcd;

public class ArmsLengthOffGcd : PldSlotResolverBase
{
    private static readonly MitigationSkillDef? Def = MitigationConfig.Find(PldSkills.亲疏);

    protected override string ResolverDescription => "亲疏自行（团队减 / 防击退）（调度器）";

    protected override int CheckReal() => MitigationResolverSupport.Check(Def);

    public override void Build(Slot slot) => MitigationResolverSupport.Cast(slot, Def);
}
