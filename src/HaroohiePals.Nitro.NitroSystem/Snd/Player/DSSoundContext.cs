using HaroohiePals.Nitro.Snd;

namespace HaroohiePals.Nitro.NitroSystem.Snd.Player
{
    public class DSSoundContext
    {
        private static readonly int[] ChannelOrder = { 4, 5, 6, 7, 2, 0, 3, 1, 8, 9, 0xA, 0xB, 0xE, 0xC, 0xF, 0xD };

        public NitroMixer Mixer      { get; private set; }
        public Work       Work       { get; private set; }
        public SharedWork SharedWork { get; private set; }

        public uint   sLockChannel;
        public uint   sWeakLockChannel;
        public byte[] sOrgPan         = new byte[16];
        public byte[] sOrgVolume      = new byte[16];
        public int    sMasterPan      = -1;
        public uint   sSurroundDecay  = 0;
        public bool   sMmlPrintEnable = true;

        public DSSoundContext()
        {
            Mixer      = new NitroMixer();
            Work       = new Work(this);
            SharedWork = new SharedWork(this);
        }

        private uint sRand = 0x12345678;

        public ushort CalcRandom()
        {
            sRand = sRand * 1664525 + 1013904223;
            return (ushort)(sRand >> 16);
        }

        public void ExChannelInit()
        {
            for (int i = 0; i < 16; i++)
            {
                Work.Channels[i].MyNo     = (byte)i;
                Work.Channels[i].SyncFlag = 0;
                Work.Channels[i].Active   = false;
            }

            sLockChannel     = 0;
            sWeakLockChannel = 0;
        }

        public void SeqInit()
        {
            for (int i = 0; i < 16; i++)
            {
                Work.Players[i].Active = false;
                Work.Players[i].MyNo   = (byte)i;
            }

            for (int i = 0; i < 32; i++)
                Work.Tracks[i].Active = false;
        }

        public void SeqMain(bool play)
        {
            uint v2 = 0;
            for (int i = 0; i < 16; i++)
            {
                if (Work.Players[i].Active)
                {
                    if (Work.Players[i].Prepared)
                    {
                        if (play && !Work.Players[i].Paused)
                            Work.Players[i].TempoMain();
                        Work.Players[i].UpdateChannel();
                    }

                    v2 |= 1u << i;
                }
            }

            SharedWork.PlayerStatus = v2;
        }

        private int AllocTrack()
        {
            for (int i = 0; i < 32; i++)
            {
                if (!Work.Tracks[i].Active)
                {
                    Work.Tracks[i].Active = true;
                    return i;
                }
            }

            return -1;
        }

        public void PrepareSeq(int playerNr, byte[] data, int offset, Sbnk bank)
        {
            if (Work.Players[playerNr].Active)
                Work.Players[playerNr].Finish();
            Work.Players[playerNr].Init(bank);
            int track = AllocTrack();
            if (track >= 0)
            {
                Work.Tracks[track].Init();
                Work.Tracks[track].Start(data, offset);
                Work.Players[playerNr].Tracks[0] = (byte)track;
                if ((MmlCommand)data[Work.Tracks[track].DataOffset++] == MmlCommand.AllocTrack)
                {
                    ushort mask = (ushort)((data[Work.Tracks[track].DataOffset++] |
                                            (data[Work.Tracks[track].DataOffset++] << 8)) >> 1);
                    int trackidx = 1;
                    while (mask != 0)
                    {
                        if ((mask & 1) != 0)
                        {
                            int track2 = AllocTrack();
                            if (track2 < 0) break;
                            Work.Tracks[track2].Init();
                            Work.Players[playerNr].Tracks[trackidx] = (byte)track2;
                        }

                        trackidx++;
                        mask >>= 1;
                    }
                }
                else
                    Work.Tracks[track].DataOffset--;

                Work.Players[playerNr].Active   = true;
                Work.Players[playerNr].Prepared = false;
            }

            SharedWork.PlayerStatus |= 1u << playerNr;
        }

        public void StartPreparedSeq(int playerNr)
            => Work.Players[playerNr].Prepared = true;

        public void StartSeq(int playerNr, byte[] data, int offset, Sbnk bank)
        {
            PrepareSeq(playerNr, data, offset, bank);
            StartPreparedSeq(playerNr);
        }

