using HaroohiePals.Gui.Viewport;
using HaroohiePals.MarioKartToolbox.Application.Settings;
using OpenTK.Mathematics;

namespace HaroohiePals.MarioKartToolbox.Gui.Viewport;

internal class CameraPreviewViewportPanel : InteractiveViewportPanel
{
    private readonly PerspectiveCameraControls _cameraControls = new();
    private readonly RenderGroupScene _scene;

    public bool EnableCameraControls = false;

    public CameraPreviewViewportPanel(RenderGroupScene scene, IApplicationSettingsService applicationSettings)
        : base("CameraPreview", scene, applicationSettings)
    {
        _gizmo.IsOrthographic = false;
        _scene = scene;
    }

    public void ApplyView(Matrix4 view) => Context.ViewMatrix = view;
    public void ApplyProjection(Matrix4 projection)
    {
        Context.ForceCustomProjectionMatrix = true;
        Context.CustomProjectionMatrix = projection;
    }
    public void ApplyFov(float fov)
    {
        if (_scene is RenderGroupScenePerspective persp)
        {
            Context.ForceCustomProjectionMatrix = false;
            persp.Projection.Fov = fov;
        }
    }

    public override void UpdateControls(float deltaTime)
    {
        if (EnableCameraControls)
            _cameraControls.Update(Context, deltaTime);
    }

    public override void RenderControls()
    {
    }

    protected override void RenderTopToolbar()
    {
    }
}
