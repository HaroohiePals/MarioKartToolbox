using System.ComponentModel;
using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.Course;

public class MkdsGlobalMapSettings
{
    [Category("Global Map Coordinates")]
    [DisplayName("Top Left")]
    public Vector2d TopLeft { get; set; } = new(-6000, -3000);
    [Category("Global Map Coordinates")]
    [DisplayName("Bottom Right")]
    public Vector2d BottomRight { get; set; } = new(0, 3000);
    public bool Rotate90Degrees { get; set; } = false;
}