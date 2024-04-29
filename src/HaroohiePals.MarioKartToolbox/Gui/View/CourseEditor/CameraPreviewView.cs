using HaroohiePals.Actions;
using HaroohiePals.Gui;
using HaroohiePals.Gui.Input;
using HaroohiePals.Gui.View;
using HaroohiePals.MarioKartToolbox.Application.Settings;
using HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;
using HaroohiePals.MarioKartToolbox.Gui.Viewport;
using HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups.MapData;
using HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups.NitroSystem;
using HaroohiePals.NitroKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using HaroohiePals.NitroKart.MapObj;
using HaroohiePals.NitroKart.Race;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace HaroohiePals.MarioKartToolbox.Gui.View.CourseEditor;

internal class CameraPreviewView : CourseViewportView
{
    private readonly IApplicationSettingsService _applicationSettings;
    private readonly CameraPreviewViewportPanel _bottomViewportPanel;

    private readonly CameraPreviewSettings _settings = new();

    private NitroKartRenderGroupScenePerspective _topScene => (NitroKartRenderGroupScenePerspective)_scene;
    private NitroKartRenderGroupScenePerspective _bottomScene;

    private float _topFov = 45f;
    private OpenTK.Mathematics.Matrix4 _topView;
    private OpenTK.Mathematics.Matrix4 _topProj;
    private OpenTK.Mathematics.Matrix4 _bottomView;
    private OpenTK.Mathematics.Matrix4 _bottomProj;
    private OpenTK.Mathematics.Matrix4 _nextTopView;
    private OpenTK.Mathematics.Matrix4 _nextTopProj;
    private OpenTK.Mathematics.Vector3d _editEye;

    //todo: create viewmodel
    private RaceContext _raceContext;
    private Driver _driver = new();
    private MkdsEnemyPoint _currentEnemyPoint;
    private MkdsEnemyPoint _nextEnemyPoint;

    private int _lastFrame = -2;
    private int _lastActionCount = 0;

    public CameraPreviewView(ICourseEditorContext context, IApplicationSettingsService applicationSettings,
        string title = "Camera Preview") : base(title, context)
    {
        _applicationSettings = applicationSettings;
        _lastActionCount = Context.ActionStack.UndoActionsCount;

        if (Context.Course.MapData == null)
            return;

        var scene = CreateScene();

        _scene = scene;
        _bottomScene = CreateScene();

        _viewportPanel = new CameraPreviewViewportPanel(_topScene, applicationSettings);
        _bottomViewportPanel = new CameraPreviewViewportPanel(_bottomScene, applicationSettings);

        context.SceneObjectHolder.SelectionChanged += SelectionChanged;

        Initialize();
    }

    private void SelectionChanged()
    {
        if (_settings.Mode == CameraPreviewMode.Edit)
            Initialize();
    }

    private NitroKartRenderGroupScenePerspective CreateScene()
    {
        var scene = new NitroKartRenderGroupScenePerspective
        {
            StageInfo = Context.Course.MapData.StageInfo
        };

        if (Context.Course.MapData.MapObjects != null)
        {
            var mobjRenderGroup = new MkdsMObjRenderGroup(Context);
            scene.RenderGroups.Add(mobjRenderGroup);
            mobjRenderGroup.EditMode = false;
        }

        var perspectiveNsbmdGroup = new CourseModelRenderGroup();
        perspectiveNsbmdGroup.Load(Context.Course);
        scene.RenderGroups.Add(perspectiveNsbmdGroup);

        return scene;
    }

    public override void Dispose()
    {
        base.Dispose();

        _bottomScene?.Dispose();
    }

    public override void Update(UpdateArgs args)
    {
        base.Update(args);

        if (_settings.Mode == CameraPreviewMode.Edit)
            return;

        // Recalculate current preview frame if an action was done
        if (_lastActionCount != Context.ActionStack.UndoActionsCount || ImGui.IsMouseDragging(ImGuiMouseButton.Left))
        {
            _lastActionCount = Context.ActionStack.UndoActionsCount;
            RecalculateFrame();
        }
        else
        {
            if (_lastFrame <= Context.TimelineManager.CurrentFrame)
                AdvanceFrames(Context.TimelineManager.CurrentFrame - _lastFrame);
            else
                GoToFrame(Context.TimelineManager.CurrentFrame);
        }
    }

    private void RecalculateFrame()
    {
        Initialize();
        AdvanceFrames(Context.TimelineManager.CurrentFrame);
    }

    private void AdvanceFrames(int count)
    {
        for (int i = 0; i < count; i++)
            AdvanceFrame();
    }

