using OpenTK.Mathematics;

namespace HaroohiePals.Gui.Viewport;

public struct ImGuizmoStyle
{
    public ImGuizmoStyle()
    {
        Colors = new Color4[]
        {
            //Colors[DIRECTION_X]
            new Color4(0.666f, 0.000f, 0.000f, 1.000f),
            //Colors[DIRECTION_Y]
            new Color4(0.000f, 0.666f, 0.000f, 1.000f),
            //Colors[DIRECTION_Z]
            new Color4(0.000f, 0.000f, 0.666f, 1.000f),
            //Colors[PLANE_X]
            new Color4(0.666f, 0.000f, 0.000f, 0.380f),
            //Colors[PLANE_Y]
            new Color4(0.000f, 0.666f, 0.000f, 0.380f),
            //Colors[PLANE_Z]
            new Color4(0.000f, 0.000f, 0.666f, 0.380f),
            //Colors[SELECTION]
            new Color4(1.000f, 0.500f, 0.062f, 0.541f),
            //Colors[INACTIVE]
            new Color4(0.600f, 0.600f, 0.600f, 0.600f),
            //Colors[TRANSLATION_LINE]
            new Color4(0.666f, 0.666f, 0.666f, 0.666f),
            //Colors[SCALE_LINE]
            new Color4(0.250f, 0.250f, 0.250f, 1.000f),
            //Colors[ROTATION_USING_BORDER]
            new Color4(1.000f, 0.500f, 0.062f, 1.000f),
            //Colors[ROTATION_USING_FILL]
            new Color4(1.000f, 0.500f, 0.062f, 0.500f),
            //Colors[HATCHED_AXIS_LINES]
            new Color4(0.000f, 0.000f, 0.000f, 0.500f),
            //Colors[TEXT]
            new Color4(1.000f, 1.000f, 1.000f, 1.000f),
            //Colors[TEXT_SHADOW]
            new Color4(0.000f, 0.000f, 0.000f, 1.000f),
        };
    }

    public float TranslationLineThickness = 3.0f;     // Thickness of lines for translation gizmo
    public float TranslationLineArrowSize = 6.0f;       // Size of arrow at the end of lines for translation gizmo
    public float RotationLineThickness = 2.0f;       // Thickness of lines for rotation gizmo
    public float RotationOuterLineThickness = 3.0f;       // Thickness of line surrounding the rotation gizmo
    public float ScaleLineThickness = 3.0f;       // Thickness of lines for scale gizmo
    public float ScaleLineCircleSize = 6.0f;       // Size of circle at the end of lines for scale gizmo
    public float HatchedAxisLineThickness = 6.0f;       // Thickness of hatched axis lines
    public float CenterCircleSize = 6.0f;       // Size of circle at the center of the translate/scale gizmo

    public Color4[] Colors;
}
