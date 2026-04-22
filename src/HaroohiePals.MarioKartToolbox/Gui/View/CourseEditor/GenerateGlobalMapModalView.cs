#nullable enable
using HaroohiePals.Graphics3d;
using HaroohiePals.Graphics3d.OpenGL;
using HaroohiePals.Gui;
using HaroohiePals.Gui.View.Modal;
using HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;
using HaroohiePals.MarioKartToolbox.Tools;
using ImGuiNET;
using NativeFileDialogs.Net;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace HaroohiePals.MarioKartToolbox.Gui.View.CourseEditor;

class GenerateGlobalMapModalView(GenerateGlobalMapViewModel viewModel)
    : ModalView("Global Map Generator",
        new System.Numerics.Vector2(ImGuiEx.CalcUiScaledValue(600), ImGuiEx.CalcUiScaledValue(820)))
{
    private int _offsetX;
    private int _offsetY;

    private GLTexture? _previewTexture;
    private int _previewTextureVersion = -1;

    private GLTexture? _courseBgTexture;
    private GLTexture? _hudOverlayTexture;
    private bool _backgroundTexturesInitialized;

    protected override void DrawContent()
    {
        if (_previewTextureVersion == -1 && viewModel.HasGeometry)
        {
            viewModel.ReloadTriangles();
            viewModel.RenderPreview();
            RefreshPreviewTextureIfNeeded();
        }
        
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

        DrawPngExport();

        ImGui.Separator();

        DrawActionButtons();
    }

    protected override void OnClose()
    {
        _previewTexture?.Dispose();
        _previewTexture = null;

        _courseBgTexture?.Dispose();
        _courseBgTexture = null;

        _hudOverlayTexture?.Dispose();
        _hudOverlayTexture = null;

        _backgroundTexturesInitialized = false;
    }

    private void DrawSourceSelection()
    {
        ImGui.TextUnformatted("Source");

        int sourceInt = (int)viewModel.Settings.SourceType;
        if (ImGui.RadioButton("Current course KCL (road triangles only)", ref sourceInt,
                (int)GenerateGlobalMapSourceType.CourseKcl))
        {
            viewModel.Settings.SourceType = GenerateGlobalMapSourceType.CourseKcl;
            viewModel.ReloadTriangles();
            viewModel.RenderPreview();
            RefreshPreviewTextureIfNeeded();
        }

        if (ImGui.RadioButton("External OBJ file (all triangles)", ref sourceInt,
                (int)GenerateGlobalMapSourceType.ExternalObj))
        {
            viewModel.Settings.SourceType = GenerateGlobalMapSourceType.ExternalObj;
            viewModel.ReloadTriangles();
        }

        if (viewModel.Settings.SourceType == GenerateGlobalMapSourceType.ExternalObj)
        {
            ImGui.PushItemWidth(-ImGuiEx.CalcUiScaledValue(90));
            string path = viewModel.Settings.ObjFilePath ?? "";
            if (ImGui.InputText("##ObjFilePath", ref path, 10000))
            {
                viewModel.Settings.ObjFilePath = path;
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
        if (viewModel.HasError)
        {
            ImGui.TextUnformatted($"Error: {viewModel.ErrorMessage}");
            return;
        }

        ImGui.TextUnformatted($"{viewModel.TriangleCount} triangle(s) loaded.");
    }

    private void DrawMode()
    {
        ImGui.TextUnformatted("Mode");
        if (!ImGuiEx.ComboEnum("##Mode", ref viewModel.Mode)) 
            return;
        
        viewModel.AutoComputeBounds();
        viewModel.RenderPreview();
        RefreshPreviewTextureIfNeeded();
    }

    private void DrawCoordinates()
    {
        ImGui.TextUnformatted("Coordinates");

        bool canAuto = viewModel.HasGeometry;
        if (!canAuto) ImGui.BeginDisabled();
        if (ImGui.Button("Auto-compute from geometry"))
        {
            viewModel.AutoComputeBounds();
            viewModel.RenderPreview();
            RefreshPreviewTextureIfNeeded();
        }
        if (!canAuto) ImGui.EndDisabled();

        int[] tl = [(int)viewModel.TopLeft.X, (int)viewModel.TopLeft.Y];
        if (ImGui.DragInt2("Top Left", ref tl[0], 10f))
            viewModel.TopLeft = new Vector2d(tl[0], tl[1]);
        if (ImGui.IsItemDeactivatedAfterEdit())
        {
            viewModel.RenderPreview();
            RefreshPreviewTextureIfNeeded();
        }
        
        int[] br = [(int)viewModel.BottomRight.X, (int)viewModel.BottomRight.Y];
        if (ImGui.DragInt2("Bottom Right", ref br[0], 10f))
            viewModel.BottomRight = new Vector2d(br[0], br[1]);
        if (ImGui.IsItemDeactivatedAfterEdit())
        {
            viewModel.RenderPreview();
            RefreshPreviewTextureIfNeeded();
        }

        double sizeX = Math.Abs(viewModel.BottomRight.X - viewModel.TopLeft.X);
        double sizeY = Math.Abs(viewModel.BottomRight.Y - viewModel.TopLeft.Y);
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
            viewModel.ApplyOffset(_offsetX, _offsetY);
            viewModel.RenderPreview();
            RefreshPreviewTextureIfNeeded();
            _offsetX = 0;
            _offsetY = 0;
        }
    }

    private void DrawPngExport()
    {
        ImGui.TextUnformatted("PNG Export");

        ImGui.PushItemWidth(ImGuiEx.CalcUiScaledValue(140));
        ImGui.DragFloat("Triangle Expansion (px)", ref viewModel.TriangleExpansion, 0.1f, 0f, 5f, "%.2f");
        if (ImGui.IsItemDeactivatedAfterEdit())
        {
            viewModel.RenderPreview();
            RefreshPreviewTextureIfNeeded();
        }
        ImGui.PopItemWidth();

        bool canRender = viewModel.HasGeometry;
        if (!canRender) 
            ImGui.BeginDisabled();
        if (ImGui.Button("Save PNG..."))
            BrowseAndSavePng();
        if (!canRender) 
            ImGui.EndDisabled();

        RefreshPreviewTextureIfNeeded();

        if (_previewTexture is not null)
        {
            float side = ImGuiEx.CalcUiScaledValue(GlobalMapRasterizer.CANVAS_WIDTH);
            float displayH = ImGuiEx.CalcUiScaledValue(192);
            var origin = ImGui.GetCursorPos();

            DrawLayer(_courseBgTexture,   origin, side, displayH);
            DrawLayer(_hudOverlayTexture, origin, side, displayH);
            DrawLayer(_previewTexture,    origin, side, side);

            ImGui.SetCursorPos(origin);
            ImGui.Dummy(new System.Numerics.Vector2(side, side));
        }
        else
        {
            ImGui.TextDisabled("Click Render Preview or Save PNG to generate a thumbnail.");
        }
    }

    private static void DrawLayer(GLTexture? tex, System.Numerics.Vector2 origin, float w, float h)
    {
        if (tex is null)
            return;
        ImGui.SetCursorPos(origin);
        ImGui.Image(tex.Handle, new System.Numerics.Vector2(w, h));
    }

    private void DrawActionButtons()
    {
        bool canApply = !viewModel.HasError;

        if (!canApply)
            ImGui.BeginDisabled();
        if (ImGui.Button("Apply") && viewModel.Commit())
            Close();
        if (!canApply)
            ImGui.EndDisabled();
        ImGui.SameLine();
        if (ImGui.Button("Close"))
            Close();
    }

    private void RefreshPreviewTextureIfNeeded()
    {
        EnsureBackgroundTextures();

        if (viewModel.PreviewBitmap is null || _previewTextureVersion == viewModel.PreviewVersion)
            return;

        _previewTexture?.Dispose();
        _previewTexture = new GLTexture(viewModel.PreviewBitmap, TextureWrapMode.Clamp, TextureWrapMode.Clamp);
        _previewTexture.SetFilterMode(TextureFilterMode.Nearest, TextureFilterMode.Nearest);
        _previewTextureVersion = viewModel.PreviewVersion;
    }

    private void EnsureBackgroundTextures()
    {
        if (_backgroundTexturesInitialized)
            return;
        _backgroundTexturesInitialized = true;

        if (viewModel.BackgroundBitmap is not null)
        {
            _courseBgTexture = new GLTexture(
                viewModel.BackgroundBitmap, TextureWrapMode.Clamp, TextureWrapMode.Clamp);
            _courseBgTexture.SetFilterMode(TextureFilterMode.Nearest, TextureFilterMode.Nearest);
        }

        if (viewModel.HudOverlayBitmap is not null)
        {
            _hudOverlayTexture = new GLTexture(
                viewModel.HudOverlayBitmap, TextureWrapMode.Clamp, TextureWrapMode.Clamp);
            _hudOverlayTexture.SetFilterMode(TextureFilterMode.Nearest, TextureFilterMode.Nearest);
        }
    }

    private void BrowseForObjFile()
    {
        var result = Nfd.OpenDialog(out string? outPath, new Dictionary<string, string>
        {
            { "Wavefront OBJ", "obj" }
        });

        if (result != NfdStatus.Ok || string.IsNullOrEmpty(outPath))
            return;

        viewModel.Settings.ObjFilePath = outPath;
        viewModel.ReloadTriangles();
        viewModel.RenderPreview();
        RefreshPreviewTextureIfNeeded();
    }

    private void BrowseAndSavePng()
    {
        var result = Nfd.SaveDialog(out string? outPath, new Dictionary<string, string>
        {
            { "PNG Image", "png" }
        });

        if (result != NfdStatus.Ok || string.IsNullOrEmpty(outPath))
            return;

        if (!outPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            outPath += ".png";

        viewModel.RenderPreview();
        RefreshPreviewTextureIfNeeded();
        viewModel.SavePng(outPath);
    }
}