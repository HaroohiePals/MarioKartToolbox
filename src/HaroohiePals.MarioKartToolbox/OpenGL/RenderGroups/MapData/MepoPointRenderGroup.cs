using HaroohiePals.Gui.Viewport;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.MarioKartToolbox.OpenGL.Renderers;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using System;
using System.Drawing;
using System.Linq;

namespace HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups.MapData
{
    internal class MepoPointRenderGroup : MapDataEntryRenderGroup, IDisposable
    {
        private const int PathIdShift = 13;
        private const int PointIdMask = 0x1FFF;

        private MapDataCollection<MkdsMgEnemyPath> _paths;

        public MepoPointRenderGroup(MapDataCollection<MkdsMgEnemyPath> paths, Color color, bool render2d, IRendererFactory rendererFactory)
            : base(color, render2d, rendererFactory)
        {
            _paths = paths;
        }

        public override void Render(ViewportContext context)
        {
            int index = 0;

            foreach (var path in _paths)
            {
                _renderer.Points = MktbRendererUtil.GetMapDataPoints(path.Points, Color, context, PickingGroupId, PathIdShift, PointIdMask, index++);
                _renderer.Render(context);
            }
        }

        public override object GetObject(int index) => _paths[index >> PathIdShift].Points[index & PointIdMask];
        public override bool ContainsObject(object obj) => obj is MkdsMgEnemyPoint instance && _paths.Any(x => x.Points.Contains(instance));
    }
}
