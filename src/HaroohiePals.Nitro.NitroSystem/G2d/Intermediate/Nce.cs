using HaroohiePals.IO;
using HaroohiePals.Nitro.G2;
using System;
using System.Drawing;
using System.IO;
using System.Text;

namespace HaroohiePals.Nitro.NitroSystem.G2d.Intermediate
{
    public class Nce
    {
        public const uint NceSignature = 0x424F434E;

        public Nce(byte[] data)
            : this(new MemoryStream(data, false)) { }

        public Nce(Stream stream)
        {
            using (var er = new EndianBinaryReaderEx(stream, Endianness.LittleEndian))
            {
                Header = new BinaryFileHeader(er);
                if (Header.Signature != NceSignature)
                    throw new SignatureNotCorrectException(Header.Signature, NceSignature,
                        er.BaseStream.Position - 0x10);

                for (int i = 0; i < Header.DataBlocks; i++)
                {
                    uint type = er.Read<uint>();
                    er.BaseStream.Position -= 4;
                    switch (type)
                    {
                        case Cell.CellBlockType:
                            CellData = new Cell(er);
                            break;
                        case Cnum.CnumBlockType:
                            CharacterNumberData = new Cnum(er);
                            break;
                        case Grp.GrpBlockType:
                            GroupNumberData = new Grp(er);
                            break;
                        case Anim.AnimBlockType:
                            CellAnimationData = new Anim(er);
                            break;
                        case Actl.ActlBlockType:
                            CellAnimationControlData = new Actl(er);
                            break;
                        case Mode.ModeBlockType:
                            MappingModeInformationData = new Mode(er);
                            break;
                        case Char.CharBlockType:
                            Character1DData = new Char(er);
                            break;
                        case Link.LinkBlockType:
                            LinkFileNameData = new Link(er);
                            break;
                        case Labl.LablBlockType:
                            CellAnimationLabelData = new Labl(er);
                            break;
                        case Cmnt.CmntBlockType:
                            CommentData = new Cmnt(er);
                            break;
                        case Ccmt.CcmtBlockType:
                            CellCommentData = new Ccmt(er);
                            break;
                        case Ecmt.EcmtBlockType:
                            CellAnimationCommentData = new Ecmt(er);
                            break;
                        case Fcmt.FcmtBlockType:
                            CellAnimationFrameCommentsData = new Fcmt(er);
                            break;
                        case Clbl.ClblBlockType:
                            CellColorLabelData = new Clbl(er);
                            break;
                        case Extr.ExtrBlockType:
                            ExtendedData = new Extr(er);
                            break;
                        default:
                            throw new Exception("Unknown block found!");
                    }
                }
            }
        }

        public BinaryFileHeader Header;

        public Cell CellData;

        public class Cell
        {
            public const uint CellBlockType = 0x4C4C4543;

            public Cell(EndianBinaryReaderEx er)
            {
                BlockType = er.ReadSignature(CellBlockType);
                BlockSize = er.Read<uint>();

                uint nrCells = er.Read<uint>();
                Cells = new Obj[nrCells][];
                for (int i = 0; i < nrCells; i++)
                {
                    uint nrObjs = er.Read<uint>();
                    Cells[i] = new Obj[nrObjs];
                    for (int j = 0; j < nrObjs; j++)
                        Cells[i][j] = new Obj(er);
                }
            }

            public uint BlockType;
            public uint BlockSize;

            public Obj[][] Cells;

            public class Obj
            {
                public Obj(EndianBinaryReaderEx er)
                {
                    OAMPositionX  = er.Read<short>();
                    OAMPositionY  = er.Read<short>();
                    OAMRsMode     = er.Read<byte>() == 1;
                    OAMRsParam    = er.Read<byte>();
                    OAMHFlip      = er.Read<byte>() == 1;
                    OAMVFlip      = er.Read<byte>() == 1;
                    OAMRsDEnable  = er.Read<byte>() == 1;
                    OAMObjMode    = (GxOamMode)er.Read<byte>();
                    OAMMosaic     = er.Read<byte>() == 1;
                    OAMColorMode  = (GxOamColorMode)er.Read<byte>();
                    OAMShape      = er.Read<byte>();
                    OAMSize       = er.Read<byte>();
                    OAMPriority   = er.Read<byte>();
                    OAMColorParam = er.Read<byte>();
                    OAMCharName   = er.Read<ushort>();
                    er.Read<ushort>(); //padding
                }

