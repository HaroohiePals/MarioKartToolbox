using HaroohiePals.Actions;
using OpenTK.Mathematics;

namespace HaroohiePals.Gui.Viewport;

public class ViewportContext
{
    public const uint InvalidPickingId = ~0u;

    public bool ForceCustomProjectionMatrix = false;
    public Matrix4 CustomProjectionMatrix { get; set; } = Matrix4.Identity;

    public Matrix4 ProjectionMatrix { get; set; } = Matrix4.Identity;
    public Matrix4 ViewMatrix       { get; set; } = Matrix4.Identity;

    public Vector2i ViewportSize { get; set; }

    public bool TranslucentPass { get; set; }

    public ActionStack       ActionStack       { get; set; }
    public SceneObjectHolder SceneObjectHolder { get; set; } = new();
    public SelectionHandle   HoverObject       { get; set; }

    public PickingResult PickingResult { get; set; }

    public bool IsSelected(object obj, int subIndex = -1)
        => SceneObjectHolder.IsSubIndexSelected(obj, subIndex);

    public bool IsHovered(object obj, int subIndex = -1)
        => HoverObject != null && HoverObject.Object == obj && (subIndex == -1 || HoverObject.SubIndex == subIndex);

    //Add 1 to the Group ID to fix an issue where Alpha = 0 pixels would be discarded on some GPUs
    public static uint GetPickingId(int groupId, int index, int subIndex = -1)
        => ((uint)(groupId + 1) & 0xFF) << 24 | ((uint)(subIndex + 1) & 0x1F) << 19 | (uint)index & 0x7FFFF;
}