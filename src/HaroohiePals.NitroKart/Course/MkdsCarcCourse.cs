#nullable enable
using HaroohiePals.IO.Archive;
using HaroohiePals.IO.Compression;
using HaroohiePals.Nitro.NitroSystem.Fnd;
using System.IO;

namespace HaroohiePals.NitroKart.Course;

public sealed class MkdsCarcCourse(
    string mainPath,
    string? texPath,
    string courseMapPath,
    IMkdsCourseMetadataService metadataService)
    : MkdsBinaryCourse(LoadCarc(mainPath), texPath is null ? null : LoadCarc(texPath),
        courseMapPath, metadataService)
{
    private static MemoryArchive LoadCarc(string path)
    {
        byte[] carcData = File.ReadAllBytes(path);
        byte[] narcData = new Lz77CompressionAlgorithm().Decompress(carcData);
        var narc = new Narc(narcData);
        return new MemoryArchive(narc.ToArchive());
    }

    public override bool Save()
    {
        bool result = base.Save();

        if (!result)
            return true;

        var lz77 = new Lz77CompressionAlgorithm();
        File.WriteAllBytes(mainPath, lz77.Compress(new Narc(_mainArchive).Write()));
        if (texPath is not null)
        {
            File.WriteAllBytes(texPath, lz77.Compress(new Narc(_texArchive).Write()));
        }

        return true;
    }
}