                public short          OAMPositionX;
                public short          OAMPositionY;
                public bool           OAMRsMode;
                public byte           OAMRsParam;
                public bool           OAMHFlip;
                public bool           OAMVFlip;
                public bool           OAMRsDEnable;
                public GxOamMode      OAMObjMode;
                public bool           OAMMosaic;
                public GxOamColorMode OAMColorMode;
                public byte           OAMShape;
                public byte           OAMSize;
                public byte           OAMPriority;
                public byte           OAMColorParam;
                public ushort         OAMCharName;

                public GxOamAttr ToOamAttr()
                {
                    var result = new GxOamAttr();
                    result.X = OAMPositionX;
                    result.Y = OAMPositionY;
                    if (OAMRsMode && OAMRsDEnable)
                        result.SetEffect(GxOamEffect.AffineDouble, OAMRsParam);
                    else if (OAMRsMode && !OAMRsDEnable)
                        result.SetEffect(GxOamEffect.Affine, OAMRsParam);
                    else if (!OAMHFlip && !OAMVFlip)
                        result.SetEffect(GxOamEffect.None, OAMRsParam);
                    else if (OAMHFlip && !OAMVFlip)
                        result.SetEffect(GxOamEffect.FlipH, OAMRsParam);
                    else if (!OAMHFlip && OAMVFlip)
                        result.SetEffect(GxOamEffect.FlipV, OAMRsParam);
                    else
                        result.SetEffect(GxOamEffect.FlipHV, OAMRsParam);

                    result.Mode       = OAMObjMode;
                    result.ColorParam = OAMColorParam;
                    result.Mosaic     = OAMMosaic;
                    result.ColorMode  = OAMColorMode;
                    result.Shape = (GxOamShape)((OAMShape << GxOamAttr.OamAttr01ShapeShift) |
                                                (OAMSize << GxOamAttr.OamAttr01SizeShift));
                    result.Priority = OAMPriority;
                    result.CharName = OAMCharName;
                    return result;
                }
            }
        }

        public Cnum CharacterNumberData;

        public class Cnum
        {
            public const uint CnumBlockType = 0x4D554E43;

            public Cnum(EndianBinaryReaderEx er)
            {
                BlockType = er.ReadSignature(CnumBlockType);
                BlockSize = er.Read<uint>();

                uint nrCells = er.Read<uint>();
                CharNumber = new uint[nrCells][];
                for (int i = 0; i < nrCells; i++)
                {
                    uint nrObjs = er.Read<uint>();
                    CharNumber[i] = er.Read<uint>((int)nrObjs);
                }
            }

            public uint BlockType;
            public uint BlockSize;

            public uint[][] CharNumber;
        }

        public Grp GroupNumberData;

        public class Grp
        {
            public const uint GrpBlockType = 0x20505247;

            public Grp(EndianBinaryReaderEx er)
            {
                BlockType = er.ReadSignature(GrpBlockType);
                BlockSize = er.Read<uint>();

                uint nrCells = er.Read<uint>();
                OBJGroupNumber = new uint[nrCells][];
                for (int i = 0; i < nrCells; i++)
                {
                    uint nrObjs = er.Read<uint>();
                    OBJGroupNumber[i] = er.Read<uint>((int)nrObjs);
                }
            }

            public uint BlockType;
            public uint BlockSize;

            public uint[][] OBJGroupNumber;
        }

        public Anim CellAnimationData;

        public class Anim
        {
            public const uint AnimBlockType = 0x4D494E41;

            public Anim(EndianBinaryReaderEx er)
            {
                BlockType = er.ReadSignature(AnimBlockType);
                BlockSize = er.Read<uint>();

                uint nrAnims = er.Read<uint>();
                CellAnims = new CellAnim[nrAnims];
                for (int i = 0; i < nrAnims; i++)
                    CellAnims[i] = new CellAnim(er);
            }

            public uint BlockType;
            public uint BlockSize;

            public CellAnim[] CellAnims;