        public void StopSeq(int playerNr)
        {
            if (!Work.Players[playerNr].Active)
                return;
            Work.Players[playerNr].Finish();
            SharedWork.PlayerStatus &= ~(1u << playerNr);
        }

        public void PauseSeq(int playerNr, bool pause)
        {
            Work.Players[playerNr].Paused = pause;
            if (!pause)
                return;
            for (int i = 0; i < 16; i++)
            {
                Track t = Work.Players[playerNr].GetTrack(i);
                if (t == null)
                    continue;
                t.ReleaseChannelAll(Work.Players[playerNr], 127);
                t.FreeChannelAll();
            }
        }

        public ExChannel AllocExChannel(uint chBitMask, byte prio, bool strongRequest,
            ExChannel.ExChannelCallback callback, object callbackData)
        {
            uint channels = chBitMask & ~sLockChannel;
            if (!strongRequest)
                channels &= ~sWeakLockChannel;
            ExChannel exChannel = null;
            for (int i = 0; i < 16; i++)
            {
                int channel = ChannelOrder[i];
                if ((channels & (1 << channel)) == 0)
                    continue;
                if (exChannel != null)
                {
                    int v12 = exChannel.Priority;
                    int v13 = Work.Channels[channel].Priority;
                    if (v13 <= v12 && (v13 != v12 || ExChannel.CompareVolume(exChannel, Work.Channels[channel]) < 0))
                        exChannel = Work.Channels[channel];
                }
                else
                    exChannel = Work.Channels[channel];
            }

            if (exChannel != null && prio >= exChannel.Priority)
            {
                exChannel.Callback?.Invoke(exChannel, ExChannel.ExChannelCallbackStatus.Drop, exChannel.CallbackData);
                exChannel.SyncFlag = 1;
                exChannel.Active   = false;
                exChannel.InitAlloc(callback, callbackData, prio);
                return exChannel;
            }

            return null;
        }

        public void StopUnlockedChannel(uint chBitMask)
        {
            for (int i = 0; i < 16; i++)
            {
                if ((chBitMask & 1) != 0 && (sLockChannel & (1 << i)) == 0)
                {
                    if (Work.Channels[i].Callback != null)
                        Work.Channels[i].Callback(Work.Channels[i], ExChannel.ExChannelCallbackStatus.Drop,
                            Work.Channels[i].CallbackData);
                    StopChannel(i, false);
                    Work.Channels[i].Priority = 0;
                    Work.Channels[i].Free();
                    Work.Channels[i].SyncFlag = 0;
                    Work.Channels[i].Active   = false;
                }

                chBitMask >>= 1;
                if (chBitMask == 0) break;
            }
        }

        public void LockChannel(uint chBitMask, uint flags)
        {
            uint chBitMaskSave = chBitMask;
            for (int i = 0; i < 16; i++)
            {
                if ((chBitMask & 1) != 0 && (sLockChannel & (1 << i)) == 0)
                {
                    if (Work.Channels[i].Callback != null)
                        Work.Channels[i].Callback(Work.Channels[i], ExChannel.ExChannelCallbackStatus.Drop,
                            Work.Channels[i].CallbackData);
                    StopChannel(i, false);
                    Work.Channels[i].Priority = 0;
                    Work.Channels[i].Free();
                    Work.Channels[i].SyncFlag = 0;
                    Work.Channels[i].Active   = false;
                }

                chBitMask >>= 1;
                if (chBitMask == 0) break;
            }

            if ((flags & 1) != 0)
                sWeakLockChannel |= chBitMaskSave;
            else
                sLockChannel |= chBitMaskSave;
        }

        public void UnlockChannel(uint chBitMask, uint flags)
        {
            if ((flags & 1) != 0)
                sWeakLockChannel &= ~chBitMask;
            else
                sLockChannel &= ~chBitMask;
        }

        public uint GetLockedChannel(uint flags)
            => (flags & 1) != 0 ? sWeakLockChannel : sLockChannel;

