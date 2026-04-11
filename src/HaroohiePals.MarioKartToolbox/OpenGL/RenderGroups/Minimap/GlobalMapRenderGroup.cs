#nullable enable
using HaroohiePals.Graphics3d;
using HaroohiePals.Graphics3d.OpenGL;
using HaroohiePals.Graphics3d.OpenGL.Renderers;
using HaroohiePals.Gui.Viewport;
using HaroohiePals.MarioKartToolbox.OpenGL.Renderers;
using HaroohiePals.Nitro.Gx;
using HaroohiePals.Nitro.NitroSystem.G2d;
using HaroohiePals.NitroKart.Course;
using OpenTK.Mathematics;

namespace HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups.Minimap;

class GlobalMapRenderGroup : RenderGroup
{
    private readonly QuadRenderer? _quadRenderer;
    private readonly QuadRenderer? _quadRendererTranslucent;
    private readonly IMkdsCourse _course;

    public bool RenderTranslucent { get; set; } = false;

    public GlobalMapRenderGroup(IMkdsCourse course)
    {
        _course = course;

        var texture = CreateTexture(false);
        var textureTranslucent = CreateTexture(true);

        _quadRenderer = texture is null ? null : new QuadRenderer(texture, true, false);
        _quadRendererTranslucent = textureTranslucent is null ? null : new QuadRenderer(textureTranslucent, true, false);
    }

    public override void Render(ViewportContext context)
    {
        var renderer = RenderTranslucent ? _quadRendererTranslucent : _quadRenderer;

        if (renderer is null)
            return;

        var color = Color4.White;
        renderer.Points = [new InstancedPoint(new Vector3(0, 3000, 300), Vector3.Zero,
            new Vector3(100), color, true, null, ViewportContext.InvalidPickingId,
            false, false)];
        renderer.Render(context);
    }

    private GLTexture? CreateTexture(bool translucent)
    {
        var tiles = _course.GetTexFileOrDefault<Ncgr>("Map2D/global.NCGR");
        var palette = _course.GetTexFileOrDefault<Nclr>("Map2D/global.NCLR");
        var map = _course.GetTexFileOrDefault<Nscr>("Map2D/global1.NSCR");

        var decoded = GxUtil.DecodeChar(tiles.Character.CharacterData, palette.Palette.Palette,
            map.Screen.ScreenData, ImageFormat.Pltt16, MapFormat.Text, map.Screen.Width,
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
}