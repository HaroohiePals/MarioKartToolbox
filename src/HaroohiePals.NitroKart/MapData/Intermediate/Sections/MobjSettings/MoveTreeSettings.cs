namespace HaroohiePals.NitroKart.MapData.Intermediate.Sections.MobjSettings
{
    public class MoveTreeSettings : MkdsMobjSettings
    {
        public MoveTreeSettings() { }

        public MoveTreeSettings(MkdsMobjSettings settings)
            : base(settings) { }

        public short PathSpeed
        {
            get => Settings[0];
            set => Settings[0] = value;
        }
    }
}