using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.View.JobView;
using AEAssist.Helper;
using AEAssist.CombatRoutine.View.JobView.HotkeyResolver;
using LittleDart.Paladin.Hotkey;
using LittleDart.Paladin.Mitigation;
using LittleDart.Paladin.Setting;

namespace LittleDart.Paladin.Skill;

public static class PldCommand
{
    public const string MainCommand = "/LittleDart-PLD";

    public static void OnCommand(string command, string args)
    {
        args = args.Trim();

        if (string.IsNullOrEmpty(args))
        {
            PrintHelp();
            return;
        }

        string[] parts = args.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string action = parts[0].ToLowerInvariant();

        switch (action)
        {
            case "qt":
                if (parts.Length < 2)
                {
                    PldHelper.Log("用法：/LittleDart-PLD qt <开关名>");
                    return;
                }

                ToggleQt(parts[1]);
                break;

            case "hk":
                if (parts.Length < 2)
                {
                    PldHelper.Log("用法：/LittleDart-PLD hk <快捷键名>");
                    return;
                }

                TriggerHotkey(parts[1]);
                break;

            case "mode":
                SetMode(parts.Length >= 2 ? parts[1] : null);
                break;

            case "mit":
                PldHelper.Log(MitigationScheduler.Instance.DescribeStatus());
                break;

            case "clear":
                PldHelper.ClearHighPriorityQueues();
                break;

            case "help":
                PrintHelp();
                break;

            default:
                PldHelper.Log($"未知参数：{parts[0]}，输入 /LittleDart-PLD help 查看用法");
                break;
        }
    }

    public static void ToggleQt(string qtName)
    {
        bool wasOn = LittleDartRotationEntry.QT.GetQt(qtName);

        if (!IsValidQtName(qtName))
        {
            PldHelper.Log($"QT 开关不存在：{qtName}，输入 /LittleDart-PLD help 查看可用开关");
            return;
        }

        LittleDartRotationEntry.QT.ReverseQt(qtName);
        PldHelper.Log($"已{(wasOn ? "关闭" : "开启")}：{qtName}");
    }

    private static bool IsValidQtName(string qtName)
    {
        string[] all = LittleDartRotationEntry.QT.GetQtArray();
        return all.Contains(qtName);
    }

    public static void TriggerHotkey(string hotkeyName)
    {
        var resolver = GetHotkeyResolverByName(hotkeyName);

        if (resolver == null)
        {
            PldHelper.Log($"快捷键不存在：{hotkeyName}，输入 /LittleDart-PLD help 查看可用快捷键");
            return;
        }

        if (resolver.Check() >= 0)
        {
            resolver.Run();
            PldHelper.Log($"已触发：{hotkeyName}");
        }
        else
        {
            PldHelper.Log($"无法触发 {hotkeyName}：条件不满足（技能未就绪 / 无有效目标）");
        }
    }

