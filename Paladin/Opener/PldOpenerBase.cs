using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.CombatRoutine.Module.Opener;
using LittleDart.Paladin.Setting;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.Opener;

public abstract class PldOpenerBase : IOpener
{
    private const int InterveneInCountDownMs = 300;

    public abstract List<Action<Slot>> Sequence { get; }

    public virtual void InitCountDown(CountDownHandler countDownHandler)
    {
        PldSettings s = PldSettings.Instance;

        if (s.CurtainCallInCountdown)
        {
            countDownHandler.AddAction(10000, PldSkills.圣光幕帘, SpellTargetType.Self);
        }

        if (s.OpenerPreReadMs > 0)
        {
            countDownHandler.AddAction(s.OpenerPreReadMs, PldSkills.圣灵, SpellTargetType.Target);
        }

        countDownHandler.AddAction(InterveneInCountDownMs, PldSkills.调停, SpellTargetType.Target);
    }

    protected static void AddPotionIfEnabled(Slot slot)
    {
        if (LittleDartRotationEntry.QT.GetQt(PldQt.UsePotion))
        {
            slot.Add(Spell.CreatePotion());
        }
    }
}
