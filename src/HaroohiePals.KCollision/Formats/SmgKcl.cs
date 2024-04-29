using HaroohiePals.IO;

namespace HaroohiePals.KCollision.Formats
{
    public class SmgKcl : FloatPlatformerKcl
    {
        public SmgKcl(byte[] data)
            : this(new MemoryStream(data, false)) { }

        public SmgKcl(Stream stream)
            : base(stream, Endianness.BigEndian) { }
    }
}