using HaroohiePals.Gui.Input;
using ImGuiNET;

namespace HaroohiePals.Gui.Viewport
{
    public class ViewportKeyBindings
    {
        public KeyBinding Forward = new KeyBinding(ImGuiKey.W);
        public KeyBinding Left = new KeyBinding(ImGuiKey.A);
        public KeyBinding Backward = new KeyBinding(ImGuiKey.S);
        public KeyBinding Right = new KeyBinding(ImGuiKey.D);
        public KeyBinding Up = new KeyBinding(ImGuiKey.Q);
        public KeyBinding Down = new KeyBinding(ImGuiKey.E);
    }
}