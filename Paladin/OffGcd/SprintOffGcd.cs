using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using LittleDart.Paladin.Mitigation.Calc;
using LittleDart.Paladin.Setting;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.OffGcd;

public class SprintOffGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "冲刺（自动，仅多目标）";

    protected override int CheckReal()
    {
        if (!LittleDartRotationEntry.QT.GetQt(PldQt.AutoSprint))
        {
            return -1;
        }

        var settings = PldSettings.Instance;
        if (settings == null)
        {
            return -1;
        }

        if (!SpellExtension.IsUnlock(PldSkills.疾跑)
            || !SpellExtension.IsReadyWithCanCast(PldSkills.疾跑.GetSpell(SpellTargetType.Self)))
        {
            return -1;
        }

        int enemies = PldHelper.GetNearbyEnemyCount((int)Math.Round(settings.MitigationEnemyCountRadius));
        if (enemies <= settings.MitigationSprintMinEnemies)
        {
            return -1;
        }

        return DistanceCalc.InRange(settings.MitigationSprintMinRange, settings.MitigationSprintMaxRange) ? 0 : -1;
    }

    public override void Build(Slot slot) => slot.Add(PldSkills.疾跑.GetSpell(SpellTargetType.Self));
}
