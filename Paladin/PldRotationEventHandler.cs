using System.Reflection;
using System.Threading.Tasks;
using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using Dalamud.Game.Command;
using ECommons.DalamudServices;
using LittleDart.Paladin.GCD;
using LittleDart.Paladin.Mitigation;
using LittleDart.Paladin.OffGcd;
using LittleDart.Paladin.Setting;
using LittleDart.Paladin.Skill;
using LittleDart.Paladin.Triggers;

namespace LittleDart.Paladin;

public class PldRotationEventHandler : IRotationEventHandler
{

    public async Task OnPreCombat()
    {
        TryAutoTankStanceIn4Man("进入战斗前");

        await Task.CompletedTask;
    }

    public void OnResetBattle()
    {
        LittleDartRotationEntry.QT.SetQt(PldQt.StopAttack, false);

        PldSettings.Instance.AutoPullActive = false;

        ConfiteorDelay.Reset();

        MitigationScheduler.Instance.Reset();
    }

    public async Task OnNoTarget()
    {

        await Task.CompletedTask;
    }

    public void OnSpellCastSuccess(Slot slot, Spell spell)
    {
    }

    public void BeforeSpell(Slot slot, Spell spell)
    {
    }

    public void AfterSpell(Slot slot, Spell spell)
    {
        if (spell != null && spell.Id == PldSkills.沥血剑)
        {
            ConfiteorDelay.StartDelay();
        }
        else
        {
            ConfiteorDelay.Tick();
        }

    }

    public void OnBattleUpdate(int currTimeInMs)
    {
        Diagnostics.TickComboSnapshot();

        MitigationScheduler.Instance.Tick();

        _ = currTimeInMs;
    }

    public void OnEnterRotation()
    {
        ApplyModePreset();

        TryAutoTankStanceIn4Man("启用 ACR 时");

        try
        {
            Svc.Commands.RemoveHandler(PldCommand.MainCommand);
        }
        catch (Exception)
        {
        }

        Svc.Commands.AddHandler(PldCommand.MainCommand, new CommandInfo(PldCommand.OnCommand)
        {
            HelpMessage = "LittleDart 骑士 ACR：/LittleDart-PLD qt <开关名> | /LittleDart-PLD hk <快捷键名> | /LittleDart-PLD help",
        });

        PldHelper.Log($"欢迎使用；暂无时间轴适配，接下来会制作极神和绝伊甸轴。 <Build {BuildStamp}>");
    }

    private const string BuildStampMetadataKey = "BuildStamp";

    private static string BuildStamp
    {
        get
        {
            try
            {
                foreach (AssemblyMetadataAttribute meta in
                         typeof(PldRotationEventHandler).Assembly
                             .GetCustomAttributes(typeof(AssemblyMetadataAttribute), false))
                {
                    if (meta.Key == BuildStampMetadataKey)
                    {
                        return meta.Value ?? "no-stamp";
                    }
                }

                return "no-stamp";
            }
            catch
            {
                return "unknown";
            }
        }
    }

    public void OnExitRotation()
    {
        try
        {
            Svc.Commands.RemoveHandler(PldCommand.MainCommand);
        }
        catch (Exception)
        {
        }

        PldHelper.Log("骑士 ACR 已停用,感谢使用。");
    }

    public void OnTerritoryChanged()
    {
        PldSettings.Instance.AutoPullActive = false;

        MitigationScheduler.Instance.Reset();

        TryAutoTankStanceIn4Man("切换地图");

    }

    private static bool TryAutoTankStanceIn4Man(string source)
    {
        if (!PldSettings.Instance.AutoTankStanceIn4Man)
        {
            return false;
        }

        var duty = Core.Resolve<MemApiDuty>();

        if (!duty.IsBoundByDuty())
        {
            return false;
        }

        if (duty.DutyMembersNumber() != 4)
        {
            return false;
        }

        if (PldHelper.HasTankStance())
        {
            return false;
        }

        if (PldSkills.钢铁信念.RecentlyUsed(3000))
        {
            return false;
        }

        PldHelper.SetTankStance(true);
        PldHelper.Log(
            $"[ST6] {source}：检测到四人副本且当前无坦克姿态 → 已自动开启「钢铁信念」" +
            "（八人本 / 副本外不会自动开；出本也不会自动关）");

        return true;
    }

    public static bool ApplyModePreset()
    {
        LittleDartRotationEntry.ApplyQtVisibility();

        if (!PldSettings.Instance.AutoAdjustQtOnModeSwitch)
        {
            return false;
        }

        if (PldSettings.Instance.Mode == 0)
        {
            LittleDartRotationEntry.QT.SetQt(PldQt.UseSingleTargetOpener, true);
            LittleDartRotationEntry.QT.SetQt(PldQt.UseAoe, false);
            LittleDartRotationEntry.QT.SetQt(PldQt.UseFof, true);
            LittleDartRotationEntry.QT.SetQt(PldQt.UsePotion, false);
            LittleDartRotationEntry.QT.SetQt(PldQt.UseAutoMitigation, false);
            LittleDartRotationEntry.QT.SetQt(PldQt.MitigationClemency, false);
            LittleDartRotationEntry.QT.SetQt(PldQt.AutoSprint, false);
            LittleDartRotationEntry.QT.SetQt(PldQt.HolySpiritRanged, true);
            LittleDartRotationEntry.QT.SetQt(PldQt.ShieldLobRanged, false);
            PldSettings.Instance.InterveneKeepDistance = 0f;
            LittleDartRotationEntry.QT.SetQt(PldQt.IntervenePull, false);
        }
        else
        {
            LittleDartRotationEntry.QT.SetQt(PldQt.UseSingleTargetOpener, false);
            LittleDartRotationEntry.QT.SetQt(PldQt.UseAoe, true);
            LittleDartRotationEntry.QT.SetQt(PldQt.UseFof, true);
            LittleDartRotationEntry.QT.SetQt(PldQt.UsePotion, false);
            LittleDartRotationEntry.QT.SetQt(PldQt.UseAutoMitigation, true);
            LittleDartRotationEntry.QT.SetQt(PldQt.AutoSprint, true);
            LittleDartRotationEntry.QT.SetQt(PldQt.MitigationClemency, true);
            LittleDartRotationEntry.QT.SetQt(PldQt.ShieldLobRanged, true);
            LittleDartRotationEntry.QT.SetQt(PldQt.IntervenePull, true);
        }

        PldSettings.Instance.Save();

        PldHelper.ToastModeBanner(PldSettings.Instance.Mode != 0);

        return true;
    }
}
