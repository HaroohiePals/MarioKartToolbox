using System;

namespace HaroohiePals.Nitro.NitroSystem.Snd.Player
{
    public class Track
    {
        public bool Active;
        public bool NoteWait;
        public bool Mute;
        public bool Tie;
        public bool NoteFinishWait;
        public bool Portamento;
        public bool CompareFlag;
        public bool HasChannelMask;

        public byte PanRange;

        public ushort ProgramNumber;

        public byte  Volume;
        public byte  Volume2;
        public sbyte PitchBend;
        public byte  BendRange;

        public sbyte Pan;
        public sbyte ExtendedPan;
        public short ExtendedFader;
        public short ExtendedPitch;

        public byte Attack;
        public byte Decay;
        public byte Sustain;
        public byte Release;

        public byte  Priority;
        public sbyte Transpose;
        public byte  PortamentoKey;
        public byte  PortamentoTime;
        public short SweepPitch;

        public Lfo.LfoParam Modulation = new();
        public ushort       ChannelMask;

        public int Wait;

        public byte[] Data;
        public int    DataOffset;

        public readonly int[]  CallStack = new int[3];
        public readonly byte[] LoopCount = new byte[3];
        public          byte   CallStackDepth;

        public ExChannel ChannelList;

        private DSSoundContext _context;

        public Track(DSSoundContext context)
        {
            _context = context;
        }

        public void Init()
        {
            Data           = null;
            DataOffset     = 0;
            NoteWait       = true;
            Mute           = false;
            Tie            = false;
            NoteFinishWait = false;
            Portamento     = false;
            CompareFlag    = true;
            HasChannelMask = false;
            CallStackDepth = 0;
            ProgramNumber  = 0;
            Priority       = 64;
            Volume         = 127;
            Volume2        = 127;
            ExtendedFader  = 0;
            Pan            = 0;
            ExtendedPan    = 0;
            PitchBend      = 0;
            ExtendedPitch  = 0;
            Attack         = 0xFF;
            Decay          = 0xFF;
            Sustain        = 0xFF;
            Release        = 0xFF;
            PanRange       = 127;
            BendRange      = 2;
            PortamentoKey  = 60;
            PortamentoTime = 0;
            SweepPitch     = 0;
            Transpose      = 0;
            ChannelMask    = 0xFFFF;
            Modulation.Init();
            Wait        = 0;
            ChannelList = null;
        }

        public void Close(Player player)
        {
            ReleaseChannelAll(player, -1);
            FreeChannelAll();
        }

        public void Start(byte[] data, int offset)
        {
            Data       = data;
            DataOffset = offset;
        }

        public void SetMute(Player player, int type)
        {
            switch (type)
            {
                case 0:
                    Mute = false;
                    break;
                case 1:
                    Mute = true;
                    break;
                case 2:
                    Mute = true;
                    ReleaseChannelAll(player, -1);
                    break;
                case 3:
                    Mute = true;
                    ReleaseChannelAll(player, 127);
                    FreeChannelAll();
                    break;
            }
        }

        public void ReleaseChannelAll(Player player, int release)
        {
            UpdateChannel(player, release);
            var chan = ChannelList;
            while (chan != null)
            {
                if (chan.IsActive())
                {
                    if (release >= 0)
                        chan.SetRelease(release);
                    chan.Priority = 1;
                    chan.ReleaseChannel();
                }

                chan = chan.Next;
            }
        }

        public void FreeChannelAll()
        {
            var chan = ChannelList;
            while (chan != null)
            {
                chan.Free();
                chan = chan.Next;
            }

            ChannelList = null;
        }

