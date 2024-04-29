using System.ComponentModel;

namespace HaroohiePals.NitroKart.MapData.Intermediate.Sections.MobjSettings
{
    public class IronBallSettings : MkdsMobjSettings
    {
        public IronBallSettings() { }

        public IronBallSettings(MkdsMobjSettings settings)
            : base(settings) { }

        public short Speed
        {
            get => Settings[0];
            set => Settings[0] = value;
        }

        [Description(
            "Counted in half frames. 120 is one second. For non-simple collision modes it is assumed that a lap takes 50 seconds to determine the lap for activation.")]
        public ushort InitialActivationDelay
        {
            get => (ushort)Settings[1];
            set => Settings[1] = (short)value;
        }

        [Description(
            "Used in simple collision modes. After activation this is counted in frames, but once a ball despawns counting goes twice as fast. In non-simple collision modes respawning happens in the next lap if this value is at least 1. A value of 0 disables respawning completely.")]
        public ushort MinTimeUntilRespawn
        {
            get => (ushort)Settings[2];
            set => Settings[2] = (short)value;
        }

        [Description("Time between activation and the actual spawning in frames. 60 is one second.")]
        public ushort SpawnDelay
        {
            get => (ushort)Settings[3];
            set => Settings[3] = (short)value;
        }
    }
}