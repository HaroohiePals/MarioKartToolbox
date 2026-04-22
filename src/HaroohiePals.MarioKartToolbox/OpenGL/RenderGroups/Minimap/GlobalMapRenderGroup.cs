#nullable enable
using System;
using HaroohiePals.Graphics;
using HaroohiePals.Graphics3d;
using HaroohiePals.Graphics3d.OpenGL;
using HaroohiePals.Graphics3d.OpenGL.Renderers;
using HaroohiePals.Gui.Viewport;
using HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;
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
        var rotation = (Vector3)transform.Rotation;

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

        // Scale is in the quad's local frame (pre-rotation), so Scale.X is
        // always halfWidth and Scale.Z is always halfHeight regardless of mode.
        double halfWidth = Math.Abs(transform.Scale.X);
        double halfHeight = Math.Abs(transform.Scale.Z);

        // In rotated modes, world X/Z map back to storage Y/X (transpose).
        bool rotated = _mapSettings.Mode != MkdsGlobalMapMode.Normal;
        double storageCenterX = rotated ? transform.Translation.Z : transform.Translation.X;
        double storageCenterY = rotated ? transform.Translation.X : transform.Translation.Z;

        // Preserve the stored diagonal's sign convention (which corner was
        // labelled TopLeft vs BottomRight) so gizmo drags don't silently
        // normalize bounds that arrived X-flipped or Y-flipped.
        var oldTopLeft = _mapSettings.TopLeft;
        var oldBottomRight = _mapSettings.BottomRight;
        bool xDescending = oldTopLeft.X > oldBottomRight.X;
        bool yDescending = oldTopLeft.Y > oldBottomRight.Y;

        double newTopLeftX = xDescending ? storageCenterX + halfWidth : storageCenterX - halfWidth;
        double newBottomRightX = xDescending ? storageCenterX - halfWidth : storageCenterX + halfWidth;
        double newTopLeftY = yDescending ? storageCenterY + halfHeight : storageCenterY - halfHeight;
        double newBottomRightY = yDescending ? storageCenterY - halfHeight : storageCenterY + halfHeight;

        _mapSettings.TopLeft = new Vector2d(Math.Round(newTopLeftX), Math.Round(newTopLeftY));
        _mapSettings.BottomRight = new Vector2d(Math.Round(newBottomRightX), Math.Round(newBottomRightY));

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

        double storageCenterX = (topLeft.X + bottomRight.X) * 0.5;
        double storageCenterY = (topLeft.Y + bottomRight.Y) * 0.5;
        double halfWidth = Math.Abs(bottomRight.X - topLeft.X) * 0.5;
        double halfHeight = Math.Abs(bottomRight.Y - topLeft.Y) * 0.5;

        // In rotated modes, the stored bounds live in a frame whose X and Y
        // axes are transposed relative to world XZ. Which rotation direction
        // aligns the authored texture with the course depends on the mode.
        bool rotated = _mapSettings.Mode != MkdsGlobalMapMode.Normal;
        double worldCenterX = rotated ? storageCenterY : storageCenterX;
        double worldCenterZ = rotated ? storageCenterX : storageCenterY;

        double rotationY = _mapSettings.Mode switch
        {
            MkdsGlobalMapMode.RotateCounterClockwise => -90.0,
            MkdsGlobalMapMode.RotateClockwise => 90.0,
            _ => 0.0,
        };
        var rotation = new Vector3d(0, rotationY, 0);

        return new Transform(
            new Vector3d(worldCenterX, QUAD_HEIGHT_Y, worldCenterZ),
            rotation,
            new Vector3d(halfWidth, 1, halfHeight));
    }

    private GLTexture? CreateTexture(bool translucent)
    {
        var tiles = _course.GetTexFileOrDefault<Ncgr>("Map2D/global.NCGR");
        var palette = _course.GetTexFileOrDefault<Nclr>("Map2D/global.NCLR");
        var map = _course.GetTexFileOrDefault<Nscr>("Map2D/global1.NSCR");

        var decoded = GxUtil.DecodeChar(tiles.Character.CharacterData, palette.Palette.Palette,
            map.Screen.ScreenData, ImageFormat.Pltt16, MapFormat.Text, 
            MkdsGlobalMapConsts.DISPLAY_WIDTH, MkdsGlobalMapConsts.DISPLAY_HEIGHT, true);

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