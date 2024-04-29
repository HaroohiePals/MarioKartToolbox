using HaroohiePals.Graphics3d.OpenGL.Renderers;
using HaroohiePals.Gui.Viewport;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using OpenTK.Mathematics;
using System;
using System.Drawing;

namespace HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups.MapData
{
    internal class CheckPointPathRenderGroup : RenderGroup, IColoredRenderGroup, IDisposable
    {
        private readonly MapDataCollection<MkdsCheckPointPath> _paths;
        private readonly LineRenderer _lineRenderer;

        public Color Color { get; set; }

        public CheckPointPathRenderGroup(MapDataCollection<MkdsCheckPointPath> paths, Color color)
        {
            Color = color;
            _paths = paths;
            _lineRenderer = new();
            _lineRenderer.Offset2dY = -0.25f;
        }

        public override void Render(ViewportContext context)
        {
            if (context.TranslucentPass)
                return;

            _lineRenderer.Thickness = 2;
            _lineRenderer.Loop = false;
            _lineRenderer.Render2d = true;
            _lineRenderer.Color = Color;

            for (int i = 0; i < _paths.Count; i++)
            {
                var path = _paths[i];

                if (path.Points.Count == 0)
                    continue;

                _lineRenderer.PickingId = ViewportContext.InvalidPickingId; //ViewportContext.GetPickingId(PickingGroupId, i);

                var points = new Vector3[path.Points.Count];
                for (int j = 0; j < path.Points.Count; j++)
                    points[j] = new Vector3((float)path.Points[j].Point1.X, 0, (float)path.Points[j].Point1.Y);

                _lineRenderer.Points = points;
                _lineRenderer.Render(context.ViewMatrix, context.ProjectionMatrix, context.TranslucentPass, context.ViewportSize);

                for (int j = 0; j < path.Points.Count; j++)
                    points[j] = new Vector3((float)path.Points[j].Point2.X, 0, (float)path.Points[j].Point2.Y);

                _lineRenderer.Points = points;
                _lineRenderer.Render(context.ViewMatrix, context.ProjectionMatrix, context.TranslucentPass, context.ViewportSize);

                points = new Vector3[2];
                points[0] = new Vector3((float)path.Points[^1].Point1.X, 0, (float)path.Points[^1].Point1.Y);
                _lineRenderer.PickingId = ViewportContext.InvalidPickingId;
                foreach (var next in path.Next)
                {
                    if (next == null || next.Target == null || next.Target.Points.Count == 0)
                        continue;
                    points[1] = new Vector3((float)next.Target.Points[0].Point1.X, 0,
                        (float)next.Target.Points[0].Point1.Y);
                    _lineRenderer.Points = points;
                    _lineRenderer.Render(context.ViewMatrix, context.ProjectionMatrix, context.TranslucentPass, context.ViewportSize);
                }

                points[0] = new Vector3((float)path.Points[^1].Point2.X, 0, (float)path.Points[^1].Point2.Y);
                _lineRenderer.PickingId = ViewportContext.InvalidPickingId;
                foreach (var next in path.Next)
                {
                    if (next == null || next.Target == null || next.Target.Points.Count == 0)
                        continue;
                    points[1] = new Vector3((float)next.Target.Points[0].Point2.X, 0,
                        (float)next.Target.Points[0].Point2.Y);
                    _lineRenderer.Points = points;
                    _lineRenderer.Render(context.ViewMatrix, context.ProjectionMatrix, context.TranslucentPass, context.ViewportSize);
                }
            }
        }

        public override object GetObject(int index) => _paths[index];

        public void Dispose()
        {
            _lineRenderer.Dispose();
        }
    }
}