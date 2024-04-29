using HaroohiePals.Graphics;
using HaroohiePals.IO;
using HaroohiePals.IO.Serialization;

namespace HaroohiePals.NitroKart.MapData.Binary
{
    public class NkmdStag
    {
        public const uint STAGSignature = 0x47415453;

        public NkmdStag()
        {
            NrLaps    = 3;
            FogColor  = Rgb555.White;
            KclLightColors = new[] { Rgb555.White, Rgb555.White, Rgb555.White, Rgb555.White };
        }

        public NkmdStag(EndianBinaryReader er)
        {
            uint signature = er.Read<uint>();
            if (signature != STAGSignature)
                throw new SignatureNotCorrectException(signature, STAGSignature, er.BaseStream.Position - 4);

            CourseId        = er.Read<ushort>();
            NrLaps          = er.Read<short>();
            PolePosition    = er.Read<byte>();
            FogEnabled      = er.Read<byte>() == 1;
            FogTableGenMode = er.Read<byte>();
            FogShift        = er.Read<byte>();

            er.Read<byte>(8);

            FogOffset = er.Read<int>();

            uint fogColor = er.Read<uint>();
            FogColor = (Rgb555)(fogColor & 0x7FFF);
            FogAlpha = fogColor >> 15 & 0x1F;

            KclLightColors = new Rgb555[4]
            {
                 er.Read<ushort>(), er.Read<ushort>(), er.Read<ushort>(), er.Read<ushort>()
            };

            MobjFarClip = er.ReadFx32();
            FrustumFar  = er.ReadFx32();
        }

        public void Write(EndianBinaryWriter er)
        {
            er.Write(STAGSignature);
            er.Write(CourseId);
            er.Write(NrLaps);
            er.Write(PolePosition);
            er.Write((byte)(FogEnabled ? 1 : 0));
            er.Write(FogTableGenMode);
            er.Write(FogShift);
            er.Write(new byte[8], 0, 8);
            er.Write(FogOffset);
            er.Write(FogColor | (FogAlpha & 0x1F) << 15);
            er.Write<ushort>(KclLightColors[0]);
            er.Write<ushort>(KclLightColors[1]);
            er.Write<ushort>(KclLightColors[2]);
            er.Write<ushort>(KclLightColors[3]);
            er.WriteFx32(MobjFarClip);
            er.WriteFx32(FrustumFar);
        }

        public ushort CourseId;
        public short  NrLaps;
        public byte   PolePosition;
        public bool   FogEnabled;
        public byte   FogTableGenMode;
        public byte   FogShift;

        public int FogOffset;

        [Type(FieldType.U16)]
        public Rgb555 FogColor;

        public uint FogAlpha;

        [Type(FieldType.U16), ArraySize(4)]
        public Rgb555[] KclLightColors;

        [Fx32]
        public double MobjFarClip;

        [Fx32]
        public double FrustumFar;
    }
}