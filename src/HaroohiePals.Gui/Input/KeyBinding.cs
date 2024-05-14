using ImGuiNET;
using System;
using System.ComponentModel;

namespace HaroohiePals.Gui.Input
{
    public record struct KeyBinding(ImGuiKey Key, bool Ctrl = false, bool Shift = false, bool Alt = false)
    {
        private const bool EXCLUSIVE_DEFAULT = true;

        /// <summary>
        /// Checks if the bound keys are currently held down.
        /// </summary>
        /// <param name="exclusive">
        /// If set to <see langword="true"/>, it checks for an exact match with the current input, including modifiers (Ctrl, Shift, Alt).
        /// </param>
        /// <returns> <see langword="true"/> if the key binding keys are currently held down; otherwise, <see langword="false"/>.</returns>
        public bool IsDown(bool exclusive = EXCLUSIVE_DEFAULT)
        {
            var io = ImGui.GetIO();

            if (exclusive)
                return ImGui.IsKeyDown(Key) && Ctrl == io.KeyCtrl && Shift == io.KeyShift && Alt == io.KeyAlt;

            return ImGui.IsKeyDown(Key) && (!Ctrl || io.KeyCtrl) && (!Shift || io.KeyShift) && (!Alt || io.KeyAlt);
        }

        /// <summary>
        /// Checks if the bound keys have been pressed.
        /// </summary>
        /// <param name="exclusive">
        /// If set to <see langword="true"/>, it checks for an exact match with the current input, including modifiers (Ctrl, Shift, Alt).
        /// </param>
        /// <returns><see langword="true"/> if the key binding keys have been pressed; otherwise, <see langword="false"/>.</returns>
        public bool IsPressed(bool exclusive = EXCLUSIVE_DEFAULT)
        {
            var io = ImGui.GetIO();

            if (exclusive)
                return ImGui.IsKeyPressed(Key, false) && Ctrl == io.KeyCtrl && Shift == io.KeyShift && Alt == io.KeyAlt;

            return ImGui.IsKeyPressed(Key, false) && (!Ctrl || io.KeyCtrl) && (!Shift || io.KeyShift) && (!Alt || io.KeyAlt);
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
