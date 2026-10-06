using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.CombatRoutine.Module.Opener;
using AEAssist.CombatRoutine.Trigger;
using AEAssist.CombatRoutine.View.JobView;
using AEAssist.CombatRoutine.View.JobView.HotkeyResolver;
using AEAssist.GUI;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using LittleDart.Paladin;
using LittleDart.Paladin.GCD;
using LittleDart.Paladin.Hotkey;
using LittleDart.Paladin.Mitigation;
using LittleDart.Paladin.OffGcd;
using LittleDart.Paladin.Opener;
using LittleDart.Paladin.Setting;
using LittleDart.Paladin.Skill;
using LittleDart.Paladin.Triggers;

namespace LittleDart;

public class LittleDartRotationEntry : IRotationEntry, IDisposable
{
    private readonly List<SlotResolverData> SlotResolvers;

    private readonly PldHelper Helper;

    public string AuthorName { get; set; } = "LittleDart";

    public static JobViewWindow QT { get; private set; } = null!;

    public JobViewSave JobViewSave { get; } = new JobViewSave();

    public Rotation Build(string settingFolder)
    {
        PldSettings.Build(settingFolder);
        BuildQT();

        Rotation rotation = new Rotation(SlotResolvers)
        {
            TargetJob = Jobs.Paladin,
            AcrType = AcrType.Both,
            MinLevel = 1,
            MaxLevel = 100,
            Description = "LittleDart骑士。支持全等级日随和高难。时间轴缓慢施工中。",
        };

        rotation.AddOpener(GetOpener);
        rotation.SetRotationEventHandler(new PldRotationEventHandler());

        rotation.AddTriggerAction(new TriggerActionSetQt());
        rotation.AddTriggerAction(new TriggerActionSetTankStance());
        rotation.AddTriggerCondition(new TriggerConditionSelfPosition());
        rotation.AddTriggerCondition(new TriggerConditionTargetPosition());

        return rotation;
    }

    private IOpener? GetOpener(uint level)
    {
        PldSettings? s = PldSettings.Instance;
        if (s == null)
        {
            return null;
        }

        if (!s.OpenerEnabled || !QT.GetQt(PldQt.UseSingleTargetOpener))
        {
            return null;
        }

        if (PldHelper.IsDailyMode())
        {
            return null;
        }

        _ = level;
        return s.OpenerIndex switch
        {
            0 => new PldOpener100(),
            1 => new PldOpener100Fru(),
            2 => new PldOpener90(),
            3 => new PldOpener80(),
            4 => new PldOpener70(),
            _ => null,
        };
    }