    private void AdvanceFrame()
    {
        if (_raceContext is null)
            return;

        bool isOddFrame = (_lastFrame & 1) == 1;

        _raceContext.Update(isOddFrame, Context.Course.MapData.IsMgStage || _settings.ViewportMode == CameraPreviewViewportMode.SingleScreen);

        void updateSingleCam()
        {
            _topView = _raceContext.CurrentCameraView;
            _topProj = _raceContext.CurrentCameraProjection;
            _topFov = _raceContext.CurrentCameraFov;
        }

        void updateDoubleCam()
        {
            if (!isOddFrame)
            {
                //Apply the view/proj in the next frame
                _nextTopView = _raceContext.CurrentCameraView;
                _nextTopProj = _raceContext.CurrentCameraProjection;
            }
            else
            {
                _topView = _nextTopView;
                _topProj = _nextTopProj;
                _bottomView = _nextTopView; //_raceContext.CurrentCameraView;
                _bottomProj = _raceContext.CurrentCameraProjection;
            }
        }

        switch (_settings.Mode)
        {
            case CameraPreviewMode.AnimIntro:
                if (Context.Course.MapData.IsMgStage || _settings.ViewportMode == CameraPreviewViewportMode.SingleScreen)
                    updateSingleCam();
                else
                    updateDoubleCam();
                break;
            case CameraPreviewMode.AnimReplay:
                updateSingleCam();

                if (_settings.ReplayMoveDriver &&
                    (!_settings.ReplaySimulateCountdown ||
                    _settings.ReplaySimulateCountdown && _lastFrame > 60 * 3))
                    UpdateDriver(1 / 60f);
                break;
        }

        _lastFrame++;
    }

    private void GoToFrame(int targetFrame)
    {
        int framesToAdvance = targetFrame;
        if (targetFrame > _lastFrame)
            framesToAdvance = targetFrame - _lastFrame;
        else
            Initialize();

        AdvanceFrames(framesToAdvance);
    }

    private void ApplyViewProjection(CameraPreviewViewportPanel viewportPanel, OpenTK.Mathematics.Matrix4 view, OpenTK.Mathematics.Matrix4 proj, float? fov = null)
    {
        viewportPanel.ApplyView(view);

        if (_settings.ViewportMode == CameraPreviewViewportMode.SingleScreen && fov.HasValue)
            viewportPanel.ApplyFov(fov.Value);
        else
            viewportPanel.ApplyProjection(proj);
    }

    public override bool Draw()
    {
        float scale = ImGuiEx.GetUiScale();
        float timeControlFixedSpace = 0; // 50f * scale;

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        if (ImGui.Begin(_title))
        {
            if (ImGui.IsWindowAppearing())
            {
                ImGui.SetWindowFocus("Timeline");
                ImGui.SetWindowFocus(_title);
            }

            ImGui.PopStyleVar();
            RenderControls();
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);

            float screenGap = 10f;
            var availableSpace = ImGui.GetContentRegionAvail();

            availableSpace.Y -= timeControlFixedSpace;

            float screenY = _settings.ViewportMode == CameraPreviewViewportMode.DualScreen ? availableSpace.Y / 2 - screenGap : 0;
            float screenX = _settings.ViewportMode == CameraPreviewViewportMode.DualScreen ? screenY / 192 * 256 : 0;

            var frameSize = _settings.ViewportMode == CameraPreviewViewportMode.DualScreen ? new Vector2(screenX, screenY * 2 + 10) : new Vector2(0);

            ImGui.SetCursorPosX(_settings.ViewportMode == CameraPreviewViewportMode.DualScreen ? availableSpace.X / 2f - screenX / 2f : 0f);
            if (ImGui.BeginChild(ImGui.GetID("TestFrames"), frameSize, false, ImGuiWindowFlags.None))
            {
                _viewportPanel.Size = new Vector2(screenX, screenY);
                _bottomViewportPanel.Size = new Vector2(screenX, screenY);

                bool isEdit = _settings.Mode == CameraPreviewMode.Edit;
                if (_viewportPanel is CameraPreviewViewportPanel topPanel)
                {
                    topPanel.EnableCameraControls = isEdit;

                    if (!isEdit)
                        ApplyViewProjection(topPanel, _topView, _topProj, _topFov);

                    _viewportPanel.Draw();
                }

                if (_settings.ViewportMode == CameraPreviewViewportMode.DualScreen)
                {
                    ApplyViewProjection(_bottomViewportPanel,
                        isEdit ? _viewportPanel.Context.ViewMatrix : _bottomView, _bottomProj, null);
                    _bottomViewportPanel.Draw();
                }

                ImGui.EndChild();
            }

            ImGui.End();
        }
        ImGui.PopStyleVar();