            public class CellAnim
            {
                public CellAnim(EndianBinaryReaderEx er)
                {
                    CellAnimLabelIndex = er.Read<uint>();
                    CellAnimCmntIndex  = er.Read<uint>();

                    uint nrCells = er.Read<uint>();
                    Cells = new Cell[nrCells];
                    for (int i = 0; i < nrCells; i++)
                        Cells[i] = new Cell(er);
                }

                public uint CellAnimLabelIndex;
                public uint CellAnimCmntIndex;

                public Cell[] Cells;

                public class Cell
                {
                    public Cell(EndianBinaryReaderEx er)
                    {
                        CellIndex             = er.Read<ushort>();
                        WaitFrames            = er.Read<ushort>();
                        CellAffineParamRot    = er.Read<float>();
                        CellAffineParamScaleX = er.Read<float>();
                        CellAffineParamScaleY = er.Read<float>();
                        CellTranslocateX      = er.Read<int>();
                        CellTranslocateY      = er.Read<int>();
                    }

                    public ushort CellIndex;
                    public ushort WaitFrames;
                    public double CellAffineParamRot;
                    public double CellAffineParamScaleX;
                    public double CellAffineParamScaleY;
                    public int    CellTranslocateX;
                    public int    CellTranslocateY;
                }
            }
        }

        public Actl CellAnimationControlData;

        public class Actl
        {
            public const uint ActlBlockType = 0x4C544341;

            public Actl(EndianBinaryReaderEx er)
            {
                BlockType = er.ReadSignature(ActlBlockType);
                BlockSize = er.Read<uint>();

                uint nrAnims = er.Read<uint>();
                CellAnimControls = new CellAnimControl[nrAnims];
                for (int i = 0; i < nrAnims; i++)
                    CellAnimControls[i] = new CellAnimControl(er);
            }

            public uint BlockType;
            public uint BlockSize;

            public CellAnimControl[] CellAnimControls;

            public class CellAnimControl
            {
                public enum LoopMode : uint
                {
                    Stop        = 0,
                    Loop        = 1,
                    Reverse     = 2,
                    ReverseLoop = 3
                }

                public CellAnimControl(EndianBinaryReaderEx er)
                {
                    CellAnimLoopMode       = (LoopMode)er.Read<uint>();
                    CellAnimLoopStartFrame = er.Read<uint>();
                    CellAnimSpeed          = er.Read<float>();
                }

                public LoopMode CellAnimLoopMode;
                public uint     CellAnimLoopStartFrame;
                public double   CellAnimSpeed;
            }
        }

        public Mode MappingModeInformationData;

        public class Mode
        {
            public const uint ModeBlockType = 0x45444F4D;

            public enum Mapping : uint
            {
                Mode2D     = 0,
                Mode1D32K  = 1,
                Mode1D64K  = 2,
                Mode1D128K = 3,
                Mode1D256K = 4
            }

            public enum Compression : uint
            {
                Cell,
                File
            }

            public Mode(EndianBinaryReaderEx er)
            {
                BlockType       = er.ReadSignature(ModeBlockType);
                BlockSize       = er.Read<uint>();
                MappingMode     = (Mapping)er.Read<uint>();
                CompressionMode = (Compression)er.Read<uint>();
            }

            public uint BlockType;
            public uint BlockSize;

            public Mapping     MappingMode;
            public Compression CompressionMode;
        }

        public Char Character1DData;

        public class Char
        {
            public const uint CharBlockType = 0x52414843;

            public Char(EndianBinaryReaderEx er)
            {
                BlockType = er.ReadSignature(CharBlockType);
                BlockSize = er.Read<uint>();

                uint nrCells = er.Read<uint>();
                CharData = new byte[nrCells][];
                for (int i = 0; i < nrCells; i++)
                {
                    uint charSize = er.Read<uint>();
                    CharData[i] = er.Read<byte>((int)charSize);
                }
            }

            public uint BlockType;
            public uint BlockSize;

            public byte[][] CharData;
        }

        public Link LinkFileNameData;

        public Labl CellAnimationLabelData;

        public class Labl
        {
            public const uint LablBlockType = 0x4C42414C;

