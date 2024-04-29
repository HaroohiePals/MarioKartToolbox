namespace HaroohiePals.NitroKart.MapData.Intermediate.Sections.MobjSettings
{
    public class GearSettings : RotatingCylinderSettings
    {
        public GearSettings() { }

        public GearSettings(MkdsMobjSettings settings)
            : base(settings) { }

        public bool IsBlack
        {
            get => Settings[5] != 0;
            set => Settings[5] = (short)(value ? 1 : 0);
        }
    }
}