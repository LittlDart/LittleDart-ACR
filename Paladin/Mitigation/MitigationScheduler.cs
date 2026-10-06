using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.JobApi;
using AEAssist.MemoryApi;
using LittleDart.Paladin.Mitigation.Calc;
using LittleDart.Paladin.Setting;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.Mitigation;

public sealed class MitigationScheduler : IMitigationScheduler
{
    public static MitigationScheduler Instance { get; } = new();

    public MitigationState State { get; } = new();

    private MitigationDecision? _cached;
    private long _cachedAt;

    private MitigationScheduler()
    {
    }

    public bool Enabled
    {
        get
        {
            var settings = PldSettings.Instance;
            if (settings == null)
            {
                return false;
            }

            var qt = LittleDartRotationEntry.QT;
            if (qt == null)
            {
                return false;
            }

            bool daily = PldHelper.IsDailyMode();
            if (!daily)
            {
                return false;
            }

            if (!qt.GetQt(PldQt.UseAutoMitigation) || qt.GetQt(PldQt.StopAttack))
            {
                return false;
            }

            if (settings.MitigationRequireTankStance && !PldHelper.HasTankStance())
            {
                return false;
            }

            return true;
        }
    }

    public void Tick()
    {
        State.Tick();

        DiagTargetCast();

        bool waveEnded = State.ConsumeWaveEnded();
        if (Enabled && waveEnded)
        {
            AuditAfterPull();
        }
    }

    private const int CastDiagIntervalMs = 1000;

    private long _lastCastDiagMs;

    private void DiagTargetCast()
    {
        if (!Diagnostics.Enabled)
        {
            return;
        }

        var settings = PldSettings.Instance;
        if (settings == null)
        {
            return;
        }

        var target = PldHelper.GetCurrentTarget();
        if (target == null || !target.IsCasting)
        {
            return;
        }

        long now = State.NowMs;
        if (now - _lastCastDiagMs < CastDiagIntervalMs)
        {
            return;
        }

        _lastCastDiagMs = now;

        uint castId = target.CastActionId;
        string castName = Core.Resolve<MemApiSpell>().GetName(castId);
        int remainMs = (int)((target.TotalCastTime - target.CurrentCastTime) * 1000f);

        bool deathSentence = TargetHelper.targetCastingIsDeathSentence(target);

        bool inAoeSet = Data.AoeActions.Contains(castId);
        bool aoeGate = EnemyAoeDetector.IsIncomingAoe(settings.MitigationAoeWarnMs);

        PldHelper.LogTagged(
            $"[LittleDart-PLD/Mit/死刑] 读条={castName}({castId}) 死刑={deathSentence} " +
            $"AOE集={inAoeSet} AOE闸={aoeGate} 剩余={remainMs}ms 减伤Enabled={Enabled}");
    }

    public bool ShouldCast(uint skillId)
    {
        MitigationDecision? decision = GetCurrentDecision();
        return decision.HasValue && decision.Value.SkillId == skillId;
    }

    public void NotifyCast(uint skillId)
    {
        State.RecordCast(skillId);
        Invalidate();
    }

    public void Invalidate()
    {
        _cached = null;
        _cachedAt = 0;
    }

    public void Reset()
    {
        State.Reset();
        Invalidate();
    }

    public MitigationDecision? GetCurrentDecision()
    {
        long now = State.NowMs;
        if (_cached.HasValue && now - _cachedAt < MitigationConfig.EvalThrottleMs)
        {
            return _cached;
        }

        MitigationDecision? previous = _cached;
        _cached = Evaluate(now);
        _cachedAt = now;

        if (Diagnostics.Enabled
            && _cached.HasValue
            && (!previous.HasValue || previous.Value.SkillId != _cached.Value.SkillId))
        {
            MitigationSkillDef? def = MitigationConfig.Find(_cached.Value.SkillId);
            PldHelper.LogTagged(
                $"[LittleDart-PLD/Mit] 交={def?.Name ?? _cached.Value.SkillId.ToString()} | {_cached.Value.Reason} | 死刑={DeathSentenceDetector.IsCastingDeathSentence()}");
        }

        return _cached;
    }

