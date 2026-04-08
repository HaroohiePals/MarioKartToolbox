#nullable enable
using System.ComponentModel;
using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.Course;

public class MkdsLocalMapSettings
{
    // hardcoded cross_course values
    [Category("Local Map Coordinates")]
    [DisplayName("Top Left")]
    public Vector2d TopLeft { get; set; } = new(-6000, -3002);
    [Category("Local Map Coordinates")]
    [DisplayName("Bottom Right")]
    public Vector2d BottomRight { get; set; } = new(0, 2998);
    [Category("Extended Local Map Coordinates")]
    [DisplayName("Top Left")]
    public Vector2d ExtendedTopLeft { get; set; } = new(0, -3002);
    [Category("Extended Local Map Coordinates")]
    [DisplayName("Bottom Right")]
    public Vector2d ExtendedBottomRight { get; set; } = new(6000, 2998);
    public MkdsLocalMapMode Mode { get; set; } = MkdsLocalMapMode.Extended;
}
