using HaroohiePals.IO;

namespace HaroohiePals.KCollision.Formats
{
    public class MkwiiKcl : FloatKartKcl
    {
        public MkwiiKcl(byte[] data)
            : this(new MemoryStream(data, false)) { }

        public MkwiiKcl(Stream stream)
            : base(stream, Endianness.BigEndian) { }
    }
}