    private MitigationDecision? Evaluate(long now)
    {
        if (!Enabled)
        {
            return null;
        }

        var settings = PldSettings.Instance;
        var st = State;
        if (settings == null || !st.WaveActive)
        {
            return null;
        }

        int level = Core.Me.Level;
        var pool = UsablePool(level);
        if (pool.Count == 0)
        {
            return null;
        }

        float damagePercent = st.DamageTakenPercent(settings.MitigationDamageWindowMs);

        float selfHpPercent = Core.Me.CurrentHpPercent();

        MitigationDecision? invulnPick = TryPickInvulnByCurrentHp(
            pool, level, st, settings, selfHpPercent, damagePercent);
        if (invulnPick.HasValue)
        {
            return invulnPick;
        }

        if (DeathSentenceDetector.IsCastingDeathSentence())
        {
            MitigationDecision? deathSentencePick = TryPickDeathSentence(pool, level, st, settings, damagePercent);
            if (deathSentencePick.HasValue)
            {
                return deathSentencePick;
            }
        }

        if (!DeathSentenceDetector.IsCastingDeathSentence()
            && EnemyAoeDetector.IsIncomingAoe(settings.MitigationAoeWarnMs))
        {
            MitigationDecision? aoePick = TryPickAoeTeam(pool, level, st, settings);
            if (aoePick.HasValue)
            {
                return aoePick;
            }
        }

        int enemies = st.EnemyCount;
        bool tight = IsResourceTight(pool, damagePercent, settings);
        bool hasFrequentBig = pool.Any(d => d.Category == MitigationCategory.FrequentBig);
        var categories = ResolveCategories(st.SincePullMomentMs, enemies, st.AggroOnMe, damagePercent, tight, hasFrequentBig, settings);

        foreach (var category in categories)
        {
            if (IsCategoryCovered(category, pool, level))
            {
                continue;
            }

            foreach (var def in CandidatesForCategory(category, pool))
            {
                if (def.AoeOnly)
                {
                    continue;
                }

                if (!IsEligible(def, level, st, settings, enforceReprisalGate: true))
                {
                    continue;
                }

                if (IsSkillActive(def, level))
                {
                    continue;
                }

                if (IsBlockedByTargetDeath(def, settings))
                {
                    continue;
                }

                if (!CanUseWithoutStarving(def, damagePercent, tight, pool, level, settings))
                {
                    continue;
                }

                return new MitigationDecision(def.SkillId, def.Category,
                    DescribeSituation(st, enemies, damagePercent, tight, category));
            }
        }

        return null;
    }

    private MitigationDecision? TryPickAoeTeam(List<MitigationSkillDef> pool, int level, MitigationState st, PldSettings settings)
    {
        MitigationSkillDef? best = null;

        foreach (var def in pool)
        {
            if (def.Category != MitigationCategory.Team || def.AoeEquivalentMitigation <= 0)
            {
                continue;
            }

            if (!IsEligible(def, level, st, settings, enforceReprisalGate: false))
            {
                continue;
            }

            if (IsSkillActive(def, level))
            {
                continue;
            }

            if (best == null || def.AoeEquivalentMitigation > best.AoeEquivalentMitigation)
            {
                best = def;
            }
        }

        if (best == null)
        {
            return null;
        }

        return new MitigationDecision(best.SkillId, best.Category,
            $"检测到AOE 等效减伤={best.AoeEquivalentMitigation:F0} 怪={st.EnemyCount} 掉血={st.DamageTakenPercent(settings.MitigationDamageWindowMs):F1}%");
    }

    private MitigationDecision? TryPickDeathSentence(
        List<MitigationSkillDef> pool, int level, MitigationState st, PldSettings settings, float damagePercent)
    {
        MitigationCategory[] categories =
        {
            MitigationCategory.ScarceBig,
            MitigationCategory.FrequentBig,
            MitigationCategory.Short,
        };

        foreach (var category in categories)
        {
            if (IsCategoryCovered(category, pool, level))
            {
                continue;
            }

            foreach (var def in CandidatesForCategory(category, pool))
            {
                if (def.AoeOnly)
                {
                    continue;
                }

                if (!IsEligible(def, level, st, settings, enforceReprisalGate: true))
                {
                    continue;
                }

                if (IsSkillActive(def, level))
                {
                    continue;
                }

                if (IsBlockedByTargetDeath(def, settings))
                {
                    continue;
                }

                return new MitigationDecision(def.SkillId, def.Category,
                    $"死刑读条 大减+短减 类别={def.CategoryName} 怪={st.EnemyCount} 掉血={damagePercent:F1}%");
            }
        }

        return null;
    }

