using Dalamud.Bindings.ImGui;

namespace LittleDart.Paladin.Setting;

public class PldSettingUi
{
    public static readonly PldSettingUi Instance = new();

    public void Draw()
    {
        ImGui.Text("这是一个骑士ACR");
    }
}
