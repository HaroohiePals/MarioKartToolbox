using HaroohiePals.Graphics;
using HaroohiePals.Nitro.G3;
using HaroohiePals.Nitro.NitroSystem.G3d;
using HaroohiePals.Nitro.NitroSystem.G3d.Animation;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Animation.TexturePatternAnimation;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;
using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.MapObj
{
    public class BillboardModel
    {
        private readonly MkdsContext _context;

        public double            PosScale;
        public uint              DiffAmb;
        public uint              SpeEmi;
        public GxPolygonAttr     PolygonAttr;
        public ushort            TexIdx;
        public Model             Model;
        public GxTexImageParam[] TexParamList;
        public MaterialTextureInfo[]      FrameTexInfos;
        public AnimManager       NsbtpAnim;

        public BillboardModel(MkdsContext context, Nsbmd nsbmd, Nsbtp nsbtp = null)
        {
            _context = context;

            Model = new Model(context, nsbmd);
            if (nsbtp != null)
                NsbtpAnim = MObjUtil.anim_20EA514(context, Model, nsbtp);

            var matData = Model.RenderObj.ModelResource.Materials.Materials[0];
            PosScale = Model.RenderObj.ModelResource.Info.PosScale;
            DiffAmb  = matData.DiffuseAmbient;
            SpeEmi   = matData.SpecularEmission;

            PolygonAttr           = matData.PolygonAttribute;
            PolygonAttr.LightMask = 0;

            TexIdx = 0;

            if (NsbtpAnim != null)
            {
                var    texImageParam = matData.TexImageParam;
                ushort frameCount    = (ushort)NsbtpAnim.GetAnimLength();
                TexParamList  = new GxTexImageParam[frameCount];
                FrameTexInfos = new MaterialTextureInfo[frameCount];
                for (int i = 0; i < frameCount; i++)
                {
                    NsbtpAnim.SetFrame(i);
                    var matAnmResult = new MaterialAnimationResult();
                    ((AnimateMaterialFunction)NsbtpAnim.AnmObjs[0].AnimationFunction)(matAnmResult, NsbtpAnim.AnmObjs[0], 0);
                    TexParamList[i]         = matAnmResult.PrmTexImage;
                    TexParamList[i].TexGen  = texImageParam.TexGen;
                    TexParamList[i].FlipS   = texImageParam.FlipS;
                    TexParamList[i].FlipT   = texImageParam.FlipT;
                    TexParamList[i].RepeatS = texImageParam.RepeatS;
                    TexParamList[i].RepeatT = texImageParam.RepeatT;
                    FrameTexInfos[i]        = matAnmResult.TextureInfo;
                }
            }
            else
            {
                TexParamList = new GxTexImageParam[]
                    { matData.TexImageParam | Model.RenderObj.MaterialTextureInfos[0].TexImageParam };
                FrameTexInfos = new[] { Model.RenderObj.MaterialTextureInfos[0] };
            }
        }

        public void SetDefaultMatParams()
        {
            SpeEmi                &= ~0x7FFF0000u;
            SpeEmi                |= 0x294A0000u;
            PolygonAttr.PolygonId =  63;
            PolygonAttr.LightMask =  1 << 1;
        }

        public void SetLightMask(uint lightMask)
        {
            PolygonAttr.LightMask = lightMask;
        }

        public void SetAlpha(byte alpha)
        {
            PolygonAttr.Alpha = alpha;
        }

        public void SetEmission(Rgb555 emission)
        {
            SpeEmi &= ~0x7FFF0000u;
            SpeEmi |= (uint)emission << 16;
        }

        public void ApplyMaterial()
        {
            if (TexParamList.Length == 1)
            {
                _context.RenderContext.GeState.TexImageParam = TexParamList[0];
                _context.RenderContext.ApplyTexParams(FrameTexInfos[0].Texture);
            }

            _context.RenderContext.GeState.MaterialColor0 = DiffAmb;
            _context.RenderContext.GeState.MaterialColor1 = SpeEmi;
        }

        public void Render(byte alpha, in Matrix4x3d mtx)
        {
            _context.RenderContext.GeState.RestoreMatrix(30);
            _context.RenderContext.GeState.MultMatrix(mtx);
            Render(alpha);
        }

        public void Render(byte alpha, in Matrix4x3d mtx, in Vector3d scale)
        {
            _context.RenderContext.GeState.RestoreMatrix(30);
            _context.RenderContext.GeState.MultMatrix(mtx);
            _context.RenderContext.GeState.Scale(scale);
            Render(alpha);
        }

        public void Render(byte alpha)
        {
            _context.RenderContext.GeState.Scale(new Vector3d(PosScale));
            byte modelAlpha  = (byte)PolygonAttr.Alpha;
            var  polygonAttr = PolygonAttr;
            if (alpha < modelAlpha)
            {
                polygonAttr.Alpha     = alpha;
                polygonAttr.PolygonId = _context.MObjState.GetCyclicPolygonId();
            }

            _context.RenderContext.GeState.PolygonAttr = polygonAttr;
            if (TexParamList.Length != 1)
            {
                _context.RenderContext.GeState.TexImageParam = TexParamList[TexIdx];
                _context.RenderContext.ApplyTexParams(FrameTexInfos[TexIdx].Texture);
            }

            _context.RenderContext.RenderShp(Model.RenderObj.ModelResource.Shapes.Shapes[0], Model.RenderObj.ShapeProxies[0]);
        }
    }
}