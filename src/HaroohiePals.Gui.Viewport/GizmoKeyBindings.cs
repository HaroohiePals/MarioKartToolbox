using HaroohiePals.Gui.Input;
using ImGuiNET;

namespace HaroohiePals.Gui.Viewport
{
    public class GizmoKeyBindings
    {
        public KeyBinding ToolsDraw = new(ImGuiKey.D);
        public KeyBinding ToolsTranslate = new(ImGuiKey.G);
        public KeyBinding ToolsRotate = new(ImGuiKey.R);
        public KeyBinding ToolsScale = new(ImGuiKey.S);

        public KeyBinding SnapToCollision = new(ImGuiKey.C);

        public KeyBinding CancelOperation = new(ImGuiKey.Escape);

        public KeyBinding ToolsAxisConstraintX = new(ImGuiKey.X);
        public KeyBinding ToolsAxisConstraintY = new(ImGuiKey.Y);
        public KeyBinding ToolsAxisConstraintZ = new(ImGuiKey.Z);

        public KeyBinding ToolsAxisConstraintXY = new(ImGuiKey.X, false, true);
        public KeyBinding ToolsAxisConstraintYZ = new(ImGuiKey.Y, false, true);
        public KeyBinding ToolsAxisConstraintXZ = new(ImGuiKey.Z, false, true);
    }
}
