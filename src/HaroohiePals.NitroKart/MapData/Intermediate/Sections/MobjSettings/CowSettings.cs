namespace HaroohiePals.NitroKart.MapData.Intermediate.Sections.MobjSettings
{
    public class CowSettings : MkdsMobjSettings
    {
        public CowSettings() { }

        public CowSettings(MkdsMobjSettings settings)
            : base(settings) { }

        public short NsbtpFrame
        {
            get => Settings[0];
            set => Settings[0] = value;
        }
    }
}
