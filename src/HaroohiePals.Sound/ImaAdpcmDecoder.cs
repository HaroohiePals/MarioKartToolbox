namespace HaroohiePals.Sound
{
    public class ImaAdpcmDecoder
    {
        private readonly byte[] _data;

        public int Last { get; set; }
        public int Index { get; set; }

        public int Offset { get; set; }

        public bool SecondNibble { get; set; }

        public ImaAdpcmDecoder(byte[] data, int offset)
        {
            Last = (short) (data[offset] | (data[offset + 1] << 8));
            Index = (short) (data[offset + 2] | (data[offset + 3] << 8)) & 0x7F;
            offset += 4;
            Offset = offset;
            _data = data;
        }

        public short GetSample()
        {
            short samp = GetSample((byte) ((_data[Offset] >> (SecondNibble ? 4 : 0)) & 0xF));
            if (SecondNibble)
                Offset++;
            SecondNibble = !SecondNibble;
            return samp;
        }

        private short GetSample(byte nibble)
        {
            int diff =
                ImaAdpcm.StepTable[Index] / 8 +
                ImaAdpcm.StepTable[Index] / 4 * ((nibble >> 0) & 1) +
                ImaAdpcm.StepTable[Index] / 2 * ((nibble >> 1) & 1) +
                ImaAdpcm.StepTable[Index] * ((nibble >> 2) & 1);

            int samp = Last + diff * ((((nibble >> 3) & 1) == 1) ? -1 : 1);
            Last = ImaAdpcm.ClampSample(samp);
            Index = ImaAdpcm.ClampIndex(Index + ImaAdpcm.IndexTable[nibble & 7]);
            return (short) Last;
        }

        public static short[] Decode(byte[] data)
        {
            var dec     = new ImaAdpcmDecoder(data, 0);
            int samples = (data.Length - 4) * 2;
            var result  = new short[samples];
            for (int i = 0; i < samples; i++)
                result[i] = dec.GetSample();
            return result;
        }
    }
}