namespace HaroohiePals.Nitro.NitroSystem.Snd.Player
{
    public class Lfo
    {
        public readonly LfoParam Param = new();
        public ushort   DelayCounter;
        public ushort   Counter;

        public void Start()
        {
            DelayCounter = 0;
            Counter      = 0;
        }

        public void Update()
        {
            if (DelayCounter < Param.Delay)
            {
                DelayCounter++;
                return;
            }

            int v2 = Param.Speed << 6;
            int i;
            for (i = (Counter + v2) >> 8; i >= 0x80; i -= 128) ;
            Counter += (ushort)v2;
            Counter &= 0xFF;
            Counter |= (ushort)(i << 8);
        }

        public int GetValue()
        {
            if (Param.Depth != 0 && DelayCounter >= Param.Delay)
                return Param.Range * Param.Depth * Util.SinIdx(Counter >> 8);
            return 0;
        }

        public class LfoParam
        {
            public enum LfoTarget : byte
            {
                Pitch  = 0,
                Volume = 1,
                Pan    = 2
            }

            public LfoTarget Target;
            public byte      Speed; // 256 samples, one at a time
            public byte      Depth; // 1.0 time at 128
            public byte      Range;
            public ushort    Delay;

            public void Init()
            {
                Target = LfoTarget.Pitch;
                Depth  = 0;
                Range  = 1;
                Speed  = 16;
                Delay  = 0;
            }
        }
    }
}