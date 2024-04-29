using HaroohiePals.Graphics;
using HaroohiePals.IO;
using OpenTK.Mathematics;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace HaroohiePals.Nitro.JNLib.Spl.Emitter
{
    [XmlRoot("Emitter")]
    public partial class SplEmitter
    {
        private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(SplEmitter),
            new OverrideXml().Override<Vector3d>()
                .Member("X").XmlAttribute("x")
                .Member("Y").XmlAttribute("y")
                .Member("Z").XmlAttribute("z")
                .Commit());

        public enum EmitterShape
        {
            Point = 0,
            Sphere,
            Circle,
            CircleEven,
            SphereVolume,
            CircleVolume,
            Cylinder,
            CylinderEven,
            Hemisphere,
            HemisphereVolume
        }

        public enum ParticleType
        {
            Billboard = 0,
            DirBillboard,
            Polygon,
            DirPolygon
        }

        public enum AxisDirection
        {
            Z = 0,
            Y,
            X,
            Custom
        }

        public enum QuadDirection
        {
            XY = 0,
            XZ
        }

        public enum ParticleScaleMode
        {
            Uniform = 0,
            X,
            Y
        }

        public enum RotMtxMode
        {
            Y = 0,
            XYZ
        }

        public SplEmitter()
        {
        }

        public SplEmitter(EndianBinaryReader reader)
        {
            Flags = new EmitterFlags(reader);
            Position = reader.ReadVecFx32();
            EmissionVolume = reader.ReadFx32();
            EmitterRadius = reader.ReadFx32();
            EmitterLength = reader.ReadFx32();
            EmitterAxis = reader.ReadVecFx16();
            Color = reader.Read<ushort>();
            ParticlePosVeloMag = reader.ReadFx32();
            ParticleAxisVeloMag = reader.ReadFx32();
            ParticleBaseScale = reader.ReadFx32();
            AspectRatio = reader.ReadFx16();
            EmissionStartTime = reader.Read<ushort>();
            MinRotVelocity = reader.Read<short>() * 360f / 65536f;
            MaxRotVelocity = reader.Read<short>() * 360f / 65536f;
            ParticleRotation = reader.Read<short>() * 360f / 65536f;
            reader.Read<ushort>();
            EmissionTime = reader.Read<ushort>();
            ParticleLifetime = reader.Read<ushort>();
            ParticleScaleRandomness = reader.Read<byte>() / 256f;
            ParticleLifetimeRandomness = reader.Read<byte>() / 256f;
            ParticleVeloMagRandomness = reader.Read<byte>() / 256f;
            reader.Read<byte>();
            EmissionInterval = reader.Read<byte>();
            ParticleAlpha = reader.Read<byte>();
            AirResistance = (reader.Read<byte>() + 384) / 512f;
            TextureId = reader.Read<byte>();
            LoopFrame = reader.Read<byte>();
            DirBillboardScale = reader.Read<ushort>();
            byte tmp = reader.Read<byte>();
            TexRepeatShiftS = (byte)(tmp & 3);
            TexRepeatShiftT = (byte)(tmp >> 2 & 3);
            ScaleMode = (ParticleScaleMode)(tmp >> 4 & 7);
            CenterDirPolygon = tmp >> 7 == 1;

            uint tmp2 = reader.Read<uint>();
            TexFlipS = (tmp2 & 1) == 1;
            TexFlipT = (tmp2 >> 1 & 1) == 1;

            QuadXOffset = reader.ReadFx16();
            QuadYZOffset = reader.ReadFx16();

            UserData = reader.Read<uint>();

            if (Flags.HasScaleAnim)
                ScaleAnim = new ScaleAnimParams(reader);
            if (Flags.HasColorAnim)
                ColorAnim = new ColorAnimParams(reader);
            if (Flags.HasAlphaAnim)
                AlphaAnim = new AlphaAnimParams(reader);
            if (Flags.HasTexAnim)
                TexAnim = new TexAnimParams(reader);

            if (Flags.HasChildParticles)
                Child = new ChildParams(reader);

            if (Flags.HasFieldGravity)
                FieldGravity = new FieldGravityParams(reader);
            if (Flags.HasFieldRandom)
                FieldRandom = new FieldRandomParams(reader);
            if (Flags.HasFieldMagnetic)
                FieldMagnet = new FieldMagnetParams(reader);
            if (Flags.HasFieldSpin)
                FieldSpin = new FieldSpinParams(reader);
            if (Flags.HasFieldCollision)
                FieldCollision = new FieldCollisionParams(reader);
            if (Flags.HasFieldConvergence)
                FieldConvergence = new FieldConvergenceParams(reader);
        }

        public void Write(EndianBinaryWriterEx ew)
        {
            Flags.Write(ew);
            ew.WriteVecFx32(Position);
            ew.WriteFx32(EmissionVolume);
            ew.WriteFx32(EmitterRadius);
            ew.WriteFx32(EmitterLength);
            ew.WriteVecFx16(EmitterAxis);
            ew.Write(Color);
            ew.WriteFx32(ParticlePosVeloMag);
            ew.WriteFx32(ParticleAxisVeloMag);
            ew.WriteFx32(ParticleBaseScale);
            ew.WriteFx16(AspectRatio);
            ew.Write(EmissionStartTime);
            ew.Write((short)System.Math.Round(MinRotVelocity * 65536f / 360f));
            ew.Write((short)System.Math.Round(MaxRotVelocity * 65536f / 360f));
            ew.Write((short)System.Math.Round(ParticleRotation * 65536f / 360f));
            ew.Write((ushort)0);
            ew.Write(EmissionTime);
            ew.Write(ParticleLifetime);
            ew.Write(FloatHelper.ToByte(ParticleScaleRandomness, nameof(ParticleScaleRandomness)));
            ew.Write(FloatHelper.ToByte(ParticleLifetimeRandomness, nameof(ParticleLifetimeRandomness)));
            ew.Write(FloatHelper.ToByte(ParticleVeloMagRandomness, nameof(ParticleVeloMagRandomness)));
            ew.Write((byte)0);
            ew.Write(EmissionInterval);
            ew.Write(ParticleAlpha);
            int airResInt = (int)System.Math.Round(AirResistance * 512) - 384;
            if (airResInt < 0 || airResInt > 255)
                throw new Exception("AirResistance out of range (0.75 <= x < 1.25)");
            ew.Write((byte)airResInt);
            ew.Write(TextureId);
            ew.Write(LoopFrame);
            ew.Write(DirBillboardScale);
            ew.Write((byte)(TexRepeatShiftS & 3u |
                             (TexRepeatShiftT & 3u) << 2 |
                             ((uint)ScaleMode & 7u) << 4 |
                             (CenterDirPolygon ? 1u : 0) << 7));
            ew.Write((TexFlipS ? 1u : 0) |
                             (TexFlipT ? 1u : 0) << 1);
            ew.WriteFx16(QuadXOffset);
            ew.WriteFx16(QuadYZOffset);
            ew.Write(UserData);

            if (Flags.HasScaleAnim)
                ScaleAnim.Write(ew);
            if (Flags.HasColorAnim)
                ColorAnim.Write(ew);
            if (Flags.HasAlphaAnim)
                AlphaAnim.Write(ew);
            if (Flags.HasTexAnim)
                TexAnim.Write(ew);

            if (Flags.HasChildParticles)
                Child.Write(ew);

            if (Flags.HasFieldGravity)
                FieldGravity.Write(ew);
            if (Flags.HasFieldRandom)
                FieldRandom.Write(ew);
            if (Flags.HasFieldMagnetic)
                FieldMagnet.Write(ew);
            if (Flags.HasFieldSpin)
                FieldSpin.Write(ew);
            if (Flags.HasFieldCollision)
                FieldCollision.Write(ew);
            if (Flags.HasFieldConvergence)
                FieldConvergence.Write(ew);
        }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlElement(IsNullable = false)]
        public EmitterFlags Flags { get; set; }

        public Vector3d Position { get; set; }

        /// <summary>
        /// Fractional number of particles to emit per emission
        /// </summary>
        public double EmissionVolume { get; set; }

        [DefaultValue(0f)]
        public double EmitterRadius { get; set; }

        [DefaultValue(0f)]
        public double EmitterLength { get; set; }

        public Vector3d EmitterAxis { get; set; }
        public Rgb555 Color { get; set; }

        [DefaultValue(0f)]
        public double ParticlePosVeloMag { get; set; }

        [DefaultValue(0f)]
        public double ParticleAxisVeloMag { get; set; }

        [DefaultValue(1f)]
        public double ParticleBaseScale { get; set; } = 1;

        [DefaultValue(1f)]
        public double AspectRatio { get; set; } = 1;

        /// <summary>
        /// Time in frames to wait before the first emission
        /// </summary>
        [DefaultValue((object)(ushort)0)]
        public ushort EmissionStartTime { get; set; }

        [DefaultValue(0f)]
        public double MinRotVelocity { get; set; }

        [DefaultValue(0f)]
        public double MaxRotVelocity { get; set; }

        [DefaultValue(0f)]
        public double ParticleRotation { get; set; }

        /// <summary>
        /// Lifetime of the emitter in frames, 0 = infinite
        /// </summary>
        [DefaultValue((object)(ushort)0)]
        public ushort EmissionTime { get; set; }

        /// <summary>
        /// Lifetime of the particles emitted in frames
        /// </summary>
        public ushort ParticleLifetime { get; set; }

        [DefaultValue(0f)]
        public double ParticleScaleRandomness { get; set; }

        [DefaultValue(0f)]
        public double ParticleLifetimeRandomness { get; set; }

        [DefaultValue(0f)]
        public double ParticleVeloMagRandomness { get; set; }

        /// <summary>
        /// Time between emissions (1 = each frame, 2 = every other frame, etc.)
        /// </summary>
        public byte EmissionInterval { get; set; }

        public byte ParticleAlpha { get; set; }

        [DefaultValue(1f)]
        public float AirResistance { get; set; } = 1;

        [XmlIgnore]
        public byte TextureId { get; set; }

        public string TextureName { get; set; }

        /// <summary>
        /// Length of an animation loop iteration in frames
        /// </summary>
        public byte LoopFrame { get; set; }

        [DefaultValue((object)(ushort)0)]
        public ushort DirBillboardScale { get; set; }

        /// <summary>
        /// Horizontal texture repeat shift (0 = 1, 1 = 2, 2 = 4, 3 = 8)
        /// </summary>
        [DefaultValue((byte)0)]
        public byte TexRepeatShiftS { get; set; }

        /// <summary>
        /// Vertical texture repeat shift (0 = 1, 1 = 2, 2 = 4, 3 = 8)
        /// </summary>
        [DefaultValue((byte)0)]
        public byte TexRepeatShiftT { get; set; }

        /// <summary>
        /// Determines if scale animation is applied only to X, only to Y or to both 
        /// </summary>
        [DefaultValue(ParticleScaleMode.Uniform)]
        public ParticleScaleMode ScaleMode { get; set; } = ParticleScaleMode.Uniform;

        [DefaultValue(false)]
        public bool CenterDirPolygon { get; set; }

        /// <summary>
        /// Completely mirrors the texture horizontally when true
        /// </summary>
        [DefaultValue(false)]
        public bool TexFlipS { get; set; }

        /// <summary>
        /// Completely mirrors the texture vertically when true
        /// </summary>
        [DefaultValue(false)]
        public bool TexFlipT { get; set; }

        [DefaultValue(0f)]
        public double QuadXOffset { get; set; }

        [DefaultValue(0f)]
        public double QuadYZOffset { get; set; }

        [XmlAttribute]
        [DefaultValue((object)0u)]
        public uint UserData { get; set; }

        public ScaleAnimParams ScaleAnim { get; set; }
        public ColorAnimParams ColorAnim { get; set; }
        public AlphaAnimParams AlphaAnim { get; set; }
        public TexAnimParams TexAnim { get; set; }

        public ChildParams Child { get; set; }

        public FieldGravityParams FieldGravity { get; set; }
        public FieldRandomParams FieldRandom { get; set; }
        public FieldMagnetParams FieldMagnet { get; set; }
        public FieldSpinParams FieldSpin { get; set; }
        public FieldCollisionParams FieldCollision { get; set; }
        public FieldConvergenceParams FieldConvergence { get; set; }

        public byte[] ToXml()
        {
            var scaleAnim = ScaleAnim;
            var colorAnim = ColorAnim;
            var alphaAnim = AlphaAnim;
            var texAnim = TexAnim;
            var child = Child;
            var fieldGravity = FieldGravity;
            var fieldRandom = FieldRandom;
            var fieldMagnet = FieldMagnet;
            var fieldSpin = FieldSpin;
            var fieldCollision = FieldCollision;
            var fieldConvergence = FieldConvergence;

            if (!Flags.HasScaleAnim)
                ScaleAnim = null;
            if (!Flags.HasColorAnim)
                ColorAnim = null;
            if (!Flags.HasAlphaAnim)
                AlphaAnim = null;
            if (!Flags.HasTexAnim)
                TexAnim = null;

            if (!Flags.HasChildParticles)
                Child = null;

            if (!Flags.HasFieldGravity)
                FieldGravity = null;
            if (!Flags.HasFieldRandom)
                FieldRandom = null;
            if (!Flags.HasFieldMagnetic)
                FieldMagnet = null;
            if (!Flags.HasFieldSpin)
                FieldSpin = null;
            if (!Flags.HasFieldCollision)
                FieldCollision = null;
            if (!Flags.HasFieldConvergence)
                FieldConvergence = null;

            var m = new MemoryStream();
            var xns = new XmlSerializerNamespaces();
            xns.Add(string.Empty, string.Empty);
            using (var writer = XmlWriter.Create(m, new XmlWriterSettings { Indent = true }))
            {
                Serializer.Serialize(writer, this, xns);
                writer.Close();
            }

            ScaleAnim = scaleAnim;
            ColorAnim = colorAnim;
            AlphaAnim = alphaAnim;
            TexAnim = texAnim;
            Child = child;
            FieldGravity = fieldGravity;
            FieldRandom = fieldRandom;
            FieldMagnet = fieldMagnet;
            FieldSpin = fieldSpin;
            FieldCollision = fieldCollision;
            FieldConvergence = fieldConvergence;

            return m.ToArray();
        }

        public static SplEmitter FromXml(byte[] data)
        {
            SplEmitter result;
            using (var reader = new StreamReader(new MemoryStream(data)))
                result = (SplEmitter)Serializer.Deserialize(reader);

            result.Flags.HasScaleAnim = result.ScaleAnim != null;
            result.Flags.HasColorAnim = result.ColorAnim != null;
            result.Flags.HasAlphaAnim = result.AlphaAnim != null;
            result.Flags.HasTexAnim = result.TexAnim != null;

            result.Flags.HasChildParticles = result.Child != null;

            result.Flags.HasFieldGravity = result.FieldGravity != null;
            result.Flags.HasFieldRandom = result.FieldRandom != null;
            result.Flags.HasFieldMagnetic = result.FieldMagnet != null;
            result.Flags.HasFieldSpin = result.FieldSpin != null;
            result.Flags.HasFieldCollision = result.FieldCollision != null;
            result.Flags.HasFieldConvergence = result.FieldConvergence != null;

            return result;
        }
    }
}