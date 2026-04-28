#nullable enable
using HaroohiePals.IO.Archive;

namespace HaroohiePals.NitroKart.Course;

public static class MkdsCourseTexPaths
{
    private const string MAP_2D = "/Map2D";

    public const string GLOBAL_NCGR = $"{MAP_2D}/global.NCGR";
    public const string GLOBAL_NCLR = $"{MAP_2D}/global.NCLR";
    public const string GLOBAL_1_NSCR = $"{MAP_2D}/global1.NSCR";
    public const string GLOBAL_2_NCGR = $"{MAP_2D}/global2.NCGR";
    public const string GLOBAL_2_NCLR = $"{MAP_2D}/global2.NCLR";
    public const string GLOBAL_2_NSCR = $"{MAP_2D}/global2.NSCR";

    public const string LOCAL_NCGR = $"{MAP_2D}/local.NCGR";
    public const string LOCAL_NCLR = $"{MAP_2D}/local.NCLR";
    public const string LOCAL_2_NSCR = $"{MAP_2D}/local2.NSCR";
    public const string LOCAL_3_NSCR = $"{MAP_2D}/local3.NSCR";

    public static bool IsGlobalMapPath(string path)
        => Archive.PathEqual(path, GLOBAL_NCGR)
        || Archive.PathEqual(path, GLOBAL_NCLR)
        || Archive.PathEqual(path, GLOBAL_1_NSCR);
}
