using HaroohiePals.Gui;
using HaroohiePals.Gui.Viewport;
using HaroohiePals.MarioKartToolbox.Application.Settings;
using System.Collections.Generic;

namespace HaroohiePals.MarioKartToolbox.Gui.Viewport;

class MinimapViewportPanel : InteractiveViewportPanel
{
    private readonly TopDownCameraControls _cameraControls;

    public MinimapViewportPanel(NitroKartRenderGroupSceneTopDown scene, 
        IApplicationSettingsService applicationSettings, 
        IReadOnlyList<ViewportSideToolbarExtraTool> tools)
        : base("MinimapViewport", scene, applicationSettings, [GizmoTool.Translate, GizmoTool.Scale], tools)
    {
        _gizmo.IsOrthographic = true;
        _cameraControls = new TopDownCameraControls(scene.OrthographicProjection, 64, 8192);
    }

    public override void UpdateControls(float deltaTime)
    {
        base.UpdateControls(deltaTime);
        _cameraControls.Update(Context, deltaTime);
    }

    protected override void RenderTopToolbar()
    {
        
    }
}