    private MitigationDecision? TryPickInvulnByCurrentHp(
        List<MitigationSkillDef> pool, int level, MitigationState st, PldSettings settings,
        float selfHpPercent, float damagePercent)
    {
        int invulnHpAt = ClampPercentThreshold(settings.MitigationInvulnHpPercent);

        float hpPercent = selfHpPercent * 100f;

        if (float.IsNaN(hpPercent) || hpPercent > invulnHpAt)
        {
            return null;
        }

        if (damagePercent <= 0f)
        {
            return null;
        }

        foreach (var def in CandidatesForCategory(MitigationCategory.Invuln, pool))
        {
            if (!IsEligible(def, level, st, settings, enforceReprisalGate: true))
            {
                continue;
            }

            if (IsSkillActive(def, level))
            {
                continue;
            }

            if (IsBlockedByTargetDeath(def, settings))
            {
                continue;
            }

            return new MitigationDecision(def.SkillId, def.Category,
                $"阶段=当前血量保命 自己血量={hpPercent:F1}%(阈值={invulnHpAt}%) 已掉血={damagePercent:F1}%(>0 才交) 怪={st.EnemyCount} 5m={st.EnemiesIn5m} 一仇={st.AggroOnMe} 类别={def.Category}");
        }

        return null;
    }

    private static MitigationCategory[] ResolveCategories(
        long elapsedMs, int enemies, int aggroOnMe, float damagePercent, bool tight, bool hasFrequentBig, PldSettings settings)
    {
        MitigationCategory[] baseList;

        if (elapsedMs < settings.MitigationPhase1EndMs || elapsedMs < settings.MitigationPhase2StartMs)
        {
            baseList = Phase1(enemies, tight, hasFrequentBig, settings.MitigationMultiTargetEnemies);
        }
        else
        {
            baseList = Phase2(damagePercent, enemies, tight, settings);
        }

        if (aggroOnMe > settings.MitigationArmsLengthAggroOver)
        {
            var list = new List<MitigationCategory>(baseList) { MitigationCategory.Personal };
            return list.ToArray();
        }

        return baseList;
    }

    private static int ClampPercentThreshold(int value)
        => Math.Clamp(value, MitigationConfig.ThresholdPercentMin, MitigationConfig.ThresholdPercentMax);

    private static MitigationCategory[] Phase1(int enemies, bool tight, bool hasFrequentBig, int multiTargetEnemies)
    {
        if (enemies < multiTargetEnemies)
        {
            return Array.Empty<MitigationCategory>();
        }

        int tier = enemies <= 7 ? 1 : 2;
        if (tight)
        {
            tier--;
        }

        MitigationCategory big = hasFrequentBig ? MitigationCategory.FrequentBig : MitigationCategory.ScarceBig;

        return tier switch
        {
            0 => new[] { big, MitigationCategory.Team },
            1 => new[] { big, MitigationCategory.Short, MitigationCategory.Team },
            _ => new[]
            {
                MitigationCategory.ScarceBig, MitigationCategory.FrequentBig,
                MitigationCategory.Short, MitigationCategory.Team,
            },
        };
    }

