#nullable enable
using HaroohiePals.KCollision.Formats;
using HaroohiePals.NitroKart.MapData.Intermediate;

namespace HaroohiePals.NitroKart.Course;

public interface IMkdsCourse
{
    MkdsMapData MapData { get; }
    MkdsKcl Collision { get; set; }
    MkdsCourseMetadata Metadata { get; }

    MkdsMapGraphics? GlobalMapGraphics { get; set; }
    MkdsMapGraphics? GlobalMapBackgroundGraphics { get; set; }
    MkdsMapGraphics? LocalMapFirstGraphics { get; set; }
    MkdsMapGraphics? LocalMapSecondGraphics { get; set; }

    bool Save();

    T? GetMainFileOrDefault<T>(string path, T? defaultValue = default);
    T? GetTexFileOrDefault<T>(string path, T? defaultValue = default);

    bool ExistsMainFile(string path);
    bool ExistsTexFile(string path);

    public delegate void CourseFileUpdatedEventHandler(bool isTex, string path);

    event CourseFileUpdatedEventHandler CourseFileUpdated { add { } remove { } }
}
