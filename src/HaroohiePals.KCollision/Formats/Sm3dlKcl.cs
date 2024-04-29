using HaroohiePals.IO;

namespace HaroohiePals.KCollision.Formats
{
    public class Sm3dlKcl : FloatPlatformerKcl
    {
        public Sm3dlKcl(byte[] data)
            : this(new MemoryStream(data, false)) { }

        public Sm3dlKcl(Stream stream)
            : base(stream, Endianness.LittleEndian) { }
    }
}