        return true;
    }

    private void RenderControls()
    {
        float scale = ImGuiEx.GetUiScale();

        float btnSize = 24 * scale;
        float spacing = 2 * scale;

        CameraPreviewMode oldMode = _settings.Mode;

        if (ImGui.Button($"{FontAwesome6.Gear}##CameraPreviewSettings", new(btnSize)))
            ImGui.OpenPopup("Camera Preview Settings");

        if (ImGui.BeginPopup("Camera Preview Settings", ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGuiEx.ComboEnum("Mode", ref _settings.Mode);
            ImGuiEx.ComboEnum("Viewport mode", ref _settings.ViewportMode);

            if (_settings.Mode == CameraPreviewMode.AnimReplay)
            {
                ImGui.Checkbox("Simulate Countdown", ref _settings.ReplaySimulateCountdown);
                ImGui.Checkbox("Move Driver", ref _settings.ReplayMoveDriver);
                ImGui.SliderFloat("Driver Speed", ref _settings.ReplayDriverMoveSpeed, 0f, 100f);
                ImGui.Checkbox("Apply Driver Pos on KTP2", ref _settings.ReplayApplyDriverPosOnKtp2);
            }

            ImGui.EndPopup();
        }

        //todo: Shortcuts
        if (new KeyBinding(ImGuiKey.M).IsPressed())
            _settings.Mode = _settings.Mode == CameraPreviewMode.AnimIntro ? CameraPreviewMode.Edit : CameraPreviewMode.AnimIntro;

        if (oldMode != _settings.Mode)
            Initialize();

        if (_settings.Mode == CameraPreviewMode.Edit)
        {
            ImGui.SameLine();

            if (GetSelectedCamera() != null)
            {
                //if (ImGui.Button("Get"))
                //{
                //    Initialize();
                //}
                //ImGui.SameLine();
                if (ImGui.Button("Apply view"))
                {
                    ApplyEditView();
                }
                ImGui.SameLine();
                if (ImGui.RadioButton("Target A", !_settings.EditSecondTarget))
                {
                    _settings.EditSecondTarget = false;
                    Initialize();
                }
                ImGui.SameLine();
                if (ImGui.RadioButton("Target B", _settings.EditSecondTarget))
                {
                    _settings.EditSecondTarget = true;
                    Initialize();
                }
                ImGui.SameLine();
                ImGui.Checkbox("Keep original target distance", ref _settings.EditKeepOriginalTargetDistance);
                if (!_settings.EditKeepOriginalTargetDistance)
                {
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(200f);
                    ImGui.DragFloat("Target distance", ref _settings.EditTargetDistance, 1f, 100f, 3000f);
                }
            }
            else
            {
                ImGui.TextUnformatted("Please select a compatible camera on the tree or viewport.");
            }
        }
    }

    private MkdsCamera GetSelectedCamera()
    {
        return Context.SceneObjectHolder.GetSelection().OfType<MkdsCamera>().SingleOrDefault(x => x.Type is
                MkdsCameraType.FixedLookAtTargets or
                MkdsCameraType.RouteLookAtTargets);
    }

    private void Initialize()
    {
        try
        {
            switch (_settings.Mode)
            {
                case CameraPreviewMode.AnimReplay:
                    _lastFrame = -1;

                    if (!Context.Course.MapData.IsMgStage)
                    {
                        _currentEnemyPoint = Context.Course.MapData.EnemyPaths[0].Points[0];
                        FindNextEnemyPoint();
                    }

                    _driver.Position = Context.Course.MapData.StartPoints[0].Position;

                    _driver.Field2C0 = OpenTK.Mathematics.MathHelper.RadiansToDegrees(MObjUtil.IdxToRad(850));

                    _raceContext = new RaceContext(Context.Course, _driver);
                    AdvanceFrame();
                    break;
                case CameraPreviewMode.AnimIntro:
                    // In battle mode, the course intro is on a single screen and runs at 60 fps,
                    // so I only need to simulate 1 frame as opposed to the 2 frames of 30 fps (both screens)

                    bool isSingleScreen = Context.Course.MapData.IsMgStage || _settings.ViewportMode == CameraPreviewViewportMode.SingleScreen;

                    _lastFrame = isSingleScreen ? -1 : -2;
                    _raceContext = new RaceContext(Context.Course, _driver, true);
                    AdvanceFrames(-_lastFrame);
                    break;
                case CameraPreviewMode.Edit:
                    UpdateViewForEdit();
                    break;
            }
        }
        catch
        {
            _raceContext = null;
        }
    }

    private void UpdateViewForEdit()
    {
        var camera = GetSelectedCamera();
        if (camera == null)
            return;

        var up = OpenTK.Mathematics.Vector3.UnitY;
        _editEye = camera.Position;
        var target = _settings.EditSecondTarget ? camera.Target2 : camera.Target1;

        // Offset target to allow editing
        if ((_editEye - target).LengthSquared < 0.0001)
            target += new OpenTK.Mathematics.Vector3(1000, 0, 0);

        RaceCameraUtils.CalculateFovSinCos(camera.FovBegin, camera.FovEnd, _settings.EditSecondTarget ? 1 : 0,
            camera.FovSpeed, out double fovSin, out double fovCos);

        if (camera.Type == MkdsCameraType.RouteLookAtTargets && camera.Path?.Target is not null)
        {
            var cameraRoute = new CameraRoute(camera);

            int framesCap = _settings.EditSecondTarget ? camera.Duration : 1;

            for (int i = 0; i < framesCap; i++)
                _editEye = cameraRoute.Update(camera, camera.PathSpeed);
        }

        _topView = OpenTK.Mathematics.Matrix4.LookAt((OpenTK.Mathematics.Vector3)_editEye, (OpenTK.Mathematics.Vector3)target, up);
        _topProj = RaceCameraUtils.CalculateProjection(CameraMode.DoubleTop, fovSin, fovCos);
        _bottomProj = RaceCameraUtils.CalculateProjection(CameraMode.DoubleBottom, fovSin, fovCos);
        _topFov = OpenTK.Mathematics.MathHelper.RadiansToDegrees((float)Math.Atan2(fovSin, fovCos));

        if (_viewportPanel is CameraPreviewViewportPanel topPanel)
            ApplyViewProjection(topPanel, _topView, _topProj, _topFov);
    }

    private void ApplyEditView()
    {
        var camera = GetSelectedCamera();
        var view = _viewportPanel.Context.ViewMatrix.Inverted();

        var eye = view.ExtractTranslation();

        float distance = _settings.EditKeepOriginalTargetDistance ?
            (float)(_editEye - (_settings.EditSecondTarget ? camera.Target2 : camera.Target1)).LengthFast :
            _settings.EditTargetDistance;

        var target = eye - view.Row2.Xyz * distance;

        var actions = new List<IAction>();

        if (_settings.EditSecondTarget)
            actions.Add(camera.SetPropertyAction(x => x.Target2, target));
        else
            actions.Add(camera.SetPropertyAction(x => x.Target1, target));

        if (camera.Type == MkdsCameraType.FixedLookAtTargets)
            actions.Add(camera.SetPropertyAction(x => x.Position, eye));

        Context.ActionStack.Add(new BatchAction(actions));

        Initialize();
    }

    private void UpdateDriver(double deltaTime)
    {
        if (_settings.Mode != CameraPreviewMode.AnimReplay || Context.Course.MapData.IsMgStage)
            return;

        var diff = _currentEnemyPoint.Position - _driver.Position;
        var diffXz = _driver.Position.Xz - _currentEnemyPoint.Position.Xz;

        if (diffXz.Length < 50)
        {
            _currentEnemyPoint = _nextEnemyPoint;
            FindNextEnemyPoint();
        }

        var targetDirection = diff.Normalized();

        // Change position
        _driver.Position = _driver.Position + targetDirection * deltaTime * _settings.ReplayDriverMoveSpeed * 100;

        // Interpolate direction
        _driver.Direction = OpenTK.Mathematics.Vector3d.Lerp(_driver.Direction, targetDirection, deltaTime * _settings.ReplayDriverMoveSpeed);

        if (_settings.ReplayApplyDriverPosOnKtp2 && Context.Course.MapData.KartPoint2D.Count > 0)
            Context.Course.MapData.KartPoint2D[0].Position = _driver.Position;
    }

    private void FindNextEnemyPoint()
    {
        var path = Context.Course.MapData.EnemyPaths.FirstOrDefault(x => x.Points.Contains(_currentEnemyPoint));

        if (path is null)
            return;

        int index = path.Points.IndexOf(_currentEnemyPoint);

        if (index == -1)
            return;

        if (index < path.Points.Count - 1)
            _nextEnemyPoint = path.Points[index + 1];
        else
            _nextEnemyPoint = path.Next[0].Target.Points[0];
    }
}
