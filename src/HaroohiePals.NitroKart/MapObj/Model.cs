using HaroohiePals.Graphics;
using HaroohiePals.Nitro.G3;
using HaroohiePals.Nitro.NitroSystem.G3d;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;
using OpenTK.Mathematics;
using System;

namespace HaroohiePals.NitroKart.MapObj
{
    public class Model : IDisposable
    {
        private readonly MkdsContext _context;

        private bool _disposed;

        public G3dRenderObject RenderObj;
        public bool      CullReversed;
        public bool      Render1Mat1Shp;

        public Model(MkdsContext context, G3dModel resMdl)
        {
            _context = context;
            InitModel(resMdl, true);
            _context.ModelManager.InitializeRenderObject(RenderObj, null);
        }

        public Model(MkdsContext context, Nsbmd nsbmd, bool cullFix = true)
        {
            _context = context;
            InitModel(nsbmd.ModelSet.Models[0], cullFix);
            _context.ModelManager.InitializeRenderObject(RenderObj, nsbmd.TextureSet);
        }

        public Model(MkdsContext context, Nsbmd nsbmd, Nsbtx nsbtx)
        {
            _context = context;
            InitModel(nsbmd.ModelSet.Models[0], true);
            _context.ModelManager.InitializeRenderObject(RenderObj, nsbtx.TextureSet);
        }

        private void InitModel(G3dModel resMdl, bool cullFix)
        {
            // if (cullFix && scproc_getCurrentScene() == SCENE_RACE)
            //     CullReversed = rconf_getIsMirror();
            // else
            CullReversed   = false;
            Render1Mat1Shp = resMdl.Info.MaterialCount == 1 && resMdl.Info.ShapeCount == 1;
            RenderObj      = new G3dRenderObject(resMdl);

            if (CullReversed)
            {
                for (int i = 0; i < resMdl.Info.MaterialCount; i++)
                {
                    var mat = resMdl.Materials.Materials[i];
                    if (mat.PolygonAttribute.CullMode == GxCull.Front)
                        mat.SetCullMode(GxCull.Back);
                    else if (mat.PolygonAttribute.CullMode == GxCull.Back)
                        mat.SetCullMode(GxCull.Front);
                }
            }
        }

        public void SetCullFront()
        {
            var resMdl = RenderObj.ModelResource;
            for (int i = 0; i < resMdl.Info.MaterialCount; i++)
                resMdl.Materials.Materials[i].SetCullMode(GxCull.Front);
        }

        public void SetCullBack()
        {
            var resMdl = RenderObj.ModelResource;
            for (int i = 0; i < resMdl.Info.MaterialCount; i++)
                resMdl.Materials.Materials[i].SetCullMode(GxCull.Back);
        }

        public void ConfigShadowPass1(byte alpha)
        {
            var resMdl   = RenderObj.ModelResource;
            var cullMode = CullReversed ? GxCull.Back : GxCull.Front;
            resMdl.Materials.Materials[0].SetPolygonId(0);
            resMdl.Materials.Materials[0].SetCullMode(cullMode);
            resMdl.Materials.Materials[0].SetAlpha(alpha);
        }

        public void ConfigShadowPass2(byte alpha, byte polygonId)
        {
            var resMdl   = RenderObj.ModelResource;
            var cullMode = CullReversed ? GxCull.Front : GxCull.Back;
            resMdl.Materials.Materials[0].SetPolygonId(polygonId);
            resMdl.Materials.Materials[0].SetCullMode(cullMode);
            resMdl.Materials.Materials[0].SetAlpha(alpha);
        }

        public void Render()
        {
            _context.RenderContext.Sbc.Draw(RenderObj);
        }

        public void Render(in Matrix4x3d mtx)
        {
            _context.RenderContext.GeState.RestoreMatrix(30);
            _context.RenderContext.GeState.MultMatrix(mtx);
            Render();
        }

        public void Render(in Matrix4x3d mtx, in Vector3d scale)
        {
            _context.RenderContext.GeState.RestoreMatrix(30);
            _context.RenderContext.GeState.MultMatrix(mtx);
            _context.RenderContext.GeState.Scale(scale);
            Render();
        }

        public void SetPolygonId(byte polygonId)
        {
            RenderObj.ModelResource.SetAllPolygonId(polygonId);
        }

        public void SetLightEnableFlag(uint lightMask)
        {
            RenderObj.ModelResource.SetAllLightEnableFlags(lightMask);
        }

        public void SetEmi(Rgb555 color)
        {
            RenderObj.ModelResource.SetAllEmission(color);
        }

        public void SetPolyIdLightFlagsEmi(byte polygonId, uint lightMask, Rgb555 emi)
        {
            SetPolygonId(polygonId);
            SetLightEnableFlag(lightMask);
            SetEmi(emi);
        }

        public void SetCullMode(GxCull cullMode)
        {
            RenderObj.ModelResource.SetAllCullMode(cullMode);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _context.ModelManager.CleanupRenderObject(RenderObj);

            _disposed = true;
        }
    }
}