            public Labl(EndianBinaryReaderEx er)
            {
                BlockType = er.ReadSignature(LablBlockType);
                BlockSize = er.Read<uint>();

                uint nrCellAnims = er.Read<uint>();
                CellAnimLabel = new string[nrCellAnims];
                for (int i = 0; i < nrCellAnims; i++)
                    CellAnimLabel[i] = er.ReadString(Encoding.ASCII, 64).TrimEnd('\0');
            }

            public uint BlockType;
            public uint BlockSize;

            public string[] CellAnimLabel;
        }

        public Cmnt CommentData;

        public Ccmt CellCommentData;

        public class Ccmt
        {
            public const uint CcmtBlockType = 0x544D4343;

            public Ccmt(EndianBinaryReaderEx er)
            {
                BlockType = er.ReadSignature(CcmtBlockType);
                BlockSize = er.Read<uint>();

                uint nrCells = er.Read<uint>();
                CommentData = new string[nrCells];
                for (int i = 0; i < nrCells; i++)
                    CommentData[i] = er.ReadString(Encoding.ASCII, (int)er.Read<uint>()).TrimEnd('\0');
            }

            public uint BlockType;
            public uint BlockSize;

            public string[] CommentData;
        }

        public Ecmt CellAnimationCommentData;

        public class Ecmt
        {
            public const uint EcmtBlockType = 0x544D4345;

            public Ecmt(EndianBinaryReaderEx er)
            {
                BlockType = er.ReadSignature(EcmtBlockType);
                BlockSize = er.Read<uint>();

                uint nrCellAnims = er.Read<uint>();
                CommentData = new string[nrCellAnims];
                for (int i = 0; i < nrCellAnims; i++)
                    CommentData[i] = er.ReadString(Encoding.ASCII, (int)er.Read<uint>()).TrimEnd('\0');
            }

            public uint BlockType;
            public uint BlockSize;

            public string[] CommentData;
        }

        public Fcmt CellAnimationFrameCommentsData;

        public class Fcmt
        {
            public const uint FcmtBlockType = 0x544D4346;

            public Fcmt(EndianBinaryReaderEx er)
            {
                BlockType = er.ReadSignature(FcmtBlockType);
                BlockSize = er.Read<uint>();

                uint nrCellAnims = er.Read<uint>();
                CommentData = new string[nrCellAnims][];
                for (int i = 0; i < nrCellAnims; i++)
                {
                    uint nrFrames = er.Read<uint>();
                    CommentData[i] = new string[nrFrames];
                    for (int j = 0; j < nrFrames; j++)
                        CommentData[i][j] = er.ReadString(Encoding.ASCII, (int)er.Read<uint>()).TrimEnd('\0');
                }
            }

            public uint BlockType;
            public uint BlockSize;

            public string[][] CommentData;
        }

        public Clbl CellColorLabelData;

        public class Clbl
        {
            public const uint ClblBlockType = 0x4C424C43;

            public Clbl(EndianBinaryReaderEx er)
            {
                BlockType = er.ReadSignature(ClblBlockType);
                BlockSize = er.Read<uint>();

                uint nrCells = er.Read<uint>();
                ColorLabels = new ColorLabel[nrCells];
                for (int i = 0; i < nrCells; i++)
                    ColorLabels[i] = new ColorLabel(er);
            }

            public uint BlockType;
            public uint BlockSize;

            public ColorLabel[] ColorLabels;

            public struct ColorLabel
            {
                public enum ColorIndex : byte
                {
                    Unspecified = 0,
                    Red         = 1,
                    Orange      = 2,
                    Yellow      = 3,
                    Green       = 4,
                    Blue        = 5,
                    Indigo      = 6,
                    Purple      = 7,

                    Custom = 0xFF
                }

                public ColorLabel(EndianBinaryReaderEx er)
                {
                    R     = er.Read<byte>();
                    G     = er.Read<byte>();
                    B     = er.Read<byte>();
                    Index = (ColorIndex)er.Read<byte>();
                }

                public byte       R;
                public byte       G;
                public byte       B;
                public ColorIndex Index;

