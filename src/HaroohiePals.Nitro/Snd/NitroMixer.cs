namespace HaroohiePals.Nitro.Snd
{
    public class NitroMixer
    {
        public NitroMixer()
        {
            Channels = new NitroChannel[16];
            for (int i = 0; i < 16; i++)
                Channels[i] = new NitroChannel(i);
            MasterVolume = 127;
        }

        public NitroChannel[] Channels     { get; private set; }
        public byte           MasterVolume { get; set; }

        public (short left, short right) Evaluate(int nrTicks)
        {
            int leftTmp  = 0;
            int rightTmp = 0;
            //  5 Mixer (add channel 0..15)    20.8  -80000h    +7FFF0h
            foreach (var c in Channels)
            {
                if (c == null || !c.Enabled)
                    continue;
                var (l, r) =  c.Evaluate(nrTicks);
                leftTmp    += l;
                rightTmp   += r;
            }

            //	6 Master Volume (mul N/128/64) 14.21 -2000h     +1FF0h
            leftTmp  = leftTmp * (MasterVolume == 127 ? 128 : MasterVolume) / 128;  // / 2;// / 64;
            rightTmp = rightTmp * (MasterVolume == 127 ? 128 : MasterVolume) / 128; // / 2;// / 64;
            //	7 Strip fraction               14.0  -2000h     +1FF0h
            //	?
            //Left = (short)Left_tmp;//(short)(Left_tmp << 5);
            //Right = (short)Right_tmp;//(short)(Right_tmp << 5);
            //	8 Add Bias (0..3FFh, def=200h) 15.0  -2000h+0   +1FF0h+3FFh
            //Left_tmp += 0x200;
            //Right_tmp += 0x200;
            //	9 Clip (min/max 0h..3FFh)      10.0  0          +3FFh
            if (leftTmp < -0x8000)
                leftTmp = -0x8000;
            else if (leftTmp > 0x7FFF)
                leftTmp = 0x7FFF;
            if (rightTmp < -0x8000)
                rightTmp = -0x8000;
            else if (rightTmp > 0x7FFF)
                rightTmp = 0x7FFF;

            return ((short)leftTmp, //(short)((Left_tmp - 0x200) << 6);
                (short)rightTmp);   //(short)((Right_tmp - 0x200) << 6);
        }
    }
}