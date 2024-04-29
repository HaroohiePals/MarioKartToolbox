using HaroohiePals.Nitro.Gx;
using System;
using System.Numerics;

namespace HaroohiePals.Nitro.G3
{
    public struct GxTexImageParam
    {
        private uint _value;

        private GxTexImageParam(uint value) => _value = value;

        public ushort Address
        {
            get => (ushort)_value;
            set => _value = (_value & ~0xFFFFu) | value;
        }

        public bool RepeatS
        {
            get => (_value & (1u << 16)) != 0;
            set => _value = (_value & ~(1u << 16)) | ((value ? 1u : 0) << 16);
        }

        public bool RepeatT
        {
            get => (_value & (1u << 17)) != 0;
            set => _value = (_value & ~(1u << 17)) | ((value ? 1u : 0) << 17);
        }

        public bool FlipS
        {
            get => (_value & (1u << 18)) != 0;
            set => _value = (_value & ~(1u << 18)) | ((value ? 1u : 0) << 18);
        }

        public bool FlipT
        {
            get => (_value & (1u << 19)) != 0;
            set => _value = (_value & ~(1u << 19)) | ((value ? 1u : 0) << 19);
        }

        public int Width
        {
            get => (int)((_value >> 20) & 7);
            set => _value = (_value & ~(7u << 20)) | ((uint)(value & 7) << 20);
        }

        public int Height
        {
            get => (int)((_value >> 23) & 7);
            set => _value = (_value & ~(7u << 23)) | ((uint)(value & 7) << 23);
        }

        public ImageFormat Format
        {
            get => (ImageFormat)((_value >> 26) & 7);
            set => _value = (_value & ~(7u << 26)) | ((uint)value << 26);
        }

        public bool Color0Transparent
        {
            get => (_value & (1u << 29)) != 0;
            set => _value = (_value & ~(1u << 29)) | ((value ? 1u : 0) << 29);
        }

        public GxTexGen TexGen
        {
            get => (GxTexGen)((_value >> 30) & 3);
            set => _value = (_value & ~(3u << 30)) | ((uint)value << 30);
        }

        public static implicit operator uint(GxTexImageParam value) => value._value;
        public static implicit operator GxTexImageParam(uint value) => new(value);

        public static int ToNitroSize(int originalSize)
        {
            int result = BitOperations.Log2((uint)originalSize) - 3;

            if (((8 << result) != originalSize) || (result > 7))
                throw new Exception("Error during conversion");

            return result;
        }
    }
}