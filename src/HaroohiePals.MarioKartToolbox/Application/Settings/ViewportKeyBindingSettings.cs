using HaroohiePals.Gui.Input;
using HaroohiePals.Gui.Viewport;
using ImGuiNET;

namespace HaroohiePals.MarioKartToolbox.Application.Settings;

record struct ViewportKeyBindingSettings()
{
    public KeyBinding Forward = new(ImGuiKey.W);
    public KeyBinding Left = new(ImGuiKey.A);
    public KeyBinding Backward = new(ImGuiKey.S);
    public KeyBinding Right = new(ImGuiKey.D);
    public KeyBinding Up = new(ImGuiKey.Q);
    public KeyBinding Down = new(ImGuiKey.E);

    public readonly ViewportKeyBindings ToViewportKeyBindings() => new()
    {
        Forward = Forward,
        Left = Left,
        Backward = Backward,
        Right = Right,
        Up = Up,
        Down = Down
    };
}