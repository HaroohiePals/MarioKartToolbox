using HaroohiePals.IO;
using System;
using System.Text;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary;

public sealed class G3dAnimationHeader
{
    public G3dAnimationHeader(EndianBinaryReaderEx er, char expectedCategory0, string expectedCategory1)
    {
        Category0 = (char)er.Read<byte>();
        if (Category0 != expectedCategory0)
        {
            throw new SignatureNotCorrectException("" + Category0, "" + expectedCategory0, er.BaseStream.Position - 1);
        }

        Revision = er.Read<byte>();
        Category1 = er.ReadString(Encoding.ASCII, 2);
        if (Category1 != expectedCategory1)
        {
            throw new SignatureNotCorrectException(Category1, expectedCategory1, er.BaseStream.Position - 2);
        }
    }

    public void Write(EndianBinaryWriterEx er)
    {
        er.Write(Category0, Encoding.ASCII);
        er.Write(Revision);
        er.Write(Category1, Encoding.ASCII, false);
    }

    public char Category0;
    public byte Revision;
    public string Category1;
}
