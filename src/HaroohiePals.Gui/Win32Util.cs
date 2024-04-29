using System;
using System.Runtime.InteropServices;

namespace HaroohiePals.Gui;

static class Win32Util
{
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    public static extern void DwmSetWindowAttribute(IntPtr hWnd, int dwAttribute);

    [DllImport("uxtheme.dll", EntryPoint = "#132", CharSet = CharSet.Unicode)]
    public static extern bool ShouldAppUseDarkMode();

    [DllImport("uxtheme.dll", EntryPoint = "#138", CharSet = CharSet.Unicode)]
    public static extern bool ShouldSystemUseDarkMode();

    public static void ApplyImmersiveDarkModeOnWindow(IntPtr hWnd)
        => DwmSetWindowAttribute(hWnd, DWMWA_USE_IMMERSIVE_DARK_MODE);

}
