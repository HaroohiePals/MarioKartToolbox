using HaroohiePals.IO;
using HaroohiePals.Nitro.NitroSystem.G3d.Animation;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;
using OpenTK.Mathematics;

namespace HaroohiePals.Nitro.NitroSystem.G3d.CGTool;

public static class Basic
{
    public static void SendJointSrt(JointAnimationResult animationResult, RenderContext context)
    {
        if ((animationResult.Flag & JointAnimationResultFlag.TranslationZero) == 0)
        {
            if ((animationResult.Flag & JointAnimationResultFlag.RotationZero) == 0)
            {
                context.GeState.MultMatrix(new Matrix4x3d(
                    animationResult.Rotation.Row0, animationResult.Rotation.Row1,
                    animationResult.Rotation.Row2, animationResult.Translation));
            }
            else
            {
                context.GeState.Translate(animationResult.Translation);
            }
        }
        else if ((animationResult.Flag & JointAnimationResultFlag.RotationZero) == 0)
        {
            context.GeState.MultMatrix(animationResult.Rotation);
        }

        if ((animationResult.Flag & JointAnimationResultFlag.ScaleOne) == 0)
        {
            context.GeState.Scale(animationResult.Scale);
        }
    }

    public static void GetJointScale(JointAnimationResult animationResult, G3dNodeData nodeData,
        Pointer<byte> sbc, RenderContext context)
    {
        if ((nodeData.Flags & G3dNodeData.FLAGS_SCALE_ONE) != 0)
        {
            animationResult.Flag |= JointAnimationResultFlag.ScaleOne;
        }
        else
        {
            animationResult.Scale = nodeData.Scale;
        }

        animationResult.Flag |= JointAnimationResultFlag.ScaleEx0One | JointAnimationResultFlag.ScaleEx1One;
    }
}