using HaroohiePals.IO;
using OpenTK.Mathematics;

namespace HaroohiePals.Nitro
{
    public static class EndianBinaryWriterExtensions
    {
        public static void WriteVecFx16(this EndianBinaryWriter er, in Vector3d value)
        {
            er.WriteFx16(value.X);
            er.WriteFx16(value.Y);
            er.WriteFx16(value.Z);
        }

        public static void WriteVecFx32(this EndianBinaryWriter er, in Vector3d value)
        {
            er.WriteFx32(value.X);
            er.WriteFx32(value.Y);
            er.WriteFx32(value.Z);
        }
    }
}