    public void BuildQT()
    {
        QT = new JobViewWindow(PldSettings.Instance.JobViewSave, PldSettings.Instance.Save, "LittleDart");
        QT.SetUpdateAction(OnUIUpdate);

        QT.AddTab("职业设置", DrawQtSettingsTab);
        QT.AddTab("Dev", DrawQtDevTab);

        bool daily = PldSettings.Instance.Mode == MitigationConfig.DailyModeValue;

        QT.AddQt(PldQt.UseSingleTargetOpener, true,  "开怪是否使用单体起手序列");
        QT.AddQt(PldQt.DailyMode,             daily, "日常模式：打开才启用自动减伤调度器（也可 /LittleDart-PLD mode 1）");
        QT.AddQt(PldQt.UseAoe,                false, "是否允许打群体连招（自身 6 米内 3 个以上敌人）");
        QT.AddQt(PldQt.UseFof,                true,  "是否自动使用战逃反应（60 秒爆发窗口）");
        QT.AddQt(PldQt.UseRequiescat,         true,  "是否自动使用安魂祈祷 / 绝对统治");
        QT.AddQt(PldQt.NoBurst,               false, "不打爆发：跳过所有爆发期决策器");
        QT.AddQt(PldQt.DumpResource,          false, "倾泻资源：紧急线路，覆盖常规连招狂打高威力技能（例如 boss 即将上天）");
        QT.AddQt(PldQt.DelayFof,              false, "延后战逃：打开后不自动开战逃反应");
        QT.AddQt(PldQt.SmartFof,              false, "智能远战逃：远离目标时先打远程技能再开战逃");
        QT.AddQt(PldQt.TwinDamageDirect,      false, "双伤直出：厄运流转/偿赎剑无视战逃直接用");
        QT.AddQt(PldQt.DelayConfiteor,        false, "延后悔罪：近战时悔罪连延后 2 个 GCD");
        QT.AddQt(PldQt.HolySpiritRanged,      !daily, "远离圣灵：远离且站定时读条圣灵（优先于投盾）");
        QT.AddQt(PldQt.ShieldLobRanged,       daily, "远离投盾：远离时用投盾");
        QT.AddQt(PldQt.SlideCast,             false, "滑步模式：读条圣灵期间屏蔽移动，读到滑步窗口再放行（仅高难；锁住时无法躲 AOE，慎用）");
        QT.AddQt(PldQt.UseIntervene,          false, "自动调停（高难模式专用；保留层数/安全距离/爆发内不保留在「职业设置 → 高难 → 战斗设置」）");
        QT.AddQt(PldQt.IntervenePull,         daily, "调停拉怪：自己已是多只怪的一仇时，把没拉住/没进战斗的怪用调停拽过来（日常默认开；高难默认关且不显示）");
        QT.AddQt(PldQt.StopAttack,            false, "一键停手：打开后所有决策器都跳过");
        QT.AddQt(PldQt.UseAutoMitigation,     daily, "自动减伤：本 ACR 的减伤调度器（只在日常模式生效；高难交给时间轴）");
        QT.AddQt(PldQt.MitigationClemency,    daily, "深仁厚泽：预计自己 5 秒内死亡时自救");
        QT.AddQt(PldQt.UsePotion,             false, "开场是否使用爆发药");

        QT.AddQt(PldQt.AutoSprint, daily,
                 "自动冲刺：多目标且距目标 7~23m 时自动开疾跑（高难默认关，交给时间轴）");

        QT.AddQt(PldQt.DevDebug, false, "开发测试，别按");

        ApplyQtVisibility();

        QT.AddHotkey("铁壁",     new HotKeyResolver_NormalSpell(PldSkills.铁壁,     SpellTargetType.Self, false));
        QT.AddHotkey("极致防御", new HotKeyResolver_NormalSpell(PldSkills.极致防御, SpellTargetType.Self, false));
        QT.AddHotkey("圣盾阵",   new HotKeyResolver_NormalSpell(PldSkills.圣盾阵,   SpellTargetType.Self, false));
        QT.AddHotkey("壁垒",     new HotKeyResolver_NormalSpell(PldSkills.壁垒,     SpellTargetType.Self, false));
        QT.AddHotkey("亲疏",     new HotKeyResolver_NormalSpell(PldSkills.亲疏,     SpellTargetType.Self, false));
        QT.AddHotkey("神圣领域", new HotKeyResolver_NormalSpell(PldSkills.神圣领域, SpellTargetType.Self, false));
        QT.AddHotkey("武装戍卫", new HotKeyResolver_NormalSpell(PldSkills.武装戍卫, SpellTargetType.Self, false));

        QT.AddHotkey("挑衅", new HotKeyResolver_NormalSpell(PldSkills.挑衅, SpellTargetType.Target, false));
        QT.AddHotkey("血仇", new HotKeyResolver_NormalSpell(PldSkills.血仇, SpellTargetType.Target, false));
        QT.AddHotkey("调停", new HotKeyResolver_NormalSpell(PldSkills.调停, SpellTargetType.Target, false));

        QT.AddHotkey("圣灵", new HotKeyResolver_NormalSpell(PldSkills.圣灵, SpellTargetType.Target, false));
        QT.AddHotkey("冲刺", new HotKeyResolver_疾跑());
        QT.AddHotkey("LB",   new HotKeyResolver_LB());

        QT.AddHotkey("退避", new ShirkToPartnerHotkey());

        QT.AddHotkey("清扫队列", new ClearQueueHotkey());

        HotkeyManager.InitializePartnerPanel();
    }

