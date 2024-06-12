#nullable enable

using HaroohiePals.Gui.View.Menu;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace HaroohiePals.Gui;

public record struct ImGuiGameWindowSettings(string Title, Vector2i Size, float UiScale, IReadOnlyCollection<ImGuiIconGlyph> IconGlyphs)
{
    public static readonly ImGuiGameWindowSettings Default = new("ImGui App", new(800, 600), 1f, []);
}