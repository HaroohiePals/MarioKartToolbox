using HaroohiePals.Sound;

namespace HaroohiePals.Nitro.Snd
{
    public class NitroChannel
    {
        public enum DataShift : byte
        {
            Shift_0 = 0,
            Shift_1,
            Shift_2,
            Shift_4
        }

        public enum PSGDuty : int
        {
            Duty1_8 = 0,
            Duty2_8,
            Duty3_8,
            Duty4_8,
            Duty5_8,
            Duty6_8,
            Duty7_8
        }

        public enum RepeatMode : int
        {
            Manual = 0,
            Repeat,
            OneShot
        }

        public enum SoundFormat : byte
        {
            Pcm8 = 0,
            Pcm16,
            Adpcm,
            PsgNoise
        }

        public NitroChannel(int nr)
        {
            _channelNr = nr;
            Enabled    = false;
        }

        private readonly int _channelNr;

        public int         Volume;
        public DataShift   Shift;
        public bool        Hold;
        public int         Pan;
        public PSGDuty     Duty;
        public RepeatMode  Repeat;
        public SoundFormat Format;
        public bool        Enabled;

        public byte[] Data;
        public int    DataPosition;

        public ushort Timer;

        public ushort LoopStart;

        public uint Length;

        public  int   Counter;
        private short _curLeft;
        private short _curRight;

        public ushort NoiseCounter = 0x7FFF;
        public int    PsgCounter   = 0;

        public  ImaAdpcmDecoder AdpcmDecoder   = null;
        private int             _adpcmLoopLast = 0;
        private int             _adpcmLoopIdx  = 0;

        public (short left, short right) Evaluate(int nrTicks)
        {
            if (!Enabled || Timer <= 0)
            {
                _curLeft = _curRight = 0;
                return (0, 0);
            }

            int nrsamp = (Counter + nrTicks) / -(short)Timer;
            Counter = ((int)Counter + nrTicks) % -(short)Timer;
            short left = _curLeft, right = _curRight;
            for (int i = 0; i < nrsamp; i++)
            {
                if (Enabled)
                    (left, right) = GetSample();
                else
                    left = right = 0;
            }

            _curLeft  = left;
            _curRight = right;
            return (left, right);
        }

        private (short left, short right) GetSample()
        {
            //  0 Incoming PCM16 Data          16.0  -8000h     +7FFFh
            if (Format == SoundFormat.Pcm8 || Format == SoundFormat.Pcm16 || Format == SoundFormat.Adpcm)
            {
                if (Format == SoundFormat.Adpcm && AdpcmDecoder != null && AdpcmDecoder.Offset == LoopStart * 4 &&
                    !AdpcmDecoder.SecondNibble)
                {
                    _adpcmLoopLast = AdpcmDecoder.Last;
                    _adpcmLoopIdx  = AdpcmDecoder.Index;
                }

                if ((Format != SoundFormat.Adpcm && DataPosition >= GetSoundLength()) || (Format == SoundFormat.Adpcm &&
                    AdpcmDecoder != null && AdpcmDecoder.Offset >= GetSoundLength() && !AdpcmDecoder.SecondNibble))
                {
                    if (Repeat == RepeatMode.Repeat)
                    {
                        if (Format == SoundFormat.Adpcm)
                        {
                            AdpcmDecoder.Offset       = LoopStart * 4;
                            AdpcmDecoder.Index        = _adpcmLoopIdx;
                            AdpcmDecoder.Last         = _adpcmLoopLast;
                            AdpcmDecoder.SecondNibble = false;
                        }
                        else DataPosition = LoopStart * 4;
                    }
                    else                  //shouldn't happen
                        DataPosition = 0; //tmp
                }
            }

            short samp = 0;
            if (DataPosition < 0)
                DataPosition++;
            else
            {
                switch (Format)
                {
                    case SoundFormat.Pcm8:
                        samp = (short)((short)((sbyte)Data[DataPosition++]) * 256);
                        break;
                    case SoundFormat.Pcm16:
                        samp = (short)(Data[DataPosition++] | (Data[DataPosition++] << 8));
                        break;
                    case SoundFormat.Adpcm:
                        if (AdpcmDecoder == null)
                            AdpcmDecoder = new ImaAdpcmDecoder(Data, DataPosition);
                        samp = AdpcmDecoder.GetSample();
                        break;
                    case SoundFormat.PsgNoise:
                        if (_channelNr >= 8 && _channelNr <= 13) //PSG
                        {
                            int pre  = (int)Duty + 1;
                            int post = 8 - ((int)Duty + 1);
                            if (PsgCounter < pre) samp = -0x7FFF;
                            else samp                  = 0x7FFF;
                            PsgCounter++;
                            if (PsgCounter >= 8) 
                                PsgCounter = 0;
                        }
                        else if (_channelNr == 14 || _channelNr == 15) //Noise
                        {
                            if ((NoiseCounter & 0x1) != 0)
                            {
                                NoiseCounter = (ushort)((NoiseCounter >> 1) ^ 0x6000);
                                samp         = -0x7FFF;
                            }
                            else
                            {
                                NoiseCounter >>= 1;
                                samp         =   0x7FFF;
                            }
                        }

                        break;
                }
            }

            //  1 Volume Divider (div 1..16)   16.4  -8000h     +7FFFh
            samp /= (short)GetVolumeDiv();
            //  2 Volume Factor (mul N/128)    16.11 -8000h     +7FFFh
            samp = (short)((int)samp * (Volume == 127 ? 128 : Volume) / 128);
            //  3 Panning (mul N/128)          16.18 -8000h     +7FFFh
            short left  = (short)((int)samp * (128 - (Pan == 127 ? 128 : Pan)) / 128);
            short right = (short)((int)samp * (Pan == 127 ? 128 : Pan) / 128);
            //  4 Rounding Down (strip 10bit)  16.8  -8000h     +7FFFh
            //	?
            if ((((Format == SoundFormat.Pcm8 || Format == SoundFormat.Pcm16) && DataPosition >= GetSoundLength()) ||
                 (Format == SoundFormat.Adpcm && AdpcmDecoder != null && AdpcmDecoder.Offset >= GetSoundLength() &&
                  !AdpcmDecoder.SecondNibble)) && Repeat == RepeatMode.OneShot)
            {
                Enabled = false;
            }

            return (left, right);
        }

        private uint GetSoundLength()
            => Repeat switch
            {
                RepeatMode.Manual  => (uint)Data.Length, //to prevent overflow
                RepeatMode.Repeat  => (uint)(LoopStart * 4 + Length * 4),
                RepeatMode.OneShot => (uint)(LoopStart * 4 + Length * 4),
                _                  => 0
            };

        private int GetVolumeDiv()
            => Shift switch
            {
                DataShift.Shift_0 => 1,
                DataShift.Shift_1 => 2,
                DataShift.Shift_2 => 4,
                DataShift.Shift_4 => 16,
                _                 => 1
            };
    }
}