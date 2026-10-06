namespace LittleDart.Paladin.Mitigation;

public enum MitigationCategory
{
    ScarceBig = 0,

    FrequentBig = 1,

    Short = 2,

    Team = 3,

    Invuln = 4,

    Personal = 5,
}

public sealed class MitigationSkillDef
{
    public string Name = string.Empty;

    public uint SkillId;

    public uint BuffId;

    public uint ShieldBuffId;

    public int MinLevel;

    public int MaxLevel;

    public double DurationSeconds;

    public double DurationSecondsUpgraded;

    public int DurationUpgradeLevel;

    public double MitigationPercent;

    public double CooldownSeconds;

    public MitigationCategory Category;

    public int OathCost;

    public bool SelfTarget = true;

    public int Priority;

    public bool ReservedForBoss;

    public bool AoeOnly;

    public bool SingleBossForbidden;

    public bool DeathSentenceForbidden;

    public int RequireAggroOnMeOver;

    public double AoeEquivalentMitigation;

    public bool LevelAllowed(int level)
    {
        if (level < MinLevel)
        {
            return false;
        }

        return MaxLevel <= 0 || level <= MaxLevel;
    }

    public double EffectiveDurationSeconds(int level)
    {
        if (DurationUpgradeLevel > 0 && level >= DurationUpgradeLevel)
        {
            return DurationSecondsUpgraded;
        }

        return DurationSeconds;
    }

    public string CategoryName => Category switch
    {
        MitigationCategory.ScarceBig => "稀缺大减",
        MitigationCategory.FrequentBig => "高频大减",
        MitigationCategory.Short => "短减",
        MitigationCategory.Team => "团队减",
        MitigationCategory.Invuln => "无敌",
        MitigationCategory.Personal => "个人减",
        _ => "未知",
    };
}
