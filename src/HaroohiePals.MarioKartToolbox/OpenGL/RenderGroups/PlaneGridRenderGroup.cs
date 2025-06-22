using HaroohiePals.Graphics3d.OpenGL.Renderers;
using HaroohiePals.Gui.Viewport;
using System;

namespace HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups;

class PlaneGridRenderGroup : RenderGroup, IDisposable
{
    private readonly GridRenderer _gridRenderer = new();

    public override void Render(ViewportContext context)
    {
        _gridRenderer.PickingId = ViewportContext.InvalidPickingId;
        _gridRenderer.Render(context.ViewMatrix, context.ProjectionMatrix, context.TranslucentPass);
    }
    public void Dispose()
    {
        _gridRenderer.Dispose();
    }
}
