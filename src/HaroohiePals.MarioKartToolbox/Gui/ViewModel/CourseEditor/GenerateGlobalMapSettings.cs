#nullable enable
using HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;
using HaroohiePals.NitroKart.Course;
using OpenTK.Mathematics;

namespace HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;

struct GenerateGlobalMapSettings
{
    private const int DEFAULT_SAFE_AREA_MARGIN_TOP = 120;
    private const int DEFAULT_SAFE_AREA_MARGIN_LEFT = 24;
    private const int DEFAULT_SAFE_AREA_MARGIN_RIGHT = 8;
    private const int DEFAULT_SAFE_AREA_MARGIN_BOTTOM = 8;

    public static Box2i DefaultSafeArea = new Box2i(
        new Vector2i(DEFAULT_SAFE_AREA_MARGIN_TOP, DEFAULT_SAFE_AREA_MARGIN_LEFT),
        new Vector2i(MkdsGlobalMapConsts.DISPLAY_WIDTH - DEFAULT_SAFE_AREA_MARGIN_RIGHT, 
            MkdsGlobalMapConsts.DISPLAY_HEIGHT - DEFAULT_SAFE_AREA_MARGIN_BOTTOM));

    public GenerateGlobalMapSourceType SourceType;
    public string? ObjFilePath;

    public MkdsGlobalMapMode Mode;
    public Vector2d TopLeft;
    public Vector2d BottomRight;
    public float TriangleExpansion = 1f;

    public Box2i SafeArea = DefaultSafeArea;
    public bool ShowSafeArea = true;

    public GenerateGlobalMapSettings() { }
}
