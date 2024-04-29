using HaroohiePals.IO;
using HaroohiePals.Nitro.Gx;
using HaroohiePals.Nitro.JNLib.Spl.Emitter;

namespace HaroohiePals.Nitro.JNLib.Spl
{
    public class SplArchive
    {
        public SplArchive()
        {
            Header = new SpaHeader();
        }

        public SplArchive(byte[] data)
        {
            using (var reader = new EndianBinaryReader(new MemoryStream(data), Endianness.LittleEndian))
            {
                Header = new SpaHeader(reader);

                Emitters = new SplEmitter[Header.EmitterCount];
                for (int i = 0; i < Header.EmitterCount; i++)
                    Emitters[i] = new SplEmitter(reader);

                reader.BaseStream.Position = Header.TextureBlockOffset;
                Textures = new Texture[Header.TextureCount];
                for (int i = 0; i < Header.TextureCount; i++)
                    Textures[i] = new Texture(reader);
            }
        }

        public byte[] Write()
        {
            using (var m = new MemoryStream())
            {
                var ew = new EndianBinaryWriterEx(m, Endianness.LittleEndian);
                Header.EmitterCount = (ushort)Emitters.Length;
                Header.TextureCount = (ushort)Textures.Length;
                Header.Write(ew);

                ew.BeginChunk();
                {
                    foreach (var emitter in Emitters)
                        emitter.Write(ew);
                    Header.EmitterBlockLength = (uint)ew.GetCurposRelative();
                }
                ew.EndChunk();

                Header.TextureBlockOffset = (uint)ew.BaseStream.Position;
                ew.BeginChunk();
                {
                    foreach (var tex in Textures)
                        tex.Write(ew);
                    Header.TextureBlockLength = (uint)ew.GetCurposRelative();
                }
                ew.EndChunk();

                m.Position = 0;
                Header.Write(ew);

                ew.Close();
                return m.ToArray();
            }
        }

        public SpaHeader Header { get; }

        public class SpaHeader
        {
            public const uint SPASignature = 0x53504120;
            public const uint SPAVersion = 0x315F3231;

            public SpaHeader()
            {
                Signature = SPASignature;
                Version = SPAVersion;
            }

            public SpaHeader(EndianBinaryReader reader)
            {
                Signature = reader.Read<uint>();
                if (Signature != SPASignature)
                    throw new SignatureNotCorrectException(Signature, SPASignature, reader.BaseStream.Position - 4);
                Version = reader.Read<uint>();
                if (Version != SPAVersion)
                    throw new SignatureNotCorrectException(Version, SPAVersion, reader.BaseStream.Position - 4);
                EmitterCount = reader.Read<ushort>();
                TextureCount = reader.Read<ushort>();
                Reserved1 = reader.Read<uint>();
                EmitterBlockLength = reader.Read<uint>();
                TextureBlockLength = reader.Read<uint>();
                TextureBlockOffset = reader.Read<uint>();
                Reserved2 = reader.Read<uint>();
            }

            public void Write(EndianBinaryWriterEx ew)
            {
                ew.Write(SPASignature);
                ew.Write(SPAVersion);
                ew.Write(EmitterCount);
                ew.Write(TextureCount);
                ew.Write(0u);
                ew.Write(EmitterBlockLength);
                ew.Write(TextureBlockLength);
                ew.Write(TextureBlockOffset);
                ew.Write(0u);
            }

            public uint Signature { get; set; }
            public uint Version { get; set; }
            public ushort EmitterCount { get; set; }
            public ushort TextureCount { get; set; }
            public uint Reserved1 { get; set; }
            public uint EmitterBlockLength { get; set; }
            public uint TextureBlockLength { get; set; }
            public uint TextureBlockOffset { get; set; }
            public uint Reserved2 { get; set; }
        }

        public SplEmitter[] Emitters { get; set; }

        public Texture[] Textures { get; set; }

        public class Texture
        {
            public const uint SPTSignature = 0x53505420;

            public Texture()
            {
                Signature = SPTSignature;
                Params = new TexParams();
            }

