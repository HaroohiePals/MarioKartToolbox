#nullable enable
using HaroohiePals.Gui;
using HaroohiePals.Gui.View.Modal;
using HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;
using ImGuiNET;
using NativeFileDialogs.Net;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace HaroohiePals.MarioKartToolbox.Gui.View.CourseEditor;

internal class GenerateGlobalMapModalView(GenerateGlobalMapViewModel viewModel)
    : ModalView("Global Map Generator",
        new System.Numerics.Vector2(ImGuiEx.CalcUiScaledValue(600), ImGuiEx.CalcUiScaledValue(560)))
{
    private readonly GenerateGlobalMapViewModel _viewModel = viewModel;

    private int _offsetX;
    private int _offsetY;

    protected override void DrawContent()
    {
        ImGui.TextUnformatted("Generate the global map (minimap) graphics and coordinates from the course geometry.");
        ImGui.Separator();

        DrawSourceSelection();

        ImGui.Separator();

        DrawStatus();

        ImGui.Separator();

        DrawMode();

        ImGui.Separator();

        DrawCoordinates();

        ImGui.Separator();

        DrawOffset();

        ImGui.Separator();

        DrawActionButtons();
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

    private void DrawMode()
    {
        ImGui.TextUnformatted("Mode");
        ImGuiEx.ComboEnum("##Mode", ref _viewModel.Mode);
    }

    private void DrawCoordinates()
    {
        ImGui.TextUnformatted("Coordinates");

        bool canAuto = _viewModel.HasGeometry;
        if (!canAuto) ImGui.BeginDisabled();
        if (ImGui.Button("Auto-compute from geometry"))
            _viewModel.AutoComputeBounds();
        if (!canAuto) ImGui.EndDisabled();

        int[] tl = [(int)_viewModel.TopLeft.X, (int)_viewModel.TopLeft.Y];
        if (ImGui.DragInt2("Top Left", ref tl[0], 10f))
            _viewModel.TopLeft = new Vector2d(tl[0], tl[1]);

        int[] br = [(int)_viewModel.BottomRight.X, (int)_viewModel.BottomRight.Y];
        if (ImGui.DragInt2("Bottom Right", ref br[0], 10f))
            _viewModel.BottomRight = new Vector2d(br[0], br[1]);

        double sizeX = Math.Abs(_viewModel.BottomRight.X - _viewModel.TopLeft.X);
        double sizeY = Math.Abs(_viewModel.BottomRight.Y - _viewModel.TopLeft.Y);
        ImGui.TextDisabled($"Size: {sizeX:F0} x {sizeY:F0}");
    }

    private void DrawOffset()
    {
        ImGui.TextUnformatted("Position Offset");

        ImGui.PushItemWidth(ImGuiEx.CalcUiScaledValue(100));
        ImGui.DragInt("Delta X", ref _offsetX, 10f);
        ImGui.SameLine();
        ImGui.DragInt("Delta Y", ref _offsetY, 10f);
        ImGui.PopItemWidth();
        ImGui.SameLine();
        if (ImGui.Button("Apply Offset"))
        {
            _viewModel.ApplyOffset(_offsetX, _offsetY);
            _offsetX = 0;
            _offsetY = 0;
        }
    }

    private void DrawActionButtons()
    {
        bool canApply = !_viewModel.HasError;

        if (!canApply)
            ImGui.BeginDisabled();
        if (ImGui.Button("Apply") && _viewModel.Commit())
            Close();
        if (!canApply)
            ImGui.EndDisabled();
        ImGui.SameLine();
        if (ImGui.Button("Close"))
            Close();
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
