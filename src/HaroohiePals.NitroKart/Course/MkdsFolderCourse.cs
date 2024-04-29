using HaroohiePals.IO.Archive;

namespace HaroohiePals.NitroKart.Course;

public class MkdsFolderCourse : MkdsBinaryCourse
{
    public MkdsFolderCourse(string mainPath, string texPath, string courseMapPath)
        : base(new DiskArchive(mainPath), texPath == null ? null : new DiskArchive(texPath), courseMapPath) { }
}