    private static readonly string[] QtAlwaysHidden =
    {
        PldQt.DevDebug,
        PldQt.DailyMode,
    };

    private static readonly string[] QtHiddenInHighEnd =
    {
        PldQt.UseAutoMitigation,
        PldQt.MitigationClemency,
        PldQt.AutoSprint,
        PldQt.IntervenePull,
    };

    private static readonly string[] QtHiddenInDaily =
    {
        PldQt.UseFof,
        PldQt.UseRequiescat,
        PldQt.DumpResource,
        PldQt.DelayFof,
        PldQt.SmartFof,
        PldQt.TwinDamageDirect,
        PldQt.DelayConfiteor,
        PldQt.SlideCast,
        PldQt.UsePotion,
    };

    public static void ApplyQtVisibility()
    {
        PldSettings? settings = PldSettings.Instance;
        if (settings == null)
        {
            return;
        }

        JobViewSave save = settings.JobViewSave;
        save.QtUnVisibleList ??= new List<string>();

        List<string> wanted = new(QtAlwaysHidden);
        wanted.AddRange(PldHelper.IsDailyMode() ? QtHiddenInDaily : QtHiddenInHighEnd);

        save.QtUnVisibleList.Clear();
        save.QtUnVisibleList.AddRange(wanted);

        settings.Save();
    }

    public IRotationUI GetRotationUI()
    {
        return QT;
    }

    public void OnDrawSetting()
    {
        PldSettingUi.Instance.Draw();
    }

    public void OnUIUpdate()
    {
        HotkeyManager.QtUpdate();

        PldSlideCast.Tick();
    }

    private static readonly Vector4 DailyTrack = new(145f / 255f, 211f / 255f, 60f / 255f, 0.95f);

    private static readonly Vector4 HardTrack = new(214f / 255f, 69f / 255f, 69f / 255f, 0.95f);

    private static readonly Vector4 SwitchKnob = new(0.97f, 0.97f, 0.98f, 1f);

    private static readonly Vector4 KnobShadow = new(0f, 0f, 0f, 0.28f);

    private static readonly Vector4 DailyTextFrom = new(74f / 255f, 140f / 255f, 16f / 255f, 1f);
    private static readonly Vector4 DailyTextTo = new(168f / 255f, 226f / 255f, 84f / 255f, 1f);
    private static readonly Vector4 HardTextFrom = new(150f / 255f, 28f / 255f, 28f / 255f, 1f);
    private static readonly Vector4 HardTextTo = new(246f / 255f, 126f / 255f, 126f / 255f, 1f);

    private static readonly Vector4 HintColor = new(0.62f, 0.64f, 0.67f, 1f);

    private static float _modeKnobT;

    private static bool _modeKnobInitialized;

    private static void HintText(string text)
    {
        ImGui.PushTextWrapPos(0f);
        ImGui.TextColored(HintColor, text);
        ImGui.PopTextWrapPos();
    }

    private static void DrawModeSwitch()
    {
        PldSettings? settings = PldSettings.Instance;
        if (settings == null)
        {
            return;
        }

        bool daily = settings.Mode == MitigationConfig.DailyModeValue;

        if (!_modeKnobInitialized)
        {
            _modeKnobT = daily ? 1f : 0f;
            _modeKnobInitialized = true;
        }

        float fontSize = ImGui.GetFontSize();
        string label = daily ? "日常模式" : "高难模式";

        float textW = ImGui.CalcTextSize(label).X;
        float padX = fontSize * 0.55f;
        float trackH = fontSize * 2.0f;
        float inset = trackH * 0.10f;
        float knobH = trackH - inset * 2f;
        float knobW = textW + padX * 2f;

        float trackW = knobW * 2f + inset * 3f;

        float avail = ImGui.GetContentRegionAvail().X;
        if (trackW > avail && trackW > 0f)
        {
            float k = avail / trackW;
            trackW *= k;
            knobW = MathF.Max(knobH, knobW * k);
        }

        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (avail - trackW) * 0.5f);
        ImGui.InvisibleButton("###LittleDartModeSwitch", new Vector2(trackW, trackH));

