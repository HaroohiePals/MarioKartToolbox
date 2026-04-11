using HaroohiePals.IO.Archive;

namespace HaroohiePals.NitroKart.Course;

public sealed class MkdsFolderCourse(
    string mainPath,
    string texPath,
    string courseMapPath,
    MkdsCourseMetadataFactory mkdsCourseMetadataFactory)
    : MkdsBinaryCourse(new DiskArchive(mainPath), texPath == null ? null : new DiskArchive(texPath),
        courseMapPath, mkdsCourseMetadataFactory);