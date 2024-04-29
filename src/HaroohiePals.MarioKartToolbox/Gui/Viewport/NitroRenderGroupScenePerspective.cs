using HaroohiePals.Gui.Viewport;
using HaroohiePals.Gui.Viewport.Framebuffers;
using OpenTK.Graphics.OpenGL4;
using System;

namespace HaroohiePals.MarioKartToolbox.Gui.Viewport;

class NitroRenderGroupScenePerspective : RenderGroupScenePerspective
{
    protected new TestGLFramebufferProvider FramebufferProvider => (TestGLFramebufferProvider)base.FramebufferProvider;

    protected NitroFogPostProcessing NitroFog { get; } = new();

    public NitroRenderGroupScenePerspective()
        : base(new TestGLFramebufferProvider(new(0, 32, 48, 255), true, true)) { }

    public override void Render(ViewportContext context)
    {
        GL.DrawBuffers(3, new[]
        {
            DrawBuffersEnum.ColorAttachment0,
            DrawBuffersEnum.ColorAttachment1,
            DrawBuffersEnum.ColorAttachment2
        });
        GL.BlendFunc(2, BlendingFactorSrc.One, BlendingFactorDest.Zero);
        GL.BlendEquation(2, BlendEquationMode.FuncAdd);
        base.Render(context);
    }

    public override void RenderPostProcessing(ViewportContext context)
    {
        NitroFog.Render(context, FramebufferProvider.DepthBufferMultiTex, FramebufferProvider.FogBufferMultiTex);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            NitroFog.Dispose();

            foreach (var group in RenderGroups)
            {
                try
                {
                    if (group is IDisposable d)
                        d.Dispose();
                }
                catch { }
            }
        }

        base.Dispose(disposing);
    }
}