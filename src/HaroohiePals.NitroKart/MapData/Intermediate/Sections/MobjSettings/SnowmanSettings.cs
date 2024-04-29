using System.ComponentModel;

namespace HaroohiePals.NitroKart.MapData.Intermediate.Sections.MobjSettings
{
    public class SnowmanSettings : MkdsMobjSettings
    {
        public SnowmanSettings() { }

        public SnowmanSettings(MkdsMobjSettings settings)
            : base(settings) { }
        
        [Description(
            "Setting this to true will disable resurrection of the snowman.\n" +
            "This behavior seems to have been meant for missions, but isn't used in the game.")]
        public bool DontResurrect
        {
            get => Settings[0] > 0;
            set => Settings[0] = (short)(value ? 1 : 0);
        }
    }
}
