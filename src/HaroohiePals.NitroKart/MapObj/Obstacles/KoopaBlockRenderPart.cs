using HaroohiePals.Graphics;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;
using HaroohiePals.NitroKart.MapObj.Obstacles;
using HaroohiePals.NitroKart.Resources;
using OpenTK.Mathematics;
using System.IO;

namespace HaroohiePals.NitroKart.MapObj.Obstacles;

class KoopaBlockRenderPart : RenderPart<KoopaBlock>
{
    private Model _koopaBlockModel;

    public KoopaBlockRenderPart(MkdsContext context)
        : base(context, RenderPartType.Normal) { }
        
    protected override void GlobalInit()
    {
        var nsbmd = MObjUtil.GetMapObjFile<Nsbmd>(_context, "koopa_block.nsbmd");
        _koopaBlockModel = new Model(_context, nsbmd);
        _koopaBlockModel.SetPolyIdLightFlagsEmi(63, 1 << 0, new Rgb555(10, 10, 10));
        foreach (var instance in _instances)
            instance.Model = _koopaBlockModel;
    }

    protected override void Render(KoopaBlock instance, in Matrix4x3d camMtx, ushort alpha)
    {
        instance.Mtx.Row3 = instance.Position / 16.0;
        MObjUtil.Model2RenderModel(_context, instance.Model, instance.Mtx, instance.Scale, (byte)alpha);
    }
}
