#nullable enable
using System;
using HaroohiePals.Graphics;
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

class LocalMapRenderGroup : RenderGroup
{
    private readonly QuadRenderer? _quadRenderer;
    private readonly IMkdsCourse _course;
    
    private bool _isExtendedMap;

    public LocalMapRenderGroup(IMkdsCourse course)
    {
        _course = course;

        var texture = CreateTexture();

        _quadRenderer = texture is null ? null : new QuadRenderer(texture);
    }

    public override void Render(ViewportContext context)
    {
        if (_quadRenderer is null)
            return;

        var position = new Vector3(0, 2500, 0);
        var scale = new Vector3(_isExtendedMap ? 200 : 100, 100, 100);
        
        _quadRenderer.Points =
        [
            new InstancedPoint(position, Vector3.Zero, scale, Color4.White, true, null,
                ViewportContext.InvalidPickingId, false, false)
        ];
        _quadRenderer.Render(context);
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