using Xunit;

namespace HaroohiePals.IO.Test.Archive
{
    public class ArchiveTest
    {
        [Theory]
        [InlineData("/", "/")]
        [InlineData("", "/")]
        [InlineData("/foo", "/foo")]
        [InlineData("/foo/", "/foo")]
        [InlineData("/foo/..", "/")]
        [InlineData("/foo/../", "/")]
        [InlineData("/bar/../.", "/")]
        [InlineData("/bar/.././", "/")]
        [InlineData("/foo//./", "/foo")]
        [InlineData("foo//./", "/foo")]
        [InlineData("foo//./test", "/foo/test")]
        public void PathNormalizationTest(string input, string expected)
        {
            Assert.Equal(expected, IO.Archive.Archive.NormalizePath(input));
        }

        [Theory]
        [InlineData(new []{"/"}, "/")]
        [InlineData(new []{"/", "foo"}, "/foo")]
        [InlineData(new []{"/", "foo", "bar"}, "/foo/bar")]
        [InlineData(new []{"/", "..", "bar"}, "/../bar")]
        public void PathJoinTest(string[] input, string expected)
        {
            Assert.Equal(expected, IO.Archive.Archive.JoinPath(input));
        }
    }
}
