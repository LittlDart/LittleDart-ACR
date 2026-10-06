using AEAssist.CombatRoutine.Module;
using LittleDart.Paladin.Mitigation;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.OffGcd;

public class SentinelOffGcd : PldSlotResolverBase
{
    private static readonly MitigationSkillDef? Def = MitigationConfig.Find(PldSkills.预警);

    protected override string ResolverDescription => "预警（高频大减，92 级被极致防御取代）（调度器）";

    protected override int CheckReal() => MitigationResolverSupport.Check(Def);

    public override void Build(Slot slot) => MitigationResolverSupport.Cast(slot, Def);
}
