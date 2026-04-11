#nullable enable
using HaroohiePals.IO.Archive;

namespace HaroohiePals.NitroKart.Course;

public class MkdsCourseMetadataFactory
{
    public MkdsCourseMetadata Create(Archive mainArchive)
    {
        //todo
        return new MkdsCourseMetadata
        {
            LocalMapSettings = new MkdsLocalMapSettings()
        };
    }
}