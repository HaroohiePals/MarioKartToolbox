using System.ComponentModel;

namespace HaroohiePals.NitroKart.MapData.Intermediate.Sections.MobjSettings
{
    public class SanboSettings : MkdsMobjSettings
    {
        public SanboSettings() { }

        public SanboSettings(MkdsMobjSettings settings)
            : base(settings) { }

        public short PathSpeed
        {
            get => Settings[0];
            set => Settings[0] = value;
        }

        [Description(
            "Setting this to true will disable resurrection of the Sanbo after a delay.\n" +
            "This behavior is meant to be used for missions.")]
        public bool DontResurrect
        {
            get => Settings[1] != 0;
            set => Settings[1] = (short) (value ? 1 : 0);
        }
    }
}
