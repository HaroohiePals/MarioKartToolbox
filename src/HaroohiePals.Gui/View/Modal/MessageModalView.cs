using System.Numerics;
using ImGuiNET;

namespace HaroohiePals.Gui.View.Modal;

public class MessageModalView(string title, string message) 
    : ModalView(title, Vector2.Zero)
{
    protected override void DrawContent()
    {
        ImGui.TextUnformatted(message);
    }
}