        public void SetupChannelPcm(int channel, byte[] data, NitroChannel.SoundFormat format,
            NitroChannel.RepeatMode repeat,
            ushort loopStart, uint loopLen, byte volume, NitroChannel.DataShift shift, ushort timer, byte pan)
        {
            sOrgPan[channel] = pan;
            if (sMasterPan >= 0)
                pan = (byte)sMasterPan;
            sOrgVolume[channel] = volume;
            if (sSurroundDecay > 0 && ((1 << channel) & 0xFFF5) != 0)
                volume = CalcSurroundDecay(volume, pan);
            Mixer.Channels[channel].Pan    = pan;
            Mixer.Channels[channel].Volume = volume;
            Mixer.Channels[channel].Shift  = shift;
            Mixer.Channels[channel].Timer  = (ushort)-timer;
            Mixer.Channels[channel].Format = format;
            Mixer.Channels[channel].Data   = data;
            switch (format)
            {
                case NitroChannel.SoundFormat.Pcm8:
                case NitroChannel.SoundFormat.Pcm16:
                    Mixer.Channels[channel].DataPosition = -3;
                    break;
                case NitroChannel.SoundFormat.Adpcm:
                    Mixer.Channels[channel].DataPosition = -11;
                    break;
                case NitroChannel.SoundFormat.PsgNoise:
                    Mixer.Channels[channel].DataPosition = -1;
                    break;
            }

            Mixer.Channels[channel].Hold         = false;
            Mixer.Channels[channel].LoopStart    = loopStart;
            Mixer.Channels[channel].Length       = loopLen;
            Mixer.Channels[channel].Repeat       = repeat;
            Mixer.Channels[channel].Enabled      = false;
            Mixer.Channels[channel].Duty         = 0;
            Mixer.Channels[channel].AdpcmDecoder = null;
            Mixer.Channels[channel].Counter      = 0;
        }

        public void SetupChannelPsg(int channel, NitroChannel.PSGDuty duty, byte volume, NitroChannel.DataShift shift,
            ushort timer, byte pan)
        {
            sOrgPan[channel] = pan;
            if (sMasterPan >= 0)
                pan = (byte)sMasterPan;
            sOrgVolume[channel] = volume;
            if (sSurroundDecay > 0 && ((1 << channel) & 0xFFF5) != 0)
                volume = CalcSurroundDecay(volume, pan);
            Mixer.Channels[channel].Pan          = pan;
            Mixer.Channels[channel].Volume       = volume;
            Mixer.Channels[channel].Shift        = shift;
            Mixer.Channels[channel].Format       = NitroChannel.SoundFormat.PsgNoise;
            Mixer.Channels[channel].LoopStart    = 0;
            Mixer.Channels[channel].Length       = 0;
            Mixer.Channels[channel].Duty         = duty;
            Mixer.Channels[channel].Data         = null;
            Mixer.Channels[channel].DataPosition = -1;
            Mixer.Channels[channel].Repeat       = NitroChannel.RepeatMode.Manual;
            Mixer.Channels[channel].Hold         = false;
            Mixer.Channels[channel].Enabled      = false;
            Mixer.Channels[channel].Timer        = (ushort)-timer;
            Mixer.Channels[channel].PsgCounter   = 0;
            Mixer.Channels[channel].Counter      = 0;
        }

        public void SetupChannelNoise(int channel, byte volume, NitroChannel.DataShift shift, ushort timer, byte pan)
        {
            sOrgPan[channel] = pan;
            if (sMasterPan >= 0)
                pan = (byte)sMasterPan;
            sOrgVolume[channel] = volume;
            if (sSurroundDecay > 0 && ((1 << channel) & 0xFFF5) != 0)
                volume = CalcSurroundDecay(volume, pan);
            Mixer.Channels[channel].Pan          = pan;
            Mixer.Channels[channel].Volume       = volume;
            Mixer.Channels[channel].Shift        = shift;
            Mixer.Channels[channel].Format       = NitroChannel.SoundFormat.PsgNoise;
            Mixer.Channels[channel].LoopStart    = 0;
            Mixer.Channels[channel].Length       = 0;
            Mixer.Channels[channel].Duty         = 0;
            Mixer.Channels[channel].Data         = null;
            Mixer.Channels[channel].DataPosition = -1;
            Mixer.Channels[channel].Repeat       = NitroChannel.RepeatMode.Manual;
            Mixer.Channels[channel].Hold         = false;
            Mixer.Channels[channel].Enabled      = false;
            Mixer.Channels[channel].Timer        = (ushort)-timer;
            Mixer.Channels[channel].NoiseCounter = 0x7FFF;
            Mixer.Channels[channel].Counter      = 0;
        }

