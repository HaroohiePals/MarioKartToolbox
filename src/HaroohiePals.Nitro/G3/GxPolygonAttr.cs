namespace HaroohiePals.Nitro.G3
{
    public struct GxPolygonAttr
    {
        private uint _value;

        private GxPolygonAttr(uint value) => _value = value;

        public uint LightMask
        {
            get => _value & 0xF;
            set => _value = (_value & ~0xFu) | (value & 0xF);
        }

        public GxPolygonMode PolygonMode
        {
            get => (GxPolygonMode)((_value >> 4) & 3);
            set => _value = (_value & ~(3u << 4)) | ((uint)value << 4);
        }

        public GxCull CullMode
        {
            get => (GxCull)((_value >> 6) & 3);
            set => _value = (_value & ~(3u << 6)) | ((uint)value << 6);
        }

        public bool TranslucentDepthUpdate
        {
            get => (_value & (1u << 11)) != 0;
            set => _value = (_value & ~(1u << 11)) | ((value ? 1u : 0) << 11);
        }

        public bool FarClip
        {
            get => (_value & (1u << 12)) != 0;
            set => _value = (_value & ~(1u << 12)) | ((value ? 1u : 0) << 12);
        }

        public bool Render1Dot
        {
            get => (_value & (1u << 13)) != 0;
            set => _value = (_value & ~(1u << 13)) | ((value ? 1u : 0) << 13);
        }

        public bool DepthEquals
        {
            get => (_value & (1u << 14)) != 0;
            set => _value = (_value & ~(1u << 14)) | ((value ? 1u : 0) << 14);
        }

        public bool FogEnable
        {
            get => (_value & (1u << 15)) != 0;
            set => _value = (_value & ~(1u << 15)) | ((value ? 1u : 0) << 15);
        }

        public uint Alpha
        {
            get => (_value >> 16) & 0x1F;
            set => _value = (_value & ~(0x1Fu << 16)) | ((uint)(value & 0x1F) << 16);
        }

        public uint PolygonId
        {
            get => (_value >> 24) & 0x3F;
            set => _value = (_value & ~(0x3Fu << 24)) | ((uint)(value & 0x3F) << 24);
        }

        public static implicit operator uint(GxPolygonAttr value) => value._value;
        public static implicit operator GxPolygonAttr(uint value) => new(value);
    }
}
