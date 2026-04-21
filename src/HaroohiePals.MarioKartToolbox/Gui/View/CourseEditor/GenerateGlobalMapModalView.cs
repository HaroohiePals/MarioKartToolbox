using HaroohiePals.Gui;
using HaroohiePals.Gui.View.Modal;
using HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;
using ImGuiNET;

namespace HaroohiePals.MarioKartToolbox.Gui.View.CourseEditor;

internal class GenerateGlobalMapModalView : ModalView
{
    private readonly GenerateGlobalMapViewModel _viewModel;

    public GenerateGlobalMapModalView(GenerateGlobalMapViewModel viewModel) : base("Global Map Generator",
        new System.Numerics.Vector2(ImGuiEx.CalcUiScaledValue(600), ImGuiEx.CalcUiScaledValue(500)))
    {
        _viewModel = viewModel;
    }

    protected override void DrawContent()
    {
        ImGui.TextUnformatted("Generate the global map (minimap) graphics and coordinates from the course geometry.");
        ImGui.Separator();

        ImGui.TextDisabled("UI coming in later steps.");

        ImGui.Separator();

        if (ImGui.Button("Close"))
            Close();
    }
}
