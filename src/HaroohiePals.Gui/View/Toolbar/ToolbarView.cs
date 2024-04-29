using ImGuiNET;
using System.Collections.Generic;
using System.Numerics;

namespace HaroohiePals.Gui.View.Toolbar;

public class ToolbarView : IView
{
    public string Id { get; }

    public readonly List<ToolbarItem> Items = new();

    public float Height { get; set; } = 22f;
    public Vector2 FramePadding { get; set; } = new Vector2(3, 3);

    public bool StickViewportTop { get; set; } = true;
    public Vector2 StickViewportTopPadding { get; set; } = new Vector2(4, 0);

    public ToolbarView(string id)
    {
        Id = id;
    }

    public bool Draw()
    {
        if (StickViewportTop)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(ImGuiEx.CalcUiScaledValue(StickViewportTopPadding.X), ImGuiEx.CalcUiScaledValue(StickViewportTopPadding.Y)));
            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, new Vector2(0, 0));
            if (ImGui.Begin(Id + "##ToolbarWindow",
                    ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove |
                    ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoDocking |
                    ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoSavedSettings |
                    ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus |
                    ImGuiWindowFlags.NoFocusOnAppearing))
            {
                ImGui.SetWindowSize(new Vector2(ImGui.GetMainViewport().Size.X, ImGuiEx.CalcUiScaledValue(Height)));
                ImGui.SetWindowPos(ImGui.GetMainViewport().WorkPos);

                DrawToolbar();
                ImGui.End();
            }

            ImGui.PopStyleVar(4);

            ImGui.GetMainViewport().WorkPos.Y += ImGuiEx.CalcUiScaledValue(Height);
            ImGui.GetMainViewport().WorkSize.Y -= ImGuiEx.CalcUiScaledValue(Height);
        }
        else
        {
            DrawToolbar();
        }

        return true;
    }

    private void DrawToolbar()
    {
        if (!ImGui.BeginChild(Id, new Vector2(0, ImGuiEx.CalcUiScaledValue(Height))))
            return;

        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, ImGui.GetStyle().ItemInnerSpacing);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(FramePadding.X, FramePadding.Y));

        bool first = true;
        foreach (var item in Items)
        {
            if (!first)
                ImGui.SameLine();

            if (!item.Enabled)
                ImGui.BeginDisabled();

            if (ImGui.Button($"{item.Icon}", new Vector2(ImGuiEx.CalcUiScaledValue(22))))
                item.Action?.Invoke();
            else if (ImGui.IsItemHovered())
                ImGui.SetTooltip(item.Label);

            if (!item.Enabled)
                ImGui.EndDisabled();

            first = false;
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleVar();
        ImGui.EndChild();
    }
}