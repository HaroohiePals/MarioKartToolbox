namespace HaroohiePals.Nitro.NitroSystem.Snd.Player
{
    public class Player
    {
        public bool Active;
        public bool Prepared;
        public bool Paused;

        public byte MyNo;

        public byte Priority;

        private byte _volume;

        public byte Volume
        {
            get => _volume;
            set => _volume = value > 127 ? (byte)127 : value;
        }

        public short ExtendedFader;

        public readonly byte[] Tracks = new byte[16];

        public ushort Tempo;
        public ushort TempoRatio;
        public ushort TempoCounter;

        public Sbnk Bank;

        private DSSoundContext _context;

        public Player(DSSoundContext context)
        {
            _context = context;
        }

        public void Init(Sbnk bank)
        {
            Paused        = false;
            Bank          = bank;
            Tempo         = 120;
            TempoRatio    = 256;
            TempoCounter  = 240;
            Volume        = 127;
            ExtendedFader = 0;
            Priority      = 64;
            for (int i = 0; i < 16; i++)
                Tracks[i] = 0xFF;
            _context.SharedWork.TickCounters[MyNo] = 0;
            for (int i = 0; i < 16; i++)
                _context.SharedWork.LocalVariables[MyNo][i] = -1;
        }

        public Track GetTrack(int trackIdx)
        {
            if (trackIdx > 15 || Tracks[trackIdx] == 0xFF)
                return null;
            return _context.Work.Tracks[Tracks[trackIdx]];
        }

        public void CloseTrack(int track)
        {
            var t = GetTrack(track);
            if (t == null)
                return;
            t.Close(this);
            t.Active      = false;
            Tracks[track] = 255;
        }

        public void Finish()
        {
            for (int i = 0; i < 16; i++)
                CloseTrack(i);
            Active = false;
        }

        public void UpdateChannel()
        {
            for (int i = 0; i < 16; i++)
                GetTrack(i)?.UpdateChannel(this, 1);
        }

        public void TempoMain()
        {
            uint v2 = 0;
            while (TempoCounter >= 240)
            {
                TempoCounter -= 240;
                v2++;
            }

            for (int i = 0; i < v2; i++)
            {
                if (SeqMain(true))
                {
                    Finish();
                    break;
                }
            }

            _context.SharedWork.TickCounters[MyNo] += v2;

            TempoCounter += (ushort)(Tempo * TempoRatio / 256);
        }

        public bool SeqMain(bool play)
        {
            bool result = false;
            for (int i = 0; i < 16; i++)
            {
                var t = GetTrack(i);
                if (t != null && t.Data != null)
                {
                    if (t.SeqMain(this, i, play) != 0)
                        CloseTrack(i);
                    else
                        result = true;
                }
            }

            return !result;
        }
        
        public ref short GetVariablePtr(int var)
        {
            if (var < 16)
                return ref _context.SharedWork.LocalVariables[MyNo][var];
            else
                return ref _context.SharedWork.GlobalVariables[var - 16];
        }
    }
}