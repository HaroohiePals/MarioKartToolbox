using HaroohiePals.Gui.Viewport;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.MarioKartToolbox.OpenGL.Renderers;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using System.Drawing;
using System.Linq;

namespace HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups.MapData
{
    internal class PathPointRenderGroup : MapDataEntryRenderGroup
    {
        private const int PathIdShift = 13;
        private const int PointIdMask = 0x1FFF;

        private readonly MapDataCollection<MkdsPath> _paths;

        public PathPointRenderGroup(MapDataCollection<MkdsPath> paths, Color color, bool render2d, IRendererFactory rendererFactory)
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
        public override bool ContainsObject(object obj) => obj is MkdsPathPoint instance && _paths.Any(x => x.Points.Contains(instance));
    }
}
