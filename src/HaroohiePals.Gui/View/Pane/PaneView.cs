#nullable enable
using ImGuiNET;
using System.Numerics;

namespace HaroohiePals.Gui.View.Pane;

public abstract class PaneView(string title, Vector2? size = null) : IView
{
    public string Title { get; set; } = title;
    public Vector2 Size { get; set; } = size ?? new Vector2(600, 400);
    public ImGuiWindowFlags Flags { get; set; } = ImGuiWindowFlags.None;
    public bool NoPadding { get; set; } = false;

    public bool Draw()
    {
        bool result = false;

        if (NoPadding)
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);

        if (ImGui.Begin(Title, Flags))
        {
            ImGui.SetWindowSize(Size, ImGuiCond.Once);

            DrawContent();

            ImGui.End();

            result = true;
        }

        if (NoPadding)
            ImGui.PopStyleVar();

        return result;
    }

    public abstract void DrawContent();

    public virtual void Update(UpdateArgs args) { }
}