using System.Numerics;
using HaroohiePals.Gui.Viewport;
using HaroohiePals.MarioKartToolbox.Application.Settings;
using HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups.Minimap;
using ImGuiNET;

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
        foreach (var renderGroup in _scene.RenderGroups)
        {
            switch (renderGroup)
            {
                case LocalMapRenderGroup localMapRenderGroup:
                    var tl = new Vector2(localMapRenderGroup.TopLeft.X, localMapRenderGroup.TopLeft.Y);
                    var br = new Vector2(localMapRenderGroup.BottomRight.X, localMapRenderGroup.BottomRight.Y);

                    ImGui.SliderFloat2("Local Map Top Left XY", ref tl, short.MinValue, short.MaxValue);
                    ImGui.SliderFloat2("Local Map Bottom Right XY", ref br, short.MinValue, short.MaxValue);
                    
                    localMapRenderGroup.TopLeft.X = tl.X;
                    localMapRenderGroup.TopLeft.Y = tl.Y;
                    localMapRenderGroup.BottomRight.X = br.X;
                    localMapRenderGroup.BottomRight.Y = br.Y;
                    break;
            }
        }
    }

    protected override void RenderTopToolbar()
    {
    }
}