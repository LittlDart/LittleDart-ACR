using AEAssist.GUI;
using Dalamud.Bindings.ImGui;
using System.Numerics;

namespace LittleDart.Paladin.Setting;

public static class PldSettingWidgets
{

    private const float SliderWidth = 150f;

    private static readonly Vector4 RedText = new(0.92f, 0.36f, 0.36f, 1f);

    private static void Save()
    {
        PldSettings.Instance?.Save();
    }

    public static void IntSlider(string label, ref int value, int min, int max)
    {
        ImGui.Text(label);
        ImGui.SameLine();

        float avail = ImGui.GetContentRegionAvail().X;
        float width = MathF.Min(SliderWidth, avail);
        if (width < 1f)
        {
            width = 1f;
        }

        float startX = ImGui.GetCursorPosX();
        float targetX = startX + avail - width;
        if (targetX > startX)
        {
            ImGui.SetCursorPosX(targetX);
        }

        int v = value;
        ImGui.PushID("##" + label);
        ImGui.SetNextItemWidth(width);
        ImGui.SliderInt("", ref v, min, max);
        ImGui.PopID();

        if (v != value)
        {
            value = v;
            Save();
        }
    }

    public static void IntStepper(string label, ref int value, int min, int max)
    {
        int v = value;
        ImGuiHelper.LeftInputInt(label, ref v, min, max);

        if (v != value)
        {
            value = v;
            Save();
        }
    }

    public static void IntStepperFloat(string label, ref float value, int min, int max)
    {
        int v = (int)MathF.Round(value);
        int before = v;
        ImGuiHelper.LeftInputInt(label, ref v, min, max);

        if (v != before)
        {
            value = v;
            Save();
        }
    }

    public static void ComboRow(string label, ref int index, string[] items)
    {
        if (items.Length == 0)
        {
            return;
        }

        if (index < 0 || index >= items.Length)
        {
            index = 0;
            Save();
        }

        int before = index;
        ImGuiHelper.LeftCombo(label, ref index, items);
        if (index != before)
        {
            Save();
        }
    }

    public static void FieldCheckbox(string label, ref bool value)
    {
        bool v = value;
        if (ImGui.Checkbox($"{label}###pld_fld_{label}", ref v))
        {
            value = v;
            Save();
        }
    }

    public static void QtCheckbox(string qtName, string label)
    {
        bool v = LittleDartRotationEntry.QT.GetQt(qtName);
        if (ImGui.Checkbox($"{label}###qt_{qtName}", ref v))
        {
            LittleDartRotationEntry.QT.SetQt(qtName, v);
            Save();
        }
    }

    public static void RedCheckbox(string label, ref bool value)
    {
        bool v = value;

        ImGui.PushStyleColor(ImGuiCol.Text, RedText);
        bool clicked = ImGui.Checkbox($"{label}###pld_red_{label}", ref v);
        ImGui.PopStyleColor();

        if (clicked)
        {
            value = v;
            Save();
        }
    }

    public static void RedQtCheckbox(string qtName, string label)
    {
        bool v = LittleDartRotationEntry.QT.GetQt(qtName);

        ImGui.PushStyleColor(ImGuiCol.Text, RedText);
        bool clicked = ImGui.Checkbox($"{label}###qt_{qtName}", ref v);
        ImGui.PopStyleColor();

        if (clicked)
        {
            LittleDartRotationEntry.QT.SetQt(qtName, v);
            Save();
        }
    }
}