                public Color GetColor()
                {
                    switch (Index)
                    {
                        case ColorIndex.Unspecified:
                        default:
                            return Color.Transparent; //idk
                        case ColorIndex.Red:
                            return Color.Red;
                        case ColorIndex.Orange:
                            return Color.Orange;
                        case ColorIndex.Yellow:
                            return Color.Yellow;
                        case ColorIndex.Green:
                            return Color.Green;
                        case ColorIndex.Blue:
                            return Color.Blue;
                        case ColorIndex.Indigo:
                            return Color.Indigo;
                        case ColorIndex.Purple:
                            return Color.Purple;
                        case ColorIndex.Custom:
                            return Color.FromArgb(R, G, B);
                    }
                }
            }
        }

        public Extr ExtendedData;

//        public Rectangle GetCellBound(int idx)
//        {
////            var objs = CellData.Cells[idx];
////
////            int xMin = int.MaxValue;
////            int xMax = int.MinValue;
////            int yMin = int.MaxValue;
////            int yMax = int.MinValue;
////
////            foreach (var obj in objs)
////            {
////                var oam = obj.ToOamAttr();
////
////                if (obj.OAMPositionX < xMin)
////                    xMin = obj.OAMPositionX;
////                if (obj.OAMPositionX + oam.Width > xMax)
////                    xMax = obj.OAMPositionX + oam.Width;
////                if (obj.OAMPositionY < yMin)
////                    yMin = obj.OAMPositionY;
////                if (obj.OAMPositionY + oam.Height > yMax)
////                    yMax = obj.OAMPositionY + oam.Height;
////            }
////
////            return new Rectangle(xMin - 256, yMin - 256, xMax - xMin, yMax - yMin);
//            return CellToBitmap().bound;
//        }

