#nullable enable

using ImGuiNET;
using System.Numerics;

namespace HaroohiePals.Gui.View;

public class DockSpaceView : IView
{
    public bool IsOverViewport { get; set; } = true;
    public Vector2 Size { get; set; } = new(0);

    public bool Draw()
    {
        if (IsOverViewport)
        {
            ImGui.DockSpaceOverViewport();
        }
        else
        {
            ImGui.DockSpace(ImGui.GetID(GetHashCode()), Size);
        }

        return true;
    }
}