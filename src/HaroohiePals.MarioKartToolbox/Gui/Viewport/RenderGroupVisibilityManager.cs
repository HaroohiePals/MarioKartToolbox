using HaroohiePals.Gui;
using HaroohiePals.Gui.Viewport;
using HaroohiePals.MarioKartToolbox.Application.Settings;
using HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups;
using HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups.KCollision;
using HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups.MapData;
using HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups.NitroSystem;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using ImGuiNET;
using System.Collections.Generic;
using System.Linq;

namespace HaroohiePals.MarioKartToolbox.Gui.Viewport;

class RenderGroupVisibilityManager
{
    public enum VisibilityType
    {
        Hidden,
        Wireframe,
        Solid,
        Light
    }

    public enum VisibleEntity
    {
        Grid,
        Course,
        Skybox,
        Collision,
        MapObjects,
        MapObjectModels,
        Paths,
        StartPoints,
        StartPointsGrid,
        RespawnPoints,
        KartPoint2d,
        CannonPoints,
        MissionPoints,
        CheckPoints,
        ItemPoints,
        ItemPointsRadius,
        EnemyPoints,
        EnemyPointsRadius,
        Areas,
        AreaShapes,
        Cameras,
        CameraTargets
    }

    private readonly IApplicationSettingsService _applicationSettings;
    private readonly Dictionary<VisibleEntity, VisibilityType> _visibility = new();
    private readonly RenderGroupScene _scene;

    private readonly string _preferencesKey;
    private bool _kclTranslucent = false;
    private bool _areaShapeShowAll = false;
    private bool _mobjEditMode = true;

    private bool _updateSettings = true;

    private static bool HasWireframeMode(VisibleEntity entity)
        => entity is VisibleEntity.Course or VisibleEntity.Skybox or VisibleEntity.Collision;

    public int EntityCount => _visibility.Count;

    public RenderGroupVisibilityManager(RenderGroupScene scene, string preferencesKey,
        IApplicationSettingsService applicationSettings)
    {
        _applicationSettings = applicationSettings;
        _scene = scene;
        _preferencesKey = preferencesKey;

        if (_applicationSettings.Settings.Viewport.VisibleEntities.ContainsKey(_preferencesKey))
        {
            _visibility = new Dictionary<VisibleEntity, VisibilityType>(
                _applicationSettings.Settings.Viewport.VisibleEntities[_preferencesKey]);
        }

        if (scene is NitroKartRenderGroupScenePerspective)
            AddVisibilitySetting(VisibleEntity.Grid, VisibilityType.Solid);
        AddVisibilitySetting(VisibleEntity.Course, VisibilityType.Solid);
        AddVisibilitySetting(VisibleEntity.Skybox, VisibilityType.Solid);
        AddVisibilitySetting(VisibleEntity.Collision, VisibilityType.Hidden);
        AddVisibilitySetting(VisibleEntity.MapObjectModels, VisibilityType.Solid);
        AddVisibilitySetting(VisibleEntity.MapObjects, VisibilityType.Solid);
        AddVisibilitySetting(VisibleEntity.Paths, VisibilityType.Solid);
        AddVisibilitySetting(VisibleEntity.StartPoints, VisibilityType.Solid);

        if (scene is NitroKartRenderGroupScenePerspective)
            AddVisibilitySetting(VisibleEntity.StartPointsGrid, VisibilityType.Solid);
        AddVisibilitySetting(VisibleEntity.RespawnPoints, VisibilityType.Solid);
        AddVisibilitySetting(VisibleEntity.KartPoint2d, VisibilityType.Solid);
        AddVisibilitySetting(VisibleEntity.CannonPoints, VisibilityType.Solid);
        AddVisibilitySetting(VisibleEntity.MissionPoints, VisibilityType.Solid);
        if (scene is NitroKartRenderGroupSceneTopDown)
            AddVisibilitySetting(VisibleEntity.CheckPoints, VisibilityType.Solid);
        AddVisibilitySetting(VisibleEntity.ItemPoints, VisibilityType.Solid);
        if (scene is NitroKartRenderGroupSceneTopDown)
            AddVisibilitySetting(VisibleEntity.ItemPointsRadius, VisibilityType.Solid);
        AddVisibilitySetting(VisibleEntity.EnemyPoints, VisibilityType.Solid);
        if (scene is NitroKartRenderGroupSceneTopDown)
            AddVisibilitySetting(VisibleEntity.EnemyPointsRadius, VisibilityType.Solid);
        AddVisibilitySetting(VisibleEntity.Areas, VisibilityType.Solid);
        AddVisibilitySetting(VisibleEntity.AreaShapes, VisibilityType.Solid);
        AddVisibilitySetting(VisibleEntity.Cameras, VisibilityType.Solid);
        AddVisibilitySetting(VisibleEntity.CameraTargets, VisibilityType.Solid);

        Update();
    }

