namespace HaroohiePals.NitroKart.MapData.Intermediate.Sections.MobjSettings
{
    public class FlipperSettings : MkdsMobjSettings
    {
        public FlipperSettings() { }

        public FlipperSettings(MkdsMobjSettings settings)
            : base(settings) { }

        public bool IsMirrored
        {
            get => Settings[0] != 0;
            set => Settings[0] = (short) (value ? 1 : 0);
        }
    }
}