using HaroohiePals.IO.Archive;
using HaroohiePals.KCollision.Formats;
using HaroohiePals.NitroKart.MapData.Binary;
using HaroohiePals.NitroKart.MapData.Intermediate;

namespace HaroohiePals.NitroKart.Course;

public abstract class MkdsBinaryCourse : IMkdsCourse
{
    public const string CourseCollisionPath = "/course_collision.kcl";

    protected readonly Archive _mainArchive;
    protected readonly Archive _texArchive;

    private string _courseMapPath;

    public CourseFileCache MainArchive { get; }
    public CourseFileCache TexArchive { get; }

    public MkdsMapData MapData { get; private set; }

    private MkdsKcl _collision;
    public MkdsKcl Collision
    {
        get => _collision; 
        set => MainArchive.SetFileData(CourseCollisionPath, value.Write());
    }

    protected MkdsBinaryCourse(Archive mainArchive, Archive texArchive, string courseMapPath)
    {
        _courseMapPath = courseMapPath;

        _mainArchive = mainArchive;
        _texArchive = texArchive;

        MainArchive = new(_mainArchive);
        MainArchive.FileUpdated += FileUpdated;

        if (_texArchive != null)
        {
            TexArchive = new(_texArchive);
            TexArchive.FileUpdated += FileUpdated;
        }

        UpdateMapData();
        _collision = MainArchive.GetFileOrDefault<MkdsKcl>(CourseCollisionPath);
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

    public virtual bool Save()
    {
        MainArchive.Flush();
        if (MapData != null)
            _mainArchive.SetFileData(_courseMapPath, NkmdFactory.FromMapData(MapData).Write());
        if (Collision != null)
            _mainArchive.SetFileData(CourseCollisionPath, Collision.Write());
        TexArchive?.Flush();

        return true;
    }

    private void FileUpdated(CourseFileCache cache, string path)
    {
        if (cache == MainArchive && Archive.PathEqual(path, _courseMapPath))
            UpdateMapData();
        else if (cache == MainArchive && Archive.PathEqual(path, CourseCollisionPath))
            _collision = MainArchive.GetFileOrDefault<MkdsKcl>(CourseCollisionPath);

        _courseFileUpdated?.Invoke(cache == TexArchive, path);
    }

    private void UpdateMapData()
    {
        MapData = MainArchive.GetFileOrDefault(_courseMapPath,
            nkmData => MkdsMapDataFactory.CreateFromNkm(new Nkmd(nkmData)));
    }

    public T GetMainFileOrDefault<T>(string path, T defaultValue = default)
        => MainArchive.GetFileOrDefault(path, defaultValue);

    public T GetTexFileOrDefault<T>(string path, T defaultValue = default)
        => TexArchive.GetFileOrDefault(path, defaultValue);

    //public void SetMainFileData(string path, byte[] data)
    //    => MainArchive.SetFileData(path, data);

    //public void SetTexFileData(string path, byte[] data)
    //    => TexArchive.SetFileData(path, data);

    public bool ExistsMainFile(string path) 
        => MainArchive.ExistsFile(path);
    public bool ExistsTexFile(string path)
        => TexArchive.ExistsFile(path);
}