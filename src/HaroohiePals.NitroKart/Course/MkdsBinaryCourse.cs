#nullable enable
using HaroohiePals.IO.Archive;
using HaroohiePals.KCollision.Formats;
using HaroohiePals.NitroKart.MapData.Binary;
using HaroohiePals.NitroKart.MapData.Intermediate;

namespace HaroohiePals.NitroKart.Course;

public abstract class MkdsBinaryCourse : IMkdsCourse
{
    private const string COURSE_COLLISION_FILENAME = "course_collision.kcl";
    private const string COURSE_METADATA_FILENAME = "metadata.json";
    private const string COURSE_COLLISION_PATH = $"/{COURSE_COLLISION_FILENAME}";
    private const string COURSE_METADATA_PATH = $"/{COURSE_METADATA_FILENAME}";

    private readonly string _courseMapPath;

    protected readonly Archive _mainArchive;
    protected readonly Archive? _texArchive;

    private event IMkdsCourse.CourseFileUpdatedEventHandler? _courseFileUpdated;

    public CourseFileCache MainArchive { get; }
    public CourseFileCache? TexArchive { get; }

    public MkdsMapData? MapData { get; private set; }

    private MkdsKcl _collision;

    public MkdsKcl Collision
    {
        get => _collision;
        set => MainArchive.SetFileData(COURSE_COLLISION_PATH, value.Write());
    }

    public MkdsCourseMetadata Metadata { get; private set; }

    protected MkdsBinaryCourse(Archive mainArchive, Archive? texArchive,
        string courseMapPath, MkdsCourseMetadataFactory mkdsCourseMetadataFactory)
    {
        _courseMapPath = courseMapPath;

        _mainArchive = mainArchive;
        _texArchive = texArchive;

        MainArchive = new CourseFileCache(_mainArchive);
        MainArchive.FileUpdated += FileUpdated;

        if (_texArchive is not null)
        {
            TexArchive = new CourseFileCache(_texArchive);
            TexArchive.FileUpdated += FileUpdated;
        }

        UpdateMapData();
        _collision = MainArchive.GetFileOrDefault<MkdsKcl>(COURSE_COLLISION_PATH);

        Metadata = mkdsCourseMetadataFactory.Create(MainArchive);
    }

    event IMkdsCourse.CourseFileUpdatedEventHandler IMkdsCourse.CourseFileUpdated
    {
        add => _courseFileUpdated += value;

        remove => _courseFileUpdated -= value;
    }

    public virtual bool Save()
    {
        MainArchive.Flush();
        _mainArchive.SetFileData(_courseMapPath, NkmdFactory.FromMapData(MapData).Write());
        _mainArchive.SetFileData(COURSE_COLLISION_PATH, Collision.Write());
        TexArchive?.Flush();

        return true;
    }

    private void FileUpdated(CourseFileCache cache, string path)
    {
        if (cache == MainArchive && Archive.PathEqual(path, _courseMapPath))
            UpdateMapData();
        else if (cache == MainArchive && Archive.PathEqual(path, COURSE_COLLISION_PATH))
            _collision = MainArchive.GetFileOrDefault<MkdsKcl>(COURSE_COLLISION_PATH);

        _courseFileUpdated?.Invoke(cache == TexArchive, path);
    }

    private void UpdateMapData()
    {
        MapData = MainArchive.GetFileOrDefault(_courseMapPath,
            nkmData => MkdsMapDataFactory.CreateFromNkm(new Nkmd(nkmData)));
    }

    public T? GetMainFileOrDefault<T>(string path, T? defaultValue = default)
        => MainArchive.GetFileOrDefault(path, defaultValue);

    public T? GetTexFileOrDefault<T>(string path, T? defaultValue = default)
        => TexArchive is null ? default : TexArchive.GetFileOrDefault(path, defaultValue);

    public bool ExistsMainFile(string path)
        => MainArchive.ExistsFile(path);

    public bool ExistsTexFile(string path)
        => TexArchive?.ExistsFile(path) ?? false;

    private void SaveMetadata()
    {
        //todo
    }
}