using HaroohiePals.IO;
using System.Collections.Generic;
using System.IO;

namespace HaroohiePals.Nitro.NitroSystem.G2d
{
    public class Ncer
    {
        public const uint NcerSignature = 0x4E434552;

        public Ncer(Cebk.CellData[] cells, uint mapping)
        {
            Header       = new BinaryFileHeader(NcerSignature, 1);
            CellDataBank = new Cebk(cells, mapping);
        }

        public byte[] Write()
        {
            var m  = new MemoryStream();
            var er = new EndianBinaryWriterEx(m, Endianness.LittleEndian);
            Header.Write(er);
            CellDataBank.Write(er);
            er.BaseStream.Position = 0x8;
            er.Write((uint)er.BaseStream.Length);
            byte[] result = m.ToArray();
            er.Close();
            return result;
        }

        public BinaryFileHeader Header;

        public Cebk CellDataBank;

        public class Cebk
        {
            public const uint CebkSignature = 0x4345424B;

            public Cebk(CellData[] cells, uint mapping)
            {
                Signature   = CebkSignature;
                MappingMode = mapping;
                NrCells     = (ushort)cells.Length;
                CellDatas   = cells;
            }

            public uint   Signature;
            public uint   BlockSize;
            public ushort NrCells;
            public ushort CellBankAttributes; //1 = with bounding box
            public uint   CellDataOffset;
            public uint   MappingMode;
            public uint   VramTransferDataOffset;
            public uint   StringBankPointer;   //runtime
            public uint   ExtendedDataPointer; //runtime

            public void Write(EndianBinaryWriter er)
            {
                //collect all oam data
                var oamData = new List<ushort>();
                foreach (var cellData in CellDatas)
                {
                    cellData.OAMDataOffset = (uint)oamData.Count * 2;
                    oamData.AddRange(cellData.OAMData);
                }

                if (((oamData.Count * 2) % 4) != 0)
                    oamData.Add(0);
                er.Write(CebkSignature);
                er.Write((uint)(0x20 + CellDatas.Length * 8 + oamData.Count * 2));
                er.Write((ushort)CellDatas.Length);
                er.Write((ushort)0);
                er.Write(0x18u);
                er.Write(MappingMode);
                er.Write(0u);
                er.Write(0u);
                er.Write(0u);
                foreach (var cellData in CellDatas)
                    cellData.Write(er);
                er.Write(oamData.ToArray(), 0, oamData.Count);
            }

            public CellData[] CellDatas;

            public class CellData
            {
                public CellData(ushort[] oamData, ushort attributes)
                {
                    NrOAMAttributes = (ushort)(oamData.Length / 3);
                    CellAttributes  = attributes;
                    OAMData         = oamData;
                }

                public void Write(EndianBinaryWriter er)
                {
                    er.Write((ushort)(OAMData.Length / 3));
                    er.Write(CellAttributes);
                    er.Write(OAMDataOffset);
                }

                public ushort NrOAMAttributes;
                public ushort CellAttributes;
                public uint   OAMDataOffset;

                public ushort[] OAMData;
            }
        }
    }
}