using HaroohiePals.IO.Archive;
using HaroohiePals.IO.Compression;
using HaroohiePals.Nitro.NitroSystem.Fnd;
using System.IO;

namespace HaroohiePals.NitroKart.Course;

public class MkdsCarcCourse : MkdsBinaryCourse
{
    public string MainPath { get; }
    public string TexPath { get; }

    private static MemoryArchive LoadCarc(string path)
    {
        var carcData = File.ReadAllBytes(path);
        var narcData = new Lz77CompressionAlgorithm().Decompress(carcData);
        var narc = new Narc(narcData);
        return new MemoryArchive(narc.ToArchive());
    }

    public MkdsCarcCourse(string mainPath, string texPath, string courseMapPath)
        : base(LoadCarc(mainPath), texPath == null ? null : LoadCarc(texPath), courseMapPath)
    {
        MainPath = mainPath;
        TexPath = texPath;
    }

    public override bool Save()
    {
        bool result = base.Save();

        if (result)
        {
            var lz77 = new Lz77CompressionAlgorithm();
            File.WriteAllBytes(MainPath, lz77.Compress(new Narc(_mainArchive).Write()));
            if (TexPath != null)
            {
                File.WriteAllBytes(TexPath, lz77.Compress(new Narc(_texArchive).Write()));
            }
        }

        return true;
    }
}