using AEAssist.Helper;
using AEAssist.IO;
using AEAssist.CombatRoutine.View.JobView;
using LittleDart.Paladin.Mitigation;
using System.IO;

namespace LittleDart.Paladin.Setting;

public class PldSettings
{

    public static PldSettings Instance = null!;

    private static string _path = string.Empty;

    public static void Build(string settingFolder)
    {
        _path = Path.Combine(settingFolder, "PldSettings.json");

        if (!File.Exists(_path))
        {
            Instance = new PldSettings();
            Instance.Save();
            return;
        }

        try
        {
            var json = File.ReadAllText(_path);
            Instance = JsonHelper.FromJson<PldSettings>(json);
        }
        catch (Exception ex)
        {
            Instance = new PldSettings();
            LogHelper.Error($"[LittleDart-PLD] PldSettings 读取失败，已重置为默认值：{ex}");
        }
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        File.WriteAllText(_path, JsonHelper.ToJson(this));
    }

    public JobViewSave JobViewSave = new();

    public float AoeRadius = 6f;

    public int AoeEnemyCount = 3;

    public float TwinDamageFofLockMin = 12.5f;

    public float TwinDamageFofLockMax = 60f;

    public float TwinDamageReqLockMin = 10f;

    public float TwinDamageReqLockMax = 60f;

    public uint HolySpiritMinMp = 2000;

    public bool DumpHardCastHolySpirit = false;

    public float RampartThreshold = 0.6f;

    public float GuardianThreshold = 0.6f;

    public float HolySheltronThreshold = 0.7f;

    public float BulwarkThreshold = 0.6f;

    public float ReprisalThreshold = 0.6f;

    public float SupportThreshold = 0.7f;

    public bool MitigationRequireTankStance = true;

    public float MitigationInvulnThresholdPercent = MitigationConfig.InvulnThresholdPercent;

    public float MitigationDefaultNextPullSeconds = MitigationConfig.DefaultNextPullSeconds;

    public float MitigationEnemyCountRadius = MitigationConfig.EnemyCountRadius;

    public int MitigationDamageWindowMs = MitigationConfig.DamageWindowMs;

    public int MitigationPhase1EndMs = MitigationConfig.Phase1EndMs;

    public int MitigationPhase2StartMs = MitigationConfig.Phase2StartMs;

    public int MitigationWaveEndDebounceMs = MitigationConfig.WaveEndDebounceMs;

    public int MitigationReprisalMinEnemiesIn5m = MitigationConfig.ReprisalMinEnemiesIn5m;

    public int MitigationAoeWarnMs = MitigationConfig.AoeWarnMs;

    public int MitigationArmsLengthAggroOver = MitigationConfig.ArmsLengthAggroOnMeOver;

    public float MitigationTargetDeathSkipSeconds = (float)MitigationConfig.TargetDeathSkipSeconds;

    public float MitigationTargetDeathSkipCooldownSeconds = (float)MitigationConfig.TargetDeathSkipCooldownSeconds;

    public float MitigationClemencyDeathSeconds = (float)MitigationConfig.ClemencyDeathSeconds;

    public float MitigationSprintMinRange = MitigationConfig.SprintMinRange;

    public float MitigationSprintMaxRange = MitigationConfig.SprintMaxRange;

    public int MitigationSprintMinEnemies = MitigationConfig.SprintMinEnemies;

    public uint ClemencyMinMp = 2000;

    public int MitigationReserveMinCooldownSeconds = MitigationConfig.ReserveMinCooldownSeconds;

    public int MitigationMultiTargetEnemies = MitigationConfig.MultiTargetEnemies;

    public float MitigationStressEnemyWeight = MitigationConfig.StressEnemyWeight;

    public float MitigationStressDamageWeight = MitigationConfig.StressDamageWeight;

    public float MitigationStressMidThreshold = MitigationConfig.StressMidThreshold;

    public float MitigationStressHighThreshold = MitigationConfig.StressHighThreshold;

    public bool OpenerEnabled = true;

    public int OpenerPreReadMs = 1500;

    public int InterveneKeepCharges = 1;

    public float InterveneKeepDistance = 0f;

    public bool InterveneKeepInBurst;

    public int IntervenePullMinAggro = 3;

    public int RangedHolySpiritMinDistance = 3;

    public int RangedShieldLobMinDistance = 8;

    public int SlideCastReleaseMs = 350;

    public bool AutoTankStanceIn4Man = true;

    public int MitigationScarceBigThresholdPercent = MitigationConfig.ScarceBigThresholdPercent;

    public int MitigationBigThresholdPercent = MitigationConfig.BigThresholdPercent;

    public int MitigationInvulnHpPercent = MitigationConfig.InvulnHpPercent;

    public bool AutoDetectBurst;

    public float BurstHpThreshold = 2f;

    public int Mode;

    public bool CurtainCallInCountdown;

    public bool ReprisalInOpener;

    public bool AutoAdjustQtOnModeSwitch = true;

    public bool UseToast = true;

    public int OpenerIndex;

    public string DutyState = string.Empty;

    public bool AutoPullActive;

    public bool PartnerPanelShow { get; set; } = true;

    public System.Numerics.Vector2 PartnerPanelIconSize { get; set; } = new(50f, 50f);
}
