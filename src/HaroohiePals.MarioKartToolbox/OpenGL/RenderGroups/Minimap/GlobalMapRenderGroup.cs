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

class GlobalMapRenderGroup : RenderGroup
{
    private const float QUAD_HEIGHT_Y = 3000f;

    private readonly QuadRenderer? _quadRenderer;
    private readonly QuadRenderer? _quadRendererTranslucent;
    private readonly IMkdsCourse _course;

    private MkdsGlobalMapSettings _mapSettings => _course.Metadata.GlobalMapSettings!;

    public bool RenderTranslucent { get; set; } = false;

    public GlobalMapRenderGroup(IMkdsCourse course)
    {
        _course = course;

        var texture = CreateTexture(false);
        var textureTranslucent = CreateTexture(true);

        _quadRenderer = texture is null ? null : new QuadRenderer(texture, true, false);
        _quadRendererTranslucent =
            textureTranslucent is null ? null : new QuadRenderer(textureTranslucent, true, false);
    }

    public override void Render(ViewportContext context)
    {
        var renderer = RenderTranslucent ? _quadRendererTranslucent : _quadRenderer;

        if (renderer is null)
            return;

        var transform = GetCurrentTransform();
        var position = (Vector3)transform.Translation;
        var scale = (Vector3)transform.Scale / 10;
        var rotation = _mapSettings.Rotate90Degrees ? new Vector3(0, 90, 0) : Vector3.Zero;

        uint pickingId = context.GetPickingId(PickingGroupId, 0);
        bool isHovered = context.IsHovered(_mapSettings);

        renderer.Points =
        [
            new InstancedPoint(position, rotation, scale, Color4.White, true, _mapSettings,
                pickingId, isHovered, false)
        ];
        renderer.Render(context);
    }

    public override object GetObject(int index) => _mapSettings;

    public override bool ContainsObject(object obj) => obj == _mapSettings;

    public override bool TryGetObjectTransform(object obj, int subIndex, out Transform transform)
    {
        if (obj != _mapSettings)
        {
            transform = Transform.Identity;
            return false;
        }

        transform = GetCurrentTransform();
        return true;
    }

    public override bool TrySetObjectTransform(object obj, int subIndex, in Transform transform)
    {
        if (obj != _mapSettings)
            return false;

        double halfTotalWidth = transform.Scale.X;
        double halfHeight = transform.Scale.Z;
        double centerX = transform.Translation.X;
        double centerY = transform.Translation.Z;

        double totalWidth = halfTotalWidth * 2f;
        double height = halfHeight * 2f;
        double leftEdge = centerX - totalWidth * 0.5;
        double topEdge = centerY - height * 0.5;

        _mapSettings.TopLeft = new Vector2d(Math.Round(leftEdge), Math.Round(topEdge));
        _mapSettings.BottomRight = new Vector2d(Math.Round(leftEdge + totalWidth), Math.Round(topEdge + height));

        return true;
    }

    public override bool TryGetLocalObjectBounds(object obj, int subIndex, out Box3d bounds)
    {
        if (obj != _mapSettings)
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
        var topLeft = _mapSettings.TopLeft;
        var bottomRight = _mapSettings.BottomRight;

        double width = bottomRight.X - topLeft.X;
        double height = bottomRight.Y - topLeft.Y;

        double centerX = topLeft.X + width * 0.5;
        double centerY = (topLeft.Y + bottomRight.Y) * 0.5;

        return new Transform(
            new Vector3d(centerX, QUAD_HEIGHT_Y, centerY),
            Vector3d.Zero,
            new Vector3d(width * 0.5f, 1, height * 0.5f));
    }

    private GLTexture? CreateTexture(bool translucent)
    {
        var tiles = _course.GetTexFileOrDefault<Ncgr>("Map2D/global.NCGR");
        var palette = _course.GetTexFileOrDefault<Nclr>("Map2D/global.NCLR");
        var map = _course.GetTexFileOrDefault<Nscr>("Map2D/global1.NSCR");

        const int dsWidth = 256;
        const int dsHeight = 192;

        var decoded = GxUtil.DecodeChar(tiles.Character.CharacterData, palette.Palette.Palette,
            map.Screen.ScreenData, ImageFormat.Pltt16, MapFormat.Text, dsWidth, dsHeight, true);

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
}