    private void AddVisibilitySetting(VisibleEntity key, VisibilityType defaultValue)
    {
        if (!_visibility.ContainsKey(key))
            _visibility.Add(key, defaultValue);
    }

    public void Update()
    {
        if (!_updateSettings)
            return;

        if (_scene.RenderGroups.Count == 0)
            return;

        foreach (var r in _scene.RenderGroups)
        {
            var entity = VisibleEntity.MapObjects;

            switch (r)
            {
                case PlaneGridRenderGroup gridRenderGroup:
                    entity = VisibleEntity.Grid;
                    break;
                case CourseModelRenderGroup nsbmdRenderGroup:
                    nsbmdRenderGroup.EnableCourseModel = _visibility[VisibleEntity.Course] != VisibilityType.Hidden;
                    nsbmdRenderGroup.EnableCourseModelV = _visibility[VisibleEntity.Skybox] != VisibilityType.Hidden;
                    nsbmdRenderGroup.WireframeCourseModel = _visibility[VisibleEntity.Course] == VisibilityType.Wireframe;
                    nsbmdRenderGroup.WireframeCourseModelV = _visibility[VisibleEntity.Skybox] == VisibilityType.Wireframe;
                    continue;
                case KclPrismRenderGroup kcl:
                    kcl.Enabled = _visibility[VisibleEntity.Collision] != VisibilityType.Hidden;
                    kcl.ColoringMode = _visibility[VisibleEntity.Collision] == VisibilityType.Light
                        ? KclPrismRenderGroup.PrismColoringMode.Light
                        : KclPrismRenderGroup.PrismColoringMode.Material;
                    kcl.Wireframe = _visibility[VisibleEntity.Collision] == VisibilityType.Wireframe;
                    kcl.Seethrough = _kclTranslucent;
                    continue;
                case MkdsMObjRenderGroup mobjRenderGroup:
                    entity = VisibleEntity.MapObjectModels;
                    mobjRenderGroup.EditMode = _mobjEditMode;
                    break;
                case MapDataCollectionRenderGroup<MkdsMapObject>:
                    entity = VisibleEntity.MapObjects;
                    break;
                case MapDataCollectionRenderGroup<MkdsStartPoint>:
                    entity = VisibleEntity.StartPoints;
                    break;
                case KartPointRenderGroup<MkdsStartPoint>:
                    entity = VisibleEntity.StartPointsGrid;
                    break;
                case MapDataCollectionRenderGroup<MkdsRespawnPoint>:
                    entity = VisibleEntity.RespawnPoints;
                    break;
                case MapDataCollectionRenderGroup<MkdsKartPoint2d>:
                    entity = VisibleEntity.KartPoint2d;
                    break;
                case MapDataCollectionRenderGroup<MkdsCannonPoint>:
                    entity = VisibleEntity.CannonPoints;
                    break;
                case MapDataCollectionRenderGroup<MkdsKartPointMission>:
                    entity = VisibleEntity.MissionPoints;
                    break;
                case MapDataCollectionRenderGroup<MkdsArea>:
                    entity = VisibleEntity.Areas;
                    break;
                case AreaShapeRenderGroup areaShapeRenderGroup:
                    entity = VisibleEntity.AreaShapes;
                    areaShapeRenderGroup.ShowAll = _areaShapeShowAll;
                    break;
                case MapDataCollectionRenderGroup<MkdsCamera>:
                    entity = VisibleEntity.Cameras;
                    break;
                case CameraTargetsRenderGroup:
                    entity = VisibleEntity.CameraTargets;
                    break;
                case PathRenderGroup:
                case PathPointRenderGroup:
                    entity = VisibleEntity.Paths;
                    break;
                case MepoLineRenderGroup:
                case MepoPointRenderGroup:
                case IpoiEpoiLineRenderGroup<MkdsEnemyPath, MkdsEnemyPoint>:
                case IpoiEpoiPointRenderGroup<MkdsEnemyPath, MkdsEnemyPoint>:
                    entity = VisibleEntity.EnemyPoints;
                    break;

                case MepoRadiusRenderGroup:
                case IpoiEpoiRadiusRenderGroup<MkdsEnemyPath, MkdsEnemyPoint>:
                    entity = VisibleEntity.EnemyPointsRadius;
                    break;

                case IpoiEpoiLineRenderGroup<MkdsItemPath, MkdsItemPoint>:
                case IpoiEpoiPointRenderGroup<MkdsItemPath, MkdsItemPoint>:
                    entity = VisibleEntity.ItemPoints;
                    break;

                case IpoiEpoiRadiusRenderGroup<MkdsItemPath, MkdsItemPoint>:
                    entity = VisibleEntity.ItemPointsRadius;
                    break;

                case CheckPointPathRenderGroup:
                case CheckPointRenderGroup:
                    entity = VisibleEntity.CheckPoints;
                    break;
            }

            r.Enabled = _visibility[entity] != VisibilityType.Hidden;
        }

        _applicationSettings.Set((ref ApplicationSettings settings) =>
        {
            var dict = new Dictionary<string, IReadOnlyDictionary<VisibleEntity, VisibilityType>>(
                settings.Viewport.VisibleEntities);
            dict[_preferencesKey] = new Dictionary<VisibleEntity, VisibilityType>(_visibility);
            settings.Viewport.VisibleEntities = dict;
        });

        _updateSettings = false;
    }

