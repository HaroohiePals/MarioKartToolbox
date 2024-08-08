using HaroohiePals.Graphics;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;
using OpenTK.Mathematics;
using System.IO;

namespace HaroohiePals.NitroKart.MapObj.Common;

class ItemboxBoxRenderPart : RenderPart<Itembox>
{
    // temporary
    private static readonly Nsbmd _nsbmd;
    public MObjModel Model { get; private set; }

    private ItemboxBoxAnimationFrame[] _boxAnimation = new ItemboxBoxAnimationFrame[300];

    // temporary
    static ItemboxBoxRenderPart()
    {
        _nsbmd = new Nsbmd(File.ReadAllBytes("testmodels/box.nsbmd"));
    }

    public ItemboxBoxRenderPart(MkdsContext context) 
        : base(context, RenderPartType.Billboard, false, true)
    { }

    protected override void GlobalInit()
    {
        Model = MObjUtil.LoadBillboardModel(_context, this, _nsbmd);
        Model.BbModel.SetDefaultMatParams();
        Model.BbModel.SetEmission(new Rgb555(20, 20, 20));

        for (int i = 0; i < _boxAnimation.Length; i++)
        {
            ushort boxAngle = (ushort)i;
            boxAngle = (ushort)((boxAngle << 16) / _boxAnimation.Length);

            double cos = MObjUtil.CosIdx(boxAngle);
            double sin = MObjUtil.SinIdx(boxAngle);

            _boxAnimation[i] = new ItemboxBoxAnimationFrame(sin, cos, cos * cos, sin * sin, cos * sin);
        }

        //if (rconf_getCourse() == COURSE_RAINBOW_COURSE)
        if (_context.Course.MapData.StageInfo.CourseId == 44)
            Model.BbModel.PolygonAttr &= 0xFFFF7FFF;
    }

    protected override void GlobalPreRender()
    {
        Model.BbModel.ApplyMaterial();
    }

    protected override void Render(Itembox instance, in Matrix4x3d camMtx, ushort alpha)
    {
        Matrix4x3d boxMtx;

        if (instance.BoxAlpha >= 1)
        {
            if (instance.BoxAnimFunc is not null)
            {
                boxMtx = instance.BoxAnimFunc(instance.BoxAnimFuncArg);
                Model.BbModel.PolygonAttr.PolygonId = _context.MObjState.GetCyclicPolygonId();
            }
            else
            {
                var animEntry = _boxAnimation[instance.BoxFrameCounter];

                boxMtx = new Matrix4x3d();

                boxMtx[0, 0] = animEntry.Cos;
                boxMtx[0, 1] = animEntry.Sin;
                boxMtx[0, 2] = 0;

                boxMtx[1, 0] = -animEntry.Field10;
                boxMtx[1, 1] = animEntry.ScaleY;
                boxMtx[1, 2] = boxMtx[0, 1];

                boxMtx[2, 0] = animEntry.FieldC;
                boxMtx[2, 1] = boxMtx[1, 0];
                boxMtx[2, 2] = boxMtx[0, 0];

                boxMtx.Row3 = instance.RenderPos / 16.0;
                Model.BbModel.PolygonAttr.PolygonId = instance.PolygonId;
            }
            
            Model.BbModel.SetAlpha(instance.BoxAlpha);
            Model.BbModel.Render((byte)alpha, boxMtx, instance.Scale);
        }
    }
}
