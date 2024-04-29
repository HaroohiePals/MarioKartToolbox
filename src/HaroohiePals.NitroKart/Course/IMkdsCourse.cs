using HaroohiePals.KCollision.Formats;
using HaroohiePals.NitroKart.MapData.Intermediate;

namespace HaroohiePals.NitroKart.Course;

public interface IMkdsCourse
{
    MkdsMapData MapData { get; }
    MkdsKcl Collision { get; set; }

    bool Save();

    T GetMainFileOrDefault<T>(string path, T defaultValue = default);
    T GetTexFileOrDefault<T>(string path, T defaultValue = default);

    //void SetMainFileData(string path, byte[] data);
    //void SetTexFileData(string path, byte[] data);

    bool ExistsMainFile(string path);
    bool ExistsTexFile(string path);

    public delegate void CourseFileUpdatedEventHandler(bool isTex, string path);

    event CourseFileUpdatedEventHandler CourseFileUpdated { add { } remove { } }
}
