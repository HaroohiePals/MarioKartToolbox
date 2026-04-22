#nullable enable
using HaroohiePals.Actions;
using HaroohiePals.Graphics;
using HaroohiePals.Graphics3d;
using HaroohiePals.MarioKartToolbox.KCollision;
using HaroohiePals.MarioKartToolbox.Resources;
using HaroohiePals.MarioKartToolbox.Tools;
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
    private const int DISPLAY_WIDTH = 256;
    private const int DISPLAY_HEIGHT = 192;
    private const int SAFE_OFFSET_X = 120;
    private const int SAFE_OFFSET_Y = 24;
    private const int SAFE_WIDTH = DISPLAY_WIDTH - SAFE_OFFSET_X;
    private const int SAFE_HEIGHT = DISPLAY_HEIGHT - SAFE_OFFSET_Y;

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

    public GenerateGlobalMapSettings Settings;
    public MkdsGlobalMapMode Mode;
    public Vector2d TopLeft;
    public Vector2d BottomRight;
    public float TriangleExpansion = 1f;
    
    public string? ErrorMessage { get; private set; }
    public Rgba8Bitmap? PreviewBitmap { get; private set; }
    public int PreviewVersion { get; private set; }
    public int TriangleCount => _loadedTriangles.Count;
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool HasGeometry => _loadedTriangles.Count > 0;

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
            Mode = existing.Mode;
            TopLeft = existing.TopLeft;
            BottomRight = existing.BottomRight;
        }
        else
        {
            Mode = MkdsGlobalMapMode.Normal;
            TopLeft = new Vector2d(-6000, -3000);
            BottomRight = new Vector2d(0, 3000);
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

    public bool AutoComputeBounds()
    {
        if (!HasGeometry)
            return false;

        (TopLeft, BottomRight) = ComputeBoundsFromTriangles(_loadedTriangles, Mode);
        return true;
    }

    public void ApplyOffset(double dx, double dy)
    {
        TopLeft = new Vector2d(TopLeft.X + dx, TopLeft.Y + dy);
        BottomRight = new Vector2d(BottomRight.X + dx, BottomRight.Y + dy);
    }

    public bool RenderPreview()
    {
        if (!HasGeometry)
            return false;

        PreviewBitmap = GlobalMapRasterizer.Rasterize(
            _loadedTriangles, TopLeft, BottomRight, Mode, TriangleExpansion);
        PreviewVersion++;
        return true;
    }

    public bool SavePng(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        var bitmap = PreviewBitmap ?? GlobalMapRasterizer.Rasterize(
            _loadedTriangles, TopLeft, BottomRight, Mode, TriangleExpansion);

        try
        {
            GlobalMapRasterizer.SavePng(bitmap, path);
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
                TopLeft = TopLeft,
                BottomRight = BottomRight,
                Mode = Mode,
            };
            _courseEditorContext.ActionStack.Add(
                metadata.SetPropertyAction(m => m.GlobalMapSettings, (MkdsGlobalMapSettings?)created));
        }
        else
        {
            var actions = new List<IAction>
            {
                existing.SetPropertyAction(s => s.TopLeft, TopLeft),
                existing.SetPropertyAction(s => s.BottomRight, BottomRight),
                existing.SetPropertyAction(s => s.Mode, Mode),
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

    private static (Vector2d TopLeft, Vector2d BottomRight) ComputeBoundsFromTriangles(
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

        double kx = SAFE_WIDTH / (uMax - uMin);
        double ky = SAFE_HEIGHT / (vMax - vMin);
        double k = Math.Min(kx, ky);

        double geomPxW = k * (uMax - uMin);
        double geomPxH = k * (vMax - vMin);
        double pxOx = SAFE_OFFSET_X + (SAFE_WIDTH - geomPxW) * 0.5;
        double pxOy = SAFE_OFFSET_Y + (SAFE_HEIGHT - geomPxH) * 0.5;

        double uAt0 = uMin - pxOx / k;
        double uAtMax = uAt0 + DISPLAY_WIDTH / k;
        double vAt0 = vMin - pxOy / k;
        double vAtMax = vAt0 + DISPLAY_HEIGHT / k;

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
                256, 192, firstTransparent: true);
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