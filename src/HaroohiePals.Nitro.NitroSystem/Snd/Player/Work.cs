namespace HaroohiePals.Nitro.NitroSystem.Snd.Player
{
    public class Work
    {
        public ExChannel[] Channels { get; } = new ExChannel[16];
        public Player[]    Players  { get; } = new Player[16];
        public Track[]     Tracks   { get; } = new Track[32];

        private DSSoundContext _context;

        public Work(DSSoundContext context)
        {
            _context = context;
            for (int i = 0; i < 16; i++)
            {
                Channels[i] = new ExChannel(context);
                Players[i]  = new Player(context);
            }

            for (int i = 0; i < 32; i++)
            {
                Tracks[i] = new Track(context);
            }
        }
    }
}