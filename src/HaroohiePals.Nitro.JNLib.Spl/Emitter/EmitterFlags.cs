using HaroohiePals.IO;
using System.ComponentModel;
using System.Xml.Serialization;

namespace HaroohiePals.Nitro.JNLib.Spl.Emitter
{
    public class EmitterFlags
    {
        public EmitterFlags()
        {
        }

        public EmitterFlags(EndianBinaryReader reader)
        {
            uint flags = reader.Read<uint>();
            EmitterShape = (SplEmitter.EmitterShape)(flags & 0xF);
            ParticleType = (SplEmitter.ParticleType)(flags >> 4 & 3);
            AxisDirType = (SplEmitter.AxisDirection)(flags >> 6 & 3);
            HasScaleAnim = (flags >> 8 & 1) == 1;
            HasColorAnim = (flags >> 9 & 1) == 1;
            HasAlphaAnim = (flags >> 10 & 1) == 1;
            HasTexAnim = (flags >> 11 & 1) == 1;
            HasRandomParticleDeltaRotation = (flags >> 12 & 1) == 1;
            HasRandomParticleRotation = (flags >> 13 & 1) == 1;
            EmitterIsOneTime = (flags >> 14 & 1) == 1;
            ParticlesFollowEmitter = (flags >> 15 & 1) == 1;
            HasChildParticles = (flags >> 16 & 1) == 1;
            RotMtxMode = (SplEmitter.RotMtxMode)(flags >> 17 & 3);
            QuadDirection = (SplEmitter.QuadDirection)(flags >> 19 & 1);
            RandomizeParticleProgressOffset = (flags >> 20 & 1) == 1;
            RenderChildParticlesFirst = (flags >> 21 & 1) == 1;
            DontRenderMainParticles = (flags >> 22 & 1) == 1;
            RelativePosAsRotOrigin = (flags >> 23 & 1) == 1;
            HasFieldGravity = (flags >> 24 & 1) == 1;
            HasFieldRandom = (flags >> 25 & 1) == 1;
            HasFieldMagnetic = (flags >> 26 & 1) == 1;
            HasFieldSpin = (flags >> 27 & 1) == 1;
            HasFieldCollision = (flags >> 28 & 1) == 1;
            HasFieldConvergence = (flags >> 29 & 1) == 1;
            UseConstPolygonIdForMainParticles = (flags >> 30 & 1) == 1;
            UseConstPolygonIdForChildParticles = (flags >> 31 & 1) == 1;
        }

        public void Write(EndianBinaryWriterEx ew)
        {
            uint flags = 0;
            flags |= (uint)EmitterShape & 0xF;
            flags |= ((uint)ParticleType & 0x3) << 4;
            flags |= ((uint)AxisDirType & 0x3) << 6;
            flags |= (HasScaleAnim ? 1u : 0) << 8;
            flags |= (HasColorAnim ? 1u : 0) << 9;
            flags |= (HasAlphaAnim ? 1u : 0) << 10;
            flags |= (HasTexAnim ? 1u : 0) << 11;
            flags |= (HasRandomParticleDeltaRotation ? 1u : 0) << 12;
            flags |= (HasRandomParticleRotation ? 1u : 0) << 13;
            flags |= (EmitterIsOneTime ? 1u : 0) << 14;
            flags |= (ParticlesFollowEmitter ? 1u : 0) << 15;
            flags |= (HasChildParticles ? 1u : 0) << 16;
            flags |= ((uint)RotMtxMode & 3) << 17;
            flags |= ((uint)QuadDirection & 1) << 19;
            flags |= (RandomizeParticleProgressOffset ? 1u : 0) << 20;
            flags |= (RenderChildParticlesFirst ? 1u : 0) << 21;
            flags |= (DontRenderMainParticles ? 1u : 0) << 22;
            flags |= (RelativePosAsRotOrigin ? 1u : 0) << 23;
            flags |= (HasFieldGravity ? 1u : 0) << 24;
            flags |= (HasFieldRandom ? 1u : 0) << 25;
            flags |= (HasFieldMagnetic ? 1u : 0) << 26;
            flags |= (HasFieldSpin ? 1u : 0) << 27;
            flags |= (HasFieldCollision ? 1u : 0) << 28;
            flags |= (HasFieldConvergence ? 1u : 0) << 29;
            flags |= (UseConstPolygonIdForMainParticles ? 1u : 0) << 30;
            flags |= (UseConstPolygonIdForChildParticles ? 1u : 0) << 31;
            ew.Write(flags);
        }

        public SplEmitter.EmitterShape EmitterShape { get; set; }
        public SplEmitter.ParticleType ParticleType { get; set; }
        public SplEmitter.AxisDirection AxisDirType { get; set; }

        [XmlIgnore]
        public bool HasScaleAnim { get; set; }

        [XmlIgnore]
        public bool HasColorAnim { get; set; }

        [XmlIgnore]
        public bool HasAlphaAnim { get; set; }

        [XmlIgnore]
        public bool HasTexAnim { get; set; }

        [DefaultValue(false)]
        public bool HasRandomParticleDeltaRotation { get; set; }

        [DefaultValue(false)]
        public bool HasRandomParticleRotation { get; set; }

        [DefaultValue(false)]
        public bool EmitterIsOneTime { get; set; }

        public bool ParticlesFollowEmitter { get; set; }

        [XmlIgnore]
        public bool HasChildParticles { get; set; }

        public SplEmitter.RotMtxMode RotMtxMode { get; set; }
        public SplEmitter.QuadDirection QuadDirection { get; set; }

        [DefaultValue(false)]
        public bool RandomizeParticleProgressOffset { get; set; }

        public bool RenderChildParticlesFirst { get; set; }

        [DefaultValue(false)]
        public bool DontRenderMainParticles { get; set; }

        [DefaultValue(false)]
        public bool RelativePosAsRotOrigin { get; set; }

        [XmlIgnore]
        public bool HasFieldGravity { get; set; }

        [XmlIgnore]
        public bool HasFieldRandom { get; set; }

        [XmlIgnore]
        public bool HasFieldMagnetic { get; set; }

        [XmlIgnore]
        public bool HasFieldSpin { get; set; }

        [XmlIgnore]
        public bool HasFieldCollision { get; set; }

        [XmlIgnore]
        public bool HasFieldConvergence { get; set; }

        [DefaultValue(false)]
        public bool UseConstPolygonIdForMainParticles { get; set; }

        [DefaultValue(false)]
        public bool UseConstPolygonIdForChildParticles { get; set; }
    }
}