        private int ReadArg(Player player, int type)
        {
            switch (type)
            {
                case 0:
                    return Data[DataOffset++];
                case 1:
                    return Data[DataOffset++] | (Data[DataOffset++] << 8);
                case 2: //variable length
                    byte val;
                    int  result = 0;
                    do
                    {
                        val    = Data[DataOffset++];
                        result = (result << 7) | (val & 0x7F);
                    } while ((val & 0x80) != 0);

                    return result;
                case 3:
                    short min = (short)(Data[DataOffset++] | (Data[DataOffset++] << 8));
                    short max = (short)(Data[DataOffset++] | (Data[DataOffset++] << 8));
                    return ((_context.CalcRandom() * (max - min + 1)) >> 16) + min;
                case 4:
                    byte id = Data[DataOffset++];
                    return player.GetVariablePtr(id);

                default:
                    return 0;
            }
        }

        public void UpdateChannel(Player player, int release)
        {
            int userDecay = Util.DecibelSquareTable[player.Volume] + Util.DecibelSquareTable[Volume] +
                            Util.DecibelSquareTable[Volume2];
            int userDecay2 = ExtendedFader + player.ExtendedFader;
            int userPitch  = (int)ExtendedPitch + ((int)PitchBend * ((int)BendRange * 64) / 128);
            int userPan    = Pan;
            if (PanRange != 127)
                userPan = (userPan * (int)PanRange + 64) / 128;
            userPan += ExtendedPan;
            if (userDecay < -32768)
                userDecay = -32768;
            if (userDecay2 < -32768)
                userDecay2 = -32768;
            if (userPan < -128)
                userPan = -128;
            else if (userPan > 127)
                userPan = 127;
            ExChannel chan = ChannelList;
            while (chan != null)
            {
                chan.UserDecay2 = (short)userDecay2;
                if (chan.EnvelopeStatus != ExChannel.SoundEnvelopeStatus.Release)
                {
                    chan.UserDecay        = (short)userDecay;
                    chan.UserPitch        = (short)userPitch;
                    chan.UserPan          = (sbyte)userPan;
                    chan.PanRange         = PanRange;
                    chan.Lfo.Param.Target = Modulation.Target;
                    chan.Lfo.Param.Speed  = Modulation.Speed;
                    chan.Lfo.Param.Depth  = Modulation.Depth;
                    chan.Lfo.Param.Range  = Modulation.Range;
                    chan.Lfo.Param.Delay  = Modulation.Delay;
                    if (chan.Length == 0 && release != 0)
                    {
                        chan.Priority = 1;
                        chan.ReleaseChannel();
                    }
                }

                chan = chan.Next;
            }
        }

        private void ChannelCallback(ExChannel channel, ExChannel.ExChannelCallbackStatus status, object callbackData)
        {
            if (status == ExChannel.ExChannelCallbackStatus.Finish)
            {
                channel.Priority = 0;
                channel.Free();
            }

            var track = (Track)callbackData;
            var list  = track.ChannelList;
            if (list == channel) track.ChannelList = channel.Next;
            else
            {
                while (true)
                {
                    if (list.Next == null) break;
                    if (list.Next == channel)
                    {
                        list.Next = channel.Next;
                        return;
                    }

                    list = list.Next;
                }
            }
        }

