using System.Numerics;
using AEAssist;
using AEAssist.CombatRoutine.Trigger;
using AEAssist.CombatRoutine.Trigger.Node;
using AEAssist.CombatRoutine.View.JobView;
using AEAssist.Extension;
using AEAssist.GUI;
using AEAssist.GUI.Tree;
using Dalamud.Bindings.ImGui;
using LittleDart.Paladin.Setting;

namespace LittleDart.Paladin.Triggers;

public class TriggerActionSetQt : ITriggerAction, ITriggerlineCheck
{
    public string DisplayName { get; } = "LittleDart/QT 开关控制";

    public string Remark { get; set; } = string.Empty;

    [LabelName("QT 开关名")]
    public string QtName { get; set; } = PldQt.StopAttack;

    [LabelName("设为开启")]
    public bool Value { get; set; } = true;

    private int _selectIndex;

    private static string[] AllQtNames()
    {
        JobViewWindow? qt = LittleDartRotationEntry.QT;
        return qt?.GetQtArray() ?? Array.Empty<string>();
    }

    public bool Draw()
    {
        string[] all = AllQtNames();
        if (all.Length == 0)
        {
            ImGui.Text("悬浮窗尚未就绪，稍后再打开时间轴编辑器");
            return true;
        }

        bool valid = all.Contains(QtName);

        _selectIndex = valid ? Array.IndexOf(all, QtName) : 0;

        int before = _selectIndex;
        ImGuiHelper.LeftCombo("QT 开关名", ref _selectIndex, all, 240);

        if (_selectIndex != before && _selectIndex >= 0 && _selectIndex < all.Length)
        {
            QtName = all[_selectIndex];
            valid = true;
        }

        ImGui.SameLine();

        bool value = Value;
        if (ImGui.Checkbox("设为开启", ref value))
        {
            Value = value;
        }

        if (!valid)
        {
            ImGui.TextColored(new Vector4(1f, 0.4f, 0.4f, 1f),
                $"⚠ 开关「{QtName}」不存在，请从下拉框重新选择");
        }

        return true;
    }

    public void Check(TreeCompBase parent, TreeNodeBase currNode, TriggerLine triggerLine,
                      Env env, TriggerlineCheckResult checkResult)
    {
        string[] all = AllQtNames();

        if (all.Length == 0)
        {
            return;
        }

        if (!all.Contains(QtName))
        {
            checkResult.AddError(currNode,
                $"LittleDart：QT 开关「{QtName}」不存在，这个节点不会有任何效果。" +
                "请在行为的下拉框里重新选择（可用开关：/LittleDart-PLD help 会列出来）。");
        }
    }

    public bool Handle()
    {
        string[] all = AllQtNames();
        if (all.Length > 0 && !all.Contains(QtName))
        {
            PldHelper.Log($"时间轴行为失败：QT 开关「{QtName}」不存在（未做任何操作）");
            return true;
        }

        LittleDartRotationEntry.QT.SetQt(QtName, Value);

        PldHelper.Log($"时间轴行为：QT「{QtName}」→ {(Value ? "开启" : "关闭")}");

        return true;
    }
}

public class TriggerActionSetTankStance : ITriggerAction
{
    public string DisplayName { get; } = "LittleDart/坦克姿态控制";

    public string Remark { get; set; } = string.Empty;

    [LabelName("开启姿态")]
    public bool On { get; set; } = true;

    public bool Draw()
    {
        return false;
    }

    public bool Handle()
    {
        PldHelper.SetTankStance(On);
        PldHelper.Log($"时间轴行为：坦克姿态 → {(On ? "开启" : "关闭")}");

        return true;
    }
}

public class TriggerConditionSelfPosition : ITriggerCond
{
    public string DisplayName { get; } = "LittleDart/自身位置范围";

    public string Remark { get; set; } = string.Empty;

    [LabelName("自身位置起始 X")]
    public float Point1X { get; set; }

    [LabelName("自身位置起始 Z")]
    public float Point1Z { get; set; }

    [LabelName("自身位置终止 X")]
    public float Point2X { get; set; }

    [LabelName("自身位置终止 Z")]
    public float Point2Z { get; set; }

    public bool Draw()
    {
        return false;
    }

    public bool Handle(ITriggerCondParams? condParams = null)
    {
        Vector3 position = Core.Me.Position;

        float minX = Math.Min(Point1X, Point2X);
        float maxX = Math.Max(Point1X, Point2X);
        float minZ = Math.Min(Point1Z, Point2Z);
        float maxZ = Math.Max(Point1Z, Point2Z);

        return position.X >= minX && position.X <= maxX
            && position.Z >= minZ && position.Z <= maxZ;
    }
}

public class TriggerConditionTargetPosition : ITriggerCond
{
    public string DisplayName { get; } = "LittleDart/目标位置范围";

    public string Remark { get; set; } = string.Empty;

    [LabelName("目标位置起始 X")]
    public float Point1X { get; set; }

    [LabelName("目标位置起始 Z")]
    public float Point1Z { get; set; }

    [LabelName("目标位置终止 X")]
    public float Point2X { get; set; }

    [LabelName("目标位置终止 Z")]
    public float Point2Z { get; set; }

    public bool Draw()
    {
        return false;
    }

    public bool Handle(ITriggerCondParams? condParams = null)
    {
        var target = Core.Me.GetCurrTarget();
        if (target == null)
        {
            return false;
        }

        Vector3 position = target.Position;

        float minX = Math.Min(Point1X, Point2X);
        float maxX = Math.Max(Point1X, Point2X);
        float minZ = Math.Min(Point1Z, Point2Z);
        float maxZ = Math.Max(Point1Z, Point2Z);

        return position.X >= minX && position.X <= maxX
            && position.Z >= minZ && position.Z <= maxZ;
    }
}
