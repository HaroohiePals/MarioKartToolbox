using HaroohiePals.Nitro.G3;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;
using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.MapObj
{
    public class ShadowModel
    {
        private readonly MkdsContext _context;

        public Model Model;
        public byte  PolygonId;
        public byte  Alpha;
        public bool  Enabled;

        public ShadowModel(MkdsContext context, Nsbmd nsbmd, byte polygonId)
        {
            _context  = context;
            Model     = new Model(context, nsbmd);
            PolygonId = polygonId;
            Alpha     = 13;
            Enabled   = true;
            Model.RenderObj.ModelResource.Materials.Materials[0].SetPolygonMode(GxPolygonMode.Shadow);
            Model.RenderObj.ModelResource.Materials.Materials[0].SetLightEnableFlags(0);
        }

        public void Render()
        {
            if (!Enabled)
                return;

            Model.ConfigShadowPass1(Alpha);
            Model.Render();
            Model.ConfigShadowPass2(Alpha, PolygonId);
            Model.Render();
        }

        public void Render(in Matrix4x3d mtx)
        {
            if (!Enabled)
                return;

            _context.RenderContext.GeState.RestoreMatrix(30);
            _context.RenderContext.GeState.MultMatrix(mtx);
            Model.ConfigShadowPass1(Alpha);
            Model.Render();
            Model.ConfigShadowPass2(Alpha, PolygonId);
            Model.Render();
        }

        public void Render(in Matrix4x3d mtx, in Vector3d scale)
        {
            if (!Enabled)
                return;

            _context.RenderContext.GeState.RestoreMatrix(30);
            _context.RenderContext.GeState.MultMatrix(mtx);
            _context.RenderContext.GeState.Scale(scale);
            Model.ConfigShadowPass1(Alpha);
            Model.Render();
            Model.ConfigShadowPass2(Alpha, PolygonId);
            Model.Render();
        }

        public void Render(byte alpha)
        {
            if (alpha < Alpha)
            {
                byte oldPolygonId = PolygonId;
                byte oldAlpha     = alpha;
                PolygonId = _context.MObjState.GetCyclicPolygonId();
                Alpha     = alpha;
                Render();
                PolygonId = oldPolygonId;
                Alpha     = oldAlpha;
            }
            else
                Render();
        }
    }
}