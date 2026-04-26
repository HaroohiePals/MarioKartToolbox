#nullable enable
using HaroohiePals.Graphics;
using HaroohiePals.MarioKartToolbox.Resources;
using HaroohiePals.NitroKart.Course;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Runtime.InteropServices;

namespace HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;

class MkdsGlobalMapRasterizer
{
    private static readonly Color4 FillColor = new(120, 120, 120, 255);
    private static readonly Color4 OutlineColor = new(248, 248, 248, 255);

    private readonly Image<Bgra32> _markerPattern = Image.Load<Bgra32>(Images.GlobalMapStartMarkerPattern);
    private readonly Image<Bgra32> _markerLabel = Image.Load<Bgra32>(Images.GlobalMapStartMarkerLabel);

    public Rgba8Bitmap Rasterize(MkdsGlobalMapRasterizeOptions options)
    {
        var bitmap = new Rgba8Bitmap(MkdsGlobalMapConsts.CANVAS_WIDTH, MkdsGlobalMapConsts.CANVAS_HEIGHT);

        int safeX0 = Math.Clamp(options.SafeArea.Min.X, 0, MkdsGlobalMapConsts.CANVAS_WIDTH);
        int safeY0 = Math.Clamp(options.SafeArea.Min.Y, 0, MkdsGlobalMapConsts.CANVAS_HEIGHT);
        int safeX1 = Math.Clamp(options.SafeArea.Max.X, 0, MkdsGlobalMapConsts.CANVAS_WIDTH);
        int safeY1 = Math.Clamp(options.SafeArea.Max.Y, 0, MkdsGlobalMapConsts.CANVAS_HEIGHT);

        double spanX = options.BottomRight.X - options.TopLeft.X;
        double spanY = options.BottomRight.Y - options.TopLeft.Y;
        if (options.Triangles.Count == 0 || spanX == 0 || spanY == 0
            || safeX0 >= safeX1 || safeY0 >= safeY1)
            return bitmap;

        foreach (var tri in options.Triangles)
        {
            var a = Project(tri.PointA, options.Mode, options.TopLeft, options.BottomRight);
            var b = Project(tri.PointB, options.Mode, options.TopLeft, options.BottomRight);
            var c = Project(tri.PointC, options.Mode, options.TopLeft, options.BottomRight);

            if (options.TriangleExpansion > 0)
                ExpandFromCentroid(ref a, ref b, ref c, options.TriangleExpansion);

            FillTriangle(bitmap, a, b, c, safeX0, safeY0, safeX1, safeY1);
        }

        ApplyOutline(bitmap, safeX0, safeY0, safeX1, safeY1);

        if (!options.ShowStartMarker || options.StartMarkerWidth < 1)
            return bitmap;

        DrawStartMarker(bitmap, options.StartPointPosition, options.StartPointRotation, options.StartMarkerWidth,
            options.Mode, options.TopLeft, options.BottomRight,
            safeX0, safeY0, safeX1, safeY1);

        if (!options.ShowStartMarkerLabel)
            return bitmap;

        DrawStartMarkerLabel(bitmap, options.StartPointPosition, options.StartMarkerWidth, options.Mode,
            options.TopLeft, options.BottomRight,
            safeX0, safeY0, safeX1, safeY1, options.StartMarkerLabelOffset);

        return bitmap;
    }

