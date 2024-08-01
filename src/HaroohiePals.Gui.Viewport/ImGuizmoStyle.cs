namespace HaroohiePals.Gui.Viewport;

public class ImGuizmoStyle
{
    /// <summary>
    /// Colors of the various controls.
    /// </summary>
    public ImGuizmoColors Colors = new();
    /// <summary>
    /// Thickness of lines for translation gizmo
    /// </summary>
    public float TranslationLineThickness = 3f;
    /// <summary>
    /// Size of arrow at the end of lines for translation gizmo
    /// </summary>
    public float TranslationLineArrowSize = 6f;
    /// <summary>
    /// Thickness of lines for rotation gizmo
    /// </summary>
    public float RotationLineThickness = 2f;
    /// <summary>
    /// Thickness of line surrounding the rotation gizmo
    /// </summary>
    public float RotationOuterLineThickness = 3f;
    /// <summary>
    /// Thickness of lines for scale gizmo
    /// </summary>
    public float ScaleLineThickness = 3f;
    /// <summary>
    /// Size of circle at the end of lines for scale gizmo
    /// </summary>
    public float ScaleLineCircleSize = 6f;
    /// <summary>
    /// Thickness of hatched axis lines
    /// </summary>
    public float HatchedAxisLineThickness = 6f;
    /// <summary>
    /// Size of circle at the center of the translate/scale gizmo
    /// </summary>
    public float CenterCircleSize = 6f;
    /// <summary>
    /// Size of the big circles of the local bounds gizmo
    /// </summary>
    public float LocalBoundsLineThickness = 2f;
    /// <summary>
    /// Border thickness of the circles of the local bounds gizmo
    /// </summary>
    public float LocalBoundsAnchorBorderLineThickness = 1.2f;
    /// <summary>
    /// Size of the big circles of the local bounds gizmo
    /// </summary>
    public float LocalBoundsAnchorBigRadius = 8f;
    /// <summary>
    /// Size of the small circles of the local bounds gizmo
    /// </summary>
    public float LocalBoundsAnchorSmallRadius = 6f;
}
