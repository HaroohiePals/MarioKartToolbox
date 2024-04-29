using HaroohiePals.IO;
using OpenTK.Mathematics;

namespace HaroohiePals.Nitro
{
    public static class EndianBinaryReaderExtensions
    {
        public static Vector3d ReadVecFx16(this EndianBinaryReader er)
            => new(er.ReadFx16(), er.ReadFx16(), er.ReadFx16());

        public static Vector3d ReadVecFx32(this EndianBinaryReader er)
            => new(er.ReadFx32(), er.ReadFx32(), er.ReadFx32());
    }
}