    private static MitigationCategory[] Phase2(float damagePercent, int enemies, bool tight, PldSettings settings)
    {
        if (damagePercent < 15f)
        {
            return Array.Empty<MitigationCategory>();
        }

        int scarceBigAt = ClampPercentThreshold(settings.MitigationScarceBigThresholdPercent);
        int bigAt = Math.Max(ClampPercentThreshold(settings.MitigationBigThresholdPercent), scarceBigAt);

        bool multiTarget = enemies >= settings.MitigationMultiTargetEnemies;

        MitigationCategory[] raw;
        if (damagePercent < scarceBigAt)
        {
            raw = multiTarget
                ? new[] { MitigationCategory.Short, MitigationCategory.Team }
                : new[] { MitigationCategory.Team };
        }
        else if (damagePercent < bigAt)
        {
            raw = new[] { MitigationCategory.ScarceBig, MitigationCategory.Short, MitigationCategory.Team };
        }
        else if (damagePercent < 45f)
        {
            raw = bigAt > scarceBigAt
                ? new[] { MitigationCategory.FrequentBig, MitigationCategory.Short, MitigationCategory.Team }
                : new[]
                {
                    MitigationCategory.ScarceBig, MitigationCategory.FrequentBig,
                    MitigationCategory.Short, MitigationCategory.Team,
                };
        }
        else if (damagePercent < 55f)
        {
            raw = new[]
            {
                MitigationCategory.ScarceBig, MitigationCategory.FrequentBig,
                MitigationCategory.Short, MitigationCategory.Team,
            };
        }
        else if (damagePercent < 70f)
        {
            raw = new[]
            {
                MitigationCategory.ScarceBig, MitigationCategory.FrequentBig,
                MitigationCategory.Short, MitigationCategory.Team,
            };
        }
        else
        {
            raw = new[] { MitigationCategory.Invuln };
        }

        if (tight)
        {
            for (int i = 0; i < raw.Length; i++)
            {
                if (raw[i] == MitigationCategory.ScarceBig)
                {
                    raw[i] = MitigationCategory.FrequentBig;
                }
            }
        }

        return raw;
    }

    private static IEnumerable<MitigationSkillDef> CandidatesForCategory(MitigationCategory category, List<MitigationSkillDef> pool)
        => pool.Where(d => d.Category == category)
               .OrderBy(d => d.Priority)
               .ThenByDescending(d => d.MitigationPercent);

    private static bool IsCategoryCovered(MitigationCategory category, List<MitigationSkillDef> pool, int level)
    {
        foreach (var def in pool)
        {
            if (def.Category == category && IsSkillActive(def, level))
            {
                return true;
            }
        }

        return false;
    }

    private static List<MitigationSkillDef> UsablePool(int level)
    {
        var pool = LevelMitigationProfile.BuildPool(level);
        pool.RemoveAll(d => !SpellExtension.IsUnlock(d.SkillId));
        return pool;
    }

    private static bool IsEligible(
        MitigationSkillDef def, int level, MitigationState st, PldSettings settings, bool enforceReprisalGate)
    {
        if (!def.LevelAllowed(level) || !SpellExtension.IsUnlock(def.SkillId))
        {
            return false;
        }

        if (def.OathCost > 0 && Core.Resolve<JobApi_Paladin>().Oath < def.OathCost)
        {
            return false;
        }

        if (enforceReprisalGate
            && def.SkillId == PldSkills.血仇
            && PullPhaseDetector.IsPulling(st.EnemiesIn5m, settings.MitigationReprisalMinEnemiesIn5m))
        {
            return false;
        }

        if (def.SingleBossForbidden && st.EnemyCount <= 1)
        {
            return false;
        }

        if (def.DeathSentenceForbidden && DeathSentenceDetector.IsCastingDeathSentence())
        {
            return false;
        }

        if (def.RequireAggroOnMeOver > 0 && st.AggroOnMe <= def.RequireAggroOnMeOver)
        {
            return false;
        }

        if (!def.SelfTarget && !PldHelper.HasValidTarget())
        {
            return false;
        }

        return SpellExtension.IsReadyWithCanCast(BuildSpell(def));
    }

    private static bool IsSkillActive(MitigationSkillDef def, int level)
    {
        if (def.BuffId != 0)
        {
            bool active = def.SelfTarget
                ? PldHelper.HasAura(def.BuffId)
                : PldHelper.GetTargetAuraTimeLeft(def.BuffId) > 0;
            if (active)
            {
                return true;
            }
        }

        if (def.ShieldBuffId != 0 && PldHelper.HasAura(def.ShieldBuffId))
        {
            return true;
        }

        return PldHelper.IsRecentlyCast(def.SkillId, MitigationConfig.ReentryGuardMs);
    }

    private static Spell BuildSpell(MitigationSkillDef def)
        => def.SkillId.GetSpell(def.SelfTarget ? SpellTargetType.Self : SpellTargetType.Target);

