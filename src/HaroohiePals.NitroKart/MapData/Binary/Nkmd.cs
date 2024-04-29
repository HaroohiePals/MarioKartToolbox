using HaroohiePals.IO;
using System;
using System.IO;
using System.Text;

namespace HaroohiePals.NitroKart.MapData.Binary
{
    public class Nkmd
    {
        public Nkmd()
        {
            Header = new NKMDHeader();
        }

        public Nkmd(byte[] data) : this(new MemoryStream(data)) { }

        public Nkmd(Stream stream)
        {
            using (var er = new EndianBinaryReaderEx(stream, Endianness.LittleEndian))
            {
                Header = new NKMDHeader(er);
                foreach (var v in Header.SectionOffsets)
                {
                    er.BaseStream.Position = Header.HeaderSize + v;
                    uint sig = er.Read<uint>();
                    er.BaseStream.Position -= 4;
                    switch (sig)
                    {
                        case NkmdObji.OBJISignature:
                            ObjectInformation = new NkmdObji(er);
                            break;
                        case NkmdPath.PATHSignature:
                            Path = new NkmdPath(er);
                            break;
                        case NkmdPoit.POITSignature:
                            Point = new NkmdPoit(er);
                            break;
                        case NkmdStag.STAGSignature:
                            Stage = new NkmdStag(er);
                            break;
                        case NkmdKtps.KTPSSignature:
                            KartPointStart = new NkmdKtps(er);
                            break;
                        case NkmdKtpj.KTPJSignature:
                            KartPointJugem = new NkmdKtpj(er, Header.Version);
                            break;
                        case NkmdKtp2.KTP2Signature:
                            KartPoint2d = new NkmdKtp2(er);
                            break;
                        case NkmdKtpc.KTPCSignature:
                            KartPointCannon = new NkmdKtpc(er);
                            break;
                        case NkmdKtpm.KTPMSignature:
                            KartPointMission = new NkmdKtpm(er);
                            break;
                        case NkmdCpoi.CPOISignature:
                            CheckPoint = new NkmdCpoi(er);
                            break;
                        case NkmdCpat.CPATSignature:
                            CheckPointPath = new NkmdCpat(er);
                            break;
                        case NkmdIpoi.IPOISignature:
                            ItemPoint = new NkmdIpoi(er, Header.Version);
                            break;
                        case NkmdIpat.IPATSignature:
                            ItemPath = new NkmdIpat(er);
                            break;
                        case NkmdEpoi.EPOISignature:
                            EnemyPoint = new NkmdEpoi(er);
                            break;
                        case NkmdEpat.EPATSignature:
                            EnemyPath = new NkmdEpat(er);
                            break;
                        case NkmdMepo.MEPOSignature:
                            MgEnemyPoint = new NkmdMepo(er);
                            break;
                        case NkmdMepa.MEPASignature:
                            MgEnemyPath = new NkmdMepa(er);
                            break;
                        case NkmdArea.AREASignature:
                            Area = new NkmdArea(er);
                            break;
                        case NkmdCame.CAMESignature:
                            Camera = new NkmdCame(er);
                            break;
                        default:
                            throw new Exception("Unknown Section: " + Encoding.ASCII.GetString(
                                new byte[]
                                {
                                    (byte) (sig & 0xFF), (byte) (sig >> 8 & 0xFF),
                                    (byte) (sig >> 16 & 0xFF), (byte) (sig >> 24 & 0xFF)
                                }));
                    }
                }

                //Beta NKM don't need KTPC and KTPM
                if (Header.Version >= 37)
                {
                    if (KartPointCannon == null) KartPointCannon = new NkmdKtpc();
                    if (KartPointMission == null) KartPointMission = new NkmdKtpm();
                }
            }
        }

        private void RecalculateValues()
        {
            CheckPointPath?.UpdateSectionOrder();
            CheckPoint?.UpdateDistancesAndSinCos();
            Camera?.UpdateSinCos();
        }