        public void NoteOnCommandProc(Player player, byte key, byte velocity, int length)
        {
            ExChannel c = null;
            if (Tie && ChannelList != null)
            {
                c          = ChannelList;
                c.Key      = key;
                c.Velocity = velocity;
            }
            else
            {
                var instd = player.Bank.ReadInstData(ProgramNumber, key);
                if (instd != null)
                {
                    ushort channelMask;
                    switch (instd.Type)
                    {
                        case InstData.InstType.Pcm:
                        case InstData.InstType.DirectPcm:
                            channelMask = 0xFFFF;
                            break;
                        case InstData.InstType.Psg:
                            channelMask = 0x3F00;
                            break;
                        case InstData.InstType.Noise:
                            channelMask = 0xC000;
                            break;
                        default:
                            return;
                    }

                    c = _context.AllocExChannel((uint)(channelMask & ChannelMask), (byte)(Priority + player.Priority),
                        HasChannelMask, ChannelCallback, this);
                    if (c != null)
                    {
                        if (Tie)
                            length = -1;
                        if (!c.NoteOn(key, velocity, length, player.Bank, instd))
                        {
                            c.Priority = 0;
                            c.Free();
                            return;
                        }

                        c.Next      = ChannelList;
                        ChannelList = c;
                    }
                }
            }

            if (c != null)
            {
                if (Attack != 0xFF)
                    c.SetAttack(Attack);
                if (Decay != 0xFF)
                    c.SetDecay(Decay);
                if (Sustain != 0xFF)
                    c.SetSustain(Sustain);
                if (Release != 0xFF)
                    c.SetRelease(Release);
                c.SweepPitch = SweepPitch;
                if (Portamento)
                    c.SweepPitch += (short)((PortamentoKey - key) << 6);
                if (PortamentoTime != 0)
                    c.SweepLength = PortamentoTime * PortamentoTime * System.Math.Abs(c.SweepPitch) >> 11;
                else
                {
                    c.SweepLength = length;
                    c.AutoSweep   = false;
                }

                c.SweepCounter = 0;
            }
        }

