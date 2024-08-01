using HaroohiePals.Graphics3d.OpenGL.Renderers;
using HaroohiePals.Gui.Viewport;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.MarioKartToolbox.OpenGL.Renderers;
using HaroohiePals.Mathematics;
using HaroohiePals.NitroKart.Extensions;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using System;
using System.Drawing;

namespace HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups.MapData;

abstract class MapDataEntryRenderGroup : RenderGroup, IColoredRenderGroup, IDisposable
{
    protected bool _render2d = false;

    protected readonly InstancedPointRenderer _renderer;

    public Color Color { get; set; }

    public MapDataEntryRenderGroup(Color color, bool render2d, IRendererFactory rendererFactory)
    {
        Color = color;
        _render2d = render2d;
        if (_render2d)
            _renderer = new DotRenderer();
        else
            _renderer = rendererFactory.CreateBoxRenderer();
    }

    public void Dispose()
    {
        _renderer?.Dispose();
    }

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

        if (obj is MkdsMapObject mapObject)
            transform.Scale = mapObject.Scale;

        if (obj is MkdsArea area)
            transform = area.GetTransform();

        return true;
    }

    public override bool TrySetObjectTransform(object obj, int subIndex, in Transform transform)
    {
        if (subIndex != -1 || obj is not IPoint point)
            return false;

        point.Position = transform.Translation;

        if (obj is IRotatedPoint rotPoint)
            rotPoint.Rotation = transform.Rotation;

        if (obj is MkdsMapObject mapObject)
            mapObject.Scale = transform.Scale;

        if (obj is MkdsArea area)
            area.SetTransform(transform);

        return true;
    }
}