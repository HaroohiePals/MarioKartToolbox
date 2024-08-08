using HaroohiePals.Graphics;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;
using System.IO;

namespace HaroohiePals.NitroKart.MapObj.Common;

class ItemboxQuestionRenderPart : RenderPart<Itembox>
{
    public MObjModel Model { get; private set; }

    private ItemboxQuestionAnimationFrame[] _questionAnimation = new ItemboxQuestionAnimationFrame[130];

    public ItemboxQuestionRenderPart(MkdsContext context)
        : base(context, RenderPartType.Billboard, false, false)
    {
    }

    protected override void GlobalInit()
    {
        var nsbmd = new Nsbmd(File.ReadAllBytes("testmodels/question.nsbmd"));
        Model = MObjUtil.LoadBillboardModel(_context, this, nsbmd);
        Model.BbModel.SetDefaultMatParams();
        Model.BbModel.SetEmission(new Rgb555(20, 20, 20));

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
            Model.BbModel.PolygonAttr &= 0xFFFF7FFF;
    }
}
