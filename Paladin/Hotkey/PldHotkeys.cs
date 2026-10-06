using System.Numerics;
using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.CombatRoutine.View.JobView;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using AEAssist.JobApi;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Textures.TextureWraps;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.Hotkey;

public class ShirkToPartnerHotkey : IHotkeyResolver
{
    public void Draw(Vector2 size)
    {
        Vector2 iconSize = size * 0.8f;
        ImGui.SetCursorPos(size * 0.1f);

        IDalamudTextureWrap texture = default;
        if (Core.Resolve<MemApiIcon>().GetActionTexture(PldSkills.退避, out texture))
        {
            ImGui.Image(texture.Handle, iconSize);
        }
    }

    public void DrawExternal(Vector2 size, bool isActive)
    {
        if (Core.Resolve<MemApiSpell>().GetCooldown(PldSkills.退避).TotalSeconds == 0
            && SpellExtension.IsUnlock(PldSkills.退避)
            && PldHelper.GetAnotherTank() != null)
        {
            HotkeyHelper.DrawActiveState(size);
        }
        else
        {
            HotkeyHelper.DrawDisabledState(size);
        }

        HotkeyHelper.DrawCooldownText(PldSkills.退避.GetSpell(), size);
    }

    public int Check()
    {
        if (!SpellExtension.IsUnlock(PldSkills.退避))
        {
            return -1;
        }

        if (Core.Resolve<MemApiSpell>().GetCooldown(PldSkills.退避).TotalSeconds > 0)
        {
            return -1;
        }

        if (PldHelper.GetAnotherTank() == null)
        {
            return -1;
        }

        return 0;
    }

    public void Run()
    {
        var partner = PldHelper.GetAnotherTank();
        if (partner == null)
        {
            PldHelper.Log("没有找到搭档坦克，退避未执行");
            return;
        }

        AI.Instance.BattleData.NextSlot ??= new Slot(600);

        AI.Instance.BattleData.NextSlot.Add(new Spell(PldSkills.退避, partner));

        PldHelper.Log($"已退避 → {partner.Name}");
    }
}

public class ClearQueueHotkey : IHotkeyResolver
{
    private const string IconPath = "../../ACR/LittleDart/Resources/ClearQueue.png";

    public void Draw(Vector2 size)
    {
        Vector2 iconSize = size * 0.8f;
        ImGui.SetCursorPos(size * 0.1f);

        IDalamudTextureWrap texture = default;
        if (Core.Resolve<MemApiIcon>().TryGetTexture(IconPath, out texture))
        {
            ImGui.Image(texture.Handle, iconSize);
            return;
        }

        ImGui.TextColored(new Vector4(1f, 0.85f, 0.2f, 1f), "清");
    }

    public void DrawExternal(Vector2 size, bool isActive)
    {
    }

    public int Check()
    {
        return 0;
    }

    public void Run()
    {
        PldHelper.ClearHighPriorityQueues();
    }
}
