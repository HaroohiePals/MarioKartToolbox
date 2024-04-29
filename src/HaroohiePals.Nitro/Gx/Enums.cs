using System.ComponentModel;

namespace HaroohiePals.Nitro.Gx
{
    public enum ImageFormat : uint
    {
        [Description("None")]
        None = 0,

        [Description("A3I5")]
        A3I5 = 1,

        [Description("Palette 4")]
        Pltt4 = 2,

        [Description("Palette 16")]
        Pltt16 = 3,

        [Description("Palette 256")]
        Pltt256 = 4,

        [Description("4x4")]
        Comp4x4 = 5,

        [Description("A5I3")]
        A5I3 = 6,

        [Description("Direct")]
        Direct = 7
    }

    public enum CharFormat : uint
    {
        Char,
        Bmp
    }

    public enum MapFormat : uint
    {
        Text,
        Affine
    }
}