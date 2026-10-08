using HaroohiePals.Graphics3d.OpenGL.Renderers;
using HaroohiePals.Gui.Viewport;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups.MapData;

public class MepoLineRenderGroup(MapDataCollection<MkdsMgEnemyPath> paths, Color color, bool render2d = false)
    : RenderGroup, IColoredRenderGroup, IDisposable
{
    private readonly LineRenderer _lineRenderer = new();

    public Color Color { get; set; } = color;

    public override void Render(ViewportContext context)
    {
        if (context.TranslucentPass)
            return;

        _lineRenderer.Thickness = 2;
        _lineRenderer.Loop = false;
        _lineRenderer.Color = Color;
        _lineRenderer.Render2d = render2d;
        _lineRenderer.PickingId = ViewportContext.InvalidPickingId;

        foreach (var path in paths.Where(p => p.Points.Count > 0))
        {
            var points = new List<Vector3>();

            var start = (Vector3)path.Points[0].Position;
            foreach (var prev in path.Previous.Where(prev => prev?.Target != null))
            {
                points.Add(start);
                points.Add((Vector3)prev.Target.Position);
            }

            points.AddRange(path.Points.Select(point => (Vector3)point.Position));

            var last = (Vector3)path.Points[^1].Position;
            foreach (var next in path.Next.Where(next => next?.Target != null))
            {
                points.Add((Vector3)next.Target.Position);
                points.Add(last);
            }

            _lineRenderer.Points = points.ToArray();
            _lineRenderer.Render(context.ViewMatrix, context.ProjectionMatrix, context.TranslucentPass,
                context.ViewportSize);
        }
    }

    public override object GetObject(int index) => paths[index];

    public void Dispose()
    {
        _lineRenderer.Dispose();
    }
}