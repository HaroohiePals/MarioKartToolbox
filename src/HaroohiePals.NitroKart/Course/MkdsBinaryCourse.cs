#nullable enable
using System;
using HaroohiePals.IO.Archive;
using HaroohiePals.KCollision.Formats;
using HaroohiePals.Nitro.NitroSystem.G2d;
using HaroohiePals.NitroKart.MapData.Binary;
using HaroohiePals.NitroKart.MapData.Intermediate;

namespace HaroohiePals.NitroKart.Course;

public abstract class MkdsBinaryCourse : IMkdsCourse
{
    private const string COURSE_COLLISION_FILENAME = "course_collision.kcl";
    private const string COURSE_COLLISION_PATH = $"/{COURSE_COLLISION_FILENAME}";

    private readonly string _courseMapPath;
    private readonly IMkdsCourseMetadataService _metadataService;

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

    private MkdsMapGraphics? _globalMapGraphics;
    private MkdsMapGraphics? _globalMapBackgroundGraphics;
    private MkdsMapGraphics? _localMapFirstGraphics;
    private MkdsMapGraphics? _localMapSecondGraphics;

    public MkdsMapGraphics? GlobalMapGraphics
    {
        get => _globalMapGraphics;
        set => SetMapGraphics(value, MkdsCourseTexPaths.GLOBAL_NCGR,
            MkdsCourseTexPaths.GLOBAL_NCLR, MkdsCourseTexPaths.GLOBAL_1_NSCR);
    }

    public MkdsMapGraphics? GlobalMapBackgroundGraphics
    {
        get => _globalMapBackgroundGraphics;
        set => SetMapGraphics(value, MkdsCourseTexPaths.GLOBAL_2_NCGR,
            MkdsCourseTexPaths.GLOBAL_2_NCLR, MkdsCourseTexPaths.GLOBAL_2_NSCR);
    }

    public MkdsMapGraphics? LocalMapFirstGraphics
    {
        get => _localMapFirstGraphics;
        set => SetMapGraphics(value, MkdsCourseTexPaths.LOCAL_NCGR,
            MkdsCourseTexPaths.LOCAL_NCLR, MkdsCourseTexPaths.LOCAL_2_NSCR);
    }

    public MkdsMapGraphics? LocalMapSecondGraphics
    {
        get => _localMapSecondGraphics;
        set => SetMapGraphics(value, MkdsCourseTexPaths.LOCAL_NCGR,
            MkdsCourseTexPaths.LOCAL_NCLR, MkdsCourseTexPaths.LOCAL_3_NSCR);
    }

    private void SetMapGraphics(MkdsMapGraphics? value, string ncgrPath, string nclrPath, string nscrPath)
    {
        if (TexArchive is null)
            throw new InvalidOperationException("Course has no tex archive.");
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        TexArchive.SetFileData(ncgrPath, value.Ncgr.Write());
        TexArchive.SetFileData(nclrPath, value.Nclr.Write());
        TexArchive.SetFileData(nscrPath, value.Nscr.Write());
    }

    private static MkdsMapGraphics? LoadMapGraphics(CourseFileCache? tex,
        string ncgrPath, string nclrPath, string nscrPath)
    {
        if (tex is null)
            return null;

        var ncgr = tex.GetFileOrDefault<Ncgr>(ncgrPath);
        var nclr = tex.GetFileOrDefault<Nclr>(nclrPath);
        var nscr = tex.GetFileOrDefault<Nscr>(nscrPath);
        if (ncgr is null || nclr is null || nscr is null)
            return null;

        return new MkdsMapGraphics(ncgr, nclr, nscr);
    }

