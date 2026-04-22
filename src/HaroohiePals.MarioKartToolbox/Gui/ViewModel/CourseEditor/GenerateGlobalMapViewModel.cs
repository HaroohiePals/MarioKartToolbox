#nullable enable
using HaroohiePals.Actions;
using HaroohiePals.Graphics;
using HaroohiePals.Graphics3d;
using HaroohiePals.MarioKartToolbox.KCollision;
using HaroohiePals.MarioKartToolbox.Resources;
using HaroohiePals.Mathematics;
using HaroohiePals.Nitro.Gx;
using HaroohiePals.Nitro.NitroSystem.G2d;
using HaroohiePals.NitroKart.Course;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;

class GenerateGlobalMapViewModel
{
    private static readonly HashSet<MkdsCollisionType> RoadTypes =
    [
        MkdsCollisionType.Road,
        MkdsCollisionType.SlipperyRoad,
        MkdsCollisionType.SlipperyRoad2,
        MkdsCollisionType.BoostPad,
        MkdsCollisionType.JumpPad,
        MkdsCollisionType.RoadNoDrivers,
        MkdsCollisionType.FallsWater,
        MkdsCollisionType.BoostPadMinSpeed,
        MkdsCollisionType.Loop,
        MkdsCollisionType.SpecialRoad,
    ];

    private readonly ICourseEditorContext _courseEditorContext;

    private IReadOnlyList<Triangle> _loadedTriangles = [];

    public GenerateGlobalMapSettings Settings = new GenerateGlobalMapSettings();

    public string? ErrorMessage { get; private set; }
    public Rgba8Bitmap? PreviewBitmap { get; private set; }
    public int PreviewVersion { get; private set; }
    public int TriangleCount => _loadedTriangles.Count;
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool HasGeometry => _loadedTriangles.Count > 0;
    public bool HasExistingSettings => _courseEditorContext.Course.Metadata.GlobalMapSettings is not null;

    public Rgba8Bitmap? BackgroundBitmap { get; }
    public Rgba8Bitmap? HudOverlayBitmap { get; }

    public GenerateGlobalMapViewModel(ICourseEditorContext courseEditorContext)
    {
        _courseEditorContext = courseEditorContext;
        Settings.SourceType = GenerateGlobalMapSourceType.CourseKcl;
        Settings.ObjFilePath = "";

        var existing = _courseEditorContext.Course.Metadata.GlobalMapSettings;
        if (existing is not null)
        {
            Settings.Mode = existing.Mode;
            Settings.TopLeft = existing.TopLeft;
            Settings.BottomRight = existing.BottomRight;
        }
        else
        {
            Settings.Mode = MkdsGlobalMapMode.Normal;
            Settings.TopLeft = new Vector2d(-6000, -3000);
            Settings.BottomRight = new Vector2d(0, 3000);
        }

        BackgroundBitmap = LoadBackground(_courseEditorContext.Course);
        HudOverlayBitmap = LoadHudOverlay();

        ReloadTriangles();
    }


    public void ReloadTriangles()
    {
        ErrorMessage = null;

        try
        {
            _loadedTriangles = Settings.SourceType switch
            {
                GenerateGlobalMapSourceType.CourseKcl => LoadFromCourseKcl(),
                GenerateGlobalMapSourceType.ExternalObj => LoadFromObj(Settings.ObjFilePath),
                _ => [],
            };
        }
        catch (Exception ex)
        {
            _loadedTriangles = [];
            ErrorMessage = ex.Message;
        }
    }

    public bool LoadExistingSettings()
    {
        var existing = _courseEditorContext.Course.Metadata.GlobalMapSettings;
        if (existing is null)
            return false;

        Settings.Mode = existing.Mode;
        Settings.TopLeft = existing.TopLeft;
        Settings.BottomRight = existing.BottomRight;
        return true;
    }

    public bool AutoComputeBounds()
    {
        if (!HasGeometry)
            return false;

        (Settings.TopLeft, Settings.BottomRight) = ComputeBoundsFromTriangles(_loadedTriangles, Settings.Mode);
        return true;
    }

    public void ApplyOffset(double dx, double dy)
    {
        Settings.TopLeft = new Vector2d(Settings.TopLeft.X + dx, Settings.TopLeft.Y + dy);
        Settings.BottomRight = new Vector2d(Settings.BottomRight.X + dx, Settings.BottomRight.Y + dy);
    }

    public void ApplyPanDeltaPixels(double canvasDx, double canvasDy)
    {
        double sx = Settings.BottomRight.X - Settings.TopLeft.X;
        double sy = Settings.BottomRight.Y - Settings.TopLeft.Y;
        double dWx = -canvasDx * sx / MkdsGlobalMapConsts.DISPLAY_WIDTH;
        double dWy = -canvasDy * sy / MkdsGlobalMapConsts.DISPLAY_HEIGHT;

        Settings.TopLeft = new Vector2d(
            Math.Round(Settings.TopLeft.X + dWx),
            Math.Round(Settings.TopLeft.Y + dWy));
        Settings.BottomRight = new Vector2d(
            Math.Round(Settings.BottomRight.X + dWx),
            Math.Round(Settings.BottomRight.Y + dWy));
    }

