using AEAssist;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using LittleDart.Paladin.Setting;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.OffGcd;

public class InterveneOffGcd : PldSlotResolverBase
{
    protected override string ResolverDescription => "oGCD：调停（保留层数 / 安全距离 / 爆发内不保留）";

    private const string LogTag = "[LittleDart-PLD/调停]";

    private const int DiagIntervalMs = 1000;

    private static long _lastDiagTime;

    private static void Diag(
        bool qtOn, bool delayFof,
        bool hasTarget, bool unlock, bool ready,
        float distanceToCircle, float keepDistance, bool passDistance,
        float rawCharges, int usableCharges, int keepCharges, bool burstExempt, bool passCharges,
        bool fofBuff, bool fofRecent, bool inBurst)
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

        bool pass = qtOn && !delayFof && hasTarget && unlock && ready
                    && passDistance && passCharges;

        string distText = hasTarget ? $"{distanceToCircle:F1}" : "n/a";

        PldHelper.LogTagged(
            $"{LogTag} pass={pass} " +
            $"| qt={qtOn} 延后战逃={delayFof} " +
            $"| target={hasTarget} unlock={unlock} ready={ready} " +
            $"| 圈距={distText} 安全距离={keepDistance:F1} 距离闸={passDistance} " +
            $"| 原值={rawCharges:F2} 可用层={usableCharges} 保留={keepCharges} 层数闸={passCharges} " +
            $"| fofBuff={fofBuff} fofRecent={fofRecent} inBurst={inBurst} " +
            $"爆发豁免={burstExempt}");
    }

    protected override int CheckReal()
    {
        if (PldHelper.IsDailyMode())
        {
            return -1;
        }

        bool qtOn = LittleDartRotationEntry.QT.GetQt(PldQt.UseIntervene);
        bool delayFof = LittleDartRotationEntry.QT.GetQt(PldQt.DelayFof);

        bool hasTarget = PldHelper.HasValidTarget();
        bool unlock = SpellExtension.IsUnlock(PldSkills.调停);
        bool ready = SpellExtension.IsReadyWithCanCast(PldSkills.调停.GetSpell());

        float keepDistance = PldSettings.Instance.InterveneKeepDistance;
        float distanceToCircle = PldHelper.GetDistanceToTargetCircle();
        bool passDistance = keepDistance <= 0f || distanceToCircle <= keepDistance;

        float rawCharges = Core.Resolve<MemApiSpell>().GetCharges(PldSkills.调停);
        int usableCharges = (int)MathF.Floor(rawCharges);

        int keepCharges = PldSettings.Instance.InterveneKeepCharges;

        bool fofBuff = PldHelper.IsFofActive();
        bool fofRecent = PldHelper.IsFofRecentCast();
        bool inBurst = PldHelper.IsFofInside();

        bool burstExempt = PldSettings.Instance.InterveneKeepInBurst && inBurst;

        bool passCharges = burstExempt || usableCharges > keepCharges;

        Diag(qtOn, delayFof,
             hasTarget, unlock, ready,
             distanceToCircle, keepDistance, passDistance,
             rawCharges, usableCharges, keepCharges, burstExempt, passCharges,
             fofBuff, fofRecent, inBurst);

        if (!qtOn)
        {
            return -1;
        }

        if (delayFof)
        {
            return -1;
        }

        if (!hasTarget)
        {
            return -1;
        }

        if (!unlock)
        {
            return -1;
        }

        if (!ready)
        {
            return -1;
        }

        if (!passDistance)
        {
            return -1;
        }

        if (!passCharges)
        {
            return -1;
        }

        return 0;
    }

    public override void Build(Slot slot)
    {
        slot.Add(PldSkills.调停.GetSpell());
    }
}
