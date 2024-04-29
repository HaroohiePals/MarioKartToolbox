using HaroohiePals.IO;

namespace HaroohiePals.KCollision.Formats
{
    public class Mk7Kcl : FloatKartKcl
    {
        public Mk7Kcl(byte[] data)
            : this(new MemoryStream(data, false)) { }

        public Mk7Kcl(Stream stream)
            : base(stream, Endianness.LittleEndian) { }
    }
}