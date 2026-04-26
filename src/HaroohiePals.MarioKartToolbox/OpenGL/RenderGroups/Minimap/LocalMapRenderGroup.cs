#nullable enable
using System;
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
    private const string MAP_2D_FOLDER_NAME = "Map2D";
    private const string TILES_FILENAME = $"{MAP_2D_FOLDER_NAME}/local.NCGR";
    private const string PALETTE_FILENAME = $"{MAP_2D_FOLDER_NAME}/local.NCLR";
    private const string FIRST_SCREEN_FILENAME = $"{MAP_2D_FOLDER_NAME}/local2.NSCR";
    private const string SECOND_SCREEN_FILENAME = $"{MAP_2D_FOLDER_NAME}/local3.NSCR";

    private const int LOCAL_MAP_FIRST_SCREEN_SUB_INDEX = 0;
    private const int LOCAL_MAP_SECOND_SCREEN_SUB_INDEX = 1;
    private const float QUAD_HEIGHT_Y = 2500f;

    private readonly QuadRenderer? _firstLocalMapRenderer;
    private readonly QuadRenderer? _secondLocalMapRenderer;
    private readonly QuadRenderer? _firstLocalMapRendererTranslucent;
    private readonly QuadRenderer? _secondLocalMapRendererTranslucent;
    private readonly IMkdsCourse _course;

    private MkdsLocalMapSettings _mapSettings => _course.Metadata.LocalMapSettings!;

    public bool RenderTranslucent { get; set; } = false;

    public LocalMapRenderGroup(IMkdsCourse course)
    {
        _course = course;

        var texture1 = CreateTexture(false, false);
        var texture2 = CreateTexture(false, true);
        var texture1Translucent = CreateTexture(true, false);
        var texture2Translucent = CreateTexture(true, true);

        _firstLocalMapRenderer = texture1 is null ? null : new QuadRenderer(texture1, true, false);
        _secondLocalMapRenderer = texture2 is null ? null : new QuadRenderer(texture2, true, false);
        _firstLocalMapRendererTranslucent =
            texture1Translucent is null ? null : new QuadRenderer(texture1Translucent, true, false);
        _secondLocalMapRendererTranslucent =
            texture2Translucent is null ? null : new QuadRenderer(texture2Translucent, true, false);
    }

    public override void Render(ViewportContext context)
    {
        var first = RenderTranslucent ? _firstLocalMapRendererTranslucent : _firstLocalMapRenderer;
        var second = RenderTranslucent ? _secondLocalMapRendererTranslucent : _secondLocalMapRenderer;

        // Render second screen first because its less prioritary
        if (second is not null)
        {
            if (_mapSettings.Mode == MkdsLocalMapMode.Extended)
            {
                var transform = GetCurrentTransform(LOCAL_MAP_SECOND_SCREEN_SUB_INDEX);

                var position = (Vector3)transform.Translation;
                var scale = (Vector3)transform.Scale / 10;

                uint pickingId = context.GetPickingId(PickingGroupId, 0, LOCAL_MAP_SECOND_SCREEN_SUB_INDEX);
                bool isHovered = context.IsHovered(_mapSettings, LOCAL_MAP_SECOND_SCREEN_SUB_INDEX);

                second.Points =
                [
                    new InstancedPoint(position, Vector3.Zero, scale, Color4.White, true, _mapSettings,
                        pickingId, isHovered, false)
                ];
                second.Render(context);
            }
            else
            {
                var transform = GetCurrentTransform(LOCAL_MAP_FIRST_SCREEN_SUB_INDEX);

                var position = (Vector3)transform.Translation;
                var scale = (Vector3)transform.Scale / 10;

                second.Points =
                [
                    new InstancedPoint(position, Vector3.Zero, scale, Color4.White, true, _mapSettings,
                        ViewportContext.InvalidPickingId, false, false)
                ];
                second.Render(context);
            }
        }

        if (first is not null)
        {
            var transform = GetCurrentTransform(LOCAL_MAP_FIRST_SCREEN_SUB_INDEX);

            var position = (Vector3)transform.Translation;
            var scale = (Vector3)transform.Scale / 10;

            uint pickingId = context.GetPickingId(PickingGroupId, 0, LOCAL_MAP_FIRST_SCREEN_SUB_INDEX);
            bool isHovered = context.IsHovered(_mapSettings, LOCAL_MAP_FIRST_SCREEN_SUB_INDEX);

            first.Points =
            [
                new InstancedPoint(position, Vector3.Zero, scale, Color4.White, true, _mapSettings,
                    pickingId, isHovered, false)
            ];
            first.Render(context);
        }
    }

    public override object GetObject(int index) => _mapSettings;

    public override bool ContainsObject(object obj) => obj == _mapSettings;

    public override bool TryGetObjectTransform(object obj, int subIndex, out Transform transform)
    {
        if (!(subIndex == LOCAL_MAP_FIRST_SCREEN_SUB_INDEX || subIndex == LOCAL_MAP_SECOND_SCREEN_SUB_INDEX) ||
            obj != _mapSettings)
        {
            transform = Transform.Identity;
            return false;
        }

        transform = GetCurrentTransform(subIndex);
        return true;
    }

    public override bool TrySetObjectTransform(object obj, int subIndex, in Transform transform)
    {
        if (!(subIndex == LOCAL_MAP_FIRST_SCREEN_SUB_INDEX || subIndex == LOCAL_MAP_SECOND_SCREEN_SUB_INDEX) ||
            obj != _mapSettings)
            return false;

        double halfTotalWidth = transform.Scale.X;
        double halfHeight = transform.Scale.Z;
        double centerX = transform.Translation.X;
        double centerY = transform.Translation.Z;

        double totalWidth = halfTotalWidth * 2f;
        double leftWidth = totalWidth;
        double height = halfHeight * 2f;

        double leftEdge = centerX - totalWidth * 0.5;
        double topEdge = centerY - height * 0.5;

        switch (subIndex)
        {
            case LOCAL_MAP_FIRST_SCREEN_SUB_INDEX:
                _mapSettings.TopLeft = new Vector2d(Math.Round(leftEdge), Math.Round(topEdge));
                _mapSettings.BottomRight = new Vector2d(Math.Round(leftEdge + leftWidth), Math.Round(topEdge + height));
                break;
            case LOCAL_MAP_SECOND_SCREEN_SUB_INDEX:
                if (_mapSettings.Mode == MkdsLocalMapMode.Extended)
                {
                    _mapSettings.ExtendedTopLeft = new Vector2d(Math.Round(leftEdge), Math.Round(topEdge));
                    _mapSettings.ExtendedBottomRight =
                        new Vector2d(Math.Round(leftEdge + leftWidth), Math.Round(topEdge + height));
                }

                break;
        }

        return true;
    }

    public override bool TryGetLocalObjectBounds(object obj, int subIndex, out Box3d bounds)
    {
        if (!(subIndex == LOCAL_MAP_FIRST_SCREEN_SUB_INDEX || subIndex == LOCAL_MAP_SECOND_SCREEN_SUB_INDEX) ||
            obj != _mapSettings)
        {
            bounds = new Box3d();
            return false;
        }

        var scale = GetCurrentTransform(subIndex).Scale;
        bounds = new Box3d(
            new Vector3d(-scale.X, 0, -scale.Z),
            new Vector3d(scale.X, 0, scale.Z));
        return true;
    }

    private Transform GetCurrentTransform(int subIndex)
    {
        var topLeft = _mapSettings.TopLeft;
        var bottomRight = _mapSettings.BottomRight;

        if (subIndex == LOCAL_MAP_SECOND_SCREEN_SUB_INDEX)
        {
            topLeft = _mapSettings.ExtendedTopLeft;
            bottomRight = _mapSettings.ExtendedBottomRight;
        }

        double leftWidth = bottomRight.X - topLeft.X;
        double height = bottomRight.Y - topLeft.Y;
        double totalWidth = leftWidth;

        double centerX = topLeft.X + totalWidth * 0.5;
        double centerY = (topLeft.Y + bottomRight.Y) * 0.5;

        return new Transform(
            new Vector3d(centerX, QUAD_HEIGHT_Y, centerY),
            Vector3d.Zero,
            new Vector3d(totalWidth * 0.5f, 1, height * 0.5f));
    }

    private GLTexture? CreateTexture(bool translucent, bool loadSecondMap)
    {
        try
        {
            var tiles = _course.GetTexFileOrDefault<Ncgr>(TILES_FILENAME);
            var palette = _course.GetTexFileOrDefault<Nclr>(PALETTE_FILENAME);
            var map = _course.GetTexFileOrDefault<Nscr>(loadSecondMap ? SECOND_SCREEN_FILENAME : FIRST_SCREEN_FILENAME);

            if (tiles is null || palette is null || map is null)
                return null;

            var decoded = GxUtil.DecodeChar(tiles.Character.CharacterData, palette.Palette.Palette,
                map.Screen.ScreenData, ImageFormat.Pltt256, MapFormat.Text, map.Screen.Width,
                map.Screen.Height, true);

            if (decoded is null)
                return null;

            if (translucent)
            {
                for (int i = 0; i < decoded.Width * decoded.Height; i++)
                {
                    if ((decoded.Pixels[i] & 0xFF000000) != 0)
                        decoded.Pixels[i] = (decoded.Pixels[i] & 0x00FFFFFF) | 0x77000000;
                }
            }

            var texture = new GLTexture(decoded, TextureWrapMode.Clamp, TextureWrapMode.Clamp);
            texture.SetFilterMode(TextureFilterMode.Nearest, TextureFilterMode.Nearest);

            return texture;
        }
        catch
        {
            return null;
        }
    }
}