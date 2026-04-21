#nullable enable
using HaroohiePals.Graphics3d;
using HaroohiePals.MarioKartToolbox.KCollision;
using HaroohiePals.Mathematics;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;

class GenerateGlobalMapViewModel
{
    // Matches the "road" classification in KclPrismRenderGroup.
    private static readonly HashSet<MkdsCollisionType> RoadTypes =
    [
        MkdsCollisionType.Road,
        MkdsCollisionType.SlipperyRoad,
        MkdsCollisionType.SlipperyRoad2,
        MkdsCollisionType.BoostPad,
        MkdsCollisionType.JumpPad,
        MkdsCollisionType.RoadNoDrivers,
        MkdsCollisionType.FallsWater,
        MkdsCollisionType.BoostPadMinSpeed,
        MkdsCollisionType.Loop,
        MkdsCollisionType.SpecialRoad,
    ];

    private readonly ICourseEditorContext _courseEditorContext;

    public GenerateGlobalMapSettings Settings;

    public IReadOnlyList<Triangle> LoadedTriangles { get; private set; } = [];
    public string? ErrorMessage { get; private set; }

    public int TriangleCount => LoadedTriangles.Count;
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public GenerateGlobalMapViewModel(ICourseEditorContext courseEditorContext)
    {
        _courseEditorContext = courseEditorContext;
        Settings.SourceType = GenerateGlobalMapSourceType.CourseKcl;
        Settings.ObjFilePath = "";
        ReloadTriangles();
    }

    public void ReloadTriangles()
    {
        ErrorMessage = null;

        try
        {
            LoadedTriangles = Settings.SourceType switch
            {
                GenerateGlobalMapSourceType.CourseKcl => LoadFromCourseKcl(),
                GenerateGlobalMapSourceType.ExternalObj => LoadFromObj(Settings.ObjFilePath),
                _ => [],
            };
        }
        catch (System.Exception ex)
        {
            LoadedTriangles = [];
            ErrorMessage = ex.Message;
        }
    }

    private IReadOnlyList<Triangle> LoadFromCourseKcl()
    {
        var kcl = _courseEditorContext.Course.Collision;
        if (kcl is null)
        {
            ErrorMessage = "The current course has no collision (KCL) data.";
            return [];
        }

        return kcl.PrismData
            .Where(p => IsRoad(p.Attribute))
            .Select(p => p.ToTriangle(kcl))
            .ToList();
    }

    private static IReadOnlyList<Triangle> LoadFromObj(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return [];

        if (!File.Exists(path))
            throw new FileNotFoundException("OBJ file not found.", path);

        var obj = new Obj(File.ReadAllBytes(path));
        var result = new List<Triangle>();
        foreach (var face in obj.Faces)
        {
            var idx = face.VertexIndices;
            if (idx is null || idx.Length < 3)
                continue;

            // Fan-triangulate any n-gon face around the first vertex.
            var a = obj.Vertices[idx[0]];
            for (int i = 1; i < idx.Length - 1; i++)
            {
                var b = obj.Vertices[idx[i]];
                var c = obj.Vertices[idx[i + 1]];
                result.Add(new Triangle(a, b, c));
            }
        }

        return result;
    }

    private static bool IsRoad(ushort rawAttribute)
    {
        MkdsKclPrismAttribute attr = rawAttribute;
        return RoadTypes.Contains(attr.Type);
    }
}
