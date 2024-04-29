using HaroohiePals.IO.Archive;
using HaroohiePals.IO.Compression;
using HaroohiePals.Nitro.Fs;
using HaroohiePals.Nitro.NitroSystem.Fnd;

namespace HaroohiePals.NitroKart.Course;

public class MkdsRomCarcCourse : MkdsBinaryCourse
{
    private readonly NitroFsArchive _romFs;

    public string MainPath { get; }
    public string TexPath { get; }

    private static MemoryArchive LoadCarc(NitroFsArchive romFs, string path)
    {
        var carcData = romFs.GetFileData(path);
        var narcData = Lz77.Decompress(carcData);
        var narc = new Narc(narcData);
        return new MemoryArchive(narc.ToArchive());
    }

    public MkdsRomCarcCourse(NitroFsArchive romFs, string mainPath, string texPath, string courseMapPath)
        : base(LoadCarc(romFs, mainPath), texPath == null ? null : LoadCarc(romFs, texPath), courseMapPath)
    {
        _romFs = romFs;
        MainPath = mainPath;
        TexPath = texPath;
    }

    public override bool Save()
    {
        bool result = base.Save();

        if (result)
        {
            _romFs.SetFileData(MainPath, Lz77.Compress(new Narc(_mainArchive).Write()));
            if (TexPath != null)
                _romFs.SetFileData(TexPath, Lz77.Compress(new Narc(_texArchive).Write()));
        }

        return true;
    }
}