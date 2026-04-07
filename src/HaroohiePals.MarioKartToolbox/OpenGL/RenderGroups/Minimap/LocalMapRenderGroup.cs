#nullable enable
using System;
using HaroohiePals.Graphics;
using HaroohiePals.Graphics3d;
using HaroohiePals.Graphics3d.OpenGL;
using HaroohiePals.Graphics3d.OpenGL.Renderers;
using HaroohiePals.Gui.Viewport;
using HaroohiePals.MarioKartToolbox.OpenGL.Renderers;
using HaroohiePals.Mathematics;
using HaroohiePals.Nitro.Gx;
using HaroohiePals.Nitro.NitroSystem.G2d;
using HaroohiePals.NitroKart.Course;
using OpenTK.Mathematics;

namespace HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups.Minimap;

class LocalMapRenderGroup : RenderGroup
{
    private const int LOCAL_MAP_SUB_INDEX = 0;
    private const float QUAD_HEIGHT_Y = 2500f;

    private readonly QuadRenderer? _quadRenderer;
    private readonly IMkdsCourse _course;

    private bool _isExtendedMap;
    private readonly LocalMapCoordsData _mapCoords = new();

    public LocalMapRenderGroup(IMkdsCourse course)
    {
        _course = course;

        var texture = CreateTexture(true);

        _quadRenderer = texture is null ? null : new QuadRenderer(texture);
    }

    public override void Render(ViewportContext context)
    {
        if (_quadRenderer is null)
            return;

        var transform = GetCurrentTransform();

        var position = (Vector3)transform.Translation;
        var scale = (Vector3)transform.Scale / 10;

        uint pickingId = context.GetPickingId(PickingGroupId, 0, LOCAL_MAP_SUB_INDEX);
        //bool isSelected = context.IsSelected(_mapCoords, LOCAL_MAP_SUB_INDEX);
        bool isHovered = context.IsHovered(_mapCoords, LOCAL_MAP_SUB_INDEX);

        _quadRenderer.Points =
        [
            new InstancedPoint(position, Vector3.Zero, scale, Color4.White, true, _mapCoords,
                pickingId, isHovered, false)
        ];
        _quadRenderer.Render(context);
    }

    public override object GetObject(int index) => _mapCoords;

    public override bool ContainsObject(object obj) => obj == _mapCoords;

    public override bool TryGetObjectTransform(object obj, int subIndex, out Transform transform)
    {
        if (subIndex != LOCAL_MAP_SUB_INDEX || obj != _mapCoords)
        {
            transform = Transform.Identity;
            return false;
        }

        transform = GetCurrentTransform();
        return true;
    }

    public override bool TrySetObjectTransform(object obj, int subIndex, in Transform transform)
    {
        if (subIndex != LOCAL_MAP_SUB_INDEX || obj != _mapCoords)
            return false;

        double halfTotalWidth = transform.Scale.X;
        double halfHeight = transform.Scale.Z;
        double centerX = transform.Translation.X;
        double centerY = transform.Translation.Z;

        double totalWidth = halfTotalWidth * 2f;
        double leftWidth = _isExtendedMap ? totalWidth * 0.5f : totalWidth;
        double height = halfHeight * 2f;

        double leftEdge = centerX - totalWidth * 0.5;
        double topEdge = centerY - height * 0.5;

        _mapCoords.TopLeft = new Vector2d(leftEdge, topEdge);
        _mapCoords.BottomRight = new Vector2d(leftEdge + leftWidth, topEdge + height);

        return true;
    }

    public override bool TryGetLocalObjectBounds(object obj, int subIndex, out Box3d bounds)
    {
        if (subIndex != LOCAL_MAP_SUB_INDEX || obj != _mapCoords)
        {
            bounds = new Box3d();
            return false;
        }

        var scale = GetCurrentTransform().Scale;
        bounds = new Box3d(
            new Vector3d(-scale.X, 0, -scale.Z),
            new Vector3d(scale.X, 0, scale.Z));
        return true;
    }

    private Transform GetCurrentTransform()
    {
        double leftWidth = _mapCoords.BottomRight.X - _mapCoords.TopLeft.X;
        double height = _mapCoords.BottomRight.Y - _mapCoords.TopLeft.Y;
        double totalWidth = _isExtendedMap ? leftWidth * 2 : leftWidth;

        double centerX = _mapCoords.TopLeft.X + totalWidth * 0.5;
        double centerY = (_mapCoords.TopLeft.Y + _mapCoords.BottomRight.Y) * 0.5;

        return new Transform(
            new Vector3d(centerX, QUAD_HEIGHT_Y, centerY),
            Vector3d.Zero,
            new Vector3d(totalWidth * 0.5f, 1, height * 0.5f));
    }

    private GLTexture? CreateTexture(bool translucent = false)
    {
        var tiles = _course.GetTexFileOrDefault<Ncgr>("Map2D/local.ncgr");
        var palette = _course.GetTexFileOrDefault<Nclr>("Map2D/local.nclr");
        var map1 = _course.GetTexFileOrDefault<Nscr>("Map2D/local2.nscr");
        var map2 = _course.GetTexFileOrDefault<Nscr>("Map2D/local3.nscr");

        var decoded = GxUtil.DecodeChar(tiles.Character.CharacterData, palette.Palette.Palette,
            map1.Screen.ScreenData, ImageFormat.Pltt256, MapFormat.Text, map1.Screen.Width,
            map1.Screen.Height, true);

        if (decoded is null)
            return null;

        // load extended map on the right
        if (map2 is not null)
        {
            _isExtendedMap = true;
            var decoded2 = GxUtil.DecodeChar(tiles.Character.CharacterData, palette.Palette.Palette,
                map2.Screen.ScreenData, ImageFormat.Pltt256, MapFormat.Text, map1.Screen.Width,
                map1.Screen.Height, true);

            if (decoded2 is not null)
            {
                var combined = new Rgba8Bitmap(decoded.Width + decoded2.Width, decoded.Height);

                for (int y = 0; y < combined.Height; y++)
                {
                    for (int x = 0; x < decoded.Width; x++)
                        combined[x, y] = decoded[x, y];

                    for (int x = 0; x < decoded2.Width; x++)
                        combined[decoded.Width + x, y] = decoded2[x, y];
                }

                decoded = combined;
            }
        }

        if (translucent)
        {
            for (int i = 0; i < decoded.Width * decoded.Height; i++)
            {
                decoded.Pixels[i] = (decoded.Pixels[i] & 0x00FFFFFF) | 0x77000000;
            }
        }

        var texture = new GLTexture(decoded, TextureWrapMode.Clamp, TextureWrapMode.Clamp);
        texture.SetFilterMode(TextureFilterMode.Nearest, TextureFilterMode.Nearest);

        return texture;
    }
}

// todo: temp
public class LocalMapCoordsData
{
    // hardcoded cross_course values
    public Vector2d TopLeft { get; set; } = new(-6000, -3002);
    public Vector2d BottomRight { get; set; } = new(0, 2998);
    public LocalMapMode Mode { get; set; } = LocalMapMode.HorizontallyExtended;
}

public enum LocalMapMode
{
    Single,
    Layered,
    HorizontallyExtended,
    VerticallyExtended,
}