    private List<MitigationSkillDef> ProjectedAvailable(
        List<MitigationSkillDef> pool, double secondsAhead, MitigationSkillDef? simulatedUse, int level)
    {
        var list = new List<MitigationSkillDef>(pool.Count);

        foreach (var def in pool)
        {
            double remaining = simulatedUse != null && def.SkillId == simulatedUse.SkillId
                ? def.CooldownSeconds
                : Core.Resolve<MemApiSpell>().GetCooldown(def.SkillId).TotalSeconds;

            double shortCd = PldSettings.Instance?.MitigationReserveMinCooldownSeconds
                             ?? MitigationConfig.ReserveMinCooldownSeconds;

            if (def.CooldownSeconds <= shortCd
                || remaining <= secondsAhead + 0.3d)
            {
                list.Add(def);
            }
        }

        return list;
    }

    private bool IsResourceTight(List<MitigationSkillDef> pool, float damagePercent, PldSettings settings)
    {
        double nextPull = State.EstimateSecondsToNextPull();
        var projected = ProjectedAvailable(pool, nextPull, null, Core.Me.Level);
        var need = MitigationConfig.MinReserve(InvertedBand(damagePercent, settings));
        return !Satisfies(projected, need);
    }

    private bool CanUseWithoutStarving(
        MitigationSkillDef def, float damagePercent, bool tight, List<MitigationSkillDef> pool, int level, PldSettings settings)
    {
        bool emergency = damagePercent >= settings.MitigationInvulnThresholdPercent;

        if (!emergency && tight && def.Category == MitigationCategory.ScarceBig)
        {
            return false;
        }

        double nextPull = State.EstimateSecondsToNextPull();
        var projected = ProjectedAvailable(pool, nextPull, def, level);
        var need = MitigationConfig.MinReserve(InvertedBand(damagePercent, settings));

        if (Satisfies(projected, need))
        {
            return true;
        }

        if (def.Category is MitigationCategory.Short or MitigationCategory.Team or MitigationCategory.Personal)
        {
            return true;
        }

        return emergency;
    }

    private static bool Satisfies(List<MitigationSkillDef> projected, ReserveRequirement need)
    {
        int big = 0;
        int @short = 0;
        int team = 0;

        foreach (var def in projected)
        {
            if (def.AoeOnly)
            {
                continue;
            }

            switch (def.Category)
            {
                case MitigationCategory.ScarceBig:
                case MitigationCategory.FrequentBig:
                    big++;
                    break;
                case MitigationCategory.Short:
                    @short++;
                    break;
                case MitigationCategory.Team:
                    team++;
                    break;
            }
        }

        return big >= need.Big && @short >= need.Short && team >= need.Team;
    }

    private PressureBand InvertedBand(float damagePercent, PldSettings settings)
    {
        float pressure = State.EnemyCount * settings.MitigationStressEnemyWeight
                         + damagePercent * settings.MitigationStressDamageWeight;

        PressureBand current = pressure >= settings.MitigationStressHighThreshold ? PressureBand.High
            : pressure >= settings.MitigationStressMidThreshold ? PressureBand.Mid
            : PressureBand.Low;

        return current switch
        {
            PressureBand.Low => PressureBand.High,
            PressureBand.High => PressureBand.Low,
            _ => PressureBand.Mid,
        };
    }

    private bool IsBlockedByTargetDeath(MitigationSkillDef def, PldSettings settings)
    {
        if (def.CooldownSeconds <= settings.MitigationTargetDeathSkipCooldownSeconds)
        {
            return false;
        }

        return TargetDeathPredictor.WillDieWithin(State.Hp, settings.MitigationTargetDeathSkipSeconds, 3000);
    }

    public void AuditAfterPull()
    {
        var settings = PldSettings.Instance;
        if (settings == null)
        {
            return;
        }

        int level = Core.Me.Level;
        var pool = UsablePool(level);
        double nextPull = State.EstimateSecondsToNextPull();
        var projected = ProjectedAvailable(pool, nextPull, null, level);
        var need = MitigationConfig.MinReserve(InvertedBand(State.LastWaveDamagePercent, settings));

        State.ResourceTight = !Satisfies(projected, need);
    }

    private static string DescribeSituation(MitigationState st, int enemies, float damagePercent, bool tight, MitigationCategory category)
    {
        string phase = st.SincePullMomentMs < MitigationConfig.Phase1EndMs ? "第一阶段(怪数)"
            : st.SincePullMomentMs < MitigationConfig.Phase2StartMs ? "过渡(怪数)"
            : "第二阶段(掉血)";
        string flags = (tight ? " resourceTight" : string.Empty);
        return $"{phase} 怪={enemies} 5m={st.EnemiesIn5m} 一仇={st.AggroOnMe} 掉血={damagePercent:F1}% 类别={category}{flags}";
    }