        public void StopChannel(int channel, bool hold)
        {
            Mixer.Channels[channel].Enabled = false;
            Mixer.Channels[channel].Hold    = hold;
        }

        public void SetChannelVolume(int channel, byte volume, NitroChannel.DataShift shift)
        {
            sOrgVolume[channel] = volume;
            if (sSurroundDecay > 0 && ((1 << channel) & 0xFFF5) != 0)
                volume = CalcSurroundDecay(volume, Mixer.Channels[channel].Pan);
            Mixer.Channels[channel].Volume = volume;
            Mixer.Channels[channel].Shift  = shift;
        }

        public void SetChannelTimer(int channel, ushort timer)
            => Mixer.Channels[channel].Timer = (ushort)-timer;

        public void SetChannelPan(int channel, byte pan)
        {
            sOrgPan[channel] = pan;
            if (sMasterPan >= 0)
                pan = (byte)sMasterPan;
            Mixer.Channels[channel].Pan = pan;
            if (sSurroundDecay > 0 && ((1 << channel) & 0xFFF5) != 0)
                Mixer.Channels[channel].Volume = CalcSurroundDecay(sOrgVolume[channel], pan);
        }

        public bool IsChannelActive(int channel) => Mixer.Channels[channel].Enabled;

        private byte CalcSurroundDecay(int volume, int pan)
        {
            if (pan < 24)
                return (byte)(volume * (sSurroundDecay * (pan + 40) + ((0x7FFF - sSurroundDecay) << 6)) >> 21);
            if (pan >= 24 && pan <= 104)
                return (byte)volume;
            return (byte)(volume * (-sSurroundDecay * (pan - 40) + ((sSurroundDecay + 0x7FFF) << 6)) >> 21);
        }

        public void ExChannelMain(bool doUpdate)
        {
            for (int i = 0; i < 16; i++)
            {
                int v3 = 0;
                if (!Work.Channels[i].Active)
                    continue;
                if (Work.Channels[i].Started)
                {
                    Work.Channels[i].SyncFlag |= 1;
                    Work.Channels[i].Started  =  false;
                }
                else if (!IsChannelActive(i))
                {
                    if (Work.Channels[i].Callback != null)
                        Work.Channels[i].Callback(Work.Channels[i], ExChannel.ExChannelCallbackStatus.Finish,
                            Work.Channels[i].CallbackData);
                    else Work.Channels[i].Priority = 0;
                    Work.Channels[i].Volume = 0;
                    Work.Channels[i].Active = false;
                    continue;
                }

                int v8 = ((int)Work.Channels[i].Key - (int)Work.Channels[i].OriginalKey) << 6;
                int v9 = Util.DecibelSquareTable[Work.Channels[i].Velocity] + Work.Channels[i].UpdateEnvelope(doUpdate);
                int v10 = Work.Channels[i].SweepMain(doUpdate);
                int db = v9 + Work.Channels[i].UserDecay + Work.Channels[i].UserDecay2;
                int pitch = v8 + v10 + Work.Channels[i].UserPitch;
                int lfoVal = Work.Channels[i].LfoMain(doUpdate);
                switch (Work.Channels[i].Lfo.Param.Target)
                {
                    case Lfo.LfoParam.LfoTarget.Pitch:
                        pitch += lfoVal;
                        break;
                    case Lfo.LfoParam.LfoTarget.Volume:
                        if (db > -32768)
                            db += lfoVal;
                        break;
                    case Lfo.LfoParam.LfoTarget.Pan:
                        v3 = lfoVal;
                        break;
                }

                int v15 = v3 + Work.Channels[i].InitPan;
                if (Work.Channels[i].PanRange != 127)
                    v15 = (v15 * (int)Work.Channels[i].PanRange + 64) / 128;
                int v17 = v15 + (int)Work.Channels[i].UserPan;
                if (Work.Channels[i].EnvelopeStatus != ExChannel.SoundEnvelopeStatus.Release || db > -723)
                {
                    ushort volume = Util.CalcChannelVolume(db);
                    ushort timer  = Util.CalcTimer(Work.Channels[i].Wave.Timer, pitch);
                    if (Work.Channels[i].Type == ExChannel.ExChannelType.Psg)
                        timer &= 0xFFFC;
                    int pan = v17 + 64;
                    if (pan < 0)
                        pan = 0;
                    else if (pan > 127)
                        pan = 127;
                    if (volume != Work.Channels[i].Volume)
                    {
                        Work.Channels[i].Volume   =  volume;
                        Work.Channels[i].SyncFlag |= 8;
                    }

                    if (timer != Work.Channels[i].Timer)
                    {
                        Work.Channels[i].Timer    =  timer;
                        Work.Channels[i].SyncFlag |= 4;
                    }

                    if (pan != Work.Channels[i].Pan)
                    {
                        Work.Channels[i].Pan      =  (byte)pan;
                        Work.Channels[i].SyncFlag |= 0x10;
                    }
                }
                else
                {
                    Work.Channels[i].SyncFlag = 2;
                    if (Work.Channels[i].Callback != null)
                        Work.Channels[i].Callback(Work.Channels[i], ExChannel.ExChannelCallbackStatus.Finish,
                            Work.Channels[i].CallbackData);
                    else
                        Work.Channels[i].Priority = 0;
                    Work.Channels[i].Volume = 0;
                    Work.Channels[i].Active = false;
                }
            }
        }