        if (ImGui.IsItemClicked())
        {
            PldCommand.SetMode(daily ? "0" : "1");
            daily = !daily;
        }

        float target = daily ? 1f : 0f;
        float step = MathF.Min(1f, ImGui.GetIO().DeltaTime * 14f);
        _modeKnobT += (target - _modeKnobT) * step;
        if (MathF.Abs(target - _modeKnobT) < 0.001f)
        {
            _modeKnobT = target;
        }

        var dl = ImGui.GetWindowDrawList();
        Vector2 pMin = ImGui.GetItemRectMin();
        Vector2 pMax = new(pMin.X + trackW, pMin.Y + trackH);
        float trackRound = trackH * 0.5f;

        Vector4 trackColor = daily ? DailyTrack : HardTrack;
        if (ImGui.IsItemHovered())
        {
            trackColor = new Vector4(
                MathF.Min(1f, trackColor.X + 0.06f),
                MathF.Min(1f, trackColor.Y + 0.06f),
                MathF.Min(1f, trackColor.Z + 0.06f),
                trackColor.W);
        }

        dl.AddRectFilled(pMin, pMax, ImGui.GetColorU32(trackColor), trackRound);

        dl.AddRect(pMin, pMax, ImGui.GetColorU32(new Vector4(0f, 0f, 0f, 0.18f)),
                   trackRound, ImDrawFlags.None, 1f);

        float leftX = pMin.X + inset;
        float rightX = pMax.X - inset - knobW;
        float knobX = leftX + (rightX - leftX) * _modeKnobT;
        Vector2 knobMin = new(knobX, pMin.Y + inset);
        Vector2 knobMax = new(knobX + knobW, knobMin.Y + knobH);
        float knobRound = knobH * 0.5f;

        dl.AddRectFilled(new Vector2(knobMin.X, knobMin.Y + 1f),
                         new Vector2(knobMax.X, knobMax.Y + 1f),
                         ImGui.GetColorU32(KnobShadow), knobRound);
        dl.AddRectFilled(knobMin, knobMax, ImGui.GetColorU32(SwitchKnob), knobRound);

        Vector2 textPos = new(
            knobMin.X + (knobW - textW) * 0.5f,
            knobMin.Y + (knobH - fontSize) * 0.5f);

        Vector4 textFrom = daily ? DailyTextFrom : HardTextFrom;
        Vector4 textTo = daily ? DailyTextTo : HardTextTo;

        float charX = 0f;
        for (int i = 0; i < label.Length; i++)
        {
            string ch = label[i].ToString();
            float t = label.Length <= 1 ? 0f : i / (float)(label.Length - 1);
            dl.AddText(new Vector2(textPos.X + charX, textPos.Y),
                       ImGui.GetColorU32(Vector4.Lerp(textFrom, textTo, t)),
                       ch);
            charX += ImGui.CalcTextSize(ch).X;
        }

