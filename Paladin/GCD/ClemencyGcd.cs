using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using LittleDart.Paladin.Mitigation;
using LittleDart.Paladin.Mitigation.Calc;
using LittleDart.Paladin.Setting;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.GCD;

public class ClemencyGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "深仁厚泽（预计5秒内死亡自救）";

    protected override int CheckReal()
    {
        if (!LittleDartRotationEntry.QT.GetQt(PldQt.MitigationClemency))
        {
            return -1;
        }

        var settings = PldSettings.Instance;
        if (settings == null)
        {
            return -1;
        }

        if (!SpellExtension.IsUnlock(PldSkills.深仁厚泽))
        {
            return -1;
        }

        if (Core.Me.CurrentMp < settings.ClemencyMinMp)
        {
            return -1;
        }

        if (PldHelper.IsMoving())
        {
            return -1;
        }

        if (PldHelper.IsRecentlyCast(PldSkills.深仁厚泽, 3000))
        {
            return -1;
        }

        if (!SpellExtension.IsReadyWithCanCast(PldSkills.深仁厚泽.GetSpell(SpellTargetType.Self)))
        {
            return -1;
        }

        return SelfDeathPredictor.WillDieWithin(
            MitigationScheduler.Instance.State.Hp, settings.MitigationClemencyDeathSeconds, 3000) ? 0 : -1;
    }

    public override void Build(Slot slot) => slot.Add(PldSkills.深仁厚泽.GetSpell(SpellTargetType.Self));
}
