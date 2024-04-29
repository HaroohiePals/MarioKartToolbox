namespace HaroohiePals.MarioKartToolbox.Application.Settings;

record struct KeyBindingSettings()
{
    public ShortcutKeyBindingSettings Shortcuts = new();
    public ViewportKeyBindingSettings Viewport = new();
    public GizmoKeyBindingSettings Gizmo = new();
}