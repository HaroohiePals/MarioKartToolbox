using HaroohiePals.IO.Archive;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace HaroohiePals.IO.Test
{
    public class DiskArchiveTest
    {
        [Fact]
        public void ArchiveOperationsTest()
        {
            if (Directory.Exists("arctest"))
                Directory.Delete("arctest", true);

            Directory.CreateDirectory("arctest/foo/bar");
            var test1Data = new byte[] { 0, 1, 2, 3 };
            var test2Data = new byte[] { 3, 2, 1, 0 };
            File.WriteAllBytes("arctest/foo/test1.bin", test1Data);
            File.WriteAllBytes("arctest/test2.bin", test2Data);
            File.WriteAllBytes("arctest/test2_2.bin", test2Data);

            var arc = new DiskArchive("arctest");
            Assert.True(arc.ExistsFile("/test2.bin"));
            Assert.True(arc.ExistsFile("test2_2.bin"));
            Assert.True(arc.ExistsFile("/foo/test1.bin"));
            Assert.True(arc.ExistsDirectory("/foo"));
            Assert.True(arc.ExistsDirectory("/foo/bar/"));
            Assert.Equal(test1Data, arc.GetFileData("/foo/test1.bin"));
            Assert.Equal(test2Data, arc.GetFileData("/test2.bin"));
            Assert.Equal(test2Data, arc.GetFileData("/test2_2.bin"));

            var files = arc.EnumerateFiles("/", false).ToArray();
            Assert.Equal(2, files.Length);
            Assert.Contains("test2.bin", files);
            Assert.Contains("test2_2.bin", files);

            var dirs = arc.EnumerateDirectories("/", false).ToArray();
            Assert.Equal(1, dirs.Length);
            Assert.Contains("foo", dirs);

            arc.DeleteFile("/test2_2.bin");
            Assert.False(arc.ExistsFile("/test2_2.bin"));
            Assert.False(File.Exists("arctest/test2_2.bin"));

            var test3Data = new byte[] { 4, 5, 6, 7 };
            arc.SetFileData("/foo/bar/test3.bin", test3Data);
            Assert.True(File.Exists("arctest/foo/bar/test3.bin"));
            Assert.Equal(test3Data, File.ReadAllBytes("arctest/foo/bar/test3.bin"));
            Assert.Equal(test3Data, arc.GetFileData("/foo/bar/test3.bin"));

            Assert.Throws<Exception>(() => arc.GetFileData("/../test"));
            Assert.Throws<Exception>(() => arc.GetFileData("/foo/bar/../../../test"));

            Directory.Delete("arctest", true);
        }
    }
}