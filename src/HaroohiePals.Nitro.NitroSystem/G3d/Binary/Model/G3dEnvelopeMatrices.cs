using HaroohiePals.IO;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;

public sealed class G3dEnvelopeMatrices
{
    public G3dEnvelopeMatrices() { }

    public G3dEnvelopeMatrices(EndianBinaryReaderEx er, int nodeCount)
    {
        Envelopes = new G3dEnvelope[nodeCount];
        for (int i = 0; i < nodeCount; i++)
        {
            Envelopes[i] = new G3dEnvelope(er);
        }
    }

    public void Write(EndianBinaryWriterEx er)
    {
        foreach (var e in Envelopes)
        {
            e.Write(er);
        }
    }

    public G3dEnvelope[] Envelopes;
}
