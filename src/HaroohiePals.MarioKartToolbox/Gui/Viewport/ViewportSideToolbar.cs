using HaroohiePals.Gui;
using HaroohiePals.Gui.Viewport;
using ImGuiNET;
using System.Linq;
using System.Numerics;

namespace HaroohiePals.MarioKartToolbox.Gui.Viewport;

internal class ViewportSideToolbar
{
    public void Draw(ViewportContext context, Gizmo gizmo)
    {
        float scale = ImGuiEx.GetUiScale();

        var padding = new Vector2(8, 8) * scale;

        float btnSize = 24 * scale;
        float spacing = 2 * scale;

        int items = 4;

        uint selectedColor = ImGui.GetColorU32(ImGuiCol.ButtonHovered) | 0xFF000000;

        // Make child frame transparent
        ImGui.PushStyleColor(ImGuiCol.ChildBg, 0);
        var targetTool = gizmo.Tool;

        var oldCursor = ImGui.GetCursorPos();
        ImGui.SetCursorPos(padding);
        if (ImGui.BeginChild(ImGui.GetID("Tools"), new(btnSize + spacing, (btnSize + spacing) * items)))
        {
            ImGui.SetCursorPosX(0);
            ImGui.SetCursorPosY(0);

            int i = 1;

            if (gizmo.EnabledTools.Contains(GizmoTool.Draw))
            {
                if (gizmo.Tool == GizmoTool.Draw)
                    ImGui.PushStyleColor(ImGuiCol.Button, selectedColor);
                if (ImGui.Button($"{FontAwesome6.Pencil}##Draw", new(btnSize)))
                    targetTool = GizmoTool.Draw;
                if (gizmo.Tool == GizmoTool.Draw)
                    ImGui.PopStyleColor();

                ImGui.SetCursorPosY((btnSize + spacing) * i++);
            }

            if (gizmo.EnabledTools.Contains(GizmoTool.Translate))
            {
                if (gizmo.Tool == GizmoTool.Translate)
                    ImGui.PushStyleColor(ImGuiCol.Button, selectedColor);
                if (ImGui.Button($"{FontAwesome6.UpDownLeftRight}##Translate", new(btnSize)))
                    targetTool = GizmoTool.Translate;
                if (gizmo.Tool == GizmoTool.Translate)
                    ImGui.PopStyleColor();

                ImGui.SetCursorPosY((btnSize + spacing) * i++);
            }

            if (gizmo.EnabledTools.Contains(GizmoTool.Rotate))
            {
                if (gizmo.Tool == GizmoTool.Rotate)
                    ImGui.PushStyleColor(ImGuiCol.Button, selectedColor);
                if (ImGui.Button($"{FontAwesome6.ArrowsRotate}##Rotate", new(btnSize)))
                    targetTool = GizmoTool.Rotate;
                if (gizmo.Tool == GizmoTool.Rotate)
                    ImGui.PopStyleColor();
                ImGui.SetCursorPosY((btnSize + spacing) * i++);
            }

            if (gizmo.EnabledTools.Contains(GizmoTool.Scale))
            {
                if (gizmo.Tool == GizmoTool.Scale)
                    ImGui.PushStyleColor(ImGuiCol.Button, selectedColor);
                if (ImGui.Button($"{FontAwesome6.UpRightAndDownLeftFromCenter}##Scale", new(btnSize)))
                    targetTool = GizmoTool.Scale;
                if (gizmo.Tool == GizmoTool.Scale)
                    ImGui.PopStyleColor();
            }
        }
        ImGui.EndChild();

        ImGui.PopStyleColor();
        ImGui.SetCursorPos(oldCursor);

        gizmo.Tool = targetTool;
    }
}