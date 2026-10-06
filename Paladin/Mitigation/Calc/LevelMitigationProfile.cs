using LittleDart.Paladin.Mitigation;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.Mitigation.Calc;

public sealed class LevelSkillRow
{
    public int MinLevel;

    public int MaxLevel;

    public uint SkillId;

    public double MitigationPercent = -1;

    public double DurationSeconds = -1;

    public double CooldownSeconds = -1;

    public bool Covers(int level) => level >= MinLevel && (MaxLevel <= 0 || level <= MaxLevel);
}

public static class LevelMitigationProfile
{
    public static readonly LevelSkillRow[] Table =
    {
        new() { MinLevel = 8, MaxLevel = 0, SkillId = PldSkills.铁壁, MitigationPercent = 20, DurationSeconds = 20, CooldownSeconds = 90 },

        new() { MinLevel = 22, MaxLevel = 0, SkillId = PldSkills.血仇, MitigationPercent = 10, DurationSeconds = 10, CooldownSeconds = 60 },

        new() { MinLevel = 32, MaxLevel = 0, SkillId = PldSkills.亲疏, MitigationPercent = 16, DurationSeconds = 12.5, CooldownSeconds = 120 },

        new() { MinLevel = 35, MaxLevel = 73, SkillId = PldSkills.盾阵, MitigationPercent = 15, DurationSeconds = 4, CooldownSeconds = 5 },
        new() { MinLevel = 74, MaxLevel = 81, SkillId = PldSkills.盾阵, MitigationPercent = 15, DurationSeconds = 6, CooldownSeconds = 5 },

        new() { MinLevel = 38, MaxLevel = 91, SkillId = PldSkills.预警, MitigationPercent = 30, DurationSeconds = 15, CooldownSeconds = 120 },

        new() { MinLevel = 50, MaxLevel = 0, SkillId = PldSkills.神圣领域, MitigationPercent = 100, DurationSeconds = 10, CooldownSeconds = 420 },

        new() { MinLevel = 52, MaxLevel = 0, SkillId = PldSkills.壁垒, MitigationPercent = 16, DurationSeconds = 10, CooldownSeconds = 90 },

        new() { MinLevel = 56, MaxLevel = 0, SkillId = PldSkills.圣光幕帘, MitigationPercent = 0, DurationSeconds = 30, CooldownSeconds = 90 },

        new() { MinLevel = 82, MaxLevel = 0, SkillId = PldSkills.圣盾阵, MitigationPercent = 15, DurationSeconds = 8, CooldownSeconds = 5 },

        new() { MinLevel = 92, MaxLevel = 0, SkillId = PldSkills.极致防御, MitigationPercent = 40, DurationSeconds = 15, CooldownSeconds = 120 },
    };

    public static List<MitigationSkillDef> BuildPool(int level)
    {
        var list = new List<MitigationSkillDef>(MitigationConfig.Registry.Length);

        foreach (var baseDef in MitigationConfig.Registry)
        {
            if (!baseDef.LevelAllowed(level))
            {
                continue;
            }

            var def = Clone(baseDef);
            LevelSkillRow? row = FindRow(baseDef.SkillId, level);
            if (row != null)
            {
                if (row.MitigationPercent >= 0)
                {
                    def.MitigationPercent = row.MitigationPercent;
                }

                if (row.DurationSeconds >= 0)
                {
                    def.DurationSeconds = row.DurationSeconds;
                    def.DurationUpgradeLevel = 0;
                }

                if (row.CooldownSeconds >= 0)
                {
                    def.CooldownSeconds = row.CooldownSeconds;
                }
            }

            list.Add(def);
        }

        return list;
    }

    public static double EffectiveCooldown(uint skillId, int level)
    {
        LevelSkillRow? row = FindRow(skillId, level);
        return row?.CooldownSeconds ?? -1;
    }

    public static LevelSkillRow? FindRow(uint skillId, int level)
    {
        LevelSkillRow? found = null;
        foreach (var row in Table)
        {
            if (row.SkillId == skillId && row.Covers(level))
            {
                found = row;
            }
        }

        return found;
    }

    private static MitigationSkillDef Clone(MitigationSkillDef d) => new()
    {
        Name = d.Name,
        SkillId = d.SkillId,
        BuffId = d.BuffId,
        ShieldBuffId = d.ShieldBuffId,
        MinLevel = d.MinLevel,
        MaxLevel = d.MaxLevel,
        DurationSeconds = d.DurationSeconds,
        DurationSecondsUpgraded = d.DurationSecondsUpgraded,
        DurationUpgradeLevel = d.DurationUpgradeLevel,
        MitigationPercent = d.MitigationPercent,
        CooldownSeconds = d.CooldownSeconds,
        Category = d.Category,
        OathCost = d.OathCost,
        SelfTarget = d.SelfTarget,
        Priority = d.Priority,
        ReservedForBoss = d.ReservedForBoss,
        AoeOnly = d.AoeOnly,
        SingleBossForbidden = d.SingleBossForbidden,
        DeathSentenceForbidden = d.DeathSentenceForbidden,
        RequireAggroOnMeOver = d.RequireAggroOnMeOver,
        AoeEquivalentMitigation = d.AoeEquivalentMitigation,
    };
}
