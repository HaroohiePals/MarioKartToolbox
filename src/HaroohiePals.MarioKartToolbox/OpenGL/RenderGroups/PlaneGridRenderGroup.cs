using HaroohiePals.Graphics3d.OpenGL.Renderers;
using HaroohiePals.Gui.Viewport;
using System;

namespace HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups;

internal class PlaneGridRenderGroup : RenderGroup, IDisposable
{
    private GridRenderer _gridRenderer = new();

    public override void Render(ViewportContext context)
    {
        _gridRenderer.Render(context.ViewMatrix, context.ProjectionMatrix, context.TranslucentPass);
    }
    public void Dispose()
    {
        _gridRenderer.Dispose();
    }
}