    public void Draw(float x, float y)
    {
        float scale = ImGuiEx.GetUiScale();
        float btnSize = 24f * scale;

        uint selectedColor = ImGui.GetColorU32(ImGuiCol.ButtonHovered) | 0xFF000000;

        foreach (var item in _visibility.Keys.OrderBy(x => (int)x))
        {
            int itemCount = 5;

            bool hasWireframeMode = HasWireframeMode(item);

            ImGui.SetCursorPosX(x);
            ImGui.SetCursorPosY(y);

            ImGui.Text(item.ToString());

            var controlWidth = ImGui.GetContentRegionMax().X;

            ImGui.SameLine();
            ImGui.SetCursorPosX(controlWidth - btnSize * itemCount-- - (10f * scale));

            var targetVisibilityType = _visibility[item];

            if (_visibility[item] == VisibilityType.Hidden)
                ImGui.PushStyleColor(ImGuiCol.Button, selectedColor);
            ImGui.Button($"{FontAwesome6.EyeSlash}", new(btnSize));
            if (ImGui.IsItemClicked())
            {
                targetVisibilityType = VisibilityType.Hidden;
            }

            if (_visibility[item] == VisibilityType.Hidden)
                ImGui.PopStyleColor();

            ImGui.SameLine();
            ImGui.SetCursorPosX(controlWidth - btnSize * itemCount-- - (10f * scale));
            if (_visibility[item] == VisibilityType.Solid)
                ImGui.PushStyleColor(ImGuiCol.Button, selectedColor);
            ImGui.Button($"{(hasWireframeMode ? FontAwesome6.Circle : FontAwesome6.Eye)}",
                new(btnSize));
            if (ImGui.IsItemClicked())
            {
                targetVisibilityType = VisibilityType.Solid;
            }

            if (_visibility[item] == VisibilityType.Solid)
                ImGui.PopStyleColor();

            if (hasWireframeMode)
            {
                ImGui.SameLine();
                ImGui.SetCursorPosX(controlWidth - btnSize * itemCount-- - (10f * scale));
                if (_visibility[item] == VisibilityType.Wireframe)
                    ImGui.PushStyleColor(ImGuiCol.Button, selectedColor);
                ImGui.Button($"{FontAwesome6.Globe}", new(btnSize));
                if (ImGui.IsItemClicked())
                {
                    targetVisibilityType = VisibilityType.Wireframe;
                }

                if (_visibility[item] == VisibilityType.Wireframe)
                    ImGui.PopStyleColor();
            }

            if (item == VisibleEntity.Collision)
            {
                // light
                ImGui.SameLine();
                ImGui.SetCursorPosX(controlWidth - btnSize * itemCount-- - (10f * scale));
                if (_visibility[item] == VisibilityType.Light)
                    ImGui.PushStyleColor(ImGuiCol.Button, selectedColor);
                ImGui.Button($"{FontAwesome6.Lightbulb}", new(btnSize));
                if (ImGui.IsItemClicked())
                    targetVisibilityType = VisibilityType.Light;
                if (_visibility[item] == VisibilityType.Light)
                    ImGui.PopStyleColor();

                // translucent
                ImGui.SameLine();
                ImGui.SetCursorPosX(controlWidth - btnSize * itemCount-- - (10f * scale));
                bool wasTranslucent = _kclTranslucent;
                if (wasTranslucent)
                    ImGui.PushStyleColor(ImGuiCol.Button, selectedColor);
                ImGui.Button($"{FontAwesome6.CircleHalfStroke}", new(btnSize));
                if (ImGui.IsItemClicked())
                {
                    _kclTranslucent = !_kclTranslucent;
                    _updateSettings = true;
                }

                if (wasTranslucent)
                    ImGui.PopStyleColor();
            }

            if (item == VisibleEntity.AreaShapes)
            {
                // all
                ImGui.SameLine();
                ImGui.SetCursorPosX(controlWidth - btnSize * itemCount-- - (10f * scale));
                bool wasAll = _areaShapeShowAll;
                if (wasAll)
                    ImGui.PushStyleColor(ImGuiCol.Button, selectedColor);
                ImGui.Button($"{FontAwesome6.ListCheck}", new(btnSize));
                if (ImGui.IsItemClicked())
                {
                    _areaShapeShowAll = !_areaShapeShowAll;
                    _updateSettings = true;
                }

                if (wasAll)
                    ImGui.PopStyleColor();
            }

            if (item == VisibleEntity.MapObjectModels)
            {
                // all
                ImGui.SameLine();
                ImGui.SetCursorPosX(controlWidth - btnSize * itemCount-- - (10f * scale));
                bool wasEditMode = _mobjEditMode;
                ImGui.Button(wasEditMode ? $"{FontAwesome6.Play}" : $"{FontAwesome6.Stop}", new(btnSize));
                if (ImGui.IsItemClicked())
                {
                    _mobjEditMode = !_mobjEditMode;
                    _updateSettings = true;
                }

                if (wasEditMode)
                    ImGui.PopStyleColor();
            }

            if (_visibility[item] != targetVisibilityType)
                _updateSettings = true;

            _visibility[item] = targetVisibilityType;

            y += btnSize;
        }
    }
}