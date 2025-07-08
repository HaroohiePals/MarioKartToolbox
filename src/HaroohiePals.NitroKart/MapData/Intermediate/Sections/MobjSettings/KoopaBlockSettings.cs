namespace HaroohiePals.NitroKart.MapData.Intermediate.Sections.MobjSettings
{
    public class KoopaBlockSettings : MkdsMobjSettings
    {
        public KoopaBlockSettings() { }

        public KoopaBlockSettings(MkdsMobjSettings settings)
            : base(settings) { }

        public short PathSpeed
        {
            get => Settings[0];
            set => Settings[0] = value;
        }
    }
}