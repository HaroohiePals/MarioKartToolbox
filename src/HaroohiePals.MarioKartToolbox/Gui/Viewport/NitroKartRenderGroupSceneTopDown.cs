using HaroohiePals.Gui.Viewport;
using HaroohiePals.Gui.Viewport.Framebuffers;
using System;

namespace HaroohiePals.MarioKartToolbox.Gui.Viewport;

internal class NitroKartRenderGroupSceneTopDown : RenderGroupSceneTopDown
{
    protected new TestGLFramebufferProvider FramebufferProvider => (TestGLFramebufferProvider)base.FramebufferProvider;

    public NitroKartRenderGroupSceneTopDown()
        : base(new TestGLFramebufferProvider(new(0, 32, 48, 255), true, true)) { }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
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