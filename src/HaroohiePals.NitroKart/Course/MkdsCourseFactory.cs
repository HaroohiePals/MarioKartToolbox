#nullable enable
using HaroohiePals.Nitro.Fs;

namespace HaroohiePals.NitroKart.Course;

public class MkdsCourseFactory : IMkdsCourseFactory
{
    private const string DEFAULT_COURSE_MAP_PATH = "/course_map.nkm";

    private readonly MkdsCourseMetadataFactory _mkdsCourseMetadataFactory = new();

    public IMkdsCourse CreateFromFolder(string mainArcPath, string? texArcPath, string? courseMapPath = null)
        => new MkdsFolderCourse(mainArcPath, texArcPath,
            courseMapPath ?? DEFAULT_COURSE_MAP_PATH, _mkdsCourseMetadataFactory);

    public IMkdsCourse CreateFromCarc(string mainArcPath, string? texArcPath, string? courseMapPath = null)
        => new MkdsCarcCourse(mainArcPath, texArcPath,
            courseMapPath ?? DEFAULT_COURSE_MAP_PATH, _mkdsCourseMetadataFactory);

    public IMkdsCourse CreateFromRomCarc(NitroFsArchive romFs, string mainArcPath, string? texArcPath,
        string? courseMapPath = null)
        => new MkdsRomCarcCourse(romFs, mainArcPath, texArcPath,
            courseMapPath ?? DEFAULT_COURSE_MAP_PATH, _mkdsCourseMetadataFactory);
}