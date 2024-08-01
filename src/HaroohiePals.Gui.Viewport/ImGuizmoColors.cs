using System.Drawing;

namespace HaroohiePals.Gui.Viewport;

public class ImGuizmoColors
{
    public Color DirectionX = Color.FromArgb(170, 0, 0);
    public Color DirectionY = Color.FromArgb(0, 170, 0);
    public Color DirectionZ = Color.FromArgb(0, 0, 170);
    public Color PlaneX = Color.FromArgb(97, 170, 0, 0);
    public Color PlaneY = Color.FromArgb(97, 0, 170, 0);
    public Color PlaneZ = Color.FromArgb(97, 0, 0, 170);
    public Color Selection = Color.FromArgb(140, 255, 128, 16);
    public Color Unselected = Color.White;
    public Color Inactive = Color.FromArgb(153, 153, 153, 153);
    public Color TranslationLine = Color.FromArgb(170, Color.DarkGray);
    public Color ScaleLine = Color.FromArgb(255, 64, 64, 64);
    public Color RotationUsingBorder = Color.FromArgb(255, 255, 128, 16);
    public Color RotationUsingFill = Color.FromArgb(127, 255, 128, 16);
    public Color HatchedAxisLines = Color.FromArgb(127, Color.Black);
    public Color Text = Color.White;
    public Color TextShadow = Color.Black;
    public Color LocalBounds = Color.DarkGray;
    public Color LocalBoundsDisabled = Color.FromArgb(127, Color.DarkGray);
    public Color LocalBoundsCircleBorder = Color.Black;
}
