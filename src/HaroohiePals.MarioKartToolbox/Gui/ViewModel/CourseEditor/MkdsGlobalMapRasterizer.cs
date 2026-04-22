#nullable enable
using HaroohiePals.Graphics;
using HaroohiePals.MarioKartToolbox.Resources;
using HaroohiePals.Mathematics;
using HaroohiePals.NitroKart.Course;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;

static class MkdsGlobalMapRasterizer
{
    private static readonly Color4 FillColor = new Color4(120, 120, 120, 255);
    private static readonly Color4 OutlineColor = new Color4(248, 248, 248, 255);

    private static Image<Bgra32>? _markerPatternCache;

    public static Rgba8Bitmap Rasterize(
        IReadOnlyList<Triangle> triangles,
        Vector2d topLeft,
        Vector2d bottomRight,
        MkdsGlobalMapMode mode,
        Box2i safeArea,
        double expandPixels,
        MkdsStartPoint? startPoint,
        int startMarkerWidth)
    {
        var bitmap = new Rgba8Bitmap(MkdsGlobalMapConsts.CANVAS_WIDTH, MkdsGlobalMapConsts.CANVAS_HEIGHT);

        int safeX0 = Math.Clamp(safeArea.Min.X, 0, MkdsGlobalMapConsts.CANVAS_WIDTH);
        int safeY0 = Math.Clamp(safeArea.Min.Y, 0, MkdsGlobalMapConsts.CANVAS_HEIGHT);
        int safeX1 = Math.Clamp(safeArea.Max.X, 0, MkdsGlobalMapConsts.CANVAS_WIDTH);
        int safeY1 = Math.Clamp(safeArea.Max.Y, 0, MkdsGlobalMapConsts.CANVAS_HEIGHT);

        double spanX = bottomRight.X - topLeft.X;
        double spanY = bottomRight.Y - topLeft.Y;
        if (triangles.Count == 0 || spanX == 0 || spanY == 0
            || safeX0 >= safeX1 || safeY0 >= safeY1)
            return bitmap;

        foreach (var tri in triangles)
        {
            var a = Project(tri.PointA, mode, topLeft, bottomRight);
            var b = Project(tri.PointB, mode, topLeft, bottomRight);
            var c = Project(tri.PointC, mode, topLeft, bottomRight);

            if (expandPixels > 0)
                ExpandFromCentroid(ref a, ref b, ref c, expandPixels);

            FillTriangle(bitmap, a, b, c, FillColor, safeX0, safeY0, safeX1, safeY1);
        }

        ApplyOutline(bitmap, FillColor, OutlineColor, safeX0, safeY0, safeX1, safeY1);

        if (startPoint is not null && startMarkerWidth >= 1)
        {
            DrawStartMarker(bitmap, startPoint, startMarkerWidth, mode, topLeft, bottomRight,
                safeX0, safeY0, safeX1, safeY1);
        }

        return bitmap;
    }

