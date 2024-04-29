using HaroohiePals.Gui.Viewport;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.MarioKartToolbox.OpenGL.Renderers;
using System.Linq;
using Color = System.Drawing.Color;

namespace HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups.MapData;

class MapDataCollectionRenderGroup<T> : MapDataEntryRenderGroup where T : IMapDataEntry
{
    private readonly MapDataCollection<T> _collection;

    public MapDataCollectionRenderGroup(MapDataCollection<T> collection, Color color, bool render2d,
        IRendererFactory rendererFactory) : base(color, render2d, rendererFactory)
    {
        _collection = collection;
    }

    public override void Render(ViewportContext context)
    {
        _renderer.Points = MktbRendererUtil.GetMapDataPoints(_collection.Cast<IPoint>(), Color, context, PickingGroupId);
        _renderer.Render(context);
    }

    public override object GetObject(int index) => _collection[index & 0x1FFF];

    public override bool ContainsObject(object obj) => obj is T instance && _collection.Contains(instance);
}