using HaroohiePals.IO.Archive;

namespace HaroohiePals.NitroKart.Course;

public class DummyMkdsCourseMetadataService : IMkdsCourseMetadataService
{
    public MkdsCourseMetadata Load(Archive mainArchive)
        => CreateDefault();

    public void Save(Archive mainArchive, MkdsCourseMetadata metadata)
    {
        // do nothing
    }
    
    private static MkdsCourseMetadata CreateDefault() => new()
    {
        LocalMapSettings = new MkdsLocalMapSettings(),
        GlobalMapSettings = new MkdsGlobalMapSettings(),
    };
}