    public string DescribeActiveMits()
    {
        int level = Core.Me.Level;
        var parts = new List<string>();
        foreach (var def in MitigationConfig.PoolForLevel(level))
        {
            if (IsSkillActive(def, level))
            {
                parts.Add(def.Name);
            }
        }

        return parts.Count == 0 ? "无" : string.Join("、", parts);
    }

    public string DescribeStatus()
    {
        var s = PldSettings.Instance;
        var qt = LittleDartRotationEntry.QT;
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("===== 减伤调度器状态（/LittleDart-PLD mit） =====");
        if (s == null || qt == null)
        {
            sb.AppendLine("PldSettings / QT 未初始化");
            return sb.ToString();
        }

        bool modeOk = s.Mode == MitigationConfig.DailyModeValue;
        bool dailyQt = qt.GetQt(PldQt.DailyMode);
        bool autoQt = qt.GetQt(PldQt.UseAutoMitigation);
        bool stopQt = qt.GetQt(PldQt.StopAttack);
        bool tankOk = !s.MitigationRequireTankStance || PldHelper.HasTankStance();

        sb.AppendLine($"存档 Mode={s.Mode}（0高难/1日常） QT[日常模式]={dailyQt} QT[自动减伤]={autoQt} QT[停手]={stopQt}");
        sb.AppendLine($"坦克姿态={PldHelper.HasTankStance()}（要求={s.MitigationRequireTankStance} → {(tankOk ? "通过" : "不通过")}）");
        sb.AppendLine($"→ Enabled = {Enabled}");
        sb.AppendLine($"    日常模式={(modeOk || dailyQt ? "通过" : "不通过")}  自动减伤={(autoQt ? "通过" : "不通过")}  未停手={(stopQt ? "不通过" : "通过")}");
        sb.AppendLine($"波次：WaveActive={State.WaveActive} 波内={State.WaveElapsedMs}ms");
        sb.AppendLine($"信号：怪10m={State.EnemyCount} 怪5m={State.EnemiesIn5m} 一仇={State.AggroOnMe} 2.5s掉血={State.DamageTakenPercent(s.MitigationDamageWindowMs):F1}%");
        sb.AppendLine($"减伤阈值：铁壁壁垒={s.MitigationScarceBigThresholdPercent}%（生效 {ClampPercentThreshold(s.MitigationScarceBigThresholdPercent)}%）"
                      + $" 大减={s.MitigationBigThresholdPercent}%（生效 {Math.Max(ClampPercentThreshold(s.MitigationBigThresholdPercent), ClampPercentThreshold(s.MitigationScarceBigThresholdPercent))}%）"
                      + $" 无敌血量={s.MitigationInvulnHpPercent}%（生效 {ClampPercentThreshold(s.MitigationInvulnHpPercent)}%）"
                      + $" 自己血量={Core.Me.CurrentHpPercent() * 100f:F1}%");
        sb.AppendLine($"目标：TTD={(TargetDeathPredictor.TryPredictSecondsToDeath(State.Hp, 3000, out double ttd) ? ttd.ToString("F1") + "s" : "未知")}");
        sb.AppendLine($"技能明细（等级 {Core.Me.Level}，未过滤解锁）：");
        foreach (var def in LevelMitigationProfile.BuildPool(Core.Me.Level))
        {
            bool unlock = SpellExtension.IsUnlock(def.SkillId);
            bool ready = unlock && SpellExtension.IsReadyWithCanCast(BuildSpell(def));
            double cd = unlock ? Core.Resolve<MemApiSpell>().GetCooldown(def.SkillId).TotalSeconds : -1;
            sb.AppendLine($"  {def.Name}({def.SkillId}) 解锁={unlock} 就绪={ready} CD={(cd < 0 ? "-" : cd.ToString("F1") + "s")} 分类={def.CategoryName}");
        }
        sb.AppendLine($"生效中：{DescribeActiveMits()}");
        var d = GetCurrentDecision();
        sb.AppendLine(d.HasValue
            ? $"当前决策={MitigationConfig.Find(d.Value.SkillId)?.Name} | {d.Value.Reason}"
            : "当前决策=无");
        return sb.ToString();
    }
}
