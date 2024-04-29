using HaroohiePals.Nitro.Snd;

namespace HaroohiePals.Nitro.NitroSystem.Snd.Player
{
    public class ExChannel
    {
        private static readonly int[] Shift = { 0, 1, 2, 4 };

        private static readonly byte[] AttackTable =
        {
            0x00, 0x01, 0x05, 0x0E, 0x1A, 0x26, 0x33, 0x3F, 0x49, 0x54, 0x5C, 0x64, 0x6D, 0x74, 0x7B, 0x7F, 0x84, 0x89,
            0x8F
        };

        public enum ExChannelType
        {
            Pcm,
            Psg,
            Noise
        }

        public enum ExChannelCallbackStatus
        {
            Drop,
            Finish
        }

        public enum SoundEnvelopeStatus
        {
            Attack,
            Decay,
            Sustain,
            Release
        }

        public delegate void ExChannelCallback(ExChannel chP, ExChannelCallbackStatus status, object userData);

        public byte                MyNo; //0
        public ExChannelType       Type;
        public SoundEnvelopeStatus EnvelopeStatus;
        public bool                Active;
        public bool                Started;
        public bool                AutoSweep;
        public byte                SyncFlag;

        public byte  PanRange;
        public byte  OriginalKey;
        public short UserDecay2;


        public byte  Key;
        public byte  Velocity;
        public sbyte InitPan;
        public sbyte UserPan;

        public short UserDecay;
        public short UserPitch;

        int _envelopeDecay; // attenuation by envelope release

        public int SweepCounter;
        public int SweepLength;

        byte   _attack;
        byte   _sustain;
        ushort _decay;
        ushort _release; //0x20

        public byte   Priority;
        public byte   Pan;
        public ushort Volume;
        public ushort Timer;

        public Lfo Lfo = new();

        public short SweepPitch;

        public int Length;

        public WaveData.WaveParam Wave = new();
        public byte[]             Data;
        public NitroChannel.PSGDuty    Duty;

        public ExChannelCallback Callback;
        public object            CallbackData;

        public ExChannel Next;

        private DSSoundContext _context;

        public ExChannel(DSSoundContext context)
        {
            _context = context;
        }

        public void Free()
        {
            Callback     = null;
            CallbackData = null;
        }

        public bool StartPcm(WaveData.WaveParam waveParam, byte[] data, int length)
        {
            Type           = ExChannelType.Pcm;
            Wave.Format    = waveParam.Format;
            Wave.Loop      = waveParam.Loop;
            Wave.Rate      = waveParam.Rate;
            Wave.Timer     = waveParam.Timer;
            Wave.LoopStart = waveParam.LoopStart;
            Wave.LoopLen   = waveParam.LoopLen;
            Data           = data;
            Start(length);
            return true;
        }

        public bool StartPsg(NitroChannel.PSGDuty duty, int length)
        {
            if (MyNo < 8 || MyNo > 0xD)
                return false;
            Type       = ExChannelType.Psg;
            Duty       = duty;
            Wave.Timer = 8006;
            Start(length);
            return true;
        }

        public bool StartNoise(int length)
        {
            if (MyNo < 0xE || MyNo > 0xF)
                return false;
            Type       = ExChannelType.Noise;
            Wave.Timer = 8006;
            Start(length);
            return true;
        }

        public int UpdateEnvelope(bool doPeriodicProc)
        {
            if (doPeriodicProc)
            {
                switch (EnvelopeStatus)
                {
                    case SoundEnvelopeStatus.Attack:
                        _envelopeDecay = -((-_envelopeDecay * _attack) >> 8);
                        if (_envelopeDecay == 0) 
                            EnvelopeStatus = SoundEnvelopeStatus.Decay;
                        break;
                    case SoundEnvelopeStatus.Decay:
                        int sustain = Util.DecibelSquareTable[_sustain] << 7;
                        _envelopeDecay -= _decay;
                        if (_envelopeDecay <= sustain)
                        {
                            _envelopeDecay = sustain;
                            EnvelopeStatus = SoundEnvelopeStatus.Sustain;
                        }

                        break;
                    case SoundEnvelopeStatus.Release:
                        _envelopeDecay -= _release;
                        break;
                }
            }

            return _envelopeDecay >> 7;
        }

        public void SetAttack(int attack)
        {
            if (attack >= 0x6D)
                _attack = AttackTable[0x7F - attack];
            else
                _attack = (byte)(0xFF - attack);
        }

