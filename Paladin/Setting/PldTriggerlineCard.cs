using AEAssist;
using AEAssist.CombatRoutine.Module;
using AEAssist.CombatRoutine.Trigger;
using AEAssist.GUI;
using AEAssist.GUI.Tree;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using Dalamud.Bindings.ImGui;

namespace LittleDart.Paladin.Setting;

public static class PldTriggerlineCard
{
    private static readonly System.Numerics.Vector4 ErrorColor = new(1f, 0.4f, 0.4f, 1f);

    private static string? _lastLoggedError;

    public static void DrawContents()
    {
        try
        {
            DrawContentsInternal();

            _lastLoggedError = null;
        }
        catch (Exception ex)
        {
            ImGui.TextColored(ErrorColor, "时间轴设置面板绘制出错：" + ex.Message);

            if (ex.Message != _lastLoggedError)
            {
                _lastLoggedError = ex.Message;
                LogHelper.Error($"[LittleDart-PLD/时间轴设置] 绘制失败：{ex}");
            }
        }
    }

    private static void DrawContentsInternal()
    {
        uint currTerrId = Core.Resolve<MemApiZoneInfo>().GetCurrTerrId();
        ImGui.Text("当前区域ID(TerritoryTypeID): " + currTerrId);
        ImGui.Text("当前天气ID: " + WeatherHelper.GetWeatherId());

        ImGui.Text("当前职能:");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(150f);

        if (ImGui.BeginCombo("###当前职能", AI.Instance.PartyRole ?? string.Empty))
        {
            foreach (string partyRole in AI.Instance.PartyRoleList)
            {
                if (ImGui.Selectable(partyRole))
                {
                    AI.Instance.PartyRole = partyRole;
                }
            }

            ImGui.EndCombo();
        }

        ImGui.Text("当前时间轴");
        ImGui.Indent();

        TriggerLine? currTriggerLine = AI.Instance.TriggerlineData.CurrTriggerLine;
        ImGui.Text(currTriggerLine == null
            ? "无"
            : "[" + currTriggerLine.Author + "]" + currTriggerLine.Name);

        if (currTriggerLine != null)
        {
            ImGui.Text("导出变量:");
            ImGui.Indent();

            if (currTriggerLine.ExposedVars != null)
            {
                foreach (string exposedVar in currTriggerLine.ExposedVars)
                {
                    int value = AI.Instance.ExposeVarsGetValueOrDefault(exposedVar);
                    ImGuiHelper.LeftInputInt(exposedVar, ref value);
                    AI.Instance.ExposeVarsSet(exposedVar, value);
                }
            }

            ImGui.Unindent();

            ImGui.Text("Debug信息:");
            ImGui.Indent();

            List<KeyValuePair<TreeActionBase, TaskCompletionSource<bool>>> waitingNodes = new();
            try
            {
                foreach (KeyValuePair<TreeActionBase, TaskCompletionSource<bool>> pair
                         in AI.Instance.TriggerlineData.ActiveActionBase2TCS)
                {
                    waitingNodes.Add(pair);
                }
            }
            catch (Exception)
            {
            }

            foreach (KeyValuePair<TreeActionBase, TaskCompletionSource<bool>> node in waitingNodes)
            {
                ImGui.Text("等待节点: Id=" + node.Key.Id + " Name=" + node.Key.Remark);
            }

            ImGui.Unindent();
        }

        ImGui.Unindent();

        if (ImGui.Button("卸载时间轴"))
        {
            AI.Instance.TriggerlineData.Clear("LittleDart/Paladin/Setting/PldTriggerlineCard.cs（职业设置卡片）");
        }

        ImGui.Text("当前场景可用时间轴 " + currTerrId + ":");
        ImGui.Indent();

        if (!string.IsNullOrEmpty(TriggerLineHelper.LastChoosed))
        {
            ImGui.Text("指定时间轴(不自动切换) : " + TriggerLineHelper.LastChoosed);
            ImGui.SameLine();

            if (ImGui.Button("清除"))
            {
                TriggerLineHelper.LastChoosed = string.Empty;
            }

            ImGui.SameLine();

            if (ImGui.Button("清除并自动切换一次"))
            {
                TriggerLineHelper.LastChoosed = string.Empty;

                TriggerLineHelper.LoadDefaultTriggerline().Wait();
            }
        }

        if (TriggerLineHelper.Terr2Triggerlines.TryGetValue(
                currTerrId, out List<HighEndTriggerline>? lines) && lines != null)
        {
            foreach (HighEndTriggerline line in lines)
            {
                using (new GroupWrapper())
                {
                    ImGui.Text("[" + line.Author + "]" + line.Name);
                    ImGui.SameLine();

                    if (ImGui.Button("指定加载"))
                    {
                        TriggerLineHelper.LastChoosed = line.Name;

                        TriggerLineHelper.ForceLoadTriggerline(line.Name);
                    }
                }
            }
        }

        ImGui.Unindent();
    }
}