    public bool ApplyZoomAtPixel(float wheel, System.Numerics.Vector2 canvasPixel)
    {
        if (wheel == 0)
            return false;

        double factor = Math.Pow(1.15, -wheel);
        double fx = Math.Clamp(canvasPixel.X / MkdsGlobalMapConsts.DISPLAY_WIDTH, 0, 1);
        double fy = Math.Clamp(canvasPixel.Y / MkdsGlobalMapConsts.DISPLAY_HEIGHT, 0, 1);
        double ax = Settings.TopLeft.X + fx * (Settings.BottomRight.X - Settings.TopLeft.X);
        double ay = Settings.TopLeft.Y + fy * (Settings.BottomRight.Y - Settings.TopLeft.Y);

        var newTl = new Vector2d(
            ax + (Settings.TopLeft.X - ax) * factor,
            ay + (Settings.TopLeft.Y - ay) * factor);

        var newBr = new Vector2d(
            ax + (Settings.BottomRight.X - ax) * factor,
            ay + (Settings.BottomRight.Y - ay) * factor);

        if (Math.Abs(newBr.X - newTl.X) < 1 || Math.Abs(newBr.Y - newTl.Y) < 1)
            return false;

        Settings.TopLeft = new Vector2d(Math.Round(newTl.X), Math.Round(newTl.Y));
        Settings.BottomRight = new Vector2d(Math.Round(newBr.X), Math.Round(newBr.Y));

        return true;
    }

    public bool RenderPreview()
    {
        if (!HasGeometry)
            return false;

        PreviewBitmap = MkdsGlobalMapRasterizer.Rasterize(
            _loadedTriangles, Settings.TopLeft, Settings.BottomRight,
            Settings.Mode, Settings.SafeArea, Settings.TriangleExpansion);
        PreviewVersion++;

        return true;
    }

    public bool SavePng(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        var bitmap = PreviewBitmap ?? MkdsGlobalMapRasterizer.Rasterize(
            _loadedTriangles, Settings.TopLeft, Settings.BottomRight,
            Settings.Mode, Settings.SafeArea, Settings.TriangleExpansion);

        try
        {
            MkdsGlobalMapRasterizer.SavePng(bitmap, path);
            ErrorMessage = null;
            return true;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to save PNG: {ex.Message}";
            return false;
        }
    }

    public bool Commit()
    {
        var metadata = _courseEditorContext.Course.Metadata;
        var existing = metadata.GlobalMapSettings;

        if (existing is null)
        {
            var created = new MkdsGlobalMapSettings
            {
                TopLeft = Settings.TopLeft,
                BottomRight = Settings.BottomRight,
                Mode = Settings.Mode,
            };
            _courseEditorContext.ActionStack.Add(
                metadata.SetPropertyAction(m => m.GlobalMapSettings, (MkdsGlobalMapSettings?)created));
        }
        else
        {
            var actions = new List<IAction>
            {
                existing.SetPropertyAction(s => s.TopLeft, Settings.TopLeft),
                existing.SetPropertyAction(s => s.BottomRight, Settings.BottomRight),
                existing.SetPropertyAction(s => s.Mode, Settings.Mode),
            };
            _courseEditorContext.ActionStack.Add(new BatchAction(actions));
        }

        return true;
    }

    private IReadOnlyList<Triangle> LoadFromCourseKcl()
    {
        var kcl = _courseEditorContext.Course.Collision;
        if (kcl is null)
        {
            ErrorMessage = "The current course has no collision (KCL) data.";
            return [];
        }

        return kcl.PrismData
            .Where(p => IsRoad(p.Attribute))
            .Select(p => p.ToTriangle(kcl))
            .ToList();
    }

    private static IReadOnlyList<Triangle> LoadFromObj(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return [];

        if (!File.Exists(path))
            throw new FileNotFoundException("OBJ file not found.", path);

        var obj = new Obj(File.ReadAllBytes(path));
        var result = new List<Triangle>();
        foreach (var face in obj.Faces)
        {
            int[]? idx = face.VertexIndices;
            if (idx is null || idx.Length < 3)
                continue;

            var a = obj.Vertices[idx[0]];
            for (int i = 1; i < idx.Length - 1; i++)
            {
                var b = obj.Vertices[idx[i]];
                var c = obj.Vertices[idx[i + 1]];
                result.Add(new Triangle(a, b, c));
            }
        }

        return result;
    }

    private static bool IsRoad(ushort rawAttribute)
    {
        MkdsKclPrismAttribute attr = rawAttribute;
        return RoadTypes.Contains(attr.Type);
    }

