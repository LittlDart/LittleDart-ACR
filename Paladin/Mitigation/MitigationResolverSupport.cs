using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using AEAssist.JobApi;
using LittleDart.Paladin.Mitigation;

namespace LittleDart.Paladin.OffGcd;

internal static class MitigationResolverSupport
{
    public static int Check(MitigationSkillDef? def)
    {
        if (def == null)
        {
            return -1;
        }

        if (!MitigationScheduler.Instance.ShouldCast(def.SkillId))
        {
            return -1;
        }

        if (!SpellExtension.IsUnlock(def.SkillId))
        {
            return -1;
        }

        if (!def.SelfTarget && !PldHelper.HasValidTarget())
        {
            return -1;
        }

        if (def.OathCost > 0 && Core.Resolve<JobApi_Paladin>().Oath < def.OathCost)
        {
            return -1;
        }

        if (!SpellExtension.IsReadyWithCanCast(BuildSpell(def)))
        {
            return -1;
        }

        return 0;
    }

    public static void Cast(Slot slot, MitigationSkillDef? def)
    {
        if (def == null)
        {
            return;
        }

        slot.Add(BuildSpell(def));

        MitigationScheduler.Instance.NotifyCast(def.SkillId);
    }

    private static Spell BuildSpell(MitigationSkillDef def)
        => def.SkillId.GetSpell(def.SelfTarget ? SpellTargetType.Self : SpellTargetType.Target);
}
