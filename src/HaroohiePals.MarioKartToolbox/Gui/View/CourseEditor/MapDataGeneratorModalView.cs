using HaroohiePals.Gui;
using HaroohiePals.Gui.View.Modal;
using HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;
using ImGuiNET;

namespace HaroohiePals.MarioKartToolbox.Gui.View.CourseEditor;

internal class MapDataGeneratorModalView : ModalView
{
    private readonly MapDataGeneratorViewModel _viewModel;

    public MapDataGeneratorModalView(MapDataGeneratorViewModel viewModel) : base("Map Data Generator",
        new System.Numerics.Vector2(ImGuiEx.CalcUiScaledValue(600), ImGuiEx.CalcUiScaledValue(500)))
    {
        _viewModel = viewModel;
    }

    protected override void DrawContent()
    {
        _viewModel.Update();

        ImGui.TextUnformatted("Note: Before starting, you must properly setup one between Item or Enemy Paths.");
        ImGui.TextUnformatted("The data will be overwritten. This operation can be undone later on.");

        ImGui.TextUnformatted("Generate from: ");
        ImGui.SameLine();
        ImGuiEx.ComboEnum("##Source Type", ref _viewModel.Settings.SourceType);

        bool topLevelDisabled = false;

        if (!_viewModel.CanGenerate || !_viewModel.ValidStartPoint || !_viewModel.ValidSourceType)
        {
            if (!_viewModel.CanGenerate)
                ImGui.TextUnformatted("WARNING: The generator doesn't support Battle stages.");
            if (!_viewModel.ValidStartPoint)
                ImGui.TextUnformatted("WARNING: The Start Point is not properly set. Please check your Map Data before proceding.");
            if (!_viewModel.ValidSourceType)
                ImGui.TextUnformatted("WARNING: The Source Type is not properly set. Please check your Map Data before proceding.");

            ImGui.BeginDisabled();
            topLevelDisabled = true;
        }

        if (ImGui.CollapsingHeader("Item/Enemy Path", ImGuiTreeNodeFlags.DefaultOpen))
        {
            if (_viewModel.Settings.SourceType == MapDataGeneratorSourceType.ItemPath)
                ImGuiEx.CheckboxFlags("Generate Enemy Path", ref _viewModel.Settings.Flags, MapDataGeneratorFlags.GenerateEnemyPath);
            if (_viewModel.Settings.SourceType == MapDataGeneratorSourceType.EnemyPath)
                ImGuiEx.CheckboxFlags("Generate Item Path", ref _viewModel.Settings.Flags, MapDataGeneratorFlags.GenerateItemPath);
        }

        if (ImGui.CollapsingHeader("Respawn Points", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Columns(2, "##Columns_RespawnPoints");

            ImGui.Text("Generate");
            ImGui.NextColumn();
            ImGuiEx.CheckboxFlags("##Generate##Respawn Points", ref _viewModel.Settings.Flags, MapDataGeneratorFlags.GenerateRespawn);
            ImGui.NextColumn();

            if (!topLevelDisabled && !_viewModel.Settings.Flags.HasFlag(MapDataGeneratorFlags.GenerateRespawn))
                ImGui.BeginDisabled();

            ImGui.Text("Skip Points");
            ImGui.NextColumn();
            ImGui.SliderInt("##Skip##Respawn Points", ref _viewModel.Settings.RespawnPointSkip, 1, 100);
            ImGui.NextColumn();


            ImGui.Text("Keep Source Path Bounds");
            ImGui.NextColumn();
            ImGui.Checkbox("##Keep Path Bounds##Respawn Points", ref _viewModel.Settings.RespawnPointKeepPathBoundaries);
            ImGui.NextColumn();

            if (!topLevelDisabled && !_viewModel.Settings.Flags.HasFlag(MapDataGeneratorFlags.GenerateRespawn))
                ImGui.EndDisabled();

            ImGui.Text("Find Nearest Item and Enemy Points");
            ImGui.NextColumn();
            ImGuiEx.CheckboxFlags("##Find Nearest Item/Enemy Points", ref _viewModel.Settings.Flags, MapDataGeneratorFlags.UpdateRespawnReferences);
            ImGui.NextColumn();

            ImGui.Columns(1);
        }

        if (ImGui.CollapsingHeader("Checkpoints", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Columns(2, "##Columns_Checkpoints");

            ImGui.Text("Generate");
            ImGui.NextColumn();
            ImGuiEx.CheckboxFlags("##Generate Checkpoints", ref _viewModel.Settings.Flags, MapDataGeneratorFlags.GenerateCheckPoint);
            ImGui.NextColumn();

            if (!topLevelDisabled && !_viewModel.Settings.Flags.HasFlag(MapDataGeneratorFlags.GenerateCheckPoint))
                ImGui.BeginDisabled();

            ImGui.Text("Skip Points");
            ImGui.NextColumn();
            ImGui.SliderInt("##Skip##Checkpoints", ref _viewModel.Settings.CheckPointPointSkip, 1, 100);
            ImGui.NextColumn();

            ImGui.Text("Keep Source Path Bounds");
            ImGui.NextColumn();
            ImGui.Checkbox("##Keep Path Bounds##Checkpoints", ref _viewModel.Settings.CheckPointKeepPathBoundaries);
            ImGui.NextColumn();

            ImGui.Text("Width");
            ImGui.NextColumn();
            float width = (float)_viewModel.Settings.CheckPointWidth;
            if (ImGui.SliderFloat("##Width##Checkpoints", ref width, 50, 500))
                _viewModel.Settings.CheckPointWidth = width;
            ImGui.NextColumn();

            if (!topLevelDisabled && !_viewModel.Settings.Flags.HasFlag(MapDataGeneratorFlags.GenerateCheckPoint))
                ImGui.EndDisabled();

            ImGui.Text("Find Nearest Respawn Points");
            ImGui.NextColumn();
            ImGuiEx.CheckboxFlags("##Find Nearest Respawn Points", ref _viewModel.Settings.Flags, MapDataGeneratorFlags.UpdateCheckPointReferences);
            ImGui.NextColumn();

            ImGui.Columns(1);
        }

        if (ImGui.Button("Generate") && _viewModel.PerformGenerate())
            Close();

        if (!_viewModel.CanGenerate || !_viewModel.ValidStartPoint || !_viewModel.ValidSourceType)
        {
            ImGui.EndDisabled();
        }
    }
}