            public Texture(EndianBinaryReader reader)
            {
                long texStart = reader.BaseStream.Position;
                Signature = reader.Read<uint>();
                if (Signature != SPTSignature)
                    throw new SignatureNotCorrectException(Signature, SPTSignature, reader.BaseStream.Position - 4);
                Params = new TexParams(reader);
                TexSize = reader.Read<uint>();
                PlttOffset = reader.Read<uint>();
                PlttSize = reader.Read<uint>();
                PlttIdxOffset = reader.Read<uint>();
                PlttIdxSize = reader.Read<uint>();
                BlockSize = reader.Read<uint>();
                TexData = reader.Read<byte>((int)TexSize);
                reader.BaseStream.Position = texStart + PlttOffset;
                PlttData = reader.Read<ushort>((int)(PlttSize / 2));
                reader.BaseStream.Position = texStart + PlttIdxOffset;
                PlttIdxData = reader.Read<byte>((int)PlttIdxSize);
                reader.BaseStream.Position = texStart + BlockSize;
            }

            public void Write(EndianBinaryWriterEx ew)
            {
                ew.Write(SPTSignature);
                Params.Write(ew);
                uint offset = 32;
                if (TexData?.Length > 0)
                {
                    ew.Write((uint)TexData.Length);
                    offset += (uint)TexData.Length;
                }
                else
                    ew.Write(0u);

                ew.Write(offset);
                if (PlttData?.Length > 0)
                {
                    ew.Write((uint)(PlttData.Length * 2));
                    offset += (uint)(PlttData.Length * 2);
                }
                else
                    ew.Write(0u);

                ew.Write(offset);
                if (PlttIdxData?.Length > 0)
                {
                    ew.Write((uint)PlttIdxData.Length);
                    offset += (uint)PlttIdxData.Length;
                }
                else
                    ew.Write(0u);

                ew.Write(offset);
                if (TexData != null)
                    ew.Write(TexData, 0, TexData.Length);
                if (PlttData != null)
                    ew.Write(PlttData, 0, PlttData.Length);
                if (PlttIdxData != null)
                    ew.Write(PlttIdxData, 0, PlttIdxData.Length);
            }

            public uint Signature { get; set; }
            public TexParams Params { get; }
            public uint TexSize { get; set; }
            public uint PlttOffset { get; set; }
            public uint PlttSize { get; set; }
            public uint PlttIdxOffset { get; set; }
            public uint PlttIdxSize { get; set; }
            public uint BlockSize { get; set; }

            public byte[] TexData { get; set; }
            public ushort[] PlttData { get; set; }
            public byte[] PlttIdxData { get; set; }

            public class TexParams
            {
                public TexParams()
                {
                }

                public TexParams(EndianBinaryReader reader)
                {
                    uint tmp = reader.Read<uint>();
                    Format = (ImageFormat)(tmp & 0xF);
                    Width = (byte)(tmp >> 4 & 0xF);
                    Height = (byte)(tmp >> 8 & 0xF);
                    Repeat = (byte)(tmp >> 12 & 0x3);
                    Flip = (byte)(tmp >> 14 & 0x3);
                    Pltt0Transparent = (tmp >> 16 & 0x1) == 1;
                    RefTexData = (tmp >> 17 & 0x1) == 1;
                    RefTexId = (byte)(tmp >> 18 & 0xFF);
                }

                public void Write(EndianBinaryWriterEx ew)
                {
                    ew.Write(
                        (uint)Format & 0xFu |
                        (Width & 0xFu) << 4 |
                        (Height & 0xFu) << 8 |
                        (Repeat & 0x3u) << 12 |
                        (Flip & 0x3u) << 14 |
                        (Pltt0Transparent ? 1u : 0) << 16 |
                        (RefTexData ? 1u : 0) << 17 |
                        (RefTexId & 0xFFu) << 18);
                }

                public ImageFormat Format { get; set; }
                public byte Width { get; set; }
                public byte Height { get; set; }
                public byte Repeat { get; set; }
                public byte Flip { get; set; }
                public bool Pltt0Transparent { get; set; }
                public bool RefTexData { get; set; }
                public byte RefTexId { get; set; }
            }
        }
    }
}