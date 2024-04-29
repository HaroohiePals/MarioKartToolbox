using ImGuiNET;

namespace HaroohiePals.Gui.Input
{
    public record struct KeyBinding(ImGuiKey Key, bool Ctrl = false, bool Shift = false, bool Alt = false)
    {
        public bool IsDown()
        {
            var io = ImGui.GetIO();
            return ImGui.IsKeyDown(Key) && (!Ctrl || io.KeyCtrl) && (!Shift || io.KeyShift) && (!Alt || io.KeyAlt);
        }

        public bool IsPressed()
        {
            var io = ImGui.GetIO();
            return ImGui.IsKeyPressed(Key) && (!Ctrl || io.KeyCtrl) && (!Shift || io.KeyShift) && (!Alt || io.KeyAlt);
        }

        public override string ToString()
        {
            string keyBinding = "";

            keyBinding += Ctrl ? "Ctrl+" : "";
            keyBinding += Alt ? "Alt+" : "";
            keyBinding += Shift ? "Shift+" : "";
            
            string keyString = Key == ImGuiKey.Tab ? "Tab" : Key.ToString();
            if (keyString.StartsWith("_"))
                keyString = keyString.Remove(0, 1);

            keyBinding += keyString;

            return keyBinding;
        }
    }
}
