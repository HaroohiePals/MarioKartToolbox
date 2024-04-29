using HaroohiePals.IO;
using System.Collections.Generic;

namespace HaroohiePals.Sound
{
    public class ImaAdpcmEncoder
    {
        private bool _isInit = false;
        private int  _last;
        private int  _index;

        public byte[] Encode(short[] waveData)
        {
            var result = new List<byte>();
            int offset = 0;
            if (!_isInit)
            {
                _last  = waveData[0];
                _index = GetBestTableIndex((waveData[1] - waveData[0]) * 8);
                byte[] header = new byte[4];
                IOUtil.WriteS16Le(header, 0, waveData[0]);
                IOUtil.WriteS16Le(header, 2, (short)_index);
                result.AddRange(header);
                offset++;
                _isInit = true;
            }

            byte[] nibbles = new byte[waveData.Length - offset]; //nibbles, lets merge it afterwards
            for (int i = offset; i < waveData.Length; i++)
            {
                int val = GetBestConfig(_index, waveData[i] - _last);
                nibbles[i - offset] = (byte)val;

                int diff =
                    ImaAdpcm.StepTable[_index] / 8 +
                    ImaAdpcm.StepTable[_index] / 4 * ((val >> 0) & 1) +
                    ImaAdpcm.StepTable[_index] / 2 * ((val >> 1) & 1) +
                    ImaAdpcm.StepTable[_index] * ((val >> 2) & 1);

                int samp = _last + diff * ((((val >> 3) & 1) == 1) ? -1 : 1);
                _last  = ImaAdpcm.ClampSample(samp);
                _index = ImaAdpcm.ClampIndex(_index + ImaAdpcm.IndexTable[val & 7]);
            }

            for (int i = 0; i < nibbles.Length; i += 2)
            {
                if (i == nibbles.Length - 1)
                {
                    result.Add((byte)(nibbles[i]));
                }
                else result.Add((byte)(nibbles[i] | (nibbles[i + 1] << 4)));
            }

            return result.ToArray();
        }

        private static int GetBestTableIndex(int diff)
        {
            int lowestDiff = int.MaxValue;
            int lowestIdx  = -1;
            for (int i = 0; i < ImaAdpcm.StepTable.Length; i++)
            {
                int diff2 = System.Math.Abs(System.Math.Abs(diff) - ImaAdpcm.StepTable[i]);
                if (diff2 < lowestDiff)
                {
                    lowestDiff = diff2;
                    lowestIdx  = i;
                }
            }

            return lowestIdx;
        }

        private static int GetBestConfig(int index, int diff)
        {
            int result = 0;
            if (diff < 0)
                result |= 1 << 3;
            diff = System.Math.Abs(diff);
            int diffNew = ImaAdpcm.StepTable[index] / 8;

            if (System.Math.Abs(diffNew - diff) >= ImaAdpcm.StepTable[index])
            {
                result  |= 1 << 2;
                diffNew += ImaAdpcm.StepTable[index];
            }

            if (System.Math.Abs(diffNew - diff) >= ImaAdpcm.StepTable[index] / 2)
            {
                result  |= 1 << 1;
                diffNew += ImaAdpcm.StepTable[index] / 2;
            }

            if (System.Math.Abs(diffNew - diff) >= ImaAdpcm.StepTable[index] / 4)
            {
                result  |= 1;
                diffNew += ImaAdpcm.StepTable[index] / 4;
            }

            return result;
        }
    }
}