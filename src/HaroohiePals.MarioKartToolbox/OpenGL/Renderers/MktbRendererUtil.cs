using HaroohiePals.Graphics3d.OpenGL.Renderers;
using HaroohiePals.Gui.Viewport;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.Extensions;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Linq;

namespace HaroohiePals.MarioKartToolbox.OpenGL.Renderers;

static class MktbRendererUtil
{
    private const int DEFAULT_INDEX_GROUP_ID_SHIFT = 13;
    private const int DEFAULT_INDEX_MASK = 0x1FFF;
    private const int DEFAULT_INDEX_GROUP_ID = -1;
    private const int DEFAULT_SUB_INDEX = -1;

    public static uint GetPickingId(ViewportContext context, int i, int pickingGroupId, 
        int indexGroupIdShift = DEFAULT_INDEX_GROUP_ID_SHIFT, int indexMask = DEFAULT_INDEX_MASK, 
        int indexGroupId = DEFAULT_INDEX_GROUP_ID, int subIndex = DEFAULT_SUB_INDEX)
        => context.GetPickingId(pickingGroupId, 
            indexGroupId != -1 ? indexGroupId << indexGroupIdShift | i & indexMask : i & indexMask, subIndex);

    public static InstancedPoint[] GetMapDataPoints(IEnumerable<IPoint> points, Vector3d scale, Color4 color, ViewportContext context,
        int pickingGroupId, int indexGroupIdShift = DEFAULT_INDEX_GROUP_ID_SHIFT,
        int indexMask = DEFAULT_INDEX_MASK, int indexGroupId = DEFAULT_INDEX_GROUP_ID, int subIndex = DEFAULT_SUB_INDEX)
    {
        return points.Select((point, i) =>
        {
            uint pickingId = GetPickingId(context, i, pickingGroupId, indexGroupIdShift, indexMask, indexGroupId, subIndex);

            bool isHovered = context.IsHovered(point, subIndex);
            bool isSelected = context.IsSelected(point, subIndex);

            switch (point)
            {
                case IRotatedPoint r:
                    return new InstancedPoint((Vector3)point.Position, (Vector3)r.Rotation, (Vector3)scale, 
                        color, true, point, pickingId, isHovered, isSelected);
                case MkdsArea a:
                {
                    var transform = a.GetTransform();
                    return new InstancedPoint((Vector3)transform.Translation, (Vector3)transform.Rotation, (Vector3)scale, 
                        color, true, point, pickingId, isHovered, isSelected);
                }
                default:
                    return new InstancedPoint((Vector3)point.Position, Vector3.Zero, (Vector3)scale, 
                        color, false, point, pickingId, isHovered, isSelected);
            }
        }).ToArray();
    }

    public static InstancedPoint[] GetMapDataPoints(IEnumerable<IPoint> points, Color4 color, ViewportContext context,
        int pickingGroupId, int indexGroupIdShift = DEFAULT_INDEX_GROUP_ID_SHIFT, int indexMask = DEFAULT_INDEX_MASK, 
        int indexGroupId = DEFAULT_INDEX_GROUP_ID, int subIndex = DEFAULT_SUB_INDEX)
        => GetMapDataPoints(points, Vector3.One, color, context, pickingGroupId, indexGroupIdShift, 
            indexMask, indexGroupId, subIndex);

    public static void Render(this InstancedPointRenderer renderer, ViewportContext context)
        => renderer.Render(context.ViewMatrix, context.ProjectionMatrix, context.TranslucentPass);
}