    public static IHotkeyResolver? GetHotkeyResolverByName(string hotkeyName)
    {
        return hotkeyName switch
        {
            "铁壁"     => new HotKeyResolver_NormalSpell(PldSkills.铁壁,     SpellTargetType.Self,   false),
            "极致防御" => new HotKeyResolver_NormalSpell(PldSkills.极致防御, SpellTargetType.Self,   false),
            "圣盾阵"   => new HotKeyResolver_NormalSpell(PldSkills.圣盾阵,   SpellTargetType.Self,   false),
            "壁垒"     => new HotKeyResolver_NormalSpell(PldSkills.壁垒,     SpellTargetType.Self,   false),
            "圣光幕帘" => new HotKeyResolver_NormalSpell(PldSkills.圣光幕帘, SpellTargetType.Self,   false),
            "亲疏"     => new HotKeyResolver_NormalSpell(PldSkills.亲疏,     SpellTargetType.Self,   false),
            "武装戍卫" => new HotKeyResolver_NormalSpell(PldSkills.武装戍卫, SpellTargetType.Self,   false),
            "神圣领域" => new HotKeyResolver_NormalSpell(PldSkills.神圣领域, SpellTargetType.Self,   false),
            "挑衅"     => new HotKeyResolver_NormalSpell(PldSkills.挑衅,     SpellTargetType.Target, false),
            "血仇"     => new HotKeyResolver_NormalSpell(PldSkills.血仇,     SpellTargetType.Target, false),
            "调停"     => new HotKeyResolver_NormalSpell(PldSkills.调停,     SpellTargetType.Target, false),
            "圣灵"     => new HotKeyResolver_NormalSpell(PldSkills.圣灵,     SpellTargetType.Target, false),
            "投盾"     => new HotKeyResolver_NormalSpell(PldSkills.投盾,     SpellTargetType.Target, false),

            "冲刺" => new HotKeyResolver_疾跑(),
            "LB"   => new HotKeyResolver_LB(),

            "退避"           => new ShirkToPartnerHotkey(),

            "干预搭档"           => new InterventionPartnerHotkey(),
            "深仁厚泽搭档"       => new ClemencyPartnerHotkey(),
            "保护搭档"           => new CoverPartnerHotkey(),
            "干预奶妈"           => new InterventionHealerHotkey(),
            "深仁厚泽奶妈"       => new ClemencyHealerHotkey(),
            "保护奶妈"           => new CoverHealerHotkey(),
            "干预血量最低"       => new InterventionLowestHpSupportHotkey(),
            "深仁厚泽血量最低"   => new ClemencyLowestHpHotkey(),
            "保护血量最低"       => new CoverLowestHpHotkey(),

            "清扫队列"       => new ClearQueueHotkey(),

            _ => null,
        };
    }

    public static readonly string[] HotkeyNames =
    [
        "铁壁", "极致防御", "圣盾阵", "壁垒", "亲疏", "神圣领域", "武装戍卫", "圣光幕帘",
        "挑衅", "血仇", "调停", "圣灵", "投盾",
        "冲刺", "LB", "退避",
        "干预搭档", "深仁厚泽搭档", "保护搭档",
        "干预奶妈", "深仁厚泽奶妈", "保护奶妈",
        "干预血量最低", "深仁厚泽血量最低", "保护血量最低",
        "清扫队列",
    ];

    public static void SetMode(string? value)
    {
        var s = PldSettings.Instance;
        if (s == null)
        {
            PldHelper.Log("设置未初始化");
            return;
        }

        if (value is "0" or "1")
        {
            s.Mode = value == "1" ? 1 : 0;
            s.Save();

            if (LittleDartRotationEntry.QT != null)
            {
                LittleDartRotationEntry.QT.SetQt(PldQt.DailyMode, s.Mode == 1);

                LittleDartRotationEntry.QT.SetQt(PldQt.AutoSprint, s.Mode == 1);
            }

            bool presetApplied = PldRotationEventHandler.ApplyModePreset();

            if (!presetApplied)
            {
                PldHelper.ToastModeBanner(s.Mode == 1);
            }

            PldHelper.Log($"已切换到{(s.Mode == 1 ? "日常" : "高难")}模式");
            return;
        }

        PldHelper.Log($"当前模式：{(s.Mode == 1 ? "日常" : "高难")}（Mode={s.Mode}）。用法：/LittleDart-PLD mode 0（高难）| /LittleDart-PLD mode 1（日常）");
    }

    public static void PrintHelp()
    {
        PldHelper.Log("========== LittleDart 骑士 ACR 指令 ==========");
        PldHelper.Log($"{MainCommand} qt <开关名>    —— 切换某个 QT 开关");
        PldHelper.Log($"{MainCommand} hk <快捷键名>  —— 触发某个快捷键");
        PldHelper.Log($"{MainCommand} mode <0|1>     —— 切换高难/日常模式（日常才启用自动减伤）");
        PldHelper.Log($"{MainCommand} mit            —— 打印减伤调度器状态（排查为什么不调度）");
        PldHelper.Log($"{MainCommand} clear          —— 清扫高优先级技能队列（时间轴插的技能卡住时用）");
        PldHelper.Log($"{MainCommand} help           —— 显示这份帮助");
        PldHelper.Log("---------------------------------------------");
        PldHelper.Log("可用 QT 开关：" + string.Join("、", LittleDartRotationEntry.QT.GetQtArray()));
        PldHelper.Log("可用快捷键：" + string.Join("、", HotkeyNames));
        PldHelper.Log("=============================================");
    }
}
