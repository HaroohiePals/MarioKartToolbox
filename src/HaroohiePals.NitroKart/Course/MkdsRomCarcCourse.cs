#nullable enable
using HaroohiePals.IO.Archive;
using HaroohiePals.IO.Compression;
using HaroohiePals.Nitro.Fs;
using HaroohiePals.Nitro.NitroSystem.Fnd;

namespace HaroohiePals.NitroKart.Course;

public class MkdsRomCarcCourse(
    NitroFsArchive romFs,
    string mainPath,
    string? texPath,
    string courseMapPath,
    IMkdsCourseMetadataService metadataService)
    : MkdsBinaryCourse(LoadCarc(romFs, mainPath), texPath is null ? null : LoadCarc(romFs, texPath), courseMapPath,
        metadataService)
{
    private static MemoryArchive LoadCarc(NitroFsArchive romFs, string path)
    {
        byte[]? carcData = romFs.GetFileData(path);
        byte[]? narcData = new Lz77CompressionAlgorithm().Decompress(carcData);
        var narc = new Narc(narcData);
        return new MemoryArchive(narc.ToArchive());
    }

    public override bool Save()
    {
        bool result = base.Save();

        if (!result)
            return true;

        var lz77 = new Lz77CompressionAlgorithm();
        romFs.SetFileData(mainPath, lz77.Compress(new Narc(_mainArchive).Write()));
        if (texPath is not null)
        {
            romFs.SetFileData(texPath, lz77.Compress(new Narc(_texArchive).Write()));
        }

        return true;
    }
}