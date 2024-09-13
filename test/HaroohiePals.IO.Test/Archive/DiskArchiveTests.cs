using HaroohiePals.IO.Archive;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace HaroohiePals.IO.Test;

class DiskArchiveTests
{
    [Test]
    public void ArchiveOperationsTest()
    {
        if (Directory.Exists("arctest"))
        {
            Directory.Delete("arctest", true);
        }

        Directory.CreateDirectory("arctest/foo/bar");
        var test1Data = new byte[] { 0, 1, 2, 3 };
        var test2Data = new byte[] { 3, 2, 1, 0 };
        File.WriteAllBytes("arctest/foo/test1.bin", test1Data);
        File.WriteAllBytes("arctest/test2.bin", test2Data);
        File.WriteAllBytes("arctest/test2_2.bin", test2Data);

        var arc = new DiskArchive("arctest");
        Assert.That(arc.ExistsFile("/test2.bin"), Is.True);
        Assert.That(arc.ExistsFile("test2_2.bin"), Is.True);
        Assert.That(arc.ExistsFile("/foo/test1.bin"), Is.True);
        Assert.That(arc.ExistsDirectory("/foo"), Is.True);
        Assert.That(arc.ExistsDirectory("/foo/bar/"), Is.True);
        Assert.That(arc.GetFileData("/foo/test1.bin"), Is.EqualTo(test1Data));
        Assert.That(arc.GetFileData("/test2.bin"), Is.EqualTo(test2Data));
        Assert.That(arc.GetFileData("/test2_2.bin"), Is.EqualTo(test2Data));

        var files = arc.EnumerateFiles("/", false).ToArray();
        Assert.That(files.Length, Is.EqualTo(2));
        Assert.That(files, Contains.Item("test2.bin"));
        Assert.That(files, Contains.Item("test2_2.bin"));

        var dirs = arc.EnumerateDirectories("/", false).ToArray();
        Assert.That(dirs.Length, Is.EqualTo(1));
        Assert.That(dirs, Contains.Item("foo"));

        arc.DeleteFile("/test2_2.bin");
        Assert.That(arc.ExistsFile("/test2_2.bin"), Is.False);
        Assert.That(File.Exists("arctest/test2_2.bin"), Is.False);

        var test3Data = new byte[] { 4, 5, 6, 7 };
        arc.SetFileData("/foo/bar/test3.bin", test3Data);
        Assert.That(File.Exists("arctest/foo/bar/test3.bin"), Is.True);
        Assert.That(File.ReadAllBytes("arctest/foo/bar/test3.bin"), Is.EqualTo(test3Data));
        Assert.That(arc.GetFileData("/foo/bar/test3.bin"), Is.EqualTo(test3Data));

        Assert.Throws<Exception>(() => arc.GetFileData("/../test"));
        Assert.Throws<Exception>(() => arc.GetFileData("/foo/bar/../../../test"));

        Directory.Delete("arctest", true);
    }
}