        // public (Bitmap bmp, Rectangle bound) CellToBitmap(int idx, NCL paletteData, NCG charData,
        //     int translucentAlpha = 16)
        // {
        //     var    objs = CellData.Cells[idx];
        //     byte[] data;
        //     if (MappingModeInformationData.MappingMode != MODE.Mapping.Mode2D)
        //     {
        //         if (Character1DData == null)
        //             return (null, new Rectangle(0, 0, 0, 0));
        //         data = Character1DData.CharData[
        //             MappingModeInformationData.CompressionMode == MODE.Compression.File ? 0 : idx];
        //     }
        //     else
        //         data = charData.CharacterData.CharData;
        //     int charUnit = 32;
        //     switch (MappingModeInformationData.MappingMode)
        //     {
        //         case MODE.Mapping.Mode1D64K:
        //             charUnit = 64;
        //             break;
        //         case MODE.Mapping.Mode1D128K:
        //             charUnit = 128;
        //             break;
        //         case MODE.Mapping.Mode1D256K:
        //             charUnit = 256;
        //             break;
        //     }
        //
        //     int xMin = 512;
        //     int xMax = 0;
        //     int yMin = 512;
        //     int yMax = 0;
        //     var b    = new Bitmap(512, 512);
        //     using (var g = Graphics.FromImage(b))
        //     {
        //         g.Clear(Color.Transparent);
        //         foreach (var obj in objs)
        //         {
        //             var oam = obj.ToOamAttr();
        //             var dec = new NitroGraphicDecoder();
        //             dec.SetPalette(paletteData.PaletteData.PaletteData, true);
        //             var cellCharData = new byte[oam.Width * oam.Height /
        //                                         (obj.OAMColorMode == OAMUtil.GXOamColorMode.Color16 ? 2 : 1)];
        //             if (MappingModeInformationData.MappingMode == MODE.Mapping.Mode2D)
        //             {
        //                 int tileSize = obj.OAMColorMode == OAMUtil.GXOamColorMode.Color16 ? 32 : 64;
        //                 int charIdx  = obj.OAMCharName * 32;
        //                 int dstIdx   = 0;
        //                 for (int y = 0; y < oam.Width / 8; y++)
        //                 {
        //                     int charIdx2 = charIdx;
        //                     for (int x = 0; x < oam.Height / 8; x++)
        //                     {
        //                         Array.Copy(data, charIdx2, cellCharData, dstIdx, tileSize);
        //                         charIdx2 += tileSize;
        //                         dstIdx   += tileSize;
        //                     }
        //
        //                     charIdx += 1024;
        //                 }
        //             }
        //             else
        //                 Array.Copy(data, obj.OAMCharName * charUnit, cellCharData, 0, cellCharData.Length);
        //
        //             dec.SetImageData(cellCharData,
        //                 obj.OAMColorMode == OAMUtil.GXOamColorMode.Color16 ? ImageFormat.Pltt16 : ImageFormat.Pltt256,
        //                 CharFormat.Char);
        //             var decData = dec.Decode(oam.Width, oam.Height,
        //                 (obj.OAMColorMode == OAMUtil.GXOamColorMode.Color16 ? 16 : 256) * obj.OAMColorParam);
        //             var attrib = new ImageAttributes();
        //             if (obj.OAMObjMode == OAMUtil.GXOamMode.Xlu)
        //                 attrib.SetColorMatrix(new ColorMatrix() {Matrix33 = translucentAlpha / 31f});
        //             if (!obj.OAMRsMode)
        //             {
        //                 Rectangle dstRect;
        //                 if (!obj.OAMHFlip && !obj.OAMVFlip)
        //                     dstRect = new Rectangle(
        //                         obj.OAMPositionX + 256,
        //                         obj.OAMPositionY + 256,
        //                         decData.Width,
        //                         decData.Height);
        //                 else if (obj.OAMHFlip && !obj.OAMVFlip)
        //                     dstRect = new Rectangle(
        //                         obj.OAMPositionX + 256 + oam.Width,
        //                         obj.OAMPositionY + 256,
        //                         -decData.Width,
        //                         decData.Height);
        //                 else if (!obj.OAMHFlip && obj.OAMVFlip)
        //                     dstRect = new Rectangle(
        //                         obj.OAMPositionX + 256,
        //                         obj.OAMPositionY + 256 + oam.Height,
        //                         decData.Width,
        //                         -decData.Height);
        //                 else
        //                     dstRect = new Rectangle(
        //                         obj.OAMPositionX + 256 + oam.Width,
        //                         obj.OAMPositionY + 256 + oam.Height,
        //                         -decData.Width,
        //                         -decData.Height);
        //                 g.DrawImage(decData, dstRect, 0, 0, decData.Width, decData.Height, GraphicsUnit.Pixel, attrib);
        //             }
        //
        //             if (obj.OAMPositionX + 256 < xMin)
        //                 xMin = obj.OAMPositionX + 256;
        //             if (obj.OAMPositionX + 256 + oam.Width > xMax)
        //                 xMax = obj.OAMPositionX + 256 + oam.Width;
        //             if (obj.OAMPositionY + 256 < yMin)
        //                 yMin = obj.OAMPositionY + 256;
        //             if (obj.OAMPositionY + 256 + oam.Height > yMax)
        //                 yMax = obj.OAMPositionY + 256 + oam.Height;
        //         }
        //     }
        //
        //     int x1 = xMax;
        //     int y1 = yMax;
        //     int x2 = xMin;
        //     int y2 = yMin;
        //     unsafe
        //     {
        //         var bd = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadOnly,
        //             PixelFormat.Format32bppArgb);
        //         uint* pixels = (uint*) bd.Scan0;
        //         for (int y = yMin; y < yMax; y++)
        //         {
        //             for (int x = xMin; x < xMax; x++)
        //             {
        //                 if ((pixels[(y * bd.Stride >> 2) + x] >> 24) == 0)
        //                     continue;
        //
        //                 if (x < x1)
        //                     x1 = x;
        //                 else if (x > x2)
        //                     x2 = x;
        //
        //                 if (y < y1)
        //                     y1 = y;
        //                 else if (y > y2)
        //                     y2 = y;
        //             }
        //         }
        //
        //         b.UnlockBits(bd);
        //     }
        //
        //     xMin = x1;
        //     xMax = x2 + 1;
        //     yMin = y1;
        //     yMax = y2 + 1;
        //
        //     Bitmap b2;
        //     if (xMax - xMin <= 0 || yMax - yMin <= 0)
        //         b2 = null;
        //     else
        //     {
        //         b2 = new Bitmap(xMax - xMin, yMax - yMin);
        //         using (var g = Graphics.FromImage(b2))
        //         {
        //             g.Clear(Color.Transparent);
        //             g.DrawImage(b, -xMin, -yMin);
        //         }
        //     }
        //
        //     return (b2, new Rectangle(xMin - 256, yMin - 256, xMax - xMin, yMax - yMin));
        // }
    }
}