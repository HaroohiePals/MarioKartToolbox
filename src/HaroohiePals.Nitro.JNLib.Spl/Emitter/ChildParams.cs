using HaroohiePals.Graphics;
using HaroohiePals.IO;
using System.ComponentModel;
using System.Xml.Serialization;

namespace HaroohiePals.Nitro.JNLib.Spl.Emitter
{
    public class ChildParams
    {
        public enum RotInherit
        {
            None = 0,
            Freeze,
            Continue
        }

        public ChildParams()
        {
        }

        public ChildParams(EndianBinaryReader reader)
        {
            ushort flags = reader.Read<ushort>();
            ApplyEmitterField = (flags & 1) == 1;
            UseScaleAnim = (flags >> 1 & 1) == 1;
            HasAlphaFade = (flags >> 2 & 1) == 1;
            RotInheritMode = (RotInherit)(flags >> 3 & 3);
            FollowEmitter = (flags >> 5 & 1) == 1;
            UseChildColor = (flags >> 6 & 1) == 1;
            ParticleType = (SplEmitter.ParticleType)(flags >> 7 & 3);
            RotMtxMode = (SplEmitter.RotMtxMode)(flags >> 9 & 3);
            QuadDirection = (SplEmitter.QuadDirection)(flags >> 11 & 1);

            InitVelocityRandomness = reader.Read<short>() / 256f;
            TargetScale = reader.ReadFx16();
            LifeTime = reader.Read<ushort>();
            VelocityInheritRatio = reader.Read<byte>() / 256f;
            Scale = (reader.Read<byte>() + 1) / 64f;
            Color = reader.Read<ushort>();
            EmissionVolume = reader.Read<byte>();
            EmissionTime = reader.Read<byte>() / 256f;
            EmissionInterval = reader.Read<byte>();
            TextureId = reader.Read<byte>();

            uint tmp = reader.Read<uint>();
            TexRepeatShiftS = (byte)(tmp & 3);
            TexRepeatShiftT = (byte)(tmp >> 2 & 3);
            TexFlipS = (tmp >> 4 & 1) == 1;
            TexFlipT = (tmp >> 5 & 1) == 1;
            CenterDirPolygon = (tmp >> 6 & 1) == 1;
        }

        public void Write(EndianBinaryWriterEx ew)
        {
            ew.Write((ushort)(
                (ApplyEmitterField ? 1u : 0) |
                (UseScaleAnim ? 1u : 0) << 1 |
                (HasAlphaFade ? 1u : 0) << 2 |
                ((uint)RotInheritMode & 3u) << 3 |
                (FollowEmitter ? 1u : 0) << 5 |
                (UseChildColor ? 1u : 0) << 6 |
                ((uint)ParticleType & 3u) << 7 |
                ((uint)RotMtxMode & 3u) << 9 |
                ((uint)QuadDirection & 1u) << 11));

            int initVeloRand = (int)System.Math.Round(InitVelocityRandomness * 256);
            if (initVeloRand < short.MinValue || initVeloRand > short.MaxValue)
                throw new Exception($"InitVelocityRandomness out of range");

            ew.Write((short)initVeloRand);
            ew.WriteFx16(TargetScale);
            ew.Write(LifeTime);
            ew.Write(FloatHelper.ToByte(VelocityInheritRatio, nameof(VelocityInheritRatio)));
            int scale = (int)System.Math.Round(Scale * 64f - 1f);
            if (scale < 0 || scale >= 256)
                throw new Exception("Child scale out of range (0 < x <= 4)");
            ew.Write((byte)scale);
            ew.Write(Color);
            ew.Write(EmissionVolume);
            ew.Write(FloatHelper.ToByte(EmissionTime, nameof(EmissionTime)));
            ew.Write(EmissionInterval);
            ew.Write(TextureId);

            ew.Write(
                TexRepeatShiftS & 3u |
                (TexRepeatShiftT & 3u) << 2 |
                (TexFlipS ? 1u : 0) << 4 |
                (TexFlipT ? 1u : 0) << 5 |
                (CenterDirPolygon ? 1u : 0) << 6);
        }

        [DefaultValue(false)]
        public bool ApplyEmitterField { get; set; }

        [DefaultValue(false)]
        public bool UseScaleAnim { get; set; }

        [DefaultValue(false)]
        public bool HasAlphaFade { get; set; }

        public RotInherit RotInheritMode { get; set; }

        [DefaultValue(false)]
        public bool FollowEmitter { get; set; }

        public bool UseChildColor { get; set; }

        [XmlAttribute]
        public SplEmitter.ParticleType ParticleType { get; set; }

        public SplEmitter.RotMtxMode RotMtxMode { get; set; }
        public SplEmitter.QuadDirection QuadDirection { get; set; }

        [DefaultValue(0f)]
        public float InitVelocityRandomness { get; set; }

        [DefaultValue(0f)]
        public double TargetScale { get; set; }

        [XmlAttribute]
        public ushort LifeTime { get; set; }

        public float VelocityInheritRatio { get; set; }
        public float Scale { get; set; }
        public Rgb555 Color { get; set; }

        public byte EmissionVolume { get; set; }
        public float EmissionTime { get; set; }
        public byte EmissionInterval { get; set; }

        [XmlIgnore]
        public byte TextureId { get; set; }

        public string TextureName { get; set; }

        [DefaultValue((byte)0)]
        public byte TexRepeatShiftS { get; set; }

        [DefaultValue((byte)0)]
        public byte TexRepeatShiftT { get; set; }

        [DefaultValue(false)]
        public bool TexFlipS { get; set; }

        [DefaultValue(false)]
        public bool TexFlipT { get; set; }

        [DefaultValue(false)]
        public bool CenterDirPolygon { get; set; }
    }
}