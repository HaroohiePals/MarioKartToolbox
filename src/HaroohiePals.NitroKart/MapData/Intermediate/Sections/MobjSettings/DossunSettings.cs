namespace HaroohiePals.NitroKart.MapData.Intermediate.Sections.MobjSettings
{
    public class DossunSettings : MkdsMobjSettings
    {
        public DossunSettings() { }

        public DossunSettings(MkdsMobjSettings settings)
            : base(settings) { }

        public bool MoveHorizontal
        {
            get => Settings[0] != 0;
            set => Settings[0] = (short) (value ? 1 : 0);
        }

        public short InitialPathPoint
        {
            get => Settings[1];
            set => Settings[1] = value;
        }
    }
}