        public byte[] Write(bool recalculateValues = true)
        {
            if (recalculateValues) RecalculateValues();

            var m = new MemoryStream();
            var er = new EndianBinaryWriterEx(m, Endianness.LittleEndian);
            int nrSections = 0;
            if (ObjectInformation != null) nrSections++;
            if (Path != null) nrSections++;
            if (Point != null) nrSections++;
            if (Stage != null) nrSections++;
            if (KartPointStart != null) nrSections++;
            if (KartPointJugem != null) nrSections++;
            if (KartPoint2d != null) nrSections++;
            if (KartPointCannon != null) nrSections++;
            if (KartPointMission != null) nrSections++;
            if (CheckPoint != null) nrSections++;
            if (CheckPointPath != null) nrSections++;
            if (ItemPoint != null) nrSections++;
            if (ItemPath != null) nrSections++;
            if (EnemyPoint != null) nrSections++;
            if (EnemyPath != null) nrSections++;
            if (MgEnemyPoint != null) nrSections++;
            if (MgEnemyPath != null) nrSections++;
            if (Area != null) nrSections++;
            if (Camera != null) nrSections++;
            Header.SectionOffsets = new uint[nrSections];
            Header.Write(er);

            int sectionIdx = 0;
            if (ObjectInformation != null)
            {
                WriteHeaderInfo(er, sectionIdx++);
                ObjectInformation.Write(er);
            }

            if (Path != null)
            {
                WriteHeaderInfo(er, sectionIdx++);
                Path.Write(er);
            }

            if (Point != null)
            {
                WriteHeaderInfo(er, sectionIdx++);
                Point.Write(er);
            }

            if (Stage != null)
            {
                WriteHeaderInfo(er, sectionIdx++);
                Stage.Write(er);
            }

            if (KartPointStart != null)
            {
                WriteHeaderInfo(er, sectionIdx++);
                KartPointStart.Write(er);
            }

            if (KartPointJugem != null)
            {
                WriteHeaderInfo(er, sectionIdx++);
                KartPointJugem.Write(er);
            }

            if (KartPoint2d != null)
            {
                WriteHeaderInfo(er, sectionIdx++);
                KartPoint2d.Write(er);
            }

            if (KartPointCannon != null)
            {
                WriteHeaderInfo(er, sectionIdx++);
                KartPointCannon.Write(er);
            }

            if (KartPointMission != null)
            {
                WriteHeaderInfo(er, sectionIdx++);
                KartPointMission.Write(er);
            }

            if (CheckPoint != null)
            {
                WriteHeaderInfo(er, sectionIdx++);
                CheckPoint.Write(er);
            }

            if (CheckPointPath != null)
            {
                WriteHeaderInfo(er, sectionIdx++);
                CheckPointPath.Write(er);
            }

            if (ItemPoint != null)
            {
                WriteHeaderInfo(er, sectionIdx++);
                ItemPoint.Write(er);
            }

            if (ItemPath != null)
            {
                WriteHeaderInfo(er, sectionIdx++);
                ItemPath.Write(er);
            }

            if (EnemyPoint != null)
            {
                WriteHeaderInfo(er, sectionIdx++);
                EnemyPoint.Write(er);
            }

            if (EnemyPath != null)
            {
                WriteHeaderInfo(er, sectionIdx++);
                EnemyPath.Write(er);
            }

            if (MgEnemyPoint != null)
            {
                WriteHeaderInfo(er, sectionIdx++);
                MgEnemyPoint.Write(er);
            }

            if (MgEnemyPath != null)
            {
                WriteHeaderInfo(er, sectionIdx++);
                MgEnemyPath.Write(er);
            }

            if (Area != null)
            {
                WriteHeaderInfo(er, sectionIdx++);
                Area.Write(er);
            }

            if (Camera != null)
            {
                WriteHeaderInfo(er, sectionIdx++);
                Camera.Write(er);
            }

            byte[] result = m.ToArray();
            er.Close();
            return result;
        }

        private void WriteHeaderInfo(EndianBinaryWriterEx er, int idx)
        {
            long curpos = er.BaseStream.Position;
            er.BaseStream.Position = 8 + idx * 4;
            er.Write((uint)(curpos - Header.HeaderSize));
            er.BaseStream.Position = curpos;
        }

        public NKMDHeader Header;

        public class NKMDHeader
        {
            public const uint NKMDSignature = 0x444D4B4E;

            public NKMDHeader()
            {
                Version = 37;
                HeaderSize = 8;
                SectionOffsets = new uint[0];
            }

            public NKMDHeader(EndianBinaryReaderEx er)
            {
                uint signature = er.Read<uint>();
                if (signature != NKMDSignature)
                    throw new SignatureNotCorrectException(signature, NKMDSignature, er.BaseStream.Position - 4);
                Version = er.Read<ushort>();
                HeaderSize = er.Read<ushort>();
                SectionOffsets = er.Read<uint>((HeaderSize - 8) / 4);
            }

            public void Write(EndianBinaryWriter er)
            {
                er.Write(NKMDSignature);
                er.Write(Version);
                HeaderSize = (ushort)(8 + SectionOffsets.Length * 4);
                er.Write(HeaderSize);
                er.Write(SectionOffsets, 0, SectionOffsets.Length);
            }

            public ushort Version;
            public ushort HeaderSize;
            public uint[] SectionOffsets;
        }

        public NkmdObji ObjectInformation;
        public NkmdPath Path;
        public NkmdPoit Point;
        public NkmdStag Stage;
        public NkmdKtps KartPointStart;
        public NkmdKtpj KartPointJugem;
        public NkmdKtp2 KartPoint2d;
        public NkmdKtpc KartPointCannon;
        public NkmdKtpm KartPointMission;
        public NkmdCpoi CheckPoint;
        public NkmdCpat CheckPointPath;
        public NkmdIpoi ItemPoint;
        public NkmdIpat ItemPath;
        public NkmdEpoi EnemyPoint;
        public NkmdEpat EnemyPath;
        public NkmdMepo MgEnemyPoint;
        public NkmdMepa MgEnemyPath;
        public NkmdArea Area;
        public NkmdCame Camera;

        public NkmdPoit.PoitEntry GetPoitOnPath(int pathId, int pointIndex)
        {
            if (pathId >= Path.Entries.Count || pointIndex >= Path[pathId].NrPoit)
                return null;
            int start = 0;
            for (int i = 0; i < pathId; i++)
                start += Path[i].NrPoit;

            return Point[start + pointIndex];
        }
    }
}