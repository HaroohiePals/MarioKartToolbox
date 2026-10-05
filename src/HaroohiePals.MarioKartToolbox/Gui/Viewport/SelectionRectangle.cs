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
        private bool _clickedInside;

        public bool Dragging { get; private set; } = false;
        public Vector2i TopLeft { get; private set; }
        public Vector2i BottomRight { get; private set; }
        public Vector2i Size { get; private set; }

        public bool Draw()
        {
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                _clickedInside = ImGui.IsWindowHovered();

            if (!ImGui.IsWindowFocused())
                return false;

            if (ImGui.IsMouseDragging(ImGuiMouseButton.Left))
            {
                if (!_clickedInside)
                    return false;

                var windowPos = ImGui.GetWindowPos();
                if (!Dragging)
                    _pointA = ImGui.GetIO().MouseClickedPos[0] - windowPos;
                _pointB = ImGui.GetMousePos() - windowPos;

                Dragging = true;
            }

            if (Dragging)
            {
                var size = ImGui.GetWindowSize();

                TopLeft = new Vector2i((int)Math.Max(Math.Min(_pointA.X, _pointB.X), 0), (int)Math.Max(Math.Min(_pointA.Y, _pointB.Y), 0));
                BottomRight = new Vector2i((int)Math.Min(Math.Max(_pointA.X, _pointB.X), size.X), (int)Math.Min(Math.Max(_pointA.Y, _pointB.Y), size.Y));

                Size = BottomRight - TopLeft;

                if (Size.X >= 1 && Size.Y >= 1)
                {
                    var color = ImGui.GetStyle().Colors[(int)ImGuiCol.TextSelectedBg];
                    var windowPos = ImGui.GetWindowPos();
                    var min = windowPos + new System.Numerics.Vector2(TopLeft.X, TopLeft.Y);
                    var max = windowPos + new System.Numerics.Vector2(BottomRight.X, BottomRight.Y);
                    var drawList = ImGui.GetWindowDrawList();

                    color.W = 0.25f;
                    drawList.AddRectFilled(min, max, ImGui.GetColorU32(color));
                    color.W = 0.75f;
                    drawList.AddRect(min, max, ImGui.GetColorU32(color));
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