        public void SetDecay(int decay)
            => _decay = CalcRelease(decay);

        public void SetSustain(byte sustain)
            => _sustain = sustain;

        public void SetRelease(int release)
            => _release = CalcRelease(release);

        public void ReleaseChannel()
            => EnvelopeStatus = SoundEnvelopeStatus.Release;

        public bool IsActive()
            => Active;

        private static ushort CalcRelease(int a)
        {
            if (a == 0x7F)
                return 0xFFFF;
            if (a == 0x7E)
                return 0x3C00;
            if (a >= 0x32)
                return (ushort)(0x1E00 / (0x7E - a));
            return (ushort)(2 * a + 1);
        }

        public void InitAlloc(ExChannelCallback callback, object callbackData, byte priority)
        {
            Next              = null;
            Callback     = callback;
            CallbackData = callbackData;
            Length            = 0;
            Priority     = priority;
            Volume            = 127;
            Started           = false;
            AutoSweep         = true;
            Key               = 60;
            OriginalKey       = 60;
            Velocity          = 127;
            InitPan           = 0;
            UserDecay         = 0;
            UserDecay2        = 0;
            UserPitch         = 0;
            UserPan           = 0;
            PanRange          = 127;
            SweepPitch        = 0;
            SweepLength       = 0;
            SweepCounter      = 0;
            SetAttack(127);
            SetDecay(127);
            SetSustain(127);
            SetRelease(127);
        }

        public void Start(int length)
        {
            _envelopeDecay = -92544;
            EnvelopeStatus = SoundEnvelopeStatus.Attack;
            Length         = length;
            Lfo.Start();
            Started = true;
            Active  = true;
        }

        public static int CompareVolume(ExChannel a, ExChannel b)
        {
            int va = 16 * (a.Volume & 0xFF) >> Shift[a.Volume >> 8];
            int vb = 16 * (b.Volume & 0xFF) >> Shift[b.Volume >> 8];
            if (va == vb)
                return 0;
            if (va >= vb)
                return -1;
            return 1;
        }

        public int SweepMain(bool doUpdate)
        {
            if (SweepPitch == 0 || SweepCounter >= SweepLength)
                return 0;
            int result = (int)(System.Math.BigMul(SweepPitch, SweepLength - SweepCounter) / (long)SweepLength);
            if (doUpdate && AutoSweep) 
                SweepCounter++;
            return result;
        }

        public int LfoMain(bool doUpdate)
        {
            long val = Lfo.GetValue();
            if (val != 0)
            {
                switch (Lfo.Param.Target)
                {
                    case Lfo.LfoParam.LfoTarget.Pitch:
                        val *= 64;
                        break;
                    case Lfo.LfoParam.LfoTarget.Volume:
                        val *= 60;
                        break;
                    case Lfo.LfoParam.LfoTarget.Pan:
                        val *= 64;
                        break;
                }

                val >>= 14;
            }

            if (doUpdate)
                Lfo.Update();
            return (int)val;
        }

        public bool NoteOn(byte key, byte velocity, int length, Sbnk bank, InstData inst)
        {
            int release = inst.Param.Release;
            if (release == 0xFF)
            {
                length  = -1;
                release = 0;
            }

            bool ok = false;
            switch (inst.Type)
            {
                case InstData.InstType.Pcm:
                    //case InstData.InstType.DirectPcm://not possible, because it passes a pointer!
                    WaveData wavedata = null;
                    //if (inst.Type == InstData.InstType.Pcm)
                    wavedata = bank.GetWaveData(inst.Param.Wave[1], inst.Param.Wave[0]);
                    //else
                    //wavedata = inst.Param.Wave[0] | (inst.Param.Wave[1] << 16);
                    if (wavedata != null)
                        ok = StartPcm(wavedata.Param, wavedata.Samples, length);
                    break;
                case InstData.InstType.Psg:
                    ok = StartPsg((NitroChannel.PSGDuty)inst.Param.Wave[0], length);
                    break;
                case InstData.InstType.Noise:
                    ok = StartNoise(length);
                    break;
            }

            if (ok)
            {
                Key         = key;
                OriginalKey = inst.Param.OriginalKey;
                Velocity    = velocity;
                SetAttack(inst.Param.Attack);
                SetDecay(inst.Param.Decay);
                SetSustain(inst.Param.Sustain);
                SetRelease(release);
                InitPan = (sbyte)((int)inst.Param.Pan - 64);
                return true;
            }

            return false;
        }
    }
}