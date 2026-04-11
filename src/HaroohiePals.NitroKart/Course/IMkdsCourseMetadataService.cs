using HaroohiePals.IO.Archive;

namespace HaroohiePals.NitroKart.Course;

public interface IMkdsCourseMetadataService
{
    MkdsCourseMetadata Load(Archive mainArchive);
    void Save(Archive mainArchive, MkdsCourseMetadata metadata);
}
