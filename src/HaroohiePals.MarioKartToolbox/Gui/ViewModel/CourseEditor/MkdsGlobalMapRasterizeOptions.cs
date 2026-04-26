#nullable enable
using HaroohiePals.Mathematics;
using HaroohiePals.NitroKart.Course;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;

struct MkdsGlobalMapRasterizeOptions
{
    public IReadOnlyList<Triangle> Triangles;
    public Vector2d TopLeft;
    public Vector2d BottomRight;
    public MkdsGlobalMapMode Mode;
    public Box2i SafeArea;
    public double TriangleExpansion;
    public Vector3d StartPointPosition;
    public Vector3d StartPointRotation;
    public bool ShowStartMarker;
    public int StartMarkerWidth;
    public bool ShowStartMarkerLabel;
    public Vector2i StartMarkerLabelOffset;
}