    private (Vector2d TopLeft, Vector2d BottomRight) ComputeBoundsFromTriangles(
        IReadOnlyList<Triangle> triangles, MkdsGlobalMapMode mode)
    {
        double wxMin = double.MaxValue;
        double wxMax = double.MinValue;
        double wzMin = double.MaxValue;
        double wzMax = double.MinValue;

        foreach (var triangle in triangles)
        {
            for (int i = 0; i < 3; i++)
            {
                var p = triangle[i];
                if (p.X < wxMin) wxMin = p.X;
                if (p.X > wxMax) wxMax = p.X;
                if (p.Z < wzMin) wzMin = p.Z;
                if (p.Z > wzMax) wzMax = p.Z;
            }
        }

        double uMin, uMax, vMin, vMax;
        switch (mode)
        {
            case MkdsGlobalMapMode.RotateCounterClockwise:
                uMin = wzMin;
                uMax = wzMax;
                vMin = -wxMax;
                vMax = -wxMin;
                break;
            case MkdsGlobalMapMode.RotateClockwise:
                uMin = -wzMax;
                uMax = -wzMin;
                vMin = wxMin;
                vMax = wxMax;
                break;
            case MkdsGlobalMapMode.Normal:
            default:
                uMin = wxMin;
                uMax = wxMax;
                vMin = wzMin;
                vMax = wzMax;
                break;
        }

        if (!(uMax > uMin) || !(vMax > vMin))
            return (Vector2d.Zero, Vector2d.Zero);

        int safeOffsetX = Settings.SafeArea.Min.X;
        int safeOffsetY = Settings.SafeArea.Min.Y;
        int safeWidth = Settings.SafeArea.Size.X;
        int safeHeight = Settings.SafeArea.Size.Y;

        if (safeWidth <= 0 || safeHeight <= 0)
            return (Vector2d.Zero, Vector2d.Zero);

        double kx = safeWidth / (uMax - uMin);
        double ky = safeHeight / (vMax - vMin);
        double k = Math.Min(kx, ky);

        double geomPxW = k * (uMax - uMin);
        double geomPxH = k * (vMax - vMin);
        double pxOx = safeOffsetX + (safeWidth - geomPxW) * 0.5;
        double pxOy = safeOffsetY + (safeHeight - geomPxH) * 0.5;

        double uAt0 = uMin - pxOx / k;
        double uAtMax = uAt0 + MkdsGlobalMapConsts.DISPLAY_WIDTH / k;
        double vAt0 = vMin - pxOy / k;
        double vAtMax = vAt0 + MkdsGlobalMapConsts.DISPLAY_HEIGHT / k;

        Vector2d tl, br;
        switch (mode)
        {
            case MkdsGlobalMapMode.RotateCounterClockwise:
                tl = new Vector2d(uAt0, -vAt0);
                br = new Vector2d(uAtMax, -vAtMax);
                break;
            case MkdsGlobalMapMode.RotateClockwise:
                tl = new Vector2d(-uAt0, vAt0);
                br = new Vector2d(-uAtMax, vAtMax);
                break;
            case MkdsGlobalMapMode.Normal:
            default:
                tl = new Vector2d(uAt0, vAt0);
                br = new Vector2d(uAtMax, vAtMax);
                break;
        }

        return (
            new Vector2d(Math.Round(tl.X), Math.Round(tl.Y)),
            new Vector2d(Math.Round(br.X), Math.Round(br.Y)));
    }

    private static Rgba8Bitmap? LoadBackground(IMkdsCourse course)
    {
        try
        {
            var tiles = course.GetTexFileOrDefault<Ncgr>("Map2D/global2.NCGR");
            var palette = course.GetTexFileOrDefault<Nclr>("Map2D/global2.NCLR");
            var screen = course.GetTexFileOrDefault<Nscr>("Map2D/global2.NSCR");
            if (tiles?.Character?.CharacterData is null
                || palette?.Palette?.Palette is null
                || screen?.Screen?.ScreenData is null)
                return null;

            return GxUtil.DecodeChar(
                tiles.Character.CharacterData,
                palette.Palette.Palette,
                screen.Screen.ScreenData,
                ImageFormat.Pltt16, MapFormat.Text,
                MkdsGlobalMapConsts.DISPLAY_WIDTH, MkdsGlobalMapConsts.DISPLAY_HEIGHT,
                firstTransparent: true);
        }
        catch
        {
            return null;
        }
    }

    private static Rgba8Bitmap? LoadHudOverlay()
    {
        try
        {
            byte[]? png = Images.BottomScreenRaceHudGlobalMap;
            if (png is null || png.Length == 0)
                return null;

            using var img = Image.Load<Bgra32>(png);
            var bmp = new Rgba8Bitmap(img.Width, img.Height);
            img.CopyPixelDataTo(MemoryMarshal.AsBytes<uint>(bmp.Pixels));
            return bmp;
        }
        catch
        {
            return null;
        }
    }

}
