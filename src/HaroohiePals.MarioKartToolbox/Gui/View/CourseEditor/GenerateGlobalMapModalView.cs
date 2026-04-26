#nullable enable
using HaroohiePals.Graphics3d;
using HaroohiePals.Graphics3d.OpenGL;
using HaroohiePals.Gui;
using HaroohiePals.Gui.View.Modal;
using HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;
using ImGuiNET;
using NativeFileDialogs.Net;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace HaroohiePals.MarioKartToolbox.Gui.View.CourseEditor;

class GenerateGlobalMapModalView(GenerateGlobalMapViewModel viewModel)
    : ModalView(MODAL_TITLE, ModalSize)
{
    private const string MODAL_TITLE = "Global Map Generator";
    private const uint SAFE_RECT_COLOR = 0xFF0000FF; // Red

    private static readonly System.Numerics.Vector2 ModalSize = new(ImGuiEx.CalcUiScaledValue(720),
        ImGuiEx.CalcUiScaledValue(500));

    private GenerateGlobalMapModalViewStep _curStep = GenerateGlobalMapModalViewStep.Source;

    private GLTexture? _previewTexture;
    private int _previewTextureVersion;

    private GLTexture? _courseBgTexture;
    private GLTexture? _hudOverlayTexture;
    private bool _backgroundTexturesInitialized;

    private bool _isPanning;
    private System.Numerics.Vector2 _panAccumPx;

    private bool _configureColumnsInitialized;

    protected override void DrawContent()
    {
        bool canContinue = false;
        if (ImGui.BeginChild("##GenerateGlobalMap_Content",
                new System.Numerics.Vector2(0, -ImGui.GetFrameHeightWithSpacing())))
        {
            ImGui.Text($"Step {(int)_curStep + 1}/{(int)GenerateGlobalMapModalViewStep.Last + 1}");
            ImGui.Separator();
            canContinue = _curStep switch
            {
                GenerateGlobalMapModalViewStep.Source => DrawSourceStep(),
                GenerateGlobalMapModalViewStep.Configure => DrawConfigureStep(),
                _ => false
            };
        }

        ImGui.EndChild();
        DrawNavigationButtons(canContinue);
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
        _configureColumnsInitialized = false;
    }

    private bool DrawSourceStep()
    {
        ImGui.TextUnformatted(
            "Generate the global map (minimap) graphics and coordinates from the course geometry.");
        ImGui.Separator();

        ImGui.TextUnformatted("Source");

        int sourceInt = (int)viewModel.Settings.SourceType;
        if (ImGui.RadioButton("Current course KCL (road triangles only)", ref sourceInt,
                (int)GenerateGlobalMapSourceType.CourseKcl))
        {
            viewModel.Settings.SourceType = GenerateGlobalMapSourceType.CourseKcl;
            viewModel.ReloadTriangles();
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
                viewModel.Settings.ObjFilePath = path;
            ImGui.PopItemWidth();
            ImGui.SameLine();
            if (ImGui.Button("Browse...##ObjFilePath"))
                BrowseForObjFile();
        }

        ImGui.Separator();

        if (viewModel.HasError)
            ImGui.TextUnformatted($"Error: {viewModel.ErrorMessage}");
        else
            ImGui.TextUnformatted($"{viewModel.TriangleCount} triangle(s) loaded.");

        return viewModel.HasGeometry && !viewModel.HasError && viewModel.TriangleCount > 0;
    }

    private bool DrawConfigureStep()
    {
        ImGui.Columns(2, "##GenerateGlobalMap_Configure");
        if (!_configureColumnsInitialized)
        {
            ImGui.SetColumnWidth(0, Size.X - ImGuiEx.CalcUiScaledValue(280));
            _configureColumnsInitialized = true;
        }

        DrawSettingsPanel();

        ImGui.NextColumn();

        DrawPreviewPanel();

        ImGui.NextColumn();
        ImGui.Columns(1);

        return !viewModel.HasError;
    }

    private void DrawSettingsPanel()
    {
        if (ImGui.BeginChild("##GenerateGlobalMap_Settings", ImGui.GetContentRegionAvail()))
        {
            DrawCoordinatesSection();
            DrawSafeAreaSection();
            DrawExpansionSection();
            DrawStartMarkerSection();
        }

        ImGui.EndChild();
    }

    private void DrawStartMarkerSection()
    {
        if (!ImGui.CollapsingHeader("Start Grid Marker##GenerateGlobalMap_StartMarker",
                ImGuiTreeNodeFlags.DefaultOpen))
            return;

        ImGui.Columns(2, "##Columns_StartMarker");

        ImGui.Text("Show");
        ImGui.NextColumn();
        if (ImGui.Checkbox("##ShowStartMarker", ref viewModel.Settings.ShowStartMarker))
        {
            viewModel.RenderPreview();
            RefreshPreviewTextureIfNeeded();
        }

        ImGui.NextColumn();

        if (!viewModel.Settings.ShowStartMarker) ImGui.BeginDisabled();
        ImGui.Text("Width (px)");
        ImGui.NextColumn();
        ImGui.PushItemWidth(-1);
        ImGui.DragInt("##StartMarkerWidth", ref viewModel.Settings.StartMarkerWidth, 1f, 1, 64);
        if (ImGui.IsItemDeactivatedAfterEdit())
        {
            viewModel.RenderPreview();
            RefreshPreviewTextureIfNeeded();
        }

        ImGui.PopItemWidth();
        ImGui.NextColumn();

        ImGui.Text("Show Label");
        ImGui.NextColumn();
        if (ImGui.Checkbox("##ShowStartMarkerLabel", ref viewModel.Settings.ShowStartMarkerLabel))
        {
            viewModel.RenderPreview();
            RefreshPreviewTextureIfNeeded();
        }

        ImGui.NextColumn();

        if (!viewModel.Settings.ShowStartMarkerLabel) ImGui.BeginDisabled();
        int[] labelOff = [viewModel.Settings.StartMarkerLabelOffset.X, viewModel.Settings.StartMarkerLabelOffset.Y];
        if (DragInt2Row("StartMarkerLabelOffset", "Label Offset (px)", labelOff, 1f, 0, 0, out bool labelOffDeact))
            viewModel.Settings.StartMarkerLabelOffset = new Vector2i(labelOff[0], labelOff[1]);
        if (labelOffDeact)
        {
            viewModel.RenderPreview();
            RefreshPreviewTextureIfNeeded();
        }

        if (!viewModel.Settings.ShowStartMarkerLabel) ImGui.EndDisabled();

        if (!viewModel.Settings.ShowStartMarker) ImGui.EndDisabled();

        ImGui.Columns(1);
    }

    private void DrawCoordinatesSection()
    {
        if (!ImGui.CollapsingHeader("Coordinates##GenerateGlobalMap_Coordinates", ImGuiTreeNodeFlags.DefaultOpen))
            return;

        ImGui.Columns(2, "##Columns_Coords");

        int[] tl = [(int)viewModel.Settings.TopLeft.X, (int)viewModel.Settings.TopLeft.Y];
        if (DragInt2Row("CoordTL", "Top Left", tl, 10f, 0, 0, out bool tlDeact))
            viewModel.Settings.TopLeft = new Vector2d(tl[0], tl[1]);
        if (tlDeact)
        {
            viewModel.RenderPreview();
            RefreshPreviewTextureIfNeeded();
        }

        int[] br = [(int)viewModel.Settings.BottomRight.X, (int)viewModel.Settings.BottomRight.Y];
        if (DragInt2Row("CoordBR", "Bottom Right", br, 10f, 0, 0, out bool brDeact))
            viewModel.Settings.BottomRight = new Vector2d(br[0], br[1]);
        if (brDeact)
        {
            viewModel.RenderPreview();
            RefreshPreviewTextureIfNeeded();
        }

        ImGui.Text("Mode");
        ImGui.NextColumn();
        ImGui.PushItemWidth(-1);
        if (ImGuiEx.ComboEnum("##Mode", ref viewModel.Settings.Mode))
        {
            viewModel.AutoComputeBounds();
            viewModel.RenderPreview();
            RefreshPreviewTextureIfNeeded();
        }

        ImGui.PopItemWidth();
        ImGui.NextColumn();

        ImGui.Columns(1);

        double sizeX = Math.Abs(viewModel.Settings.BottomRight.X - viewModel.Settings.TopLeft.X);
        double sizeY = Math.Abs(viewModel.Settings.BottomRight.Y - viewModel.Settings.TopLeft.Y);
        ImGui.TextDisabled($"Size: {sizeX:F0} x {sizeY:F0}");
        ImGui.SameLine();

        bool canAuto = viewModel.HasGeometry;
        if (!canAuto) ImGui.BeginDisabled();
        if (ImGui.Button("Auto-compute"))
        {
            viewModel.AutoComputeBounds();
            viewModel.RenderPreview();
            RefreshPreviewTextureIfNeeded();
        }

        if (!canAuto) ImGui.EndDisabled();

        ImGui.SameLine();

        bool canLoadExisting = viewModel.HasExistingSettings;
        if (!canLoadExisting) ImGui.BeginDisabled();
        if (ImGui.Button("Load existing"))
        {
            if (viewModel.LoadExistingSettings())
            {
                viewModel.RenderPreview();
                RefreshPreviewTextureIfNeeded();
            }
        }

        if (!canLoadExisting) ImGui.EndDisabled();
    }

    private void DrawSafeAreaSection()
    {
        if (!ImGui.CollapsingHeader("Safe Area##GenerateGlobalMap_SafeArea", ImGuiTreeNodeFlags.DefaultOpen))
            return;

        ImGui.TextDisabled("Canvas pixels (0..256)");

        ImGui.Columns(2, "##Columns_Safe");

        var safe = viewModel.Settings.SafeArea;
        int[] sa0 = [safe.Min.X, safe.Min.Y];
        if (DragInt2Row("SafeTL", "Top Left", sa0, 1f, 0, 256, out bool sa0Deact))
            viewModel.Settings.SafeArea = new Box2i(new Vector2i(sa0[0], sa0[1]), safe.Max);
        if (sa0Deact)
        {
            viewModel.RenderPreview();
            RefreshPreviewTextureIfNeeded();
        }

        safe = viewModel.Settings.SafeArea;
        int[] sa1 = [safe.Max.X, safe.Max.Y];
        if (DragInt2Row("SafeBR", "Bottom Right", sa1, 1f, 0, 256, out bool sa1Deact))
            viewModel.Settings.SafeArea = new Box2i(safe.Min, new Vector2i(sa1[0], sa1[1]));
        if (sa1Deact)
        {
            viewModel.RenderPreview();
            RefreshPreviewTextureIfNeeded();
        }

        ImGui.Text("Show overlay");
        ImGui.NextColumn();
        ImGui.Checkbox("##ShowSafeArea", ref viewModel.Settings.ShowSafeArea);
        ImGui.NextColumn();

        ImGui.Columns(1);
    }

    private void DrawExpansionSection()
    {
        if (!ImGui.CollapsingHeader("Rasterization Settings##GenerateGlobalMap_RasterizationSettings",
                ImGuiTreeNodeFlags.DefaultOpen))
            return;

        ImGui.Columns(2, "##Columns_Expansion");
        ImGui.Text("Triangle Expansion (px)");
        ImGui.NextColumn();
        ImGui.PushItemWidth(-1);
        ImGui.DragFloat("##TriangleExpansion", ref viewModel.Settings.TriangleExpansion, 0.1f, 0f, 5f, "%.2f");
        if (ImGui.IsItemDeactivatedAfterEdit())
        {
            viewModel.RenderPreview();
            RefreshPreviewTextureIfNeeded();
        }

        ImGui.PopItemWidth();
        ImGui.NextColumn();
        ImGui.Columns(1);
    }

    private void DrawPreviewPanel()
    {
        RefreshPreviewTextureIfNeeded();

        float side = ImGuiEx.CalcUiScaledValue(MkdsGlobalMapConsts.DISPLAY_WIDTH);
        float displayH = ImGuiEx.CalcUiScaledValue(MkdsGlobalMapConsts.DISPLAY_HEIGHT);

        ImGui.TextDisabled("Drag the minimap to adjust its position\nScroll to zoom in/out.");

        bool canSave = viewModel.HasGeometry;
        if (!canSave)
            ImGui.BeginDisabled();
        if (ImGui.Button("Save as PNG..."))
            BrowseAndSavePng();
        if (!canSave)
            ImGui.EndDisabled();

        if (_previewTexture is not null)
        {
            var origin = ImGui.GetCursorPos();
            float pxScale = side / MkdsGlobalMapConsts.DISPLAY_WIDTH;

            ImGui.SetCursorPos(origin);
            ImGui.InvisibleButton("##MapPreviewInput", new System.Numerics.Vector2(side, side));
            bool hovered = ImGui.IsItemHovered();
            var screenMin = ImGui.GetItemRectMin();

            if (hovered)
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeAll);
            }

            if (ImGui.IsItemActive() && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
            {
                _panAccumPx = ImGui.GetMouseDragDelta(ImGuiMouseButton.Left);
                _isPanning = true;
            }
            else if (_isPanning && !ImGui.IsMouseDown(ImGuiMouseButton.Left))
            {
                viewModel.ApplyPanDeltaPixels(_panAccumPx.X / pxScale, _panAccumPx.Y / pxScale);
                viewModel.RenderPreview();
                RefreshPreviewTextureIfNeeded();
                _panAccumPx = default;
                _isPanning = false;
                ImGui.ResetMouseDragDelta(ImGuiMouseButton.Left);
            }

            if (hovered && !_isPanning)
            {
                float wheel = ImGui.GetIO().MouseWheel;
                if (wheel != 0)
                {
                    var mouseLocalUi = ImGui.GetMousePos() - screenMin;
                    var canvasPixel = new System.Numerics.Vector2(
                        mouseLocalUi.X / pxScale, mouseLocalUi.Y / pxScale);
                    if (viewModel.ApplyZoomAtPixel(wheel, canvasPixel))
                    {
                        viewModel.RenderPreview();
                        RefreshPreviewTextureIfNeeded();
                    }
                }
            }

            DrawLayer(_courseBgTexture, origin, side, displayH);
            DrawLayer(_hudOverlayTexture, origin, side, displayH);
            DrawLayerOffset(_previewTexture, origin,
                _isPanning ? _panAccumPx : System.Numerics.Vector2.Zero, side, side);

            if (viewModel.Settings.ShowSafeArea)
            {
                var safe = viewModel.Settings.SafeArea;
                var dl = ImGui.GetWindowDrawList();
                var rmin = screenMin + new System.Numerics.Vector2(safe.Min.X * pxScale, safe.Min.Y * pxScale);
                var rmax = screenMin + new System.Numerics.Vector2(safe.Max.X * pxScale, safe.Max.Y * pxScale);
                dl.AddRect(rmin, rmax, SAFE_RECT_COLOR, 0f, ImDrawFlags.None, 1.5f);
            }

            ImGui.SetCursorPos(origin);
            ImGui.Dummy(new System.Numerics.Vector2(side, side));
        }
        else
        {
            ImGui.Dummy(new System.Numerics.Vector2(side, side));
        }
    }

    private void DrawNavigationButtons(bool canContinue)
    {
        bool isFirst = _curStep == GenerateGlobalMapModalViewStep.Source;
        bool isLast = _curStep == GenerateGlobalMapModalViewStep.Last;

        var contentRegionMax = ImGui.GetContentRegionAvail() + ImGui.GetCursorScreenPos() - ImGui.GetWindowPos();
        ImGui.SetCursorPosX(contentRegionMax.X - 2 * ImGuiEx.CalcUiScaledValue(80) - ImGui.GetStyle().ItemSpacing.X);

        if (isFirst) ImGui.BeginDisabled();
        if (ImGui.Button("Previous", new System.Numerics.Vector2(ImGuiEx.CalcUiScaledValue(80), 0)))
        {
            if (!isFirst)
                _curStep--;
        }

        if (isFirst) ImGui.EndDisabled();

        ImGui.SameLine();

        if (!canContinue) ImGui.BeginDisabled();
        if (ImGui.Button(isLast ? "Save" : "Next", new System.Numerics.Vector2(ImGuiEx.CalcUiScaledValue(80), 0)))
        {
            if (isLast)
            {
                if (viewModel.Commit())
                    Close();
            }
            else
            {
                if (_curStep == GenerateGlobalMapModalViewStep.Source)
                {
                    viewModel.AutoComputeBounds();
                    viewModel.RenderPreview();
                    RefreshPreviewTextureIfNeeded();
                }

                _curStep++;
            }
        }

        if (!canContinue) ImGui.EndDisabled();
    }

    private static bool DragInt2Row(string id, string display, int[] values, float speed, int min, int max,
        out bool deactivated)
    {
        ImGui.Text(display);
        ImGui.NextColumn();
        ImGui.PushItemWidth(-1);
        bool changed = min == 0 && max == 0
            ? ImGui.DragInt2($"##{id}", ref values[0], speed)
            : ImGui.DragInt2($"##{id}", ref values[0], speed, min, max);
        deactivated = ImGui.IsItemDeactivatedAfterEdit();
        ImGui.PopItemWidth();
        ImGui.NextColumn();
        return changed;
    }

    private static void DrawLayer(GLTexture? tex, System.Numerics.Vector2 origin, float w, float h)
    {
        if (tex is null)
            return;
        ImGui.SetCursorPos(origin);
        ImGui.Image(tex.Handle, new System.Numerics.Vector2(w, h));
    }

    private static void DrawLayerOffset(GLTexture? tex, System.Numerics.Vector2 origin,
        System.Numerics.Vector2 offset, float w, float h)
    {
        if (tex is null)
            return;
        ImGui.SetCursorPos(origin + offset);
        ImGui.Image(tex.Handle, new System.Numerics.Vector2(w, h));
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