using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using LittleDart.Paladin.Mitigation.Calc;
using LittleDart.Paladin.Setting;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.OffGcd;

public class IntervenePullOffGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "oGCD：调停·拉怪（把不是一仇/没进战斗的怪拽过来）";

    private const float FallbackRange = 20f;

    private const string LogTag = "[LittleDart-PLD/拉怪]";

    private const int DiagIntervalMs = 1000;

    private static long _lastDiagTime;

    private IBattleChara? _picked;

    protected override int CheckReal()
    {
        _picked = null;

        bool qtOn = LittleDartRotationEntry.QT.GetQt(PldQt.IntervenePull);
        if (!qtOn)
        {
            return -1;
        }

        if (!SpellExtension.IsUnlock(PldSkills.调停))
        {
            return -1;
        }

        int minAggro = PldSettings.Instance.IntervenePullMinAggro;
        int aggroOnMe = EnemyCounter.CountAggroOnMe();
        if (aggroOnMe < minAggro)
        {
            return -1;
        }

        float range = PldHelper.GetActionRange(PldSkills.调停);
        if (range <= 0f)
        {
            range = FallbackRange;
        }

        IBattleChara? pick = EnemyCounter.PickPullTarget(range);
        if (pick is null)
        {
            return -1;
        }

        Spell spell = new(PldSkills.调停, pick);
        bool ready = SpellExtension.IsReadyWithCanCast(spell);

        Diag(qtOn, minAggro, aggroOnMe, pick, range, ready);

        if (!ready)
        {
            return -1;
        }

        _picked = pick;
        return 0;
    }

    public override void Build(Slot slot)
    {
        if (_picked is null)
        {
            slot.Add(PldSkills.调停.GetSpell());
            return;
        }

        slot.Add(new Spell(PldSkills.调停, _picked));
    }

    private static void Diag(bool qtOn, int minAggro, int aggroOnMe, IBattleChara? pick, float range, bool ready)
    {
        if (!Diagnostics.Enabled)
        {
            return;
        }

        long now = TimeHelper.Now();
        if (now - _lastDiagTime < DiagIntervalMs)
        {
            return;
        }

        _lastDiagTime = now;

        bool hasPick = pick is not null;
        bool notInCombat = hasPick && (pick!.StatusFlags & StatusFlags.InCombat) == 0;

        PldHelper.LogTagged(
            $"{LogTag} 一仇={aggroOnMe} 门槛={minAggro} " +
            $"候选={(hasPick ? pick!.Name.ToString() : "无")} " +
            $"未进战斗={(hasPick ? notInCombat.ToString() : "n/a")} " +
            $"距={(hasPick ? PldHelper.GetDistanceTo(pick!).ToString("F1") : "n/a")} " +
            $"射程={range:F1} ready={ready}");
    }
}
