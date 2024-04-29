using HaroohiePals.IO;
using HaroohiePals.IO.Serialization;
using System;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;

public sealed class G3dShape
{
    public G3dShape(EndianBinaryReaderEx er)
    {
        er.BeginChunk();
        {
            er.ReadObject(this);

            er.JumpRelative(DisplayListOffset);
            DisplayList = er.Read<byte>((int)DisplayListSize);
        }
        er.EndChunk(Size);
    }

    public G3dShape() { }

    public void Write(EndianBinaryWriterEx er)
    {
        DisplayListSize = (uint)DisplayList.Length;
        er.BeginChunk();
        er.WriteObject(this);
        er.EndChunk();
    }

    public ushort ItemTag;

    [ChunkSize]
    public ushort Size;

    [Type(FieldType.U32)]
    public G3dShapeFlags Flags;

    public uint DisplayListOffset;
    public uint DisplayListSize;

    [Ignore]
    public byte[] DisplayList;
}
