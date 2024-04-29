using HaroohiePals.IO.Archive;
using HaroohiePals.KCollision.Formats;
using HaroohiePals.Nitro.NitroSystem.G3d.Intermediate.Model;
using HaroohiePals.NitroKart.MapData.Binary;
using HaroohiePals.NitroKart.MapData.Intermediate;

namespace HaroohiePals.NitroKart.Course;

public class MkdsIntermediateCourse : IMkdsCourse
{
    public const string CourseCollisionPath = "/course_collision.kcl";

    private string _courseMapPath;

    protected readonly Archive _fileArchive;
    public CourseFileCache FileArchive { get; }

    public MkdsMapData MapData { get; private set; }

    private MkdsKcl _collision;
    public MkdsKcl Collision
    {
        get => _collision;
        set => FileArchive.SetFileData(CourseCollisionPath, value.Write());
    }

    public MkdsIntermediateCourse(string basePath, string courseMapPath)
    {
        _courseMapPath = courseMapPath;

        _fileArchive = new DiskArchive(basePath);
        FileArchive = new CourseFileCache(_fileArchive);

        FileArchive.FileUpdated += FileUpdated;

        UpdateMapData();

        if (ExistsMainFile(CourseCollisionPath))
            UpdateCollision();
    }

    private event IMkdsCourse.CourseFileUpdatedEventHandler _courseFileUpdated;

    event IMkdsCourse.CourseFileUpdatedEventHandler IMkdsCourse.CourseFileUpdated
    {
        add
        {
            _courseFileUpdated += value;
        }

        remove
        {
            _courseFileUpdated -= value;
        }
    }

    private void UpdateMapData()
    {
        if (_courseMapPath.ToLower().EndsWith("inkm"))
            MapData = FileArchive.GetFileOrDefault(_courseMapPath, xmlData => MkdsMapDataFactory.CreateFromXml(xmlData));
        else
            MapData = FileArchive.GetFileOrDefault(_courseMapPath, nkmData => MkdsMapDataFactory.CreateFromNkm(new Nkmd(nkmData)));
    }

    private void UpdateCollision()
    {
        _collision = FileArchive.GetFileOrDefault<MkdsKcl>(CourseCollisionPath);
    }

    private void FileUpdated(CourseFileCache cache, string path)
    {
        if (Archive.PathEqual(path, _courseMapPath))
            UpdateMapData();
        else if (Archive.PathEqual(path, CourseCollisionPath))
            UpdateCollision();

        _courseFileUpdated?.Invoke(false, path);
    }

    private string GetIntermediatePath(string path)
    {
        switch (System.IO.Path.GetExtension(path).ToLower())
        {
            case ".nsbmd":
                return System.IO.Path.ChangeExtension(path, ".imd");
            case ".nsbta":
                return System.IO.Path.ChangeExtension(path, ".ita");
            case ".nkm":
                return System.IO.Path.ChangeExtension(path, ".imd");
        }

        return path;
    }

    public bool ExistsMainFile(string path) => FileArchive.ExistsFile(GetIntermediatePath(path));

    public bool ExistsTexFile(string path) => ExistsMainFile(path);

    public T GetMainFileOrDefault<T>(string path, T defaultValue = default)
    {
        string intermediatePath = GetIntermediatePath(path);
        string targetExt = System.IO.Path.GetExtension(path).ToLower();

        if (FileArchive.ExistsFile(intermediatePath))
        {
            switch (targetExt)
            {
                case ".nsbmd":
                    var imd = new Imd(FileArchive.GetFileData(intermediatePath));
                    var convertedNsbmd = imd.ToNsbmd(System.IO.Path.GetFileNameWithoutExtension(intermediatePath));
                    convertedNsbmd.TextureSet = imd.ToNsbtx().TextureSet;
                    return convertedNsbmd is T ? (T)(object)convertedNsbmd : default;
            }
        }

        return FileArchive.GetFileOrDefault(path, defaultValue);
    }

    public T GetTexFileOrDefault<T>(string path, T defaultValue = default) => GetMainFileOrDefault(path, defaultValue);

    public bool Save()
    {
        FileArchive.Flush();

        if (MapData != null)
        {
            if (_courseMapPath.ToLower().EndsWith("inkm"))
                _fileArchive.SetFileData(_courseMapPath, MapData.WriteXml());
            else
                _fileArchive.SetFileData(_courseMapPath, NkmdFactory.FromMapData(MapData).Write());
        }
            
        if (Collision != null)
            _fileArchive.SetFileData(CourseCollisionPath, Collision.Write());

        return true;
    }
}
