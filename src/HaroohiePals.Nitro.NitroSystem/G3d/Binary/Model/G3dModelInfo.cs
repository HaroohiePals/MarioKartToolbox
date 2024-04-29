using HaroohiePals.IO;
using HaroohiePals.IO.Serialization;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;

[FieldAlignment(FieldAlignment.FieldSize)]
public sealed class G3dModelInfo
{
    public G3dModelInfo() { }
    public G3dModelInfo(EndianBinaryReaderEx er) => er.ReadObject(this);
    public void Write(EndianBinaryWriterEx er) => er.WriteObject(this);

    public byte SbcType;
    public byte ScalingRule;
    public byte TextureMatrixMode;
    public byte NodeCount;
    public byte MaterialCount;
    public byte ShapeCount;
    public byte FirstUnusedMatrixStackId;

    [Fx32]
    public double PosScale, InversePosScale;

    public ushort VertexCount;
    public ushort PolygonCount;
    public ushort TriangleCount;
    public ushort QuadCount;

    [Fx16]
    public double BoxX, BoxY, BoxZ;

    [Fx16]
    public double BoxW, BoxH, BoxD;

    [Fx32]
    public double BoxPosScale, BoxInversePosScale;
}
