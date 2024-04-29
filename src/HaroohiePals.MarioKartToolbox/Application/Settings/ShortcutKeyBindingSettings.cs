using HaroohiePals.Gui.Input;
using ImGuiNET;

namespace HaroohiePals.MarioKartToolbox.Application.Settings;

record struct ShortcutKeyBindingSettings()
{
    public KeyBinding Undo = new(ImGuiKey.Z, true);
    public KeyBinding Redo = new(ImGuiKey.Y, true);

    public KeyBinding Copy = new(ImGuiKey.C, true);
    public KeyBinding Cut = new(ImGuiKey.X, true);
    public KeyBinding Paste = new(ImGuiKey.V, true);

    public KeyBinding Insert = new(ImGuiKey.I, true);
    public KeyBinding Delete = new(ImGuiKey.Delete);

    public KeyBinding SelectAll = new(ImGuiKey.A, true);
}