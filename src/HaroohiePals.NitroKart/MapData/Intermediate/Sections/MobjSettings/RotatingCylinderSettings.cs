namespace HaroohiePals.NitroKart.MapData.Intermediate.Sections.MobjSettings
{
    public class RotatingCylinderSettings : MkdsMobjSettings
    {
        public RotatingCylinderSettings() { }

        public RotatingCylinderSettings(MkdsMobjSettings settings)
            : base(settings) { }

        public short RotYVelocity
        {
            get => Settings[0];
            set => Settings[0] = value;
        }

        public ushort RotateDuration
        {
            get => (ushort)Settings[1];
            set => Settings[1] = (short)value;
        }

        public ushort StartStopDuration
        {
            get => (ushort)Settings[2];
            set => Settings[2] = (short)value;
        }

        public ushort IdleDuration
        {
            get => (ushort)Settings[3];
            set => Settings[3] = (short)value;
        }

        public bool NegateRotYVelocity
        {
            get => Settings[4] != 0;
            set => Settings[4] = (short)(value ? 1 : 0);
        }
    }
}