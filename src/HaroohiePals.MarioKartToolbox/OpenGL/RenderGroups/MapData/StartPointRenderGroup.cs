using HaroohiePals.Graphics3d.OpenGL.Renderers;
using HaroohiePals.Gui.Viewport;
using HaroohiePals.MarioKartToolbox.OpenGL.Renderers;
using HaroohiePals.NitroKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using HaroohiePals.NitroKart.Race;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups.MapData;

internal class StartPointRenderGroup : KartPointRenderGroup<MkdsStartPoint>
{
    private RaceConfig _raceConfig;
    private MkdsMapData _mapData;
    private const int _driverCount = 8;

    public StartPointRenderGroup(MkdsMapData mapData, Color color, bool render2d,
        IRendererFactory rendererFactory)
        : base(mapData.StartPoints, color, render2d, rendererFactory)
    {
        _mapData = mapData;
        _raceConfig = new RaceConfig(_driverCount, _mapData.IsMgStage ? RaceMode.MiniGame : RaceMode.Versus, RaceDisplayMode.Default);
    }

    private IEnumerable<InstancedPoint> _lastPoints = null;

    protected sealed override InstancedPoint[] GetPoints(ViewportContext context)
    {
        if (_mapData.StartPoints?.Count() == 0)
            return new InstancedPoint[0];

        try
        {
            var points = new List<InstancedPoint>();

            for (int i = 0; i < _driverCount; i++)
            {
                var ktps = MkdsMapDataUtil.GetStartPosition(_mapData, _raceConfig, i, out var calcPos, out var calcRot);

                uint pickingId = MktbRendererUtil.GetPickingId(i, PickingGroupId);
                bool isSelected = context.IsSelected(ktps);
                bool isHovered = context.IsHovered(ktps);

                points.Add(new InstancedPoint((Vector3)calcPos, (Vector3)calcRot,
                    new(16f / 10f), Color, false, ktps, pickingId, isHovered, isSelected));
            }

            _lastPoints = points;
            return points.ToArray();
        }
        catch
        {
            _lastPoints = null;
            return new InstancedPoint[0];
        }
    }

    public override object GetObject(int index) => _lastPoints?.ElementAt(index).Source ?? null;
}