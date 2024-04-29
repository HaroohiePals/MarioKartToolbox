using HaroohiePals.IO;
using OpenTK.Mathematics;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;

public sealed class G3dEnvelope
{
    public G3dEnvelope() { }

    public G3dEnvelope(EndianBinaryReaderEx er)
    {
        InversePositionMatrix = new Matrix4x3d();
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                InversePositionMatrix[y, x] = er.ReadFx32();
            }
        }

        InverseDirectionMatrix = new Matrix3d();
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                InverseDirectionMatrix[y, x] = er.ReadFx32();
            }
        }
    }

    public void Write(EndianBinaryWriterEx er)
    {
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                er.WriteFx32(InversePositionMatrix[y, x]);
            }
        }

        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                er.WriteFx32(InverseDirectionMatrix[y, x]);
            }
        }
    }

    public Matrix4x3d InversePositionMatrix;
    public Matrix3d InverseDirectionMatrix;
}
