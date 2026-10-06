using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.OffGcd;

public class FofOffGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "oGCD：战逃反应";

    protected override int CheckReal()
    {
        if (!LittleDartRotationEntry.QT.GetQt(PldQt.UseFof))
        {
            return -1;
        }

        if (LittleDartRotationEntry.QT.GetQt(PldQt.DelayFof))
        {
            return -1;
        }

        if (!PldHelper.HasValidTarget())
        {
            return -1;
        }

        if (!SpellExtension.IsUnlock(PldSkills.战逃反应))
        {
            return -1;
        }

        if (!SpellExtension.IsReadyWithCanCast(PldSkills.战逃反应.GetSpell()))
        {
            return -1;
        }

        if (LittleDartRotationEntry.QT.GetQt(PldQt.SmartFof) && PldHelper.IsTargetFar())
        {
            return -1;
        }

        return 0;
    }

    public override void Build(Slot slot)
    {
        slot.Add(PldSkills.战逃反应.GetSpell(SpellTargetType.Self));

    }
}