    private void DrawStartMarker(Rgba8Bitmap bmp, Vector3d position, Vector3d rotation, int width,
        MkdsGlobalMapMode mode, Vector2d topLeft, Vector2d bottomRight,
        int safeX0, int safeY0, int safeX1, int safeY1)
    {
        var rot =
            Matrix3d.CreateRotationX(MathHelper.DegreesToRadians(rotation.X)) *
            Matrix3d.CreateRotationY(MathHelper.DegreesToRadians(rotation.Y)) *
            Matrix3d.CreateRotationZ(MathHelper.DegreesToRadians(rotation.Z));

        var c = Project(position, mode, topLeft, bottomRight);
        var euRaw = Project(position + rot.Row0, mode, topLeft, bottomRight) - c;
        if (euRaw.LengthSquared < 0.00001)
            return;

        double angleDeg = MathHelper.RadiansToDegrees(Math.Atan2(euRaw.Y, euRaw.X));

        int patternW = _markerPattern.Width;
        int patternH = _markerPattern.Height;

        var patternPixels = new Bgra32[patternW * patternH];
        _markerPattern.CopyPixelDataTo(MemoryMarshal.AsBytes(patternPixels.AsSpan()));

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

        padded.Mutate(ctx => ctx
            // ReSharper disable once AccessToDisposedClosure
            .DrawImage(tiled, new Point(offX, offY), 1f)
            .Rotate((float)angleDeg, KnownResamplers.NearestNeighbor));

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
                                | src.B;
                }
            }
        });
    }

    private void DrawStartMarkerLabel(Rgba8Bitmap bmp, Vector3d position, int startMarkerWidth,
        MkdsGlobalMapMode mode, Vector2d topLeft, Vector2d bottomRight,
        int safeX0, int safeY0, int safeX1, int safeY1,
        Vector2i userOffset)
    {
        int lw = _markerLabel.Width;
        int lh = _markerLabel.Height;

        if (lw <= 0 || lh <= 0)
            return;

        if (lw > safeX1 - safeX0 || lh > safeY1 - safeY0)
            return;

        var c = Project(position, mode, topLeft, bottomRight);
        int cx = (int)Math.Round(c.X);
        int cy = (int)Math.Round(c.Y);

        int markerHalfSpan = Math.Max(startMarkerWidth, _markerPattern.Height) / 2 + 4;

        // try cardinal N/E/S/W first, then diagonals
        ReadOnlySpan<(int dx, int dy)> directions =
        [
            (0, -1), // N
            (1, 0), // E
            (0, 1), // S
            (-1, 0), // W
            (1, -1), // NE
            (1, 1), // SE
            (-1, 1), // SW
            (-1, -1), // NW
        ];

        const int MAX_RADIUS = 64;
        const int STEP = 2;

        int placedX = int.MinValue;
        int placedY = int.MinValue;

        for (int r = 0; r <= MAX_RADIUS && placedX == int.MinValue; r += STEP)
        {
            int d = markerHalfSpan + r;
            foreach ((int dx, int dy) in directions)
            {
                int tx = dx switch
                {
                    -1 => cx - d - lw,
                    1 => cx + d,
                    _ => cx - lw / 2,
                };
                int ty = dy switch
                {
                    -1 => cy - d - lh,
                    1 => cy + d,
                    _ => cy - lh / 2,
                };

                if (!IsRectClear(bmp, tx, ty, lw, lh, safeX0, safeY0, safeX1, safeY1))
                    continue;

                placedX = tx;
                placedY = ty;
                break;
            }
        }

        if (placedX == int.MinValue)
        {
            placedX = Math.Clamp(cx + markerHalfSpan, safeX0, safeX1 - lw);
            placedY = Math.Clamp(cy - lh / 2, safeY0, safeY1 - lh);
        }

        placedX += userOffset.X;
        placedY += userOffset.Y;

        int dx0 = Math.Max(safeX0, placedX);
        int dy0 = Math.Max(safeY0, placedY);
        int dx1 = Math.Min(safeX1 - 1, placedX + lw - 1);
        int dy1 = Math.Min(safeY1 - 1, placedY + lh - 1);

        if (dx0 > dx1 || dy0 > dy1)
            return;

        int labelX = placedX;
        int labelY = placedY;

        _markerLabel.ProcessPixelRows(accessor =>
        {
            for (int y = dy0; y <= dy1; y++)
            {
                var row = accessor.GetRowSpan(y - labelY);
                for (int x = dx0; x <= dx1; x++)
                {
                    var src = row[x - labelX];

                    if (src.A == 0)
                        continue;

                    bmp[x, y] = ((uint)src.A << 24)
                                | ((uint)src.R << 16)
                                | ((uint)src.G << 8)
                                | src.B;
                }
            }
        });
    }

    private static bool IsRectClear(Rgba8Bitmap bmp, int tx, int ty, int lw, int lh,
        int safeX0, int safeY0, int safeX1, int safeY1)
    {
        if (tx < safeX0 || ty < safeY0 || tx + lw > safeX1 || ty + lh > safeY1)
            return false;

        for (int y = ty; y < ty + lh; y++)
        {
            for (int x = tx; x < tx + lw; x++)
            {
                if (bmp[x, y] != 0)
                    return false;
            }
        }

        return true;
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

    private static void ApplyOutline(Rgba8Bitmap bmp, int safeX0, int safeY0, int safeX1, int safeY1)
    {
        uint fillPacked = PackColor(FillColor);
        uint outlinePacked = PackColor(OutlineColor);

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
        if (x <= 0 || IsOutside(bmp[x - 1, y], fillPacked, outlinePacked))
            return true;
        if (x + 1 >= bmp.Width || IsOutside(bmp[x + 1, y], fillPacked, outlinePacked))
            return true;
        if (y <= 0 || IsOutside(bmp[x, y - 1], fillPacked, outlinePacked))
            return true;
        if (y + 1 >= bmp.Height || IsOutside(bmp[x, y + 1], fillPacked, outlinePacked))
            return true;
        return false;
    }

    private static bool IsOutside(uint pixel, uint fillPacked, uint outlinePacked)
        => pixel != fillPacked && pixel != outlinePacked;

    private static void FillTriangle(Rgba8Bitmap bmp, Vector2d a, Vector2d b, Vector2d c, int safeX0, int safeY0,
        int safeX1, int safeY1)
    {
        uint packed = PackColor(FillColor);

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
}