        public int SeqMain(Player player, int trackIdx, bool play)
        {
            var chan = ChannelList;
            while (chan != null)
            {
                if (chan.Length > 0)
                    chan.Length--;
                if (!chan.AutoSweep && chan.SweepCounter < chan.SweepLength)
                    chan.SweepCounter++;
                chan = chan.Next;
            }

            if (NoteFinishWait)
            {
                if (ChannelList != null)
                    return 0;
                NoteFinishWait = false;
            }

            if (Wait > 0)
            {
                Wait--;
                if (Wait > 0)
                    return 0;
            }

            while (true)
            {
                if (Wait != 0 || NoteFinishWait)
                    return 0;
                bool argOverride = false;
                bool execute     = true;
                byte command     = Data[DataOffset++];
                if (command == (byte)MmlCommand.If)
                {
                    command = Data[DataOffset++];
                    execute = CompareFlag;
                }

                byte argOverrideType = 0;
                if (command == (byte)MmlCommand.Random)
                {
                    command         = Data[DataOffset++];
                    argOverrideType = 3;
                    argOverride     = true;
                }

                if (command == (byte)MmlCommand.Variable)
                {
                    command         = Data[DataOffset++];
                    argOverrideType = 4;
                    argOverride     = true;
                }

                if (command < 0x80) //note command
                {
                    byte velocity = Data[DataOffset++];
                    byte argtype  = 2;
                    if (argOverride)
                        argtype = argOverrideType;
                    int length = ReadArg(player, argtype);
                    int key    = command + Transpose;
                    if (execute)
                    {
                        if (key < 0)
                            key = 0;
                        else if (key > 127)
                            key = 127;
                        if (!Mute && play)
                            NoteOnCommandProc(player, (byte)key, velocity, (length <= 0 ? -1 : length));
                        PortamentoKey = (byte)key;
                        if (NoteWait)
                        {
                            Wait = length;
                            if (length == 0)
                                NoteFinishWait = true;
                        }
                    }
                }
                else
                {
                    switch (command & 0xF0)
                    {
                        case 0x80:
                        {
                            int argtype = 2;
                            if (argOverride)
                                argtype = argOverrideType;
                            int arg = (short)ReadArg(player, argtype);
                            if (execute)
                            {
                                if ((MmlCommand)command == MmlCommand.Wait)
                                    Wait = arg;
                                else if ((MmlCommand)command == MmlCommand.Prg && arg < 0x10000)
                                    ProgramNumber = (ushort)arg;
                            }

                            break;
                        }
                        case 0x90:
                        {
                            switch ((MmlCommand)command)
                            {
                                case MmlCommand.OpenTrack:
                                {
                                    int trackidx = Data[DataOffset++];
                                    int offset = Data[DataOffset++] | (Data[DataOffset++] << 8) |
                                                 (Data[DataOffset++] << 16);
                                    if (execute)
                                    {
                                        var tmpTrack = player.GetTrack(trackidx);
                                        if (tmpTrack != null && tmpTrack != this)
                                        {
                                            tmpTrack.Close(player);
                                            tmpTrack.Start(Data, offset);
                                        }
                                    }

                                    break;
                                }
                                case MmlCommand.Jump:
                                {
                                    int offset = Data[DataOffset++] | (Data[DataOffset++] << 8) |
                                                 (Data[DataOffset++] << 16);
                                    if (execute)
                                        DataOffset = offset;
                                    break;
                                }
                                case MmlCommand.Call:
                                {
                                    int offset = Data[DataOffset++] | (Data[DataOffset++] << 8) |
                                                 (Data[DataOffset++] << 16);
                                    if (execute && CallStackDepth < 3)
                                    {
                                        CallStack[CallStackDepth] = DataOffset;
                                        CallStackDepth++;
                                        DataOffset = offset;
                                    }

                                    break;
                                }
                            }

                            break;
                        }
                        case 0xB0:
                        {
                            int varIdx  = Data[DataOffset++];
                            int argtype = 1;
                            if (argOverride)
                                argtype = argOverrideType;
                            short     arg = (short)ReadArg(player, argtype);
                            ref short var = ref player.GetVariablePtr(varIdx);
                            if (execute) // && var != null)
                            {
                                switch ((MmlCommand)command)
                                {
                                    case MmlCommand.Setvar:
                                        var = arg;
                                        break;
                                    case MmlCommand.Addvar:
                                        var += arg;
                                        break;
                                    case MmlCommand.Subvar:
                                        var -= arg;
                                        break;
                                    case MmlCommand.Mulvar:
                                        var *= arg;
                                        break;
                                    case MmlCommand.Divvar:
                                        if (arg != 0)
                                            var /= arg;
                                        break;
                                    case MmlCommand.Shiftvar:
                                        if (arg < 0)
                                            var >>= -arg;
                                        else
                                            var <<= arg;
                                        break;
                                    case MmlCommand.Randvar:
                                        bool neg = false;
                                        if (arg < 0)
                                        {
                                            neg = true;
                                            arg = (short)-arg;
                                        }

                                        short rand = (short)((_context.CalcRandom() * (arg + 1)) >> 16);
                                        if (neg)
                                            rand = (short)-rand;
                                        var = rand;
                                        break;

                                    case MmlCommand.CmpEq:
                                        CompareFlag = var == arg;
                                        break;
                                    case MmlCommand.CmpGe:
                                        CompareFlag = var >= arg;
                                        break;
                                    case MmlCommand.CmpGt:
                                        CompareFlag = var > arg;
                                        break;
                                    case MmlCommand.CmpLe:
                                        CompareFlag = var <= arg;
                                        break;
                                    case MmlCommand.CmpLt:
                                        CompareFlag = var < arg;
                                        break;
                                    case MmlCommand.CmpNe:
                                        CompareFlag = var != arg;
                                        break;
                                }
                            }

                            break;
                        }
                        case 0xC0:
                        case 0xD0:
                        {
                            int argType = 0;
                            if (argOverride)
                                argType = argOverrideType;
                            int arg = ReadArg(player, argType);
                            if (execute)
                            {
                                switch ((MmlCommand)command)
                                {
                                    case MmlCommand.Pan:
                                        Pan = (sbyte)(arg - 64);
                                        break;
                                    case MmlCommand.Volume:
                                        Volume = (byte)arg;
                                        break;
                                    case MmlCommand.MainVolume:
                                        player.Volume = (byte)arg;
                                        break;
                                    case MmlCommand.Transpose:
                                        Transpose = (sbyte)arg;
                                        break;
                                    case MmlCommand.PitchBend:
                                        PitchBend = (sbyte)arg;
                                        break;
                                    case MmlCommand.BendRange:
                                        BendRange = (byte)arg;
                                        break;
                                    case MmlCommand.Prio:
                                        Priority = (byte)arg;
                                        break;
                                    case MmlCommand.NoteWait:
                                        NoteWait = (arg & 1) == 1;
                                        break;
                                    case MmlCommand.Tie:
                                        Tie = (arg & 1) == 1;
                                        ReleaseChannelAll(player, -1);
                                        FreeChannelAll();
                                        break;
                                    case MmlCommand.Porta:
                                        PortamentoKey = (byte)(arg + Transpose);
                                        Portamento    = true;
                                        break;
                                    case MmlCommand.ModDepth:
                                        Modulation.Depth = (byte)arg;
                                        break;
                                    case MmlCommand.ModSpeed:
                                        Modulation.Speed = (byte)arg;
                                        break;
                                    case MmlCommand.ModType:
                                        Modulation.Target = (Lfo.LfoParam.LfoTarget)arg;
                                        break;
                                    case MmlCommand.ModRange:
                                        Modulation.Range = (byte)arg;
                                        break;
                                    case MmlCommand.PortaSw:
                                        Portamento = (arg & 1) == 1;
                                        break;
                                    case MmlCommand.PortaTime:
                                        PortamentoTime = (byte)arg;
                                        break;
                                    case MmlCommand.Attack:
                                        Attack = (byte)arg;
                                        break;
                                    case MmlCommand.Decay:
                                        Decay = (byte)arg;
                                        break;
                                    case MmlCommand.Sustain:
                                        Sustain = (byte)arg;
                                        break;
                                    case MmlCommand.Release:
                                        Release = (byte)arg;
                                        break;
                                    case MmlCommand.LoopStart:
                                        if (CallStackDepth < 3)
                                        {
                                            CallStack[CallStackDepth] = DataOffset;
                                            LoopCount[CallStackDepth] = (byte)arg;
                                            CallStackDepth++;
                                        }

                                        break;
                                    case MmlCommand.Volume2:
                                        Volume2 = (byte)arg;
                                        break;
                                    case MmlCommand.Printvar:
                                        if (_context.sMmlPrintEnable)
                                        {
                                            int val = player.GetVariablePtr(arg);
                                            Console.WriteLine($"#{player.MyNo}[{trackIdx}]: printvar No.{arg} = {val}");
                                        }

                                        break;
                                    case MmlCommand.Mute:
                                        SetMute(player, arg);
                                        break;
                                }
                            }

                            break;
                        }
                        case 0xE0:
                        {
                            int argtype = 1;
                            if (argOverride)
                                argtype = argOverrideType;
                            short arg = (short)ReadArg(player, argtype);
                            if (!execute)
                                break;
                            switch ((MmlCommand)command)
                            {
                                case MmlCommand.ModDelay:
                                    Modulation.Delay = (ushort)arg;
                                    break;
                                case MmlCommand.Tempo:
                                    player.Tempo = (ushort)arg;
                                    break;
                                case MmlCommand.SweepPitch:
                                    SweepPitch = arg;
                                    break;
                            }

                            break;
                        }
                        case 0xF0:
                        {
                            if (!execute)
                                break;
                            switch ((MmlCommand)command)
                            {
                                case MmlCommand.LoopEnd:
                                    if (CallStackDepth != 0)
                                    {
                                        int loopCount = LoopCount[CallStackDepth - 1];
                                        if (loopCount != 0)
                                        {
                                            loopCount--;
                                            if (loopCount == 0)
                                            {
                                                CallStackDepth--;
                                                break;
                                            }
                                        }

                                        LoopCount[CallStackDepth - 1] = (byte)loopCount;
                                        DataOffset                    = CallStack[CallStackDepth - 1];
                                    }

                                    break;
                                case MmlCommand.Ret:
                                    if (CallStackDepth != 0)
                                    {
                                        CallStackDepth--;
                                        DataOffset = CallStack[CallStackDepth];
                                    }

                                    break;
                                case MmlCommand.Fin:
                                    return -1;
                            }

                            break;
                        }
                    }
                }
            }
        }
    }
}