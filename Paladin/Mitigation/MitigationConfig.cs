using LittleDart.Paladin.Mitigation.Calc;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.Mitigation;

public readonly struct ReserveRequirement
{
    public readonly int Big;

    public readonly int Short;

    public readonly int Team;

    public ReserveRequirement(int big, int @short, int team)
    {
        Big = big;
        Short = @short;
        Team = team;
    }

    public override string ToString() => $"大减×{Big} 短减×{Short} 团队×{Team}";
}

public enum PressureBand
{
    Low = 0,

    Mid = 1,

    High = 2,
}

public static class MitigationConfig
{

    public const int DailyModeValue = 1;

    public const int DefaultNextPullSeconds = 45;

    public const double MinPullIntervalSeconds = 3d;

    public const double MaxPullIntervalSeconds = 180d;

    public const int IntervalHistoryCount = 3;

    public const float EnemyCountRadius = 10f;

    public const int EnemySampleIntervalMs = 250;

    public const int HpSampleIntervalMs = 200;

    public const int DamageWindowMs = 2500;

    public const int Phase1EndMs = 1000;

    public const int Phase2StartMs = 2500;

    public const int WaveEndDebounceMs = 3000;

    public const int ReentryGuardMs = 1500;

    public const int EvalThrottleMs = 80;

    public const int OathCost = 50;

    public const float StressEnemyWeight = 1f;

    public const float StressDamageWeight = 2f;

    public const float StressMidThreshold = 4f;

    public const float StressHighThreshold = 8f;

    public const float InvulnThresholdPercent = 70f;

    public const int ThresholdPercentMin = 10;

    public const int ThresholdPercentMax = 35;

    public const int ScarceBigThresholdPercent = 25;

    public const int BigThresholdPercent = 35;

    public const int InvulnHpPercent = 25;

    public static readonly ReserveRequirement ReserveLow = new(0, 1, 1);

    public static readonly ReserveRequirement ReserveMid = new(1, 1, 1);

    public static readonly ReserveRequirement ReserveHigh = new(2, 1, 1);

    public const int ReprisalMinEnemiesIn5m = 3;

    public const int AoeWarnMs = 1500;

    public const int ArmsLengthAggroOnMeOver = 3;

    public const int ReserveMinCooldownSeconds = 20;

    public const int MultiTargetEnemies = 3;

    public const double TargetDeathSkipSeconds = 10d;

    public const double TargetDeathSkipCooldownSeconds = 60d;

    public const double ClemencyDeathSeconds = 5d;

    public const float SprintMinRange = 7f;

    public const float SprintMaxRange = 23f;

    public const int SprintMinEnemies = 4;

    public static readonly MitigationSkillDef[] Registry =
    {
        new()
        {
            Name = "铁壁", SkillId = PldSkills.铁壁, BuffId = PldSkills.铁壁buff,
            MinLevel = 8, MaxLevel = 0, DurationSeconds = 20, MitigationPercent = 20, CooldownSeconds = 90,
            Category = MitigationCategory.ScarceBig, Priority = 0, ReservedForBoss = true,
        },
        new()
        {
            Name = "壁垒", SkillId = PldSkills.壁垒, BuffId = PldSkills.壁垒buff,
            MinLevel = 52, MaxLevel = 0, DurationSeconds = 10, MitigationPercent = 16, CooldownSeconds = 90,
            Category = MitigationCategory.ScarceBig, Priority = 1, ReservedForBoss = true,
        },

        new()
        {
            Name = "预警", SkillId = PldSkills.预警, BuffId = PldSkills.预警buff,
            MinLevel = 38, MaxLevel = 91, DurationSeconds = 15, MitigationPercent = 30, CooldownSeconds = 120,
            Category = MitigationCategory.FrequentBig, Priority = 1, ReservedForBoss = false,
        },
        new()
        {
            Name = "极致防御", SkillId = PldSkills.极致防御, BuffId = PldSkills.极致防御buff,
            MinLevel = 92, MaxLevel = 0, DurationSeconds = 15, MitigationPercent = 40, CooldownSeconds = 120,
            Category = MitigationCategory.FrequentBig, Priority = 0, ReservedForBoss = false,
        },

        new()
        {
            Name = "盾阵", SkillId = PldSkills.盾阵, BuffId = PldSkills.盾阵buff, ShieldBuffId = PldSkills.盾阵格挡buff,
            MinLevel = 35, MaxLevel = 81, DurationSeconds = 4, MitigationPercent = 15, CooldownSeconds = 5,
            Category = MitigationCategory.Short, OathCost = OathCost, Priority = 1,
            DurationUpgradeLevel = 74, DurationSecondsUpgraded = 6,
        },
        new()
        {
            Name = "圣盾阵", SkillId = PldSkills.圣盾阵, BuffId = PldSkills.圣盾阵buff, ShieldBuffId = PldSkills.圣盾阵护盾buff,
            MinLevel = 82, MaxLevel = 0, DurationSeconds = 8, MitigationPercent = 15, CooldownSeconds = 5,
            Category = MitigationCategory.Short, OathCost = OathCost, Priority = 0,
        },

        new()
        {
            Name = "血仇", SkillId = PldSkills.血仇, BuffId = PldSkills.血仇buff,
            MinLevel = 22, MaxLevel = 0, DurationSeconds = 10, MitigationPercent = 10, CooldownSeconds = 60,
            Category = MitigationCategory.Team, SelfTarget = false, Priority = 0,
            AoeEquivalentMitigation = 10,
        },
        new()
        {
            Name = "圣光幕帘", SkillId = PldSkills.圣光幕帘, BuffId = PldSkills.圣光幕帘buff,
            MinLevel = 56, MaxLevel = 0, DurationSeconds = 30, MitigationPercent = 0, CooldownSeconds = 90,
            Category = MitigationCategory.Team, Priority = 1, AoeOnly = true,
            AoeEquivalentMitigation = 15,
        },

        new()
        {
            Name = "亲疏自行", SkillId = PldSkills.亲疏, BuffId = PldSkills.亲疏buff,
            MinLevel = 32, MaxLevel = 0, DurationSeconds = 12.5, MitigationPercent = 16, CooldownSeconds = 120,
            Category = MitigationCategory.Personal, Priority = 0,
            SingleBossForbidden = true, DeathSentenceForbidden = true, RequireAggroOnMeOver = 3,
        },

        new()
        {
            Name = "神圣领域", SkillId = PldSkills.神圣领域, BuffId = PldSkills.神圣领域buff,
            MinLevel = 50, MaxLevel = 0, DurationSeconds = 10, MitigationPercent = 100, CooldownSeconds = 420,
            Category = MitigationCategory.Invuln, Priority = 0, ReservedForBoss = true,
        },
    };

    public static List<MitigationSkillDef> PoolForLevel(int level)
        => LevelMitigationProfile.BuildPool(level);

    public static MitigationSkillDef? Find(uint skillId)
    {
        foreach (var def in Registry)
        {
            if (def.SkillId == skillId)
            {
                return def;
            }
        }

        return null;
    }

    public static ReserveRequirement MinReserve(PressureBand band) => band switch
    {
        PressureBand.High => ReserveHigh,
        PressureBand.Mid => ReserveMid,
        _ => ReserveLow,
    };
}
