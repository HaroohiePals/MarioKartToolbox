using HaroohiePals.IO;
using OpenTK.Mathematics;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary.Animation.JointAnimation;

public sealed class G3dJointAnimationMatrixRotation
{
    public G3dJointAnimationMatrixRotation(EndianBinaryReaderEx reader)
    {
        Data = reader.Read<ushort>(5);
    }

    public ushort[] Data;

    public Matrix3d GetRotationMatrix()
    {
        var rotation = new Matrix3d();
        short _12 = (short)(Data[4] & 7);
        rotation[1, 1] = ((short)Data[4] >> 3) / 4096.0;
        _12 = (short)(_12 << 3 | Data[0] & 7);
        rotation[0, 0] = ((short)Data[0] >> 3) / 4096.0;
        _12 = (short)(_12 << 3 | Data[1] & 7);
        rotation[0, 1] = ((short)Data[1] >> 3) / 4096.0;
        _12 = (short)(_12 << 3 | Data[2] & 7);
        rotation[0, 2] = ((short)Data[2] >> 3) / 4096.0;
        _12 = (short)(_12 << 3 | Data[3] & 7);
        rotation[1, 0] = ((short)Data[3] >> 3) / 4096.0;
        rotation[1, 2] = ((short)(_12 << 3) >> 3) / 4096.0;
        return rotation;
    }
}
