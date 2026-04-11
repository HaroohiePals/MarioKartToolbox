#nullable enable
using HaroohiePals.Gui;
using HaroohiePals.Gui.Viewport;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace HaroohiePals.MarioKartToolbox.Gui.Viewport;

sealed class ViewportSideToolbar(IReadOnlyList<ViewportSideToolbarExtraTool>? extraTools = null)
{
    public void Draw(ViewportContext context, Gizmo gizmo)
    {
        float scale = ImGuiEx.GetUiScale();

        var padding = new Vector2(8, 8) * scale;

        float btnSize = 24 * scale;
        float spacing = 2 * scale;

        var visibleExtraTools = extraTools?.Where(x => 
            x.IsVisible?.Invoke() ?? true).ToList() ?? [];

        int itemCount = gizmo.EnabledTools.Count + visibleExtraTools.Count;

        uint selectedColor = ImGui.GetColorU32(ImGuiCol.ButtonHovered) | 0xFF000000;

        // Make child frame transparent
        ImGui.PushStyleColor(ImGuiCol.ChildBg, 0);
        var targetTool = gizmo.Tool;
        string? clickedExtraTool = null;

        var oldCursor = ImGui.GetCursorPos();
        ImGui.SetCursorPos(padding);
        if (ImGui.BeginChild(ImGui.GetID("Tools"), new(btnSize + spacing, (btnSize + spacing) * itemCount)))
        {
            ImGui.SetCursorPosX(0);
            ImGui.SetCursorPosY(0);

            int currentItemNumber = 0;

            if (gizmo.EnabledTools.Contains(GizmoTool.Draw))
            {
                if (gizmo.Tool == GizmoTool.Draw)
                    ImGui.PushStyleColor(ImGuiCol.Button, selectedColor);
                if (ImGui.Button($"{FontAwesome6.Pencil}##Draw", new(btnSize)))
                    targetTool = GizmoTool.Draw;
                if (gizmo.Tool == GizmoTool.Draw)
                    ImGui.PopStyleColor();
                if (currentItemNumber < itemCount - 1)
                    ImGui.SetCursorPosY((btnSize + spacing) * ++currentItemNumber);
            }

            if (gizmo.EnabledTools.Contains(GizmoTool.Translate))
            {
                if (gizmo.Tool == GizmoTool.Translate)
                    ImGui.PushStyleColor(ImGuiCol.Button, selectedColor);
                if (ImGui.Button($"{FontAwesome6.UpDownLeftRight}##Translate", new(btnSize)))
                    targetTool = GizmoTool.Translate;
                if (gizmo.Tool == GizmoTool.Translate)
                    ImGui.PopStyleColor();
                if (currentItemNumber < itemCount - 1)
                    ImGui.SetCursorPosY((btnSize + spacing) * ++currentItemNumber);
            }

            if (gizmo.EnabledTools.Contains(GizmoTool.Rotate))
            {
                if (gizmo.Tool == GizmoTool.Rotate)
                    ImGui.PushStyleColor(ImGuiCol.Button, selectedColor);
                if (ImGui.Button($"{FontAwesome6.ArrowsRotate}##Rotate", new(btnSize)))
                    targetTool = GizmoTool.Rotate;
                if (gizmo.Tool == GizmoTool.Rotate)
                    ImGui.PopStyleColor();
                if (currentItemNumber < itemCount - 1)
                    ImGui.SetCursorPosY((btnSize + spacing) * ++currentItemNumber);
            }

            if (gizmo.EnabledTools.Contains(GizmoTool.Scale))
            {
                if (gizmo.Tool == GizmoTool.Scale)
                    ImGui.PushStyleColor(ImGuiCol.Button, selectedColor);
                if (ImGui.Button($"{FontAwesome6.UpRightAndDownLeftFromCenter}##Scale", new(btnSize)))
                    targetTool = GizmoTool.Scale;
                if (gizmo.Tool == GizmoTool.Scale)
                    ImGui.PopStyleColor();
                if (currentItemNumber < itemCount - 1)
                    ImGui.SetCursorPosY((btnSize + spacing) * ++currentItemNumber);
            }

            foreach (var extra in visibleExtraTools)
            {
                if (extra.IsSelected?.Invoke() ?? false)
                    ImGui.PushStyleColor(ImGuiCol.Button, selectedColor);
                if (ImGui.Button($"{extra.Icon}##{extra.Name}", new(btnSize)))
                    clickedExtraTool = extra.Name;
                if (extra.IsSelected?.Invoke() ?? false)
                    ImGui.PopStyleColor();
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
                    ImGui.TextUnformatted(extra.Name);
                    ImGui.PopTextWrapPos();
                    ImGui.EndTooltip();
                }

                if (currentItemNumber < itemCount - 1)
                    ImGui.SetCursorPosY((btnSize + spacing) * ++currentItemNumber);
            }
        }

        ImGui.EndChild();

        ImGui.PopStyleColor();
        ImGui.SetCursorPos(oldCursor);

        gizmo.Tool = targetTool;

        if (clickedExtraTool is not null)
        {
            var extraTool = extraTools?.FirstOrDefault(t => t.Name == clickedExtraTool);
            extraTool?.OnClick();
        }
    }
}