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
    private readonly IMkdsCourse _course;
    
    public GlobalMapRenderGroup(IMkdsCourse course)
    {
        _course = course;
        
        var texture = CreateTexture();
        
        _quadRenderer = texture is null ? null : new QuadRenderer(texture);
    }
    
    public override void Render(ViewportContext context)
    {
        if (_quadRenderer is null)
            return;
        
        var color = Color4.White;
        _quadRenderer.Points = [new InstancedPoint(new Vector3(0, 3000, 300), Vector3.Zero, new Vector3(100), color, true, null, ViewportContext.InvalidPickingId, false, false)];
        _quadRenderer.Render(context);
    }

    private GLTexture? CreateTexture()
    {
        var tiles = _course.GetTexFileOrDefault<Ncgr>("Map2D/global.ncgr");
        var palette = _course.GetTexFileOrDefault<Nclr>("Map2D/global.nclr");
        var map = _course.GetTexFileOrDefault<Nscr>("Map2D/global1.nscr");

        var decoded = GxUtil.DecodeChar(tiles.Character.CharacterData, palette.Palette.Palette,
            map.Screen.ScreenData, ImageFormat.Pltt16, MapFormat.Text, map.Screen.Width, 
            map.Screen.Height, true);

        if (decoded is null)
            return null;

        var texture = new GLTexture(decoded, TextureWrapMode.Clamp, TextureWrapMode.Clamp);
        texture.SetFilterMode(TextureFilterMode.Nearest, TextureFilterMode.Nearest);

        return texture;
    }
}