#nullable enable
using HaroohiePals.Gui;
using HaroohiePals.Gui.View.Modal;
using HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;
using ImGuiNET;
using NativeFileDialogs.Net;
using System.Collections.Generic;
using System.Numerics;

namespace HaroohiePals.MarioKartToolbox.Gui.View.CourseEditor;

internal class GenerateGlobalMapModalView(GenerateGlobalMapViewModel viewModel)
    : ModalView("Global Map Generator",
        new Vector2(ImGuiEx.CalcUiScaledValue(600), ImGuiEx.CalcUiScaledValue(500)))
{
    private readonly GenerateGlobalMapViewModel _viewModel = viewModel;

    protected override void DrawContent()
    {
        ImGui.TextUnformatted("Generate the global map (minimap) graphics and coordinates from the course geometry.");
        ImGui.Separator();

        DrawSourceSelection();

        ImGui.Separator();

        DrawStatus();

        ImGui.Separator();

        if (ImGui.Button("Close"))
            Close();
    }

    private void DrawSourceSelection()
    {
        ImGui.TextUnformatted("Source");

        int sourceInt = (int)_viewModel.Settings.SourceType;
        if (ImGui.RadioButton("Current course KCL (road triangles only)", ref sourceInt, (int)GenerateGlobalMapSourceType.CourseKcl))
        {
            _viewModel.Settings.SourceType = GenerateGlobalMapSourceType.CourseKcl;
            _viewModel.ReloadTriangles();
        }

        if (ImGui.RadioButton("External OBJ file (all triangles)", ref sourceInt, (int)GenerateGlobalMapSourceType.ExternalObj))
        {
            _viewModel.Settings.SourceType = GenerateGlobalMapSourceType.ExternalObj;
            _viewModel.ReloadTriangles();
        }

        if (_viewModel.Settings.SourceType == GenerateGlobalMapSourceType.ExternalObj)
        {
            ImGui.PushItemWidth(-ImGuiEx.CalcUiScaledValue(90));
            string path = _viewModel.Settings.ObjFilePath ?? "";
            if (ImGui.InputText("##ObjFilePath", ref path, 10000))
            {
                _viewModel.Settings.ObjFilePath = path;
            }
            ImGui.PopItemWidth();
            ImGui.SameLine();
            if (ImGui.Button("Browse...##ObjFilePath"))
            {
                BrowseForObjFile();
            }
        }
    }

    private void DrawStatus()
    {
        if (_viewModel.HasError)
        {
            ImGui.TextUnformatted($"Error: {_viewModel.ErrorMessage}");
            return;
        }

        ImGui.TextUnformatted($"{_viewModel.TriangleCount} triangle(s) loaded.");
    }

    private void BrowseForObjFile()
    {
        var result = Nfd.OpenDialog(out string? outPath, new Dictionary<string, string>
        {
            { "Wavefront OBJ", "obj" }
        });

        if (result != NfdStatus.Ok || string.IsNullOrEmpty(outPath))
            return;

        _viewModel.Settings.ObjFilePath = outPath;
        _viewModel.ReloadTriangles();
    }
}
