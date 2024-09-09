using HaroohiePals.Graphics;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;
using HaroohiePals.Nitro.NitroSystem.G3d.Intermediate.Model;
using HaroohiePals.NitroKart.Resources;
using OpenTK.Mathematics;
using System.IO;

namespace HaroohiePals.NitroKart.MapObj.Common;

class ItemboxQuestionRenderPart : RenderPart<Itembox>
{
    // temporary
    private static readonly Nsbmd _nsbmd;
    private MObjModel _model;

    private ItemboxQuestionAnimationFrame[] _questionAnimation = new ItemboxQuestionAnimationFrame[130];

    // temporary
    static ItemboxQuestionRenderPart()
    {
        var imd = new Imd(MapObjPlaceholderModels.Exclamation);
        _nsbmd = imd.ToNsbmd("question");
        _nsbmd.TextureSet = imd.ToNsbtx().TextureSet;
    }

    public ItemboxQuestionRenderPart(MkdsContext context)
        : base(context, RenderPartType.Billboard, false, false)
    {
    }

    protected override void GlobalInit()
    {
        _model = MObjUtil.LoadBillboardModel(_context, this, _nsbmd);
        _model.BbModel.SetDefaultMatParams();
        _model.BbModel.SetEmission(new Rgb555(20, 20, 20));

        for (int i = 0; i < _questionAnimation.Length; i++)
        {
            ushort questionAngle = (ushort)i;
            questionAngle = (ushort)((questionAngle << 16) / _questionAnimation.Length);

            double cos = MObjUtil.CosIdx(questionAngle);
            double sin = MObjUtil.SinIdx(questionAngle);

            _questionAnimation[i] = new ItemboxQuestionAnimationFrame(sin, cos);
        }

        //if (rconf_getCourse() == COURSE_RAINBOW_COURSE)
        if (_context.Course.MapData.StageInfo.CourseId == 44)
            _model.BbModel.PolygonAttr &= 0xFFFF7FFF;
    }

    protected override void GlobalPreRender()
    {
        _model.BbModel.ApplyMaterial();
    }

    protected override void Render(Itembox instance, in Matrix4x3d camMtx, ushort alpha)
    {
        instance.PolygonId = _context.MObjState.GetCyclicPolygonId();

        if (instance.BoxAnimFunc is not null || instance.QuestionAlpha <= 1)
            return;

        var animEntry = _questionAnimation[instance.QuestionFrameCounter];

        var questionMtx = new Matrix4x3d();

        questionMtx[0, 0] = animEntry.Cos;
        questionMtx[0, 1] = 0;
        questionMtx[0, 2] = -animEntry.Sin;

        questionMtx[1, 0] = 0;
        questionMtx[1, 1] = 1;
        questionMtx[1, 2] = 0;

        questionMtx[2, 0] = -questionMtx[0, 2];
        questionMtx[2, 1] = 0;
        questionMtx[2, 2] = questionMtx[0, 0];

        questionMtx.Row3 = instance.RenderPos / 16.0;
        _model.BbModel.SetAlpha((byte)instance.QuestionAlpha);
        _model.BbModel.PolygonAttr.PolygonId = instance.PolygonId;
        _model.BbModel.Render((byte)alpha, questionMtx, instance.Scale);
    }
}