    protected MkdsBinaryCourse(Archive mainArchive, Archive? texArchive,
        string courseMapPath, IMkdsCourseMetadataService metadataService)
    {
        _courseMapPath = courseMapPath;
        _metadataService = metadataService;

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

        Metadata = _metadataService.Load(MainArchive);

        _globalMapGraphics = LoadMapGraphics(TexArchive,
            MkdsCourseTexPaths.GLOBAL_NCGR, 
            MkdsCourseTexPaths.GLOBAL_NCLR, 
            MkdsCourseTexPaths.GLOBAL_1_NSCR);
        _globalMapBackgroundGraphics = LoadMapGraphics(TexArchive,
            MkdsCourseTexPaths.GLOBAL_2_NCGR, 
            MkdsCourseTexPaths.GLOBAL_2_NCLR,
            MkdsCourseTexPaths.GLOBAL_2_NSCR);
        _localMapFirstGraphics = LoadMapGraphics(TexArchive,
            MkdsCourseTexPaths.LOCAL_NCGR, 
            MkdsCourseTexPaths.LOCAL_NCLR, 
            MkdsCourseTexPaths.LOCAL_2_NSCR);
        _localMapSecondGraphics = LoadMapGraphics(TexArchive,
            MkdsCourseTexPaths.LOCAL_NCGR, 
            MkdsCourseTexPaths.LOCAL_NCLR, 
            MkdsCourseTexPaths.LOCAL_3_NSCR);
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
        _metadataService.Save(_mainArchive, Metadata);
        TexArchive?.Flush();

        return true;
    }

    private void FileUpdated(CourseFileCache cache, string path)
    {
        if (cache == MainArchive && Archive.PathEqual(path, _courseMapPath))
            UpdateMapData();
        else if (cache == MainArchive && Archive.PathEqual(path, COURSE_COLLISION_PATH))
            _collision = MainArchive.GetFileOrDefault<MkdsKcl>(COURSE_COLLISION_PATH);
        else if (cache == TexArchive)
            RefreshMapGraphicsCaches(path);

        _courseFileUpdated?.Invoke(cache == TexArchive, path);
    }

    private void RefreshMapGraphicsCaches(string path)
    {
        bool isGlobal = Archive.PathEqual(path, MkdsCourseTexPaths.GLOBAL_NCGR)
            || Archive.PathEqual(path, MkdsCourseTexPaths.GLOBAL_NCLR)
            || Archive.PathEqual(path, MkdsCourseTexPaths.GLOBAL_1_NSCR);
        bool isGlobalBg = Archive.PathEqual(path, MkdsCourseTexPaths.GLOBAL_2_NCGR)
            || Archive.PathEqual(path, MkdsCourseTexPaths.GLOBAL_2_NCLR)
            || Archive.PathEqual(path, MkdsCourseTexPaths.GLOBAL_2_NSCR);
        bool isLocal = Archive.PathEqual(path, MkdsCourseTexPaths.LOCAL_NCGR)
            || Archive.PathEqual(path, MkdsCourseTexPaths.LOCAL_NCLR);
        bool isLocal2 = Archive.PathEqual(path, MkdsCourseTexPaths.LOCAL_2_NSCR);
        bool isLocal3 = Archive.PathEqual(path, MkdsCourseTexPaths.LOCAL_3_NSCR);

        if (isGlobal)
        {
            _globalMapGraphics = LoadMapGraphics(TexArchive,
                MkdsCourseTexPaths.GLOBAL_NCGR, MkdsCourseTexPaths.GLOBAL_NCLR, MkdsCourseTexPaths.GLOBAL_1_NSCR);
        }

        if (isGlobalBg)
        {
            _globalMapBackgroundGraphics = LoadMapGraphics(TexArchive,
                MkdsCourseTexPaths.GLOBAL_2_NCGR, MkdsCourseTexPaths.GLOBAL_2_NCLR,
                MkdsCourseTexPaths.GLOBAL_2_NSCR);
        }

        if (isLocal || isLocal2)
        {
            _localMapFirstGraphics = LoadMapGraphics(TexArchive,
                MkdsCourseTexPaths.LOCAL_NCGR, MkdsCourseTexPaths.LOCAL_NCLR, MkdsCourseTexPaths.LOCAL_2_NSCR);
        }

        if (isLocal || isLocal3)
        {
            _localMapSecondGraphics = LoadMapGraphics(TexArchive,
                MkdsCourseTexPaths.LOCAL_NCGR, MkdsCourseTexPaths.LOCAL_NCLR, MkdsCourseTexPaths.LOCAL_3_NSCR);
        }
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
}