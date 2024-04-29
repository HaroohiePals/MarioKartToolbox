using HaroohiePals.Graphics3d.OpenGL.Renderers;
using HaroohiePals.MarioKartToolbox.Application.Settings;

namespace HaroohiePals.MarioKartToolbox.OpenGL.Renderers;

class RendererFactory : IRendererFactory
{
    private readonly IApplicationSettingsService _applicationSettings;

    public RendererFactory(IApplicationSettingsService applicationSettings)
    {
        _applicationSettings = applicationSettings;
    }

    public MeshRenderer CreateCircleRenderer()
        => new CircleRenderer();

    public MeshRenderer CreateBoxRenderer()
        => new BoxRenderer(_applicationSettings.Settings.Viewport.IntelRenderWorkaround);

    public MeshRenderer CreateBoxAreaRenderer(bool render2d)
    {
        return new MeshRenderer(Resources.Models.CubeBObj, null,
                Resources.Shaders.NkmAreaVertex, Resources.Shaders.NkmAreaFragment, render2d, true);
    }

    public MeshRenderer CreateCylinderAreaRenderer(bool render2d)
    {
        return new MeshRenderer(Resources.Models.CylinderObj, null,
                Resources.Shaders.NkmAreaVertex, Resources.Shaders.NkmAreaFragment, render2d, true);
    }

    public MeshRenderer CreateKartRenderer(bool render2d)
    {
        return new MeshRenderer(Resources.Models.Kart, null,
                Resources.Shaders.KartVertex,
                _applicationSettings.Settings.Viewport.IntelRenderWorkaround ?
                    Resources.Shaders.KartIntelFragment :
                    Resources.Shaders.KartFragment, render2d);
    }
}