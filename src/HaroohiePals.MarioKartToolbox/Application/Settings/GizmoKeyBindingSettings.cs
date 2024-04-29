using HaroohiePals.Gui.Input;
using HaroohiePals.Gui.Viewport;
using ImGuiNET;

namespace HaroohiePals.MarioKartToolbox.Application.Settings;

record struct GizmoKeyBindingSettings()
{
    public KeyBinding ToolsDraw = new(ImGuiKey.D);
    public KeyBinding ToolsTranslate = new(ImGuiKey.G);
    public KeyBinding ToolsRotate = new(ImGuiKey.R);
    public KeyBinding ToolsScale = new(ImGuiKey.S);
    public KeyBinding CancelOperation = new(ImGuiKey.Escape);
    public KeyBinding SnapToCollision = new(ImGuiKey.C);

    public KeyBinding ToolsAxisConstraintX = new(ImGuiKey.X);
    public KeyBinding ToolsAxisConstraintY = new(ImGuiKey.Y);
    public KeyBinding ToolsAxisConstraintZ = new(ImGuiKey.Z);

    public KeyBinding ToolsAxisConstraintXY = new(ImGuiKey.X, false, true);
    public KeyBinding ToolsAxisConstraintYZ = new(ImGuiKey.Y, false, true);
    public KeyBinding ToolsAxisConstraintXZ = new(ImGuiKey.Z, false, true);

    public readonly GizmoKeyBindings ToGizmoKeyBindings() => new()
    {
        ToolsDraw = ToolsDraw,
        ToolsRotate = ToolsRotate,
        ToolsScale = ToolsScale,
        ToolsTranslate = ToolsTranslate,
        CancelOperation = CancelOperation,
        SnapToCollision = SnapToCollision,
        ToolsAxisConstraintX = ToolsAxisConstraintX,
        ToolsAxisConstraintY = ToolsAxisConstraintY,
        ToolsAxisConstraintZ = ToolsAxisConstraintZ,
        ToolsAxisConstraintXY = ToolsAxisConstraintXY,
        ToolsAxisConstraintYZ = ToolsAxisConstraintYZ,
        ToolsAxisConstraintXZ = ToolsAxisConstraintXZ
    };
}