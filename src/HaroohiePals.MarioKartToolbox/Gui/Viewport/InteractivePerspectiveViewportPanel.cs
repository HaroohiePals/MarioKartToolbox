using HaroohiePals.Gui;
using HaroohiePals.Gui.Viewport;
using HaroohiePals.MarioKartToolbox.Application.Settings;
using ImGuiNET;
using System;
using System.Numerics;

namespace HaroohiePals.MarioKartToolbox.Gui.Viewport;

class InteractivePerspectiveViewportPanel : InteractiveViewportPanel
{
    private readonly PerspectiveCameraControls _cameraControls = new();
    private readonly RenderGroupScenePerspective _perspectiveScene;

    public InteractivePerspectiveViewportPanel(RenderGroupScenePerspective scene,
        IApplicationSettingsService applicationSettings)
        : base("PerspectiveView", scene, applicationSettings)
    {
        _perspectiveScene = scene;
        _gizmo.IsOrthographic = false;
    }

    public override void UpdateControls(float deltaTime)
    {
        base.UpdateControls(deltaTime);

        _cameraControls.Update(Context, deltaTime);
        _cameraControls.ViewportKeyBindings = _applicationSettings.Settings.KeyBindings.Viewport.ToViewportKeyBindings();
    }

    public override void RenderControls()
    {
        base.RenderControls();
        _cameraControls.Render(Context);
    }

    protected override void RenderTopToolbar()
    {
        float scale = ImGuiEx.GetUiScale();

        float btnSize = 24 * scale;
        float spacing = 2 * scale;
        var padding = new Vector2(36, 8) * scale;

        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Vector2.Zero);
        ImGui.SetNextWindowPos(ImGui.GetWindowPos() + ImGui.GetWindowContentRegionMin() + padding);
        if (ImGui.BeginChildFrame(ImGui.GetID("TopTools"), new Vector2(200f * scale, btnSize + spacing),
                ImGuiWindowFlags.NoBackground))
        {
            ImGui.PopStyleVar();

            int i = 0;

            ImGui.SetCursorPosX((btnSize + spacing) * i++);
            if (ImGui.Button($"{FontAwesome6.Gear}", new(btnSize)))
            {
                ImGui.OpenPopup("Viewport Settings");
            }

            if (ImGui.BeginPopup("Viewport Settings", ImGuiWindowFlags.AlwaysAutoResize))
            {
                float x = 4 * scale;
                float y = 4 * scale;

                int visibilityTypeCount = 5; //Enum.GetValues(typeof(VisibilityType)).Length;

                int fieldCount = _visibilityManager.EntityCount + 3;

                float frameHeight = btnSize + y * 2 + btnSize * fieldCount;
                frameHeight = Math.Min(500 * scale, frameHeight);
                float frameWidth = 300 * scale;

                if (ImGui.BeginChildFrame(ImGui.GetID("TopTools"),
                        new Vector2(frameWidth, frameHeight), ImGuiWindowFlags.NoBackground))
                {
                    var inputWidth = btnSize * visibilityTypeCount;

                    //Title
                    ImGui.SetCursorPosX(x);
                    ImGui.SetCursorPosY(y);

                    ImGui.BeginDisabled();
                    ImGui.Text("Settings");
                    ImGui.EndDisabled();
                    ImGui.Separator();

                    y += btnSize;

                    // Gizmo settings
                    ImGui.SetCursorPosX(x);
                    ImGui.SetCursorPosY(y);

                    ImGui.Text("Gizmo Mode");

                    ImGui.SameLine();
                    ImGui.SetCursorPosX(ImGui.GetContentRegionMax().X - inputWidth - 10f);

                    ImGui.PushItemWidth(inputWidth);
                    ImGuiEx.ComboEnum("##GizmoMode", ref _gizmo.Mode);

                    y += btnSize;

                    ImGui.SetCursorPosX(x);
                    ImGui.SetCursorPosY(y);

                    ImGui.Text("Rotate/Scale Mode");

                    ImGui.SameLine();
                    ImGui.SetCursorPosX(ImGui.GetContentRegionMax().X - inputWidth - (10f * scale));

                    ImGui.PushItemWidth(inputWidth);
                    ImGuiEx.ComboEnum("##RotateScaleMode", ref _gizmo.RotateScaleMode);

                    // Camera settings
                    y += btnSize;

                    ImGui.SetCursorPosX(x);
                    ImGui.SetCursorPosY(y);

                    ImGui.Text("Camera Speed");

                    ImGui.SameLine();
                    ImGui.SetCursorPosX(ImGui.GetContentRegionMax().X - inputWidth - (10f * scale));

                    ImGui.PushItemWidth(inputWidth);
                    float controlSpeed = _cameraControls.ControlSpeed;
                    if (ImGui.SliderFloat("##Camera speed", ref controlSpeed,
                            _cameraControls.MinControlSpeed, _cameraControls.MaxControlSpeed))
                    {
                        _cameraControls.ControlSpeed =
                            Math.Clamp(controlSpeed, _cameraControls.MinControlSpeed,
                                _cameraControls.MaxControlSpeed);
                    }

                    y += btnSize;

                    ImGui.SetCursorPosX(x);
                    ImGui.SetCursorPosY(y);
                    ImGui.Text("Field of view");

                    ImGui.SameLine();
                    ImGui.SetCursorPosX(ImGui.GetContentRegionMax().X - inputWidth - (10f * scale));

                    ImGui.PushItemWidth(inputWidth);
                    float fov = _perspectiveScene.Projection.Fov;
                    if (ImGui.SliderFloat("##Field of view", ref fov, 10f, 179f))
                        _perspectiveScene.Projection.Fov = Math.Clamp(fov, 10f, 179f);

                    // View settings
                    y += btnSize;

                    _visibilityManager.Draw(x, y);

                    ImGui.EndChildFrame();
                }

                ImGui.EndPopup();
            }

            ImGui.EndChildFrame();
        }
    }
}