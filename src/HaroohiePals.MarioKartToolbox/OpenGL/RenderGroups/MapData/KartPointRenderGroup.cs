using HaroohiePals.Graphics3d.OpenGL.Renderers;
using HaroohiePals.Gui.Viewport;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.MarioKartToolbox.OpenGL.Renderers;
using HaroohiePals.Mathematics;
using System;
using System.Drawing;
using System.Linq;

namespace HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups.MapData;

class KartPointRenderGroup<TPoint> : RenderGroup, IColoredRenderGroup, IDisposable
    where TPoint : IMapDataEntry, IRotatedPoint
{
    private readonly InstancedPointRenderer _renderer;
    private readonly MapDataCollection<TPoint> _collection;

    public KartPointRenderGroup(MapDataCollection<TPoint> collection, Color color, bool render2d, IRendererFactory rendererFactory)
    {
        Color = color;
        _collection = collection;
        _renderer = rendererFactory.CreateKartRenderer(render2d);
    }

    public Color Color { get; set; }

    protected virtual InstancedPoint[] GetPoints(ViewportContext context)
        => MktbRendererUtil.GetMapDataPoints(_collection.Cast<IPoint>(), new(16f / 10f), Color, context, PickingGroupId);

    public override void Render(ViewportContext context)
    {
        _renderer.Points = GetPoints(context);
        _renderer.Render(context);
    }

    public void Dispose()
    {
        _renderer?.Dispose();
    }

    public override object GetObject(int index) => _collection[index];

    public override bool TryGetObjectTransform(object obj, int subIndex, out Transform transform)
    {
        if (subIndex != -1 || obj is not IPoint point)
        {
            transform = new Transform(new(0), new(0), new(1));
            return false;
        }

        transform = new Transform(point.Position, new(0), new(1));

        if (point is IRotatedPoint rotPoint)
            transform.Rotation = rotPoint.Rotation;

        return true;
    }

    public override bool TrySetObjectTransform(object obj, int subIndex, in Transform transform)
    {
        if (subIndex != -1 || obj is not IPoint point)
            return false;

        point.Position = transform.Translation;

        if (obj is IRotatedPoint rotPoint)
            rotPoint.Rotation = transform.Rotation;

        return true;
    }
}
