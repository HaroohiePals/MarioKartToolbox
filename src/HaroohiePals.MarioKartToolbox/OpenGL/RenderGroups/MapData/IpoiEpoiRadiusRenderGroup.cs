using HaroohiePals.Graphics3d.OpenGL.Renderers;
using HaroohiePals.Gui.Viewport;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.MarioKartToolbox.OpenGL.Renderers;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups.MapData;

class IpoiEpoiRadiusRenderGroup<TPath, TPoint> : RenderGroup, IColoredRenderGroup, IDisposable
    where TPath : ConnectedPath<TPath, TPoint>, new()
    where TPoint : IMapDataEntry, IPoint
{
    private readonly MapDataCollection<TPath> _paths;

    private readonly MeshRenderer _renderer;

    public Color Color { get; set; }

    public IpoiEpoiRadiusRenderGroup(MapDataCollection<TPath> paths, Color color, IRendererFactory rendererFactory)
    {
        Color = color;
        _paths = paths;
        _renderer = rendererFactory.CreateCircleRenderer();
        _renderer.Offset2dY = -1f;
    }

    public void Dispose()
    {
        _renderer.Dispose();
    }

    public override void Render(ViewportContext context)
    {
        if (!context.TranslucentPass)
            return;

        int index = 0;

        foreach (var path in _paths)
        {
            //_renderer.IndexGroupId = index++;

            var points = new List<InstancedPoint>();

            foreach (var point in path.Points)
            {
                float radius = (float)(point is MkdsItemPoint ipoi ? ipoi.Radius :
                    point is MkdsEnemyPoint epoi ? epoi.Radius : 0);
                radius = radius / 10f;

                bool isSelected = context.IsSelected(point);
                bool isHovered = context.IsHovered(point);

                points.Add(new InstancedPoint((Vector3)point.Position, new(), new(radius),
                    Color, false, point, ViewportContext.InvalidPickingId, isHovered, isSelected));
            }

            _renderer.Points = points.ToArray();
            _renderer.Render(context);
        }
    }
}