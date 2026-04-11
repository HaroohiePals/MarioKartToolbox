#nullable enable
using HaroohiePals.Nitro.Fs;

namespace HaroohiePals.NitroKart.Course;

public interface IMkdsCourseFactory
{
    IMkdsCourse CreateFromFolder(string mainArcPath, string? texArcPath, string? courseMapPath = null);
    IMkdsCourse CreateFromCarc(string mainArcPath, string? texArcPath, string? courseMapPath = null);
    IMkdsCourse CreateFromRomCarc(NitroFsArchive romFs, string mainArcPath, string? texArcPath,
        string? courseMapPath = null);
}