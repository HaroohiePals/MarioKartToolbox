using HaroohiePals.Graphics3d.OpenGL.Renderers;
using HaroohiePals.Gui.Viewport;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using OpenTK.Mathematics;
using System;
using System.Drawing;

namespace HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups.MapData
{
    public class MepoLineRenderGroup : RenderGroup, IColoredRenderGroup, IDisposable
    {
        private readonly MapDataCollection<MkdsMgEnemyPath> _paths;

        private readonly LineRenderer _lineRenderer;
        private bool _render2d = false;

        public Color Color { get; set; }

        public MepoLineRenderGroup(MapDataCollection<MkdsMgEnemyPath> paths, Color color, bool render2d = false)
        {
            Color = color;
            _paths = paths;
            _lineRenderer = new();
            _render2d = render2d;
        }

        public override void Render(ViewportContext context)
        {
            if (context.TranslucentPass)
                return;

            _lineRenderer.Thickness = 2;
            _lineRenderer.Loop = false;
            _lineRenderer.Color = Color;
            _lineRenderer.Render2d = _render2d;

            for (int i = 0; i < _paths.Count; i++)
            {
                var path = _paths[i];

                if (path.Points.Count == 0)
                    continue;

                var points = new Vector3[path.Points.Count];
                for (int j = 0; j < path.Points.Count; j++)
                    points[j] = (Vector3)path.Points[j].Position;
                _lineRenderer.Points = points;
                _lineRenderer.PickingId = ViewportContext.InvalidPickingId; //ViewportContext.GetPickingId(PickingGroupId, i);
                _lineRenderer.Render(context.ViewMatrix, context.ProjectionMatrix, context.TranslucentPass, context.ViewportSize);

                points = new Vector3[2];
                points[0] = (Vector3)path.Points[^1].Position;
                _lineRenderer.PickingId = ViewportContext.InvalidPickingId;
                foreach (var next in path.Next)
                {
                    if (next == null || next.Target == null)
                        continue;
                    points[1] = (Vector3)next.Target.Position;
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