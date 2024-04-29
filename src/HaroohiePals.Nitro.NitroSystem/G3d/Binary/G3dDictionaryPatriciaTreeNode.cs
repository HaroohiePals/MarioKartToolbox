using HaroohiePals.IO;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary;

public sealed class G3dDictionaryPatriciaTreeNode
{
    public G3dDictionaryPatriciaTreeNode() { }
    public G3dDictionaryPatriciaTreeNode(EndianBinaryReaderEx er) => er.ReadObject(this);
    public void Write(EndianBinaryWriterEx er) => er.WriteObject(this);

    public byte ReferenceBit;
    public byte LeftNodeIndex;
    public byte RightNodeIndex;
    public byte EntryIndex;
}
