using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.OffGcd;

public class RequiescatOffGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "oGCD：安魂祈祷/绝对统治（强绑定）";

    protected override int CheckReal()
    {
        if (!LittleDartRotationEntry.QT.GetQt(PldQt.UseRequiescat))
        {
            return -1;
        }

        if (!PldHelper.HasValidTarget())
        {
            return -1;
        }

        if (!PldHelper.IsFofActive())
        {
            return -1;
        }

        if (TwinDamage.IsRequiescatCastable())
        {
            return 0;
        }

        return -1;
    }

    public override void Build(Slot slot)
    {
        if (SpellExtension.IsUnlock(PldSkills.绝对统治)
            && SpellExtension.IsReadyWithCanCast(PldSkills.绝对统治.GetSpell()))
        {
            slot.Add(PldSkills.绝对统治.GetSpell());
        }
        else
        {
            slot.Add(PldSkills.安魂祈祷技能.GetSpell());
        }

    }
}
