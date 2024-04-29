using HaroohiePals.Graphics3d.OpenGL.Renderers;
using HaroohiePals.Gui.Viewport;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using OpenTK.Mathematics;
using System;
using System.Drawing;

namespace HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups.MapData
{
    public class PathRenderGroup : RenderGroup, IColoredRenderGroup, IDisposable
    {
        private readonly MapDataCollection<MkdsPath> _paths;
        private readonly LineRenderer _lineRenderer;
        private bool _disableDepthTest = false;

        public Color Color { get; set; }

        public PathRenderGroup(MapDataCollection<MkdsPath> paths, Color color, bool disableDepthTest = false)
        {
            Color = color;
            _paths = paths;
            _lineRenderer = new();
            _disableDepthTest = disableDepthTest;
        }

        public override void Render(ViewportContext context)
        {
            if (context.TranslucentPass)
                return;

            _lineRenderer.Thickness = 2;
            _lineRenderer.Color = Color;
            _lineRenderer.Render2d = _disableDepthTest;

            for (int i = 0; i < _paths.Count; i++)
            {
                var path = _paths[i];
                var points = new Vector3[path.Points.Count];
                for (int j = 0; j < path.Points.Count; j++)
                    points[j] = (Vector3)path.Points[j].Position;
                _lineRenderer.Points = points;
                _lineRenderer.Loop = path.Loop;
                _lineRenderer.PickingId = ViewportContext.InvalidPickingId;// ViewportContext.GetPickingId(PickingGroupId, i);
                _lineRenderer.Render(context.ViewMatrix, context.ProjectionMatrix, context.TranslucentPass, context.ViewportSize);
            }
        }

        public override object GetObject(int index) => _paths[index];

        public void Dispose()
        {
            _lineRenderer.Dispose();
        }
    }
}