    private static void DrawStartMarker(Rgba8Bitmap bmp, MkdsStartPoint sp, int width,
        MkdsGlobalMapMode mode, Vector2d topLeft, Vector2d bottomRight,
        int safeX0, int safeY0, int safeX1, int safeY1)
    {
        var rot =
            Matrix3d.CreateRotationX(MathHelper.DegreesToRadians(sp.Rotation.X)) *
            Matrix3d.CreateRotationY(MathHelper.DegreesToRadians(sp.Rotation.Y)) *
            Matrix3d.CreateRotationZ(MathHelper.DegreesToRadians(sp.Rotation.Z));

        Vector2d c = Project(sp.Position, mode, topLeft, bottomRight);
        Vector2d euRaw = Project(sp.Position + rot.Row0, mode, topLeft, bottomRight) - c;
        if (euRaw.LengthSquared < 0.00001)
            return;

        double angleDeg = MathHelper.RadiansToDegrees(Math.Atan2(euRaw.Y, euRaw.X));

        var pattern = GetMarkerPattern();
        int patternW = pattern.Width;
        int patternH = pattern.Height;

        var patternPixels = new Bgra32[patternW * patternH];
        pattern.CopyPixelDataTo(MemoryMarshal.AsBytes(patternPixels.AsSpan()));

        using var tiled = new Image<Bgra32>(width, patternH, new Bgra32(0, 0, 0, 0));
        tiled.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < patternH; y++)
            {
                var dstRow = accessor.GetRowSpan(y);
                int rowOffset = y * patternW;
                for (int x = 0; x < width; x++)
                    dstRow[x] = patternPixels[rowOffset + (x % patternW)];
            }
        });

        int S = (int)Math.Ceiling(Math.Sqrt((double)width * width + (double)patternH * patternH)) + 2;
        using var padded = new Image<Bgra32>(S, S, new Bgra32(0, 0, 0, 0));
        int offX = (S - width) / 2;
        int offY = (S - patternH) / 2;

        padded.Mutate(ctx => ctx.DrawImage(tiled, new Point(offX, offY), 1f));
        padded.Mutate(ctx => ctx.Rotate((float)angleDeg, KnownResamplers.NearestNeighbor));

        int rotW = padded.Width;
        int rotH = padded.Height;
        int ox = (int)Math.Round(c.X - rotW * 0.5);
        int oy = (int)Math.Round(c.Y - rotH * 0.5);

        int dx0 = Math.Max(safeX0, ox);
        int dy0 = Math.Max(safeY0, oy);
        int dx1 = Math.Min(safeX1 - 1, ox + rotW - 1);
        int dy1 = Math.Min(safeY1 - 1, oy + rotH - 1);

        if (dx0 > dx1 || dy0 > dy1)
            return;

        padded.ProcessPixelRows(accessor =>
        {
            for (int y = dy0; y <= dy1; y++)
            {
                var row = accessor.GetRowSpan(y - oy);
                for (int x = dx0; x <= dx1; x++)
                {
                    var src = row[x - ox];

                    if (src.A == 0) 
                        continue;

                    bmp[x, y] = ((uint)src.A << 24)
                              | ((uint)src.R << 16)
                              | ((uint)src.G << 8)
                              | (uint)src.B;
                }
            }
        });
    }

    // Grows a triangle outward by pushing each vertex away from the centroid by
    // `pixels` pixels. Cheap way to close sub-pixel gaps between adjacent
    // triangles along shared edges.
    private static void ExpandFromCentroid(ref Vector2d a, ref Vector2d b, ref Vector2d c, double pixels)
    {
        var centroid = (a + b + c) / 3.0;
        a = OffsetFromCentroid(a, centroid, pixels);
        b = OffsetFromCentroid(b, centroid, pixels);
        c = OffsetFromCentroid(c, centroid, pixels);
    }

    private static Vector2d OffsetFromCentroid(Vector2d v, Vector2d centroid, double pixels)
    {
        var dir = v - centroid;
        double len = dir.Length;
        if (len < 0.000001)
            return v;
        return v + dir * (pixels / len);
    }

    public static void SavePng(Rgba8Bitmap bitmap, string path)
    {
        var byteSpan = MemoryMarshal.AsBytes<uint>(bitmap.Pixels);
        using var image = Image.LoadPixelData<Bgra32>(byteSpan, bitmap.Width, bitmap.Height);
        image.SaveAsPng(path);
    }

    private static Vector2d Project(
        Vector3d world, MkdsGlobalMapMode mode, Vector2d topLeft, Vector2d bottomRight)
    {
        double spanX = bottomRight.X - topLeft.X;
        double spanY = bottomRight.Y - topLeft.Y;

        double projU = mode == MkdsGlobalMapMode.Normal ? world.X : world.Z;
        double projV = mode == MkdsGlobalMapMode.Normal ? world.Z : world.X;

        double px = (projU - topLeft.X) / spanX * MkdsGlobalMapConsts.DISPLAY_WIDTH;
        double py = (projV - topLeft.Y) / spanY * MkdsGlobalMapConsts.DISPLAY_HEIGHT;
        return new Vector2d(px, py);
    }

    private static void ApplyOutline(Rgba8Bitmap bmp, Color4 fillColor, Color4 outlineColor,
        int safeX0, int safeY0, int safeX1, int safeY1)
    {
        uint fillPacked = PackColor(fillColor);
        uint outlinePacked = PackColor(outlineColor);

        for (int y = safeY0; y < safeY1; y++)
        {
            for (int x = safeX0; x < safeX1; x++)
            {
                if (bmp[x, y] != fillPacked)
                    continue;

                if (HasOutsideNeighbor(bmp, x, y, fillPacked, outlinePacked))
                    bmp[x, y] = outlinePacked;
            }
        }
    }

    private static bool HasOutsideNeighbor(Rgba8Bitmap bmp, int x, int y, uint fillPacked, uint outlinePacked)
    {
        if (x <= 0 || IsOutside(bmp[x - 1, y], fillPacked, outlinePacked)) return true;
        if (x + 1 >= bmp.Width || IsOutside(bmp[x + 1, y], fillPacked, outlinePacked)) return true;
        if (y <= 0 || IsOutside(bmp[x, y - 1], fillPacked, outlinePacked)) return true;
        if (y + 1 >= bmp.Height || IsOutside(bmp[x, y + 1], fillPacked, outlinePacked)) return true;
        return false;
    }

    private static bool IsOutside(uint pixel, uint fillPacked, uint outlinePacked)
        => pixel != fillPacked && pixel != outlinePacked;

    private static void FillTriangle(Rgba8Bitmap bmp, Vector2d a, Vector2d b, Vector2d c, Color4 color,
        int safeX0, int safeY0, int safeX1, int safeY1)
    {
        uint packed = PackColor(color);

        double minY = Math.Min(a.Y, Math.Min(b.Y, c.Y));
        double maxY = Math.Max(a.Y, Math.Max(b.Y, c.Y));
        double minX = Math.Min(a.X, Math.Min(b.X, c.X));
        double maxX = Math.Max(a.X, Math.Max(b.X, c.X));

        int y0 = Math.Max(safeY0, (int)Math.Ceiling(minY));
        int y1 = Math.Min(safeY1 - 1, (int)Math.Floor(maxY));
        int x0 = Math.Max(safeX0, (int)Math.Ceiling(minX));
        int x1 = Math.Min(safeX1 - 1, (int)Math.Floor(maxX));
        if (y0 > y1 || x0 > x1) return;

        double denom = (b.Y - c.Y) * (a.X - c.X) + (c.X - b.X) * (a.Y - c.Y);
        if (denom == 0) return;

        for (int y = y0; y <= y1; y++)
        {
            double py = y + 0.5;
            for (int x = x0; x <= x1; x++)
            {
                double px = x + 0.5;
                double w1 = ((b.Y - c.Y) * (px - c.X) + (c.X - b.X) * (py - c.Y)) / denom;
                double w2 = ((c.Y - a.Y) * (px - c.X) + (a.X - c.X) * (py - c.Y)) / denom;
                double w3 = 1 - w1 - w2;
                if (w1 >= 0 && w2 >= 0 && w3 >= 0)
                    bmp[x, y] = packed;
            }
        }
    }

    private static uint PackColor(Color4 color)
    {
        byte r = (byte)Math.Clamp((int)(color.R * 255f + 0.5f), 0, 255);
        byte g = (byte)Math.Clamp((int)(color.G * 255f + 0.5f), 0, 255);
        byte b = (byte)Math.Clamp((int)(color.B * 255f + 0.5f), 0, 255);
        byte a = (byte)Math.Clamp((int)(color.A * 255f + 0.5f), 0, 255);
        return ((uint)a << 24) | ((uint)r << 16) | ((uint)g << 8) | b;
    }

    private static Image<Bgra32> GetMarkerPattern()
    {
        if (_markerPatternCache is not null)
            return _markerPatternCache;
        _markerPatternCache = Image.Load<Bgra32>(Images.GlobalMapStartMarkerPattern);
        return _markerPatternCache;
    }
}
