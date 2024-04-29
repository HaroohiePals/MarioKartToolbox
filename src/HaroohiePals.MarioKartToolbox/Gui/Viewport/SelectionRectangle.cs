using HaroohiePals.Gui.View;
using ImGuiNET;
using OpenTK.Mathematics;
using System;

namespace HaroohiePals.MarioKartToolbox.Gui.Viewport
{
    public class SelectionRectangle : IView
    {
        private System.Numerics.Vector2 _pointA;
        private System.Numerics.Vector2 _pointB;

        public bool Dragging { get; private set; } = false;
        public Vector2i TopLeft { get; private set; }
        public Vector2i BottomRight { get; private set; }
        public Vector2i Size { get; private set; }

        public bool Draw()
        {
            if (!ImGui.IsWindowFocused())
                return false;

            if (ImGui.IsMouseDragging(ImGuiMouseButton.Left))
            {
                if (!Dragging)
                {
                    _pointA = ImGui.GetMousePos() - ImGui.GetWindowPos();
                }
                _pointB = ImGui.GetMousePos() - ImGui.GetWindowPos();

                Dragging = true;
            }

            if (Dragging)
            {
                var size = ImGui.GetWindowSize();

                TopLeft = new Vector2i((int)Math.Max(Math.Min(_pointA.X, _pointB.X), 0), (int)Math.Max(Math.Min(_pointA.Y, _pointB.Y), 0));
                BottomRight = new Vector2i((int)Math.Min(Math.Max(_pointA.X, _pointB.X), size.X), (int)Math.Min(Math.Max(_pointA.Y, _pointB.Y), size.Y));

                ImGui.SetCursorPosX(TopLeft.X);
                ImGui.SetCursorPosY(TopLeft.Y);

                Size = BottomRight - TopLeft;

                if (Size.X >= 1 && Size.Y >= 1)
                {
                    var color = ImGui.GetStyle().Colors[(int)ImGuiCol.TextSelectedBg];

                    color.W = 0.75f;
                    ImGui.PushStyleColor(ImGuiCol.Border, color);
                    color.W = 0.25f;
                    ImGui.PushStyleColor(ImGuiCol.FrameBg, color);
                    ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1);
                    ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 0);
                    if (ImGui.BeginChildFrame(ImGui.GetID("SelectionRect"), new System.Numerics.Vector2(Size.X, Size.Y)))
                    {
                        ImGui.EndChildFrame();
                    }
                    ImGui.PopStyleVar();
                    ImGui.PopStyleVar();
                    ImGui.PopStyleColor();
                    ImGui.PopStyleColor();
                }

                if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
                {
                    _pointA = new();
                    _pointB = new();

                    Dragging = false;
                    return true;
                }
            }

            if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
            {
                _pointA = new();
                _pointB = new();

                Dragging = false;
            }

            return false;
        }
    }
}
