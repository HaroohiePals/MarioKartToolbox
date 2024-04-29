using OpenTK.Mathematics;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Animation;

public sealed class JointAnimationResult
{
    public JointAnimationResultFlag Flag;
    public Vector3d Scale;
    public Vector3d ScaleEx0;
    public Vector3d ScaleEx1;
    public Matrix3d Rotation;
    public Vector3d Translation;

    public void Clear()
    {
        Flag = 0;
        Scale = Vector3d.Zero;
        ScaleEx0 = Vector3d.Zero;
        ScaleEx1 = Vector3d.Zero;
        Rotation = new Matrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
        Translation = Vector3d.Zero;
    }
}