using HaroohiePals.Graphics;
using HaroohiePals.Graphics3d;
using HaroohiePals.Nitro.G3;
using HaroohiePals.Nitro.G3.OpenGL;
using HaroohiePals.Nitro.Gx;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;
using OpenTK.Graphics.OpenGL4;
using TextureWrapMode = HaroohiePals.Graphics3d.TextureWrapMode;

namespace HaroohiePals.Nitro.NitroSystem.G3d.OpenGL
{
    public class GLRenderContext : RenderContext
    {
        public readonly GLGeStateUniformBuffer UniformBuffer;

        public GLRenderContext(GeometryEngineState geState, GLGeStateUniformBuffer uniformBuffer)
            : base(geState)
        {
            UniformBuffer = uniformBuffer;
        }

        public override void ApplyTexParams(Texture texture)
        {
            if (GeState.TexImageParam.Format == ImageFormat.None || texture == null)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, 0);
            }
            else
            {
                bool repeatS = GeState.TexImageParam.RepeatS;
                bool repeatT = GeState.TexImageParam.RepeatT;
                bool flipS   = GeState.TexImageParam.FlipS;
                bool flipT   = GeState.TexImageParam.FlipT;

                TextureWrapMode wrapS, wrapT;

                if (repeatS && flipS)
                    wrapS = TextureWrapMode.MirroredRepeat;
                else if (repeatS)
                    wrapS = TextureWrapMode.Repeat;
                else
                    wrapS = TextureWrapMode.Clamp;

                if (repeatT && flipT)
                    wrapT = TextureWrapMode.MirroredRepeat;
                else if (repeatT)
                    wrapT = TextureWrapMode.Repeat;
                else
                    wrapT = TextureWrapMode.Clamp;

                texture.SetWrapMode(wrapS, wrapT);

                GL.ActiveTexture(TextureUnit.Texture0);
                texture.Use();
            }
        }

        public override void RenderShp(G3dShape shp, DisplayListBuffer buffer)
        {
            bool isTranslucent = GeState.PolygonAttr.Alpha != 31 ||
                                 GeState.TexImageParam.Format == ImageFormat.A3I5 ||
                                 GeState.TexImageParam.Format == ImageFormat.A5I3;

            if (GeState.TranslucentPass && isTranslucent)
            {
                GeState.TranslucentPass = false;
                RenderShp(shp, buffer, false);
                GeState.TranslucentPass = true;
                RenderShp(shp, buffer, true);
            }
            else if (!GeState.TranslucentPass && !isTranslucent)
                RenderShp(shp, buffer, false);
        }

        private void RenderShp(G3dShape shp, DisplayListBuffer buffer, bool translucent)
        {
            bool isTranslucent = GeState.PolygonAttr.Alpha != 31 ||
                                 GeState.TexImageParam.Format == ImageFormat.A3I5 ||
                                 GeState.TexImageParam.Format == ImageFormat.A5I3;

            // skip translucent pass for completely opaque polygons
            if (translucent && !isTranslucent)
                return;

            // skip opaque pass for completely translucent polygons
            if (!translucent && GeState.PolygonAttr.Alpha != 31)
                return;

            buffer.Bind();
            GLNitroVertexData.EnableAllAttribs();
            if ((buffer.Flags & DisplayListBuffer.DlFlags.HasTexCoords) == 0)
            {
                GLNitroVertexData.SetupFixedTexCoord(((float)GeState.TexCoord.X, (float)GeState.TexCoord.Y));
            }

            if ((buffer.Flags & (DisplayListBuffer.DlFlags.HasNormals | DisplayListBuffer.DlFlags.HasColors)) == 0
                && (GeState.MaterialColor0 & 0x8000) != 0)
            {
                var diffuse = (Rgb555)(GeState.MaterialColor0 & 0x7FFF);
                GLNitroVertexData.SetupFixedColor((diffuse.R / 31f, diffuse.G / 31f, diffuse.B / 31f));
            }

            UniformBuffer.CommitGeState(GeState);
            UniformBuffer.Flush();

            if (translucent && !GeState.PolygonAttr.TranslucentDepthUpdate)
                GL.DepthMask(false);
            else
                GL.DepthMask(true);

            int polygonId   = (int)GeState.PolygonAttr.PolygonId;
            var polygonMode = GeState.PolygonAttr.PolygonMode;

            if (translucent)
            {
                GL.Enable(EnableCap.StencilTest);
                GL.StencilFunc(StencilFunction.Notequal, polygonId, 0x7F);
                GL.StencilMask(0x7F);
                GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);
            
                GL.BlendFunc(2, BlendingFactorSrc.One, BlendingFactorDest.One);
                GL.BlendEquation(2, BlendEquationMode.Min);
            }
            else
            {
                GL.Enable(EnableCap.StencilTest);
                GL.StencilFunc(StencilFunction.Always, polygonId | 0x40, 0x7F);
                GL.StencilMask(0x7F);
                GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);
            
                GL.BlendFunc(2, BlendingFactorSrc.One, BlendingFactorDest.Zero);
                GL.BlendEquation(2, BlendEquationMode.FuncAdd);
            }
            
            switch (GeState.PolygonAttr.CullMode)
            {
                case GxCull.All:
                    GL.CullFace(CullFaceMode.FrontAndBack);
                    GL.Enable(EnableCap.CullFace);
                    break;
                case GxCull.Front:
                    GL.CullFace(CullFaceMode.Front);
                    GL.Enable(EnableCap.CullFace);
                    break;
                case GxCull.Back:
                    GL.CullFace(CullFaceMode.Back);
                    GL.Enable(EnableCap.CullFace);
                    break;
                case GxCull.None:
                    GL.Disable(EnableCap.CullFace);
                    break;
            }
            
            if (GeState.PolygonAttr.DepthEquals)
                GL.DepthFunc(DepthFunction.Equal);
            else
                GL.DepthFunc(DepthFunction.Less);
            
            if (polygonMode == GxPolygonMode.Shadow)
            {
                if (polygonId == 0)
                {
                    GL.ColorMask(false, false, false, false);
                    GL.Enable(EnableCap.StencilTest);
                    GL.StencilFunc(StencilFunction.Always, 0x80, 0x80);
                    GL.StencilMask(0x80);
                    GL.StencilOpSeparate(StencilFace.Back, StencilOp.Keep, StencilOp.Replace, StencilOp.Keep);
                    GL.StencilOpSeparate(StencilFace.Front, StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
                }
                else
                {
                    GL.ColorMask(false, false, false, false);
                    GL.Enable(EnableCap.StencilTest);
                    GL.StencilFunc(StencilFunction.Equal, 0x80 | polygonId, 0xBF);
                    GL.StencilMask(0x80);
                    GL.StencilOp(StencilOp.Keep, StencilOp.Zero, StencilOp.Zero);
            
                    buffer.Draw();
            
                    GL.ColorMask(true, true, true, true);
                    GL.StencilFunc(StencilFunction.Equal, 0x80, 0x80);
                    GL.StencilMask(0x80);
                    GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Zero);
                }
            }

            buffer.Draw();

            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.StencilTest);
            GL.ColorMask(true, true, true, true);
            GL.DepthMask(true);
            GL.DepthFunc(DepthFunction.Less);
            
            GL.BlendFunc(2, BlendingFactorSrc.One, BlendingFactorDest.Zero);
            GL.BlendEquation(2, BlendEquationMode.FuncAdd);
        }
    }
}