        public void UpdateExChannel()
        {
            for (int i = 0; i < 16; i++)
            {
                if (Work.Channels[i].SyncFlag == 0)
                    continue;
                if ((Work.Channels[i].SyncFlag & 2) != 0)
                    StopChannel(i, false);
                if ((Work.Channels[i].SyncFlag & 1) != 0)
                {
                    switch (Work.Channels[i].Type)
                    {
                        case ExChannel.ExChannelType.Pcm:
                            SetupChannelPcm(i, Work.Channels[i].Data, Work.Channels[i].Wave.Format,
                                (Work.Channels[i].Wave.Loop
                                    ? NitroChannel.RepeatMode.Repeat
                                    : NitroChannel.RepeatMode.OneShot), Work.Channels[i].Wave.LoopStart,
                                Work.Channels[i].Wave.LoopLen, (byte)(Work.Channels[i].Volume & 0xFF),
                                (NitroChannel.DataShift)(Work.Channels[i].Volume >> 8), Work.Channels[i].Timer,
                                Work.Channels[i].Pan);
                            break;
                        case ExChannel.ExChannelType.Psg:
                            SetupChannelPsg(i, Work.Channels[i].Duty, (byte)(Work.Channels[i].Volume & 0xFF),
                                (NitroChannel.DataShift)(Work.Channels[i].Volume >> 8), Work.Channels[i].Timer,
                                Work.Channels[i].Pan);
                            break;
                        case ExChannel.ExChannelType.Noise:
                            SetupChannelNoise(i, (byte)(Work.Channels[i].Volume & 0xFF),
                                (NitroChannel.DataShift)(Work.Channels[i].Volume >> 8), Work.Channels[i].Timer,
                                Work.Channels[i].Pan);
                            break;
                    }
                }
                else
                {
                    if ((Work.Channels[i].SyncFlag & 4) != 0)
                        SetChannelTimer(i, Work.Channels[i].Timer);
                    if ((Work.Channels[i].SyncFlag & 8) != 0)
                        SetChannelVolume(i, (byte)(Work.Channels[i].Volume & 0xFF),
                            (NitroChannel.DataShift)(Work.Channels[i].Volume >> 8));
                    if ((Work.Channels[i].SyncFlag & 0x10) != 0)
                        SetChannelPan(i, Work.Channels[i].Pan);
                }
            }

            for (int i = 0; i < 16; i++)
            {
                if (Work.Channels[i].SyncFlag == 0)
                    continue;
                if ((Work.Channels[i].SyncFlag & 1) != 0)
                    Mixer.Channels[i].Enabled = true;
                Work.Channels[i].SyncFlag = 0;
            }
        }
    }
}