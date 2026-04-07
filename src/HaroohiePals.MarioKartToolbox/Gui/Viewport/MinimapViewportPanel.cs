using HaroohiePals.Gui.Viewport;
using HaroohiePals.MarioKartToolbox.Application.Settings;

namespace HaroohiePals.MarioKartToolbox.Gui.Viewport;

class MinimapViewportPanel : InteractiveViewportPanel
{
    private readonly TopDownCameraControls _cameraControls;
    private readonly RenderGroupScene _scene;

    public MinimapViewportPanel(NitroKartRenderGroupSceneTopDown scene, IApplicationSettingsService applicationSettings)
        : base("CameraPreview", scene, applicationSettings)
    {
        _gizmo.IsOrthographic = false;
        _scene = scene;
        _cameraControls = new TopDownCameraControls(scene.OrthographicProjection, 64, 8192);
    }

    public override void UpdateControls(float deltaTime)
    {
        _cameraControls.Update(Context, deltaTime);
    }
    
    public override void RenderControls()
    {
    }

    protected override void RenderTopToolbar()
    {
    }
}