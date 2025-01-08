using NUnit.Framework;

namespace HaroohiePals.IO.Test.Archive;

class ArchiveTests
{
    [TestCase("/", "/")]
    [TestCase("", "/")]
    [TestCase("/foo", "/foo")]
    [TestCase("/foo/", "/foo")]
    [TestCase("/foo/..", "/")]
    [TestCase("/foo/../", "/")]
    [TestCase("/bar/../.", "/")]
    [TestCase("/bar/.././", "/")]
    [TestCase("/foo//./", "/foo")]
    [TestCase("foo//./", "/foo")]
    [TestCase("foo//./test", "/foo/test")]
    public void Archive_NormalizePath_ReturnsNormalizedPath(string input, string expected)
    {
        Assert.That(IO.Archive.Archive.NormalizePath(input), Is.EqualTo(expected));
    }

    [TestCase(new []{"/"}, "/")]
    [TestCase(new []{"/", "foo"}, "/foo")]
    [TestCase(new []{"/", "foo", "bar"}, "/foo/bar")]
    [TestCase(new []{"/", "..", "bar"}, "/../bar")]
    public void Archive_JoinPath_JoinsPath(string[] input, string expected)
    {
        Assert.That(IO.Archive.Archive.JoinPath(input), Is.EqualTo(expected));
    }
}
