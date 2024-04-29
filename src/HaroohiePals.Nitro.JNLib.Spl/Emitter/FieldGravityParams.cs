using HaroohiePals.IO;
using OpenTK.Mathematics;

namespace HaroohiePals.Nitro.JNLib.Spl.Emitter
{
    public class FieldGravityParams
    {
        public FieldGravityParams()
        {
        }

        public FieldGravityParams(EndianBinaryReader reader)
        {
            Gravity = reader.ReadVecFx16();
            reader.Read<short>();
        }

        public void Write(EndianBinaryWriterEx ew)
        {
            ew.WriteVecFx16(Gravity);
            ew.Write((ushort)0);
        }

        public Vector3d Gravity { get; set; }
    }
}