using AEAssist.CombatRoutine.Module;
using LittleDart.Paladin.Mitigation;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.OffGcd;

public class GuardianOffGcd : PldSlotResolverBase
{
    private static readonly MitigationSkillDef? Def = MitigationConfig.Find(PldSkills.极致防御);

    protected override string ResolverDescription => "极致防御（高频大减）（调度器）";

    protected override int CheckReal() => MitigationResolverSupport.Check(Def);

    public override void Build(Slot slot) => MitigationResolverSupport.Cast(slot, Def);
}
