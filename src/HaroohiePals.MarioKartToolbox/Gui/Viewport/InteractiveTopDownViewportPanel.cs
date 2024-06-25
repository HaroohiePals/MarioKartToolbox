using HaroohiePals.Gui;
using HaroohiePals.Gui.Viewport;
using HaroohiePals.MarioKartToolbox.Application.Settings;
using ImGuiNET;
using System;
using System.Numerics;

namespace HaroohiePals.MarioKartToolbox.Gui.Viewport;

class InteractiveTopDownViewportPanel : InteractiveViewportPanel
{
    private readonly TopDownCameraControls _cameraControls;
    private readonly RenderGroupSceneTopDown _topDownScene;

    public float MaximumNear { get; set; } = 8192;

    public bool ShowNearSlider { get; set; } = true;

    public InteractiveTopDownViewportPanel(RenderGroupSceneTopDown scene,
        IApplicationSettingsService applicationSettings)
        : base("TopDownView", scene, applicationSettings)
    {
        _topDownScene = scene;
        _cameraControls = new TopDownCameraControls(_topDownScene.OrthographicProjection, 64, 8192);
        _gizmo.IsOrthographic = true;
    }

    public override void UpdateControls(float deltaTime)
    {
        base.UpdateControls(deltaTime);

        _cameraControls.Update(Context, deltaTime);
    }

    public override void RenderControls()
    {
        base.RenderControls();
        if (ShowNearSlider)
        {
            float near = _topDownScene.OrthographicProjection.Near;
            var cursor = ImGui.GetCursorPos();
            float padding = ImGui.GetStyle().ItemSpacing.X;
            ImGui.SetCursorPosX(Context.ViewportSize.X - 12 - padding);
            ImGui.SetCursorPosY(padding * 2);
            var bgCol = ImGui.GetStyle().Colors[(int)ImGuiCol.FrameBg];
            bgCol.W = 0.5f;
            ImGui.PushStyleColor(ImGuiCol.FrameBg, bgCol);
            if (ImGui.VSliderFloat("", new System.Numerics.Vector2(12, Context.ViewportSize.Y - padding * 4), ref near,
                    MaximumNear,
                    10, ""))
                _topDownScene.OrthographicProjection.Near = near;
            ImGui.PopStyleColor();
            ImGui.SetCursorPos(cursor);
        }
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

                int fieldCount = _visibilityManager.EntityCount + 1;

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

                    // Gizmo settings
                    y += btnSize;

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
                    ImGui.SetCursorPosX(ImGui.GetContentRegionMax().X - inputWidth - 10f);

                    ImGui.PushItemWidth(inputWidth);
                    ImGuiEx.ComboEnum("##RotateScaleMode", ref _gizmo.RotateScaleMode);

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