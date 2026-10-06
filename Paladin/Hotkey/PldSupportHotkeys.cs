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
using LittleDart.Paladin.Setting;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.Hotkey;

public abstract class PldSupportHotkeyBase : IHotkeyResolver
{
    private const string IconPathPrefix = "../../ACR/LittleDart/Resources/";

    protected abstract uint SkillId { get; }

    protected abstract string SkillName { get; }

    protected abstract PldHelper.SupportTargetKind TargetKind { get; }

    protected abstract string IconFile { get; }

    protected abstract float ConfiguredRange { get; }

    protected float EffectiveRange
    {
        get
        {
            float gameRange = PldHelper.GetActionRange(SkillId);
            return gameRange <= 0f ? ConfiguredRange : MathF.Min(ConfiguredRange, gameRange);
        }
    }

    protected virtual bool HasResource()
    {
        return Core.Resolve<JobApi_Paladin>().Oath >= 50;
    }

    public void Draw(Vector2 size)
    {
        Vector2 iconSize = size * 0.8f;
        ImGui.SetCursorPos(size * 0.1f);

        IDalamudTextureWrap texture = default;
        if (Core.Resolve<MemApiIcon>().TryGetTexture(IconPathPrefix + IconFile, out texture))
        {
            ImGui.Image(texture.Handle, iconSize);
        }
    }

    public void DrawExternal(Vector2 size, bool isActive)
    {
        if (IsUsableNow())
        {
            HotkeyHelper.DrawActiveState(size);
        }
        else
        {
            HotkeyHelper.DrawDisabledState(size);
        }

        HotkeyHelper.DrawCooldownText(SkillId.GetSpell(), size);
    }

    private bool IsUsableNow()
    {
        if (!SpellExtension.IsUnlock(SkillId))
        {
            return false;
        }

        if (!IsSkillReady())
        {
            return false;
        }

        if (!HasResource())
        {
            return false;
        }

        IBattleChara? target = PldHelper.GetSupportTarget(TargetKind);
        return target is not null && PldHelper.GetDistanceTo(target) <= EffectiveRange;
    }

    protected virtual bool IsSkillReady()
    {
        return Core.Resolve<MemApiSpell>().GetCooldown(SkillId).TotalSeconds <= 0;
    }

    protected virtual bool BlocksQueue => !IsSkillReady();

    protected virtual int QueueWindowMs => 600;

    protected const int GcdQueueWindowMs = 2500;

    public int Check()
    {
        return SpellExtension.IsUnlock(SkillId) ? 0 : -1;
    }

    public void Run()
    {
        if (BlocksQueue)
        {
            PldHelper.Log($"{SkillName}失败");
            return;
        }

        if (!HasResource())
        {
            PldHelper.Log($"{SkillName}失败");
            return;
        }

        IBattleChara? target = PldHelper.GetSupportTarget(TargetKind);
        if (target is null)
        {
            PldHelper.Log($"{SkillName}失败");
            return;
        }

        if (PldHelper.GetDistanceTo(target) > EffectiveRange)
        {
            PldHelper.Log($"{SkillName}失败");
            return;
        }

        AI.Instance.BattleData.NextSlot ??= new Slot(QueueWindowMs);
        AI.Instance.BattleData.NextSlot.Add(new Spell(SkillId, target));

        PldHelper.Log($"{SkillName}：{target.Name}");
    }
}

public class InterventionPartnerHotkey : PldSupportHotkeyBase
{
    protected override uint SkillId => PldSkills.干预;
    protected override string SkillName => "干预";
    protected override PldHelper.SupportTargetKind TargetKind => PldHelper.SupportTargetKind.Partner;
    protected override string IconFile => "InterventionPartner.png";
    protected override float ConfiguredRange => 30f;
}

public class InterventionHealerHotkey : PldSupportHotkeyBase
{
    protected override uint SkillId => PldSkills.干预;
    protected override string SkillName => "干预";
    protected override PldHelper.SupportTargetKind TargetKind => PldHelper.SupportTargetKind.Healer;
    protected override string IconFile => "InterventionHealer.png";
    protected override float ConfiguredRange => 30f;
}

public class InterventionLowestHpSupportHotkey : PldSupportHotkeyBase
{
    protected override uint SkillId => PldSkills.干预;
    protected override string SkillName => "干预";
    protected override PldHelper.SupportTargetKind TargetKind => PldHelper.SupportTargetKind.LowestHp;
    protected override string IconFile => "InterventionLowestHp.png";
    protected override float ConfiguredRange => 30f;
}

public class ClemencyPartnerHotkey : PldSupportHotkeyBase
{
    protected override uint SkillId => PldSkills.深仁厚泽;
    protected override string SkillName => "深仁厚泽";
    protected override PldHelper.SupportTargetKind TargetKind => PldHelper.SupportTargetKind.Partner;
    protected override string IconFile => "ClemencyPartner.png";
    protected override float ConfiguredRange => 30f;

    protected override bool HasResource()
    {
        return Core.Me.CurrentMp >= PldSettings.Instance.ClemencyMinMp;
    }

    protected override bool IsSkillReady()
    {
        return !Core.Me.IsCasting;
    }

    protected override bool BlocksQueue => false;

    protected override int QueueWindowMs => GcdQueueWindowMs;
}

public class ClemencyHealerHotkey : ClemencyPartnerHotkey
{
    protected override PldHelper.SupportTargetKind TargetKind => PldHelper.SupportTargetKind.Healer;
    protected override string IconFile => "ClemencyHealer.png";
}

public class ClemencyLowestHpHotkey : ClemencyPartnerHotkey
{
    protected override PldHelper.SupportTargetKind TargetKind => PldHelper.SupportTargetKind.LowestHp;
    protected override string IconFile => "ClemencyLowestHp.png";
}

public class CoverPartnerHotkey : PldSupportHotkeyBase
{
    protected override uint SkillId => PldSkills.保护;
    protected override string SkillName => "保护";
    protected override PldHelper.SupportTargetKind TargetKind => PldHelper.SupportTargetKind.Partner;
    protected override string IconFile => "CoverPartner.png";

    protected override float ConfiguredRange => 20f;
}

public class CoverHealerHotkey : CoverPartnerHotkey
{
    protected override PldHelper.SupportTargetKind TargetKind => PldHelper.SupportTargetKind.Healer;
    protected override string IconFile => "CoverHealer.png";
}

public class CoverLowestHpHotkey : CoverPartnerHotkey
{
    protected override PldHelper.SupportTargetKind TargetKind => PldHelper.SupportTargetKind.LowestHp;
    protected override string IconFile => "CoverLowestHp.png";
}
