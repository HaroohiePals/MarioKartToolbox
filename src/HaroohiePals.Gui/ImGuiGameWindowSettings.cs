#nullable enable

using OpenTK.Mathematics;
using System.Collections.Generic;

namespace HaroohiePals.Gui;

public sealed record ImGuiGameWindowSettings(
    string Title, 
    Vector2i Size, 
    float UiScale,
    bool IgnoreDpiScaling,
    IReadOnlyCollection<ImGuiIconGlyph> IconGlyphs, 
    IReadOnlyCollection<ImGuiFont> Fonts, 
    ImGuiFont? IconFont = null)
{
    public static readonly ImGuiGameWindowSettings Default = new("ImGui App", 
        new(800, 600), 1f, false, [], [ImGuiFont.Default], ImGuiFont.DefaultIconFont);
}