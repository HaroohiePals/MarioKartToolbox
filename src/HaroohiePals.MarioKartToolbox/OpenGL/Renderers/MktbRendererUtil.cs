using HaroohiePals.Graphics3d.OpenGL.Renderers;
using HaroohiePals.Gui.Viewport;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.Extensions;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Linq;

namespace HaroohiePals.MarioKartToolbox.OpenGL.Renderers;

internal static class MktbRendererUtil
{
    private const int DEFAULT_INDEX_GROUP_ID_SHIFT = 13;
    private const int DEFAULT_INDEX_MASK = 0x1FFF;
    private const int DEFAULT_INDEX_GROUP_ID = -1;
    private const int DEFAULT_SUB_INDEX = -1;

    public static uint GetPickingId(int i, int pickingGroupId, int indexGroupIdShift = DEFAULT_INDEX_GROUP_ID_SHIFT,
        int indexMask = DEFAULT_INDEX_MASK, int indexGroupId = DEFAULT_INDEX_GROUP_ID, int subIndex = DEFAULT_SUB_INDEX)
        => ViewportContext.GetPickingId(pickingGroupId, indexGroupId != -1 ? indexGroupId << indexGroupIdShift | i & indexMask : i & indexMask, subIndex);

    public static InstancedPoint[] GetMapDataPoints(IEnumerable<IPoint> points, Vector3d scale, Color4 color, ViewportContext context,
        int pickingGroupId, int indexGroupIdShift = DEFAULT_INDEX_GROUP_ID_SHIFT,
        int indexMask = DEFAULT_INDEX_MASK, int indexGroupId = DEFAULT_INDEX_GROUP_ID, int subIndex = DEFAULT_SUB_INDEX)
    {
        return points.Select((x, i) =>
        {
            uint pickingId = GetPickingId(i, pickingGroupId, indexGroupIdShift, indexMask, indexGroupId, subIndex);

            bool isHovered = context.IsHovered(x, subIndex);
            bool isSelected = context.IsSelected(x, subIndex);

            if (x is IRotatedPoint r)
                return new InstancedPoint((Vector3)x.Position, (Vector3)r.Rotation, (Vector3)scale, color, true, x, pickingId, isHovered, isSelected);

            if (x is MkdsArea a)
            {
                var transform = a.GetTransform();
                return new InstancedPoint((Vector3)transform.Translation, (Vector3)transform.Rotation, (Vector3)scale, color, true, x, pickingId, isHovered, isSelected);
            }

            return new InstancedPoint((Vector3)x.Position, new(), (Vector3)scale, color, false, x, pickingId, isHovered, isSelected);
        }).ToArray();
    }

    public static InstancedPoint[] GetMapDataPoints(IEnumerable<IPoint> points, Color4 color, ViewportContext context,
        int pickingGroupId, int indexGroupIdShift = 13, int indexMask = 0x1FFF, int indexGroupId = -1, int subIndex = -1)
        => GetMapDataPoints(points, Vector3.One, color, context, pickingGroupId, indexGroupIdShift, indexMask, indexGroupId, subIndex);

    public static void Render(this InstancedPointRenderer renderer, ViewportContext context)
        => renderer.Render(context.ViewMatrix, context.ProjectionMatrix, context.TranslucentPass);
}
