using AEAssist;
using AEAssist.Helper;
using LittleDart.Paladin.Skill;
using AETime = AEAssist.Helper.TimeHelper;

namespace LittleDart.Paladin;

public static class Diagnostics
{
    public static bool Enabled =>
        LittleDartRotationEntry.QT != null
        && LittleDartRotationEntry.QT.GetQt(PldQt.DevDebug);

    private static long _lastComboLogTime;

    public static void OnHit(string description, int result)
    {
        if (description.Contains("王权剑")
            || description.Contains("暴乱剑")
            || description.Contains("圣灵")
            || description.Contains("先锋剑")
            || description.Contains("赎罪剑")
            || description.Contains("葬送剑")
            || description.Contains("祈告剑"))
        {
            PldHelper.LogTagged(
                $"[LittleDart-PLD/Hit] {description} → {result} | " +
                $"combo={PldHelper.GetLastComboSpellId()} " +
                $"comboLeft={PldHelper.GetComboTimeLeftSeconds():F2}s " +
                $"divineMight={PldHelper.HasDivineMight()} " +
                $"atonementReady={PldHelper.HasAura(PldSkills.赎罪剑预备)} " +
                FofFields());
            return;
        }

        PldHelper.LogTagged($"[LittleDart-PLD/Hit] {description} → {result} | {FofFields()}");
    }

    private static string FofFields() =>
        $"fofBuff={PldHelper.IsFofActive()} " +
        $"fofRecent={PldHelper.IsFofRecentCast()} " +
        $"fofInside={PldHelper.IsFofInside()}";

    public static void TickComboSnapshot()
    {
        if (!Enabled)
        {
            return;
        }

        long now = AETime.Now();
        if (now - _lastComboLogTime < 500)
        {
            return;
        }

        _lastComboLogTime = now;

        PldHelper.LogTagged(
            $"[LittleDart-PLD/Combo] 等级={Core.Me.Level} " +
            $"combo={PldHelper.GetLastComboSpellId()} " +
            $"left={PldHelper.GetComboTimeLeftSeconds():F2}s " +
            $"divineMight={PldHelper.HasDivineMight()} " +
            FofFields() +
            " | " + ChainBuffFields());
    }

    private static string Remain(uint auraId)
    {
        if (!PldHelper.HasAura(auraId))
        {
            return "无";
        }

        if (PldHelper.HasAura(auraId, 25000)) return "≥25s";
        if (PldHelper.HasAura(auraId, 15000)) return "≥15s";
        if (PldHelper.HasAura(auraId, 5000)) return "≥5s";
        return "<5s";
    }

    private static string ChainBuffFields() =>
        $"安魂={Remain(PldSkills.安魂祈祷)} " +
        $"安魂层={PldHelper.GetAuraStack(PldSkills.安魂祈祷)} " +
        $"沥血预备={Remain(PldSkills.沥血剑预备)} " +
        $"赎罪预备={Remain(PldSkills.赎罪剑预备)} " +
        $"祈告预备={Remain(PldSkills.祈告剑预备)} " +
        $"葬送预备={Remain(PldSkills.葬送剑预备)}";
}
