using HaroohiePals.Gui;
using System.Collections.Generic;

namespace HaroohiePals.MarioKartToolbox.Gui;

static class IconConsts
{
    public static IReadOnlyList<ImGuiIconGlyph> Icons =
    [
        new ImGuiIconGlyph(Resources.Icons.Model3D_16x, FileTypes.Model, 16),
        new ImGuiIconGlyph(Resources.Icons.TrafficCone_16x, FileTypes.MapData, 16),
        new ImGuiIconGlyph(Resources.Icons.ImageStack_16x, FileTypes.Animation, 16),
    ];

    public static IReadOnlyDictionary<string, char> FileExtIcons = new Dictionary<string, char>
    {
        { ".nkm", FileTypes.MapData },
        { ".nsbmd", FileTypes.Model },
        { ".nsbtp", FileTypes.Animation },
    };

    public static class FileTypes
    {
        public static char MapData = FontAwesome6.Map[0];
        public static char Model = FontAwesome6.Cube[0];
        public static char Animation = FontAwesome6.Images[0];
    }
}
