using System.ComponentModel;

namespace HaroohiePals.NitroKart.MapData.Intermediate.Sections.MobjSettings
{
    class MkdEfBurnerSettings : MkdsMobjSettings
    {
        public MkdEfBurnerSettings() { }

        public MkdEfBurnerSettings(MkdsMobjSettings settings)
            : base(settings) { }

        public short OnTime
        {
            get => Settings[0];
            set => Settings[0] = value;
        }

        public short OffTime
        {
            get => Settings[1];
            set => Settings[1] = value;
        }

        [Description(
            "This setting seems to originally have been meant as a delay before starting the first time.\n" +
            "Instead, the choices are only to wait OffTime frames when the value is greater than 1 or a full burn cycle if less or equal to 1.")]
        public short BrokenInitialDelay
        {
            get => Settings[2];
            set => Settings[2] = value;
        }

        public bool BurnContinuously
        {
            get => Settings[0] == 0 && Settings[1] == 0 && Settings[2] == 0;
            set
            {
                if (value == BurnContinuously)
                    return;
                if (value)
                    Settings[0] = Settings[1] = Settings[2] = 0;
                else
                {
                    Settings[0] = 120;
                    Settings[1] = 130;
                    Settings[2] = 0;
                }
            }
        }

        public short PathSpeed
        {
            get => Settings[3];
            set => Settings[3] = value;
        }
    }
}