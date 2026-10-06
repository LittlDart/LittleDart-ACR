using AEAssist.CombatRoutine.View.JobView;
using LittleDart.Paladin.Hotkey;
using LittleDart.Paladin.Setting;
using System.Numerics;

namespace LittleDart.Paladin;

public static class HotkeyManager
{
    private static HotkeyWindow? _partnerPanel;

    public static void InitializePartnerPanel()
    {
        var settings = PldSettings.Instance;

        var save = new JobViewSave
        {
            QtHotkeySize = settings.PartnerPanelIconSize,
            ShowHotkey = settings.PartnerPanelShow,
        };

        _partnerPanel = new HotkeyWindow(save, "LittleDartPartnerPanel")
        {
            HotkeyLineCount = 3,
        };

        _partnerPanel.AddHotkey("干预搭档", new InterventionPartnerHotkey());
        _partnerPanel.AddHotkey("深仁厚泽搭档", new ClemencyPartnerHotkey());
        _partnerPanel.AddHotkey("保护搭档", new CoverPartnerHotkey());

        _partnerPanel.AddHotkey("干预奶妈", new InterventionHealerHotkey());
        _partnerPanel.AddHotkey("深仁厚泽奶妈", new ClemencyHealerHotkey());
        _partnerPanel.AddHotkey("保护奶妈", new CoverHealerHotkey());

        _partnerPanel.AddHotkey("干预血量最低", new InterventionLowestHpSupportHotkey());
        _partnerPanel.AddHotkey("深仁厚泽血量最低", new ClemencyLowestHpHotkey());
        _partnerPanel.AddHotkey("保护血量最低", new CoverLowestHpHotkey());
    }

    public static void QtUpdate()
    {
        if (_partnerPanel == null)
        {
            return;
        }

        _partnerPanel.DrawHotkeyWindow(new QtStyle(PldSettings.Instance.JobViewSave));
    }
}
