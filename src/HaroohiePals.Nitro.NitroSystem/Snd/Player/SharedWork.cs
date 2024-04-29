namespace HaroohiePals.Nitro.NitroSystem.Snd.Player
{
    public class SharedWork
    {
        public uint FinishCommandTag;
        public uint PlayerStatus;

        public ushort ChannelStatus;

        //public ushort CaptureStatus;
        public short[][] LocalVariables  { get; private set; }
        public uint[]    TickCounters    { get; private set; }
        public short[]   GlobalVariables { get; private set; }

        private DSSoundContext _context;

        public SharedWork(DSSoundContext context)
        {
            _context        = context;
            LocalVariables  = new short[16][];
            TickCounters    = new uint[16];
            GlobalVariables = new short[16];
            for (int i = 0; i < 16; i++)
                LocalVariables[i] = new short[16];
        }
    }
}