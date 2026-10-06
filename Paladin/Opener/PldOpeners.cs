using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.Opener;

public class PldOpener100 : PldOpenerBase
{
    public override List<Action<Slot>> Sequence { get; } = new() { Step1 };

    private static void Step1(Slot slot)
    {
        slot.Add(PldSkills.先锋剑.GetSpell());
        slot.Add(PldSkills.暴乱剑.GetSpell());
        AddPotionIfEnabled(slot);
        slot.Add(PldSkills.王权剑.GetSpell());

        slot.Add(PldSkills.战逃反应.GetSpell(SpellTargetType.Self));
        slot.Add(PldSkills.绝对统治.GetSpell());

        slot.Add(PldSkills.沥血剑.GetSpell());
        slot.Add(PldSkills.厄运流转.GetSpell());
        slot.Add(PldSkills.偿赎剑.GetSpell());

        slot.Add(PldSkills.悔罪.GetSpell());
        slot.Add(PldSkills.信念之剑.GetSpell());
        slot.Add(PldSkills.真理之剑.GetSpell());
        slot.Add(PldSkills.英勇之剑.GetSpell());

        slot.Add(PldSkills.荣耀之剑.GetSpell());

        slot.Add(PldSkills.赎罪剑.GetSpell());
        slot.Add(PldSkills.祈告剑.GetSpell());
        slot.Add(PldSkills.葬送剑.GetSpell());
    }
}

public class PldOpener100Fru : PldOpenerBase
{
    public override List<Action<Slot>> Sequence { get; } = new() { Step1 };

    private static void Step1(Slot slot)
    {
        slot.Add(PldSkills.先锋剑.GetSpell());
        slot.Add(PldSkills.厄运流转.GetSpell());
        slot.Add(PldSkills.偿赎剑.GetSpell());

        slot.Add(PldSkills.暴乱剑.GetSpell());
        AddPotionIfEnabled(slot);

        slot.Add(PldSkills.圣灵.GetSpell());
        slot.Add(PldSkills.圣灵.GetSpell());

        slot.Add(PldSkills.王权剑.GetSpell());

        slot.Add(PldSkills.赎罪剑.GetSpell());
        slot.Add(PldSkills.战逃反应.GetSpell(SpellTargetType.Self));
        slot.Add(PldSkills.绝对统治.GetSpell());

        slot.Add(PldSkills.沥血剑.GetSpell());
        slot.Add(PldSkills.调停.GetSpell());

        slot.Add(PldSkills.悔罪.GetSpell());
        slot.Add(PldSkills.信念之剑.GetSpell());
        slot.Add(PldSkills.真理之剑.GetSpell());
        slot.Add(PldSkills.英勇之剑.GetSpell());
        slot.Add(PldSkills.荣耀之剑.GetSpell());

        slot.Add(PldSkills.祈告剑.GetSpell());
        slot.Add(PldSkills.调停.GetSpell());

        slot.Add(PldSkills.葬送剑.GetSpell());
        slot.Add(PldSkills.厄运流转.GetSpell());
        slot.Add(PldSkills.偿赎剑.GetSpell());

        slot.Add(PldSkills.圣灵.GetSpell());
    }
}

public class PldOpener90 : PldOpenerBase
{
    public override List<Action<Slot>> Sequence { get; } = new() { Step1 };

    private static void Step1(Slot slot)
    {
        slot.Add(PldSkills.先锋剑.GetSpell());
        slot.Add(PldSkills.暴乱剑.GetSpell());
        AddPotionIfEnabled(slot);
        slot.Add(PldSkills.王权剑.GetSpell());

        slot.Add(PldSkills.战逃反应.GetSpell(SpellTargetType.Self));
        slot.Add(PldSkills.安魂祈祷技能.GetSpell());

        slot.Add(PldSkills.沥血剑.GetSpell());
        slot.Add(PldSkills.厄运流转.GetSpell());
        slot.Add(PldSkills.偿赎剑.GetSpell());

        slot.Add(PldSkills.悔罪.GetSpell());
        slot.Add(PldSkills.信念之剑.GetSpell());
        slot.Add(PldSkills.真理之剑.GetSpell());
        slot.Add(PldSkills.英勇之剑.GetSpell());

        slot.Add(PldSkills.赎罪剑.GetSpell());
        slot.Add(PldSkills.祈告剑.GetSpell());
        slot.Add(PldSkills.葬送剑.GetSpell());
    }
}

public class PldOpener80 : PldOpenerBase
{
    public override List<Action<Slot>> Sequence { get; } = new() { Step1 };

    private static void Step1(Slot slot)
    {
        slot.Add(PldSkills.先锋剑.GetSpell());
        slot.Add(PldSkills.暴乱剑.GetSpell());
        AddPotionIfEnabled(slot);
        slot.Add(PldSkills.王权剑.GetSpell());

        slot.Add(PldSkills.战逃反应.GetSpell(SpellTargetType.Self));
        slot.Add(PldSkills.安魂祈祷技能.GetSpell());

        slot.Add(PldSkills.沥血剑.GetSpell());
        slot.Add(PldSkills.厄运流转.GetSpell());
        slot.Add(PldSkills.深奥之灵.GetSpell());

        slot.Add(PldSkills.悔罪.GetSpell());

        slot.Add(PldSkills.圣灵.GetSpell());
        slot.Add(PldSkills.圣灵.GetSpell());
        slot.Add(PldSkills.圣灵.GetSpell());
        slot.Add(PldSkills.圣灵.GetSpell());
    }
}

public class PldOpener70 : PldOpenerBase
{
    public override List<Action<Slot>> Sequence { get; } = new() { Step1 };

    private static void Step1(Slot slot)
    {
        slot.Add(PldSkills.先锋剑.GetSpell());
        slot.Add(PldSkills.暴乱剑.GetSpell());
        AddPotionIfEnabled(slot);
        slot.Add(PldSkills.王权剑.GetSpell());

        slot.Add(PldSkills.战逃反应.GetSpell(SpellTargetType.Self));
        slot.Add(PldSkills.安魂祈祷技能.GetSpell());

        slot.Add(PldSkills.沥血剑.GetSpell());
        slot.Add(PldSkills.厄运流转.GetSpell());
        slot.Add(PldSkills.深奥之灵.GetSpell());

        slot.Add(PldSkills.圣灵.GetSpell());
        slot.Add(PldSkills.圣灵.GetSpell());
        slot.Add(PldSkills.圣灵.GetSpell());
        slot.Add(PldSkills.圣灵.GetSpell());
    }
}