        ImGui.Dummy(new Vector2(0f, 6f));
    }

    private static readonly Card DevCard = new()
    {
        Label = "开发 / 排查",
        ContentsAction = DrawDevCardContents,
    };

    private static readonly Card TriggerlineCard = new()
    {
        Label = "时间轴设置",
        ContentsAction = DrawTriggerlineCardContents,
    };

    private static readonly Card OpenerCard = new()
    {
        Label = "起手设置",
        ContentsAction = DrawOpenerCardContents,
    };

    private static readonly Card HardBattleCard = new()
    {
        Label = "战斗设置",
        ContentsAction = DrawHardBattleCardContents,
    };

    private static readonly Card RangedCard = new()
    {
        Label = "远离设置",
        ContentsAction = DrawRangedCardContents,
    };

    private static readonly Card DailyBattleCard = new()
    {
        Label = "战斗设置",
        ContentsAction = DrawDailyBattleCardContents,
    };

    private static readonly Card DailyMitigationCard = new()
    {
        Label = "减伤设置",
        ContentsAction = DrawDailyMitigationCardContents,
    };

    private static readonly Card CommonCard = new()
    {
        Label = "通用",
        ContentsAction = DrawCommonCardContents,
    };

    public static readonly string[] OpenerNames =
    [
        "100级标准起手",
        "100级绝伊甸起手",
        "90级起手",
        "80级起手",
        "70级起手",
    ];

    private void DrawQtSettingsTab(JobViewWindow jobViewWindow)
    {
        DrawModeSwitch();

        if (PldSettings.Instance == null)
        {
            HintText("设置尚未初始化，稍后再打开这个页签");
            return;
        }

        bool daily = PldSettings.Instance.Mode == MitigationConfig.DailyModeValue;

        if (daily)
        {
            DailyBattleCard.DrawStretched();
            DailyMitigationCard.DrawStretched();
        }
        else
        {
            TriggerlineCard.DrawStretched();
            OpenerCard.DrawStretched();
            HardBattleCard.DrawStretched();
            RangedCard.DrawStretched();
        }

        CommonCard.DrawStretched();
    }

    private static void DrawTriggerlineCardContents()
    {
        PldTriggerlineCard.DrawContents();
    }

    private static void DrawOpenerCardContents()
    {
        PldSettings s = PldSettings.Instance;

        PldSettingWidgets.FieldCheckbox("启用起手", ref s.OpenerEnabled);

        if (!s.OpenerEnabled)
        {
            return;
        }

        PldSettingWidgets.ComboRow("起手方案", ref s.OpenerIndex, OpenerNames);

        PldSettingWidgets.IntStepper("预读时间 (毫秒)", ref s.OpenerPreReadMs, 0, 5000);

        PldSettingWidgets.QtCheckbox(PldQt.UsePotion, "开场爆发药");

        PldSettingWidgets.FieldCheckbox("倒计时使用圣光幕帘", ref s.CurtainCallInCountdown);
    }

    private static void DrawHardBattleCardContents()
    {
        PldSettings s = PldSettings.Instance;

        PldSettingWidgets.IntSlider("调停保留层数", ref s.InterveneKeepCharges, 0, 2);

        PldSettingWidgets.IntStepperFloat("调停安全距离 (米)", ref s.InterveneKeepDistance, 0, 25);

        PldSettingWidgets.FieldCheckbox("爆发内不保留调停", ref s.InterveneKeepInBurst);
    }

    private static void DrawRangedCardContents()
    {
        PldSettings s = PldSettings.Instance;

        PldSettingWidgets.IntSlider("圣灵释放阈值 (米)", ref s.RangedHolySpiritMinDistance, 3, 25);

        PldSettingWidgets.IntSlider("飞盾释放阈值 (米)", ref s.RangedShieldLobMinDistance, 3, 25);

        PldSettingWidgets.RedQtCheckbox(PldQt.SlideCast, "圣灵强制滑步，慎用");

        PldSettingWidgets.IntStepper("滑步放行阈值 (毫秒)", ref s.SlideCastReleaseMs, 0, 1000);
    }

    private static void DrawDailyBattleCardContents()
    {
        PldSettings s = PldSettings.Instance;

        PldSettingWidgets.FieldCheckbox("四人本自动盾姿", ref s.AutoTankStanceIn4Man);

        TargetSelectorConfig? selector = SettingMgr.GetSetting<TargetSelectorConfig>();
        if (selector != null)
        {
            bool enable = selector.Enable;
            if (ImGui.Checkbox("目标选择器###ae_target_selector", ref enable))
            {
                selector.Enable = enable;
                SettingMgr.Instance.Save();
            }
        }
        else
        {
            HintText("（读不到 AE 的目标选择器设置 SettingMgr.GetSetting<TargetSelectorConfig>()）");
        }

        PldSettingWidgets.IntSlider("调停拉怪数量门槛 (只)", ref s.IntervenePullMinAggro, 1, 6);
    }

    private static void DrawDailyMitigationCardContents()
    {
        PldSettings s = PldSettings.Instance;

        PldSettingWidgets.QtCheckbox(PldQt.UseAutoMitigation, "自动减伤");

        PldSettingWidgets.IntSlider("铁壁壁垒阈值 (%)", ref s.MitigationScarceBigThresholdPercent, 10, 35);

        PldSettingWidgets.IntSlider("大减阈值 (%)", ref s.MitigationBigThresholdPercent, 10, 35);

        PldSettingWidgets.IntSlider("无敌血量 (%)", ref s.MitigationInvulnHpPercent, 10, 35);

        PldSettingWidgets.QtCheckbox(PldQt.MitigationClemency, "深仁厚泽保命");
    }

    private static void DrawCommonCardContents()
    {
        PldSettings s = PldSettings.Instance;

        PldSettingWidgets.IntSlider("AOE 敌人数阈值", ref s.AoeEnemyCount, 1, 10);

        PldSettingWidgets.FieldCheckbox("切换模式时自动调整 QT", ref s.AutoAdjustQtOnModeSwitch);

        PldSettingWidgets.FieldCheckbox("使用游戏内提示", ref s.UseToast);
    }

    private void DrawQtDevTab(JobViewWindow jobViewWindow)
    {
        DevCard.DrawStretched();
    }

    private static void DrawDevCardContents()
    {
        bool dev = QT.GetQt(PldQt.DevDebug);
        if (ImGui.Checkbox(PldQt.DevDebug, ref dev))
        {
            QT.SetQt(PldQt.DevDebug, dev);
        }

        ImGui.Spacing();

        if (ImGui.Button(_showCommandLibrary ? "收起指令库（hotkey / qt）" : "指令库（hotkey / qt）"))
        {
            _showCommandLibrary = !_showCommandLibrary;
        }

        if (_showCommandLibrary)
        {
            DrawCommandLibrary();
        }
    }

    private static bool _showCommandLibrary;

    private static void DrawCommandLibrary()
    {
        if (QT == null)
        {
            HintText("QT 还没建好，稍后再打开这个页签");
            return;
        }

        HintText($"全部指令以 {PldCommand.MainCommand} 开头；点「复制」后到游戏宏里粘贴即可。");
        ImGui.Spacing();

        if (ImGui.BeginChild("###pld_cmd_lib", new Vector2(0f, 260f), true))
        {
            ImGui.TextColored(HintColor, "— QT 开关（取反）—");
            foreach (string name in QT.GetQtArray())
            {
                DrawCommandRow($"{PldCommand.MainCommand} qt {name}");
            }

            ImGui.Spacing();
            ImGui.TextColored(HintColor, "— 快捷键（触发一次）—");
            foreach (string name in PldCommand.HotkeyNames)
            {
                DrawCommandRow($"{PldCommand.MainCommand} hk {name}");
            }

            ImGui.Spacing();
            ImGui.TextColored(HintColor, "— 其它 —");
            foreach (string extra in new[] { "mode 0", "mode 1", "mit", "clear", "help" })
            {
                DrawCommandRow($"{PldCommand.MainCommand} {extra}");
            }
        }

        ImGui.EndChild();
    }

    private static void DrawCommandRow(string command)
    {
        if (ImGui.SmallButton($"复制###pld_cmd_{command}"))
        {
            ImGui.SetClipboardText(command);
        }

        ImGui.SameLine();
        ImGui.Text(command);
    }

    public void Dispose()
    {
        PldSlideCast.OnDispose();
    }

    public LittleDartRotationEntry()
    {
        SlotResolvers = new List<SlotResolverData>(65)
        {
            new(new ClemencyGcd(),             SlotMode.Gcd),

            new(new FofBladeOfFaithGcd(),      SlotMode.Gcd),
            new(new FofBladeOfTruthGcd(),      SlotMode.Gcd),
            new(new FofBladeOfValorGcd(),      SlotMode.Gcd),

            new(new DumpAtonementGcd(),       SlotMode.Gcd),
            new(new DumpSupplicationGcd(),    SlotMode.Gcd),
            new(new DumpSepulchreGcd(),       SlotMode.Gcd),
            new(new DumpHolySpiritGcd(),      SlotMode.Gcd),

            new(new FofAoeConfiteorGcd(),      SlotMode.Gcd),
            new(new FofHolyCircleRequiescatGcd(), SlotMode.Gcd),
            new(new HolyCircleGcd(),           SlotMode.Gcd),
            new(new ProminenceGcd(),           SlotMode.Gcd),
            new(new TotalEclipseGcd(),         SlotMode.Gcd),

            new(new PldLevel70Gcd(),           SlotMode.Gcd),
            new(new PldLevel80Gcd(),           SlotMode.Gcd),
            new(new PldLevel90Gcd(),           SlotMode.Gcd),

            new(new FofGoringBladeGcd(),       SlotMode.Gcd),
            new(new FofConfiteorGcd(),         SlotMode.Gcd),
            new(new FofSpiritRequiescatGcd(),  SlotMode.Gcd),

            new(new FofAtonementGcd(),                 SlotMode.Gcd),
            new(new BaseComboAfter76AtonementGcd(),    SlotMode.Gcd),
            new(new BaseComboAfter76SupplicationGcd(), SlotMode.Gcd),
            new(new BaseComboAfter76SepulchreGcd(),    SlotMode.Gcd),

            new(new FofSpiritEmpoweredGcd(),   SlotMode.Gcd),
            new(new FofRoyalAuthorityGcd(),    SlotMode.Gcd),
            new(new FofRiotBladeGcd(),         SlotMode.Gcd),
            new(new FofFastBladeGcd(),         SlotMode.Gcd),
            new(new FofFillerSpiritGcd(),      SlotMode.Gcd),

            new(new FofRequiescatSpiritRangedGcd(), SlotMode.Gcd),
            new(new FofRangedSpiritGcd(),      SlotMode.Gcd),
            new(new FofSlideCastSpiritGcd(),   SlotMode.Gcd),
            new(new FofStillRangedSpiritGcd(), SlotMode.Gcd),
            new(new FofShieldLobGcd(),         SlotMode.Gcd),

            new(new BaseComboHaloneGcd(),              SlotMode.Gcd),

            new(new BaseComboBefore64Gcd(),            SlotMode.Gcd),

            new(new RiotBladeGcd(),                    SlotMode.Gcd),
            new(new BaseComboAfter64Gcd(),             SlotMode.Gcd),
            new(new RoyalAuthorityGcd(),               SlotMode.Gcd),
            new(new FastBladeGcd(),                    SlotMode.Gcd),

            new(new HolySpiritRangedGcd(),             SlotMode.Gcd),
            new(new SlideCastHolySpiritGcd(),          SlotMode.Gcd),
            new(new ReadCastHolySpiritRangedGcd(),     SlotMode.Gcd),
            new(new ShieldLobGcd(),                    SlotMode.Gcd),

            new(new FofOffGcd(),               SlotMode.OffGcd),
            new(new RequiescatOffGcd(),        SlotMode.OffGcd),

            new(new BladeOfHonorOffGcd(),       SlotMode.OffGcd),

            new(new HallowedGroundOffGcd(),    SlotMode.OffGcd),
            new(new RampartOffGcd(),           SlotMode.OffGcd),
            new(new GuardianOffGcd(),          SlotMode.OffGcd),
            new(new SentinelOffGcd(),          SlotMode.OffGcd),
            new(new HolySheltronOffGcd(),      SlotMode.OffGcd),
            new(new SheltronOffGcd(),          SlotMode.OffGcd),
            new(new BulwarkOffGcd(),           SlotMode.OffGcd),
            new(new ReprisalOffGcd(),          SlotMode.OffGcd),
            new(new ArmsLengthOffGcd(),        SlotMode.OffGcd),
            new(new DivineVeilOffGcd(),        SlotMode.OffGcd),
            new(new SprintOffGcd(),            SlotMode.OffGcd),

            new(new CircleOfScornOffGcd(),     SlotMode.OffGcd),
            new(new ExpiacionOffGcd(),         SlotMode.OffGcd),
            new(new SpiritsWithinOffGcd(),     SlotMode.OffGcd),

            new(new IntervenePullOffGcd(),     SlotMode.OffGcd),
            new(new InterveneOffGcd(),         SlotMode.OffGcd),
        };

        Helper = new PldHelper();
    }
}
