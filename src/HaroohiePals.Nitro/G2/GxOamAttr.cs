using System;

namespace HaroohiePals.Nitro.G2
{
    public struct GxOamAttr
    {
        public const int  OamAttr01YShift = 0;
        public const uint OamAttr01YSize  = 8;
        public const uint OamAttr01YMask  = 0x000000ff;

        public const int  OamAttr01RSEnableShift = 8;
        public const uint OamAttr01RSEnableSize  = 2;
        public const uint OamAttr01RSEnableMask  = 0x00000300;

        public const int  OamAttr01ModeShift = 10;
        public const uint OamAttr01ModeSize  = 2;
        public const uint OamAttr01ModeMask  = 0x00000c00;

        public const int  OamAttr01MosaicShift = 12;
        public const uint OamAttr01MosaicSize  = 1;
        public const uint OamAttr01MosaicMask  = 0x00001000;

        public const int  OamAttr01CMShift = 13;
        public const uint OamAttr01CMSize  = 1;
        public const uint OamAttr01CMMask  = 0x00002000;

        public const int  OamAttr01ShapeShift = 14;
        public const uint OamAttr01ShapeSize  = 2;
        public const uint OamAttr01ShapeMask  = 0x0000c000;

        public const int  OamAttr01XShift = 16;
        public const uint OamAttr01XSize  = 9;
        public const uint OamAttr01XMask  = 0x01ff0000;

        public const int  OamAttr01RSShift  = 25;
        public const uint OamAttr01RSSize   = 5;
        public const uint OamAttr01RSMask   = 0x3e000000;
        public const uint OamAttr01FlipMask = 0x30000000;

        public const int  OamAttr01HFShift = 28;
        public const uint OamAttr01HFSize  = 1;
        public const uint OamAttr01HFMask  = 0x10000000;

        public const int  OamAttr01VFShift = 29;
        public const uint OamAttr01VFSize  = 1;
        public const uint OamAttr01VFMask  = 0x20000000;

        public const int  OamAttr01SizeShift = 30;
        public const uint OamAttr01SizeSize  = 2;
        public const uint OamAttr01SizeMask  = 0xc0000000;

        public const int  OamAttr2NameShift = 0;
        public const uint OamAttr2NameSize  = 10;
        public const uint OamAttr2NameMask  = 0x03ff;

        public const int  OamAttr2PriorityShift = 10;
        public const uint OamAttr2PrioritySize  = 2;
        public const uint OamAttr2PriorityMask  = 0x0c00;

        public const int  OamAttr2CParamShift = 12;
        public const uint OamAttr2CParamSize  = 4;
        public const uint OamAttr2CParamMask  = 0xf000;

        public uint   Attr01;
        public ushort Attr2;

        public int X
        {
            get => (int)((Attr01 & OamAttr01XMask) >> OamAttr01XShift);
            set => Attr01 = (uint)((Attr01 & ~OamAttr01XMask) |
                                   ((value & (OamAttr01XMask >> OamAttr01XShift)) << OamAttr01XShift));
        }

        public int Y
        {
            get => (int)(Attr01 & OamAttr01YMask);
            set => Attr01 = (uint)((Attr01 & ~OamAttr01YMask) | (value & OamAttr01YMask));
        }

        public uint Priority
        {
            get => (Attr2 & OamAttr2PriorityMask) >> OamAttr2PriorityShift;
            set => Attr2 = (ushort)((Attr2 & ~OamAttr2PriorityMask) | (value << OamAttr2PriorityShift));
        }

        public GxOamMode Mode
        {
            get => (GxOamMode)((Attr01 & OamAttr01ModeMask) >> OamAttr01ModeShift);
            set => Attr01 = ((Attr01 & ~OamAttr01ModeMask) | ((uint)value << OamAttr01ModeShift));
        }

        public uint ColorParam
        {
            get => (Attr2 & OamAttr2CParamMask) >> OamAttr2CParamShift;
            set => Attr2 = (ushort)((Attr2 & ~OamAttr2CParamMask) | (value << OamAttr2CParamShift));
        }

        public GxOamEffect Effect
        {
            get
            {
                var effect = (GxOamEffect)(Attr01 & OamAttr01RSEnableMask);
                if (effect == GxOamEffect.Affine || effect == GxOamEffect.AffineDouble)
                    return effect;
                return (effect | (GxOamEffect)(Attr01 & OamAttr01FlipMask));
            }
        }

        public void SetEffect(GxOamEffect effect, int rsParam)
        {
            if (effect != GxOamEffect.Affine && effect != GxOamEffect.AffineDouble)
                Attr01 = ((Attr01 & ~(OamAttr01RSEnableMask | OamAttr01RSMask)) | (uint)effect);
            else
                Attr01 = ((Attr01 & ~(OamAttr01RSEnableMask | OamAttr01RSMask)) | (uint)effect |
                          ((uint)rsParam << OamAttr01RSShift));
        }

        public GxOamShape Shape
        {
            get => (GxOamShape)(Attr01 & (OamAttr01ShapeMask | OamAttr01SizeMask));
            set => Attr01 = (Attr01 & ~(OamAttr01ShapeMask | OamAttr01SizeMask)) | (uint)value;
        }

        public uint CharName
        {
            get => Attr2 & OamAttr2NameMask;
            set => Attr2 = (ushort)((Attr2 & ~OamAttr2NameMask) | value);
        }

        public GxOamColorMode ColorMode
        {
            get => (GxOamColorMode)((Attr01 & OamAttr01CMMask) >> OamAttr01CMShift);
            set => Attr01 = ((Attr01 & ~OamAttr01CMMask) | ((uint)value << OamAttr01CMShift));
        }

        public bool Mosaic
        {
            get => (Attr01 & OamAttr01MosaicMask) == OamAttr01MosaicMask;
            set => Attr01 = (Attr01 & ~OamAttr01MosaicMask) | ((value ? 1u : 0u) << OamAttr01MosaicShift);
        }

        public int Width => Shape switch
        {
            GxOamShape.Shape8x8   => 8,
            GxOamShape.Shape8x16  => 8,
            GxOamShape.Shape8x32  => 8,
            GxOamShape.Shape16x8  => 16,
            GxOamShape.Shape16x16 => 16,
            GxOamShape.Shape16x32 => 16,
            GxOamShape.Shape32x8  => 32,
            GxOamShape.Shape32x16 => 32,
            GxOamShape.Shape32x32 => 32,
            GxOamShape.Shape32x64 => 32,
            GxOamShape.Shape64x32 => 64,
            GxOamShape.Shape64x64 => 64,
            _                     => throw new ArgumentOutOfRangeException()
        };

        public int Height => Shape switch
        {
            GxOamShape.Shape8x8   => 8,
            GxOamShape.Shape16x8  => 8,
            GxOamShape.Shape32x8  => 8,
            GxOamShape.Shape8x16  => 16,
            GxOamShape.Shape16x16 => 16,
            GxOamShape.Shape32x16 => 16,
            GxOamShape.Shape8x32  => 32,
            GxOamShape.Shape16x32 => 32,
            GxOamShape.Shape32x32 => 32,
            GxOamShape.Shape64x32 => 32,
            GxOamShape.Shape32x64 => 64,
            GxOamShape.Shape64x64 => 64,
            _                     => throw new ArgumentOutOfRangeException()
        };
    }
}