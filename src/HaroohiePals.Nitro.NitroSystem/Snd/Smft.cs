using HaroohiePals.Nitro.NitroSystem.Snd.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HaroohiePals.Nitro.NitroSystem.Snd
{
    public class Smft
    {
        /*public static SSEQ ToSSEQ(String SMFT)
        {
            EndianBinaryWriter er = new EndianBinaryWriter(new MemoryStream(), Endianness.LittleEndian);
            Dictionary<String, long> globalLabels = new Dictionary<string,long>();
            Dictionary<String, long> localLabels = null;
            String curLabel = null;
            StringReader r = new StringReader(SMFT);
            int linenr = 0;
            String line;
            while ((line = r.ReadLine()) != null)
            {
                linenr++;
                line = line.Trim();
                if (line.Length == 0 || line.StartsWith(";")) continue;
                if (line.Contains(";")) line = line.Substring(0, line.IndexOf(";")).Trim();
                if (line.EndsWith(":"))//global or local label
                {
                    String label = line.Substring(0, line.Length - 1);
                    if (label.StartsWith("_"))//local
                    {
                        if (curLabel == null) throw new SyntaxNotCorrectException("A local label should be defined inside a global label!", linenr);
                        if(localLabels.ContainsKey(label)) throw new SyntaxNotCorrectException("This local label is already defined inside this global label!", linenr);
                        localLabels.Add(label, er.BaseStream.Position);
                    }
                    else
                    {
                        if (globalLabels.ContainsKey(label)) throw new SyntaxNotCorrectException("This label is already defined!", linenr);
                        globalLabels.Add(label, er.BaseStream.Position);
                        curLabel = label;
                        localLabels = new Dictionary<string, long>();
                    }
                    continue;
                }
                String[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if(parts.Length == 0) continue;
                if (parts[0].EndsWith("_if"))
                {
                    er.Write((byte)MmlCommand.If);
                    parts[0] = parts[0].Substring(0, parts.Length - 3);
                }
                bool rand = false;
                if (parts[0].EndsWith("_r"))
                {
                    er.Write((byte)MmlCommand.Random);
                    parts[0] = parts[0].Substring(0, parts.Length - 2);
                    rand = true;
                }
                bool var = false;
                if (parts[0].EndsWith("_v"))
                {
                    er.Write((byte)MmlCommand.Variable);
                    parts[0] = parts[0].Substring(0, parts.Length - 2);
                    rand = true;
                }

                switch (parts[0])
                {
                    case "wait":
                        break;
                }
            }
            return null;
        }*/

        private static readonly string[] Notes =
        {
            "cn", "cs", "dn", "ds", "en", "fn", "fs", "gn", "gs", "an", "as", "bn"
        };

        public static String ToSmft(Sseq seq)
        {
            return ToSmft(seq.Data);
        }

        public static String ToSmft(byte[] seqData, IEnumerable<int> extraLabels = null)
        {
            int[] labels = GetLabels(seqData);
            if (extraLabels != null)
                labels = labels.Concat(extraLabels).Distinct().ToArray();
            int Offset = 0;
            var b      = new StringBuilder();
            while (Offset < seqData.Length)
            {
                if (labels.Contains(Offset)) b.AppendFormat("\nLabel_0x{0:X8}:\n", Offset);
                String postfix     = "";
                bool   argoverride = false;
                byte   command     = seqData[Offset++];
                if (command == (byte)MmlCommand.If)
                {
                    command = seqData[Offset++];
                    postfix = "_if" + postfix;
                    //execute = track.CompareFlag;
                }

                byte argoverridetype = 0;
                if (command == (byte)MmlCommand.Random)
                {
                    command         = seqData[Offset++];
                    postfix         = "_r" + postfix;
                    argoverridetype = 3;
                    argoverride     = true;
                }

                if (command == (byte)MmlCommand.Variable)
                {
                    command         = seqData[Offset++];
                    postfix         = "_v" + postfix;
                    argoverridetype = 4;
                    argoverride     = true;
                }

                if (command < 0x80) //note command
                {
                    if (command == 0 && Offset >= seqData.Length - 4) break;
                    byte velocity = seqData[Offset++];

                    int note   = command % 12;
                    int octave = (command / 12) - 1;
                    b.AppendFormat("    {0}{1} {2},", Notes[note], (octave < 0 ? "m1" : "" + octave) + postfix,
                        velocity);

                    if (argoverride) 
                        WriteArgOverride(b, seqData, ref Offset, argoverridetype);
                    else 
                        b.AppendFormat(" {0}\n", ReadArg(seqData, ref Offset, 2));
                }
                else
                {
                    switch (command & 0xF0)
                    {
                        case 0x80:
                        {
                            short arg             = -1;
                            if (!argoverride) arg = (short)ReadArg(seqData, ref Offset, 2);
                            if ((MmlCommand)command == MmlCommand.Wait)
                                b.Append("    wait" + postfix);
                            else if ((MmlCommand)command == MmlCommand.Prg)
                                b.Append("    prg" + postfix);
                            if (argoverride) WriteArgOverride(b, seqData, ref Offset, argoverridetype);
                            else b.AppendFormat(" {0}\n", arg);
                            break;
                        }
                        case 0x90:
                        {
                            switch ((MmlCommand)command)
                            {
                                case MmlCommand.OpenTrack:
                                {
                                    int trackidx = seqData[Offset++];
                                    int offset = seqData[Offset++] | (seqData[Offset++] << 8) |
                                                 (seqData[Offset++] << 16);
                                    b.AppendFormat("    opentrack{0} {1}, Label_0x{2:X8}\n", postfix, trackidx, offset);
                                    break;
                                }
                                case MmlCommand.Jump:
                                {
                                    int offset = seqData[Offset++] | (seqData[Offset++] << 8) |
                                                 (seqData[Offset++] << 16);
                                    b.AppendFormat("    jump{0} Label_0x{1:X8}\n", postfix, offset);
                                    break;
                                }
                                case MmlCommand.Call:
                                {
                                    int offset = seqData[Offset++] | (seqData[Offset++] << 8) |
                                                 (seqData[Offset++] << 16);
                                    b.AppendFormat("    call{0} Label_0x{1:X8}\n", postfix, offset);
                                    break;
                                }
                            }

                            break;
                        }
                        case 0xB0:
                        {
                            switch ((MmlCommand)command)
                            {
                                case MmlCommand.Setvar:
                                    b.Append("    setvar" + postfix);
                                    break;
                                case MmlCommand.Addvar:
                                    b.Append("    addvar" + postfix);
                                    break;
                                case MmlCommand.Subvar:
                                    b.Append("    subvar" + postfix);
                                    break;
                                case MmlCommand.Mulvar:
                                    b.Append("    mulvar" + postfix);
                                    break;
                                case MmlCommand.Divvar:
                                    b.Append("    divvar" + postfix);
                                    break;
                                case MmlCommand.Shiftvar:
                                    b.Append("    shiftvar" + postfix);
                                    break;
                                case MmlCommand.Randvar:
                                    b.Append("    randvar" + postfix);
                                    break;

                                case MmlCommand.CmpEq:
                                    b.Append("    cmp_eq" + postfix);
                                    break;
                                case MmlCommand.CmpGe:
                                    b.Append("    cmp_ge" + postfix);
                                    break;
                                case MmlCommand.CmpGt:
                                    b.Append("    cmp_gt" + postfix);
                                    break;
                                case MmlCommand.CmpLe:
                                    b.Append("    cmp_le" + postfix);
                                    break;
                                case MmlCommand.CmpLt:
                                    b.Append("    cmp_lt" + postfix);
                                    break;
                                case MmlCommand.CmpNe:
                                    b.Append("    cmp_ne" + postfix);
                                    break;
                            }

                            int v47 = seqData[Offset++];
                            b.AppendFormat(" {0},", v47);
                            if (argoverride) WriteArgOverride(b, seqData, ref Offset, argoverridetype);
                            else b.AppendFormat(" {0}\n", (short)ReadArg(seqData, ref Offset, 1));
                            break;
                        }
                        case 0xC0:
                        case 0xD0:
                        {
                            int arg               = 0;
                            if (!argoverride) arg = ReadArg(seqData, ref Offset, 0);
                            switch ((MmlCommand)command)
                            {
                                case MmlCommand.Pan:
                                    b.Append("    pan" + postfix);
                                    break;
                                case MmlCommand.Volume:
                                    b.Append("    volume" + postfix);
                                    break;
                                case MmlCommand.MainVolume:
                                    b.Append("    main_volume" + postfix);
                                    break;
                                case MmlCommand.Transpose:
                                    b.Append("    transpose" + postfix);
                                    break;
                                case MmlCommand.PitchBend:
                                    b.Append("    pitchbend" + postfix);
                                    break;
                                case MmlCommand.BendRange:
                                    b.Append("    bendrange" + postfix);
                                    break;
                                case MmlCommand.Prio:
                                    b.Append("    prio" + postfix);
                                    break;
                                case MmlCommand.NoteWait:
                                    b.AppendFormat("    notewait_{0}", ((arg & 1) == 1 ? "on" : "off") + postfix);
                                    break;
                                case MmlCommand.Tie:
                                    b.AppendFormat("    tie{0}", ((arg & 1) == 1 ? "on" : "off") + postfix);
                                    break;
                                case MmlCommand.Porta:
                                    b.Append("    porta" + postfix);
                                    break;
                                case MmlCommand.ModDepth:
                                    b.Append("    mod_depth" + postfix);
                                    break;
                                case MmlCommand.ModSpeed:
                                    b.Append("    mod_speed" + postfix);
                                    break;
                                case MmlCommand.ModType:
                                    b.Append("    mod_type" + postfix);
                                    break;
                                case MmlCommand.ModRange:
                                    b.Append("    mod_range" + postfix);
                                    break;
                                case MmlCommand.PortaSw:
                                    b.AppendFormat("    porta_{0}", ((arg & 1) == 1 ? "on" : "off"));
                                    break;
                                case MmlCommand.PortaTime:
                                    b.Append("    porta_time" + postfix);
                                    break;
                                case MmlCommand.Attack:
                                    b.Append("    attack" + postfix);
                                    break;
                                case MmlCommand.Decay:
                                    b.Append("    decay" + postfix);
                                    break;
                                case MmlCommand.Sustain:
                                    b.Append("    sustain" + postfix);
                                    break;
                                case MmlCommand.Release:
                                    b.Append("    release" + postfix);
                                    break;
                                case MmlCommand.LoopStart:
                                    b.Append("    loop_start" + postfix);
                                    break;
                                case MmlCommand.Volume2:
                                    b.Append("    volume2" + postfix);
                                    break;
                                case MmlCommand.Printvar:
                                    b.Append("    printvar" + postfix);
                                    break;
                                case MmlCommand.Mute:
                                    b.Append("    mute" + postfix);
                                    break;
                            }

                            if (argoverride) WriteArgOverride(b, seqData, ref Offset, argoverridetype);
                            else
                            {
                                if ((MmlCommand)command == MmlCommand.Transpose ||
                                    (MmlCommand)command == MmlCommand.PitchBend)
                                    b.AppendFormat(" {0}\n", (sbyte)arg);
                                else if (!((MmlCommand)command == MmlCommand.NoteWait ||
                                           (MmlCommand)command == MmlCommand.Tie ||
                                           (MmlCommand)command == MmlCommand.PortaSw))
                                    b.AppendFormat(" {0}\n", arg);
                                else b.AppendLine();
                            }

                            break;
                        }
                        case 0xE0:
                        {
                            switch ((MmlCommand)command)
                            {
                                case MmlCommand.ModDelay:
                                    b.Append("    mod_delay" + postfix);
                                    break;
                                case MmlCommand.Tempo:
                                    b.Append("    tempo" + postfix);
                                    break;
                                case MmlCommand.SweepPitch:
                                    b.Append("    sweep_pitch" + postfix);
                                    break;
                            }

                            if (argoverride) WriteArgOverride(b, seqData, ref Offset, argoverridetype);
                            else
                            {
                                if ((MmlCommand)command == MmlCommand.SweepPitch)
                                    b.AppendFormat(" {0}\n", (short)ReadArg(seqData, ref Offset, 1));
                                else
                                    b.AppendFormat(" {0}\n", (ushort)ReadArg(seqData, ref Offset, 1));
                            }

                            break;
                        }
                        case 0xF0:
                        {
                            switch ((MmlCommand)command)
                            {
                                case MmlCommand.LoopEnd:
                                    b.AppendLine("    loop_end" + postfix);
                                    break;
                                case MmlCommand.Ret:
                                    b.AppendLine("    ret" + postfix);
                                    break;
                                case MmlCommand.Fin:
                                    b.AppendLine("    fin" + postfix);
                                    break;
                                case MmlCommand.AllocTrack:
                                    b.AppendFormat("    alloctrack{0} 0x{1:X4}\n", postfix,
                                        ReadArg(seqData, ref Offset, 1));
                                    break;
                            }
                        }
                            break;
                    }
                }
            }

            bool trimmed = false;

            string result = b.ToString();
            while (result.EndsWith("    fin\r\n    fin\r\n") || result.EndsWith("    ret\r\n    fin\r\n"))
            {
                result  = result.Substring(0, result.Length - 9);
                trimmed = true;
            }

            if (!trimmed && result.EndsWith("    fin\r\n"))
                result = result.Substring(0, result.Length - 9);


            return result;
        }

        private static int[] GetLabels(byte[] seqData)
        {
            int Offset       = 0;
            var labelOffsets = new List<int>();
            while (Offset < seqData.Length)
            {
                bool argoverride = false;
                byte command     = seqData[Offset++];
                if (command == (byte)MmlCommand.If)
                {
                    command = seqData[Offset++];
                    //execute = track.CompareFlag;
                }

                byte argoverridetype = 0;
                if (command == (byte)MmlCommand.Random)
                {
                    command         = seqData[Offset++];
                    argoverridetype = 3;
                    argoverride     = true;
                }

                if (command == (byte)MmlCommand.Variable)
                {
                    command         = seqData[Offset++];
                    argoverridetype = 4;
                    argoverride     = true;
                }

                if (command < 0x80) //note command
                {
                    if (command == 0 && Offset >= seqData.Length - 4) break;
                    byte velocity            = seqData[Offset++];
                    byte argtype             = 2;
                    if (argoverride)
                        argtype = argoverridetype;
                    int length               = ReadArg(seqData, ref Offset, argtype);
                }
                else
                {
                    switch (command & 0xF0)
                    {
                        case 0x80:
                        {
                            int argtype              = 2;
                            if (argoverride) argtype = argoverridetype;
                            int arg                  = (short)ReadArg(seqData, ref Offset, argtype);
                            break;
                        }
                        case 0x90:
                        {
                            switch ((MmlCommand)command)
                            {
                                case MmlCommand.OpenTrack:
                                {
                                    int trackidx = seqData[Offset++];
                                    int offset = seqData[Offset++] | (seqData[Offset++] << 8) |
                                                 (seqData[Offset++] << 16);
                                    if (!labelOffsets.Contains(offset)) labelOffsets.Add(offset);
                                    break;
                                }
                                case MmlCommand.Jump:
                                {
                                    int offset = seqData[Offset++] | (seqData[Offset++] << 8) |
                                                 (seqData[Offset++] << 16);
                                    if (!labelOffsets.Contains(offset)) labelOffsets.Add(offset);
                                    break;
                                }
                                case MmlCommand.Call:
                                {
                                    int offset = seqData[Offset++] | (seqData[Offset++] << 8) |
                                                 (seqData[Offset++] << 16);
                                    if (!labelOffsets.Contains(offset)) labelOffsets.Add(offset);
                                    break;
                                }
                            }

                            break;
                        }
                        case 0xB0:
                        {
                            int v47                  = seqData[Offset++];
                            int argtype              = 1;
                            if (argoverride) 
                                argtype = argoverridetype;
                            short arg                = (short)ReadArg(seqData, ref Offset, argtype);
                            break;
                        }
                        case 0xC0:
                        case 0xD0:
                        {
                            int argtype              = 0;
                            if (argoverride)
                                argtype = argoverridetype;
                            int arg                  = ReadArg(seqData, ref Offset, argtype);
                            break;
                        }
                        case 0xE0:
                        {
                            int argtype              = 1;
                            if (argoverride) 
                                argtype = argoverridetype;
                            short arg                = (short)ReadArg(seqData, ref Offset, argtype);
                            break;
                        }
                        case 0xF0:
                        {
                            switch ((MmlCommand)command)
                            {
                                case MmlCommand.LoopEnd:
                                    break;
                                case MmlCommand.Ret:
                                    break;
                                case MmlCommand.Fin:
                                    break;
                                case MmlCommand.AllocTrack:
                                    Offset += 2;
                                    break;
                            }

                            break;
                        }
                    }
                }
            }

            return labelOffsets.ToArray();
        }

        private static int ReadArg(byte[] data, ref int offset, int type)
        {
            switch (type)
            {
                case 0: return data[offset++];
                case 1: return data[offset++] | (data[offset++] << 8);
                case 2: //variable length
                    byte val;
                    int  result = 0;
                    do
                    {
                        val    = data[offset++];
                        result = (result << 7) | (val & 0x7F);
                    } while ((val & 0x80) != 0);

                    return result;
                case 3:
                    short  min = (short)(data[offset++] | (data[offset++] << 8));
                    ushort max = (ushort)(data[offset++] | (data[offset++] << 8));
                    return 0; //((Util.CalcRandom() * (max - min + 1)) >> 16) + min;
                case 4:
                    byte id = data[offset++];
                    //SoundVar var = GetVariablePtr(player, id);
                    //if (var != null) return var.Value;
                    return 0;
                default: return 0;
            }
        }

        private static void WriteArgOverride(StringBuilder b, byte[] data, ref int offset, int type)
        {
            switch (type)
            {
                case 3:
                    short min = (short)(data[offset++] | (data[offset++] << 8));
                    short max = (short)(data[offset++] | (data[offset++] << 8));
                    b.AppendFormat(" {0}, {1}\n", min, max);
                    return;
                case 4:
                    b.AppendFormat(" {0}\n", data[offset++]);
                    return;
            }
        }
    }
}