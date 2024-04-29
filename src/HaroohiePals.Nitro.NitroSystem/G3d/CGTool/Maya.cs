using HaroohiePals.IO;
using HaroohiePals.Nitro.G3;
using HaroohiePals.Nitro.NitroSystem.G3d.Animation;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;
using OpenTK.Mathematics;

namespace HaroohiePals.Nitro.NitroSystem.G3d.CGTool;

public static class Maya
{
    public static void SendJointSrt(JointAnimationResult animationResult, RenderContext context)
    {
        bool trFlag = (animationResult.Flag & JointAnimationResultFlag.TranslationZero) == 0;

        if ((animationResult.Flag & JointAnimationResultFlag.MayaSsc) != 0 &&
            (animationResult.Flag & JointAnimationResultFlag.ScaleEx0One) == 0)
        {
            if (trFlag)
            {
                context.GeState.Translate(animationResult.Translation);
                trFlag = false;
            }

            context.GeState.Scale(animationResult.ScaleEx0);
        }

        if ((animationResult.Flag & JointAnimationResultFlag.RotationZero) == 0)
        {
            if (trFlag)
            {
                context.GeState.MultMatrix(new Matrix4x3d(
                    animationResult.Rotation.Row0, animationResult.Rotation.Row1,
                    animationResult.Rotation.Row2, animationResult.Translation));
            }
            else
            {
                context.GeState.MultMatrix(animationResult.Rotation);
            }
        }
        else if (trFlag)
        {
            context.GeState.Translate(animationResult.Translation);
        }

        if ((animationResult.Flag & JointAnimationResultFlag.ScaleOne) == 0)
        {
            context.GeState.Scale(animationResult.Scale);
        }
    }

    public static void GetJointScale(JointAnimationResult animationResult, G3dNodeData nodeData,
        Pointer<byte> sbc, RenderContext context)
    {
        var opFlag = (Sbc.SbcNodeDescFlag)sbc[3];

        if ((nodeData.Flags & G3dNodeData.FLAGS_SCALE_ONE) != 0)
        {
            animationResult.Flag |= JointAnimationResultFlag.ScaleOne;
            if ((opFlag & Sbc.SbcNodeDescFlag.MayaSscParent) != 0)
            {
                uint nodeId = sbc[1];
                context.RenderState.IsScaleCacheOne[nodeId] = true;
            }
        }
        else
        {
            animationResult.Scale = nodeData.Scale;

            if ((opFlag & Sbc.SbcNodeDescFlag.MayaSscParent) != 0)
            {
                uint nodeId = sbc[1];
                context.RenderState.IsScaleCacheOne[nodeId] = false;
                context.GlobalRenderState.ScaleCache[nodeId].InverseScale = nodeData.InverseScale;
            }
        }

        if ((opFlag & Sbc.SbcNodeDescFlag.MayaSscApply) != 0)
        {
            uint parentId = sbc[2];
            animationResult.Flag |= JointAnimationResultFlag.MayaSsc;

            if (context.RenderState.IsScaleCacheOne[parentId])
                animationResult.Flag |= JointAnimationResultFlag.ScaleEx0One;
            else
                animationResult.ScaleEx0 = context.GlobalRenderState.ScaleCache[parentId].InverseScale;
        }

        animationResult.Flag |= JointAnimationResultFlag.ScaleEx1One;
    }

    public static void SendTextureSrt(MaterialAnimationResult animationResult, RenderContext context)
    {
        var mtx = new Matrix4d();
        mtx[3, 3] = 1;
        CalculateTextureMatrix(ref mtx, animationResult);

        mtx[0, 0] *= animationResult.MagW;
        mtx[0, 1] *= animationResult.MagW;
        mtx[3, 0] *= animationResult.MagW;

        mtx[1, 0] *= animationResult.MagH;
        mtx[1, 1] *= animationResult.MagH;
        mtx[3, 1] *= animationResult.MagH;

        context.GeState.MatrixMode = GxMtxMode.Texture;
        if ((animationResult.Flag & MaterialAnimationResultFlag.TextureMatrixSet) != 0)
        {
            context.GeState.LoadMatrix(mtx);
        }
        else
        {
            context.GeState.MultMatrix(mtx);
        }
        context.GeState.MatrixMode = GxMtxMode.PositionVector;
    }

    private static void CalculateTextureMatrix(ref Matrix4d matrix, MaterialAnimationResult animationResult)
    {
        double scaleS = (animationResult.Flag & MaterialAnimationResultFlag.TextureMatrixScaleOne) != 0
            ? 1.0 : animationResult.ScaleS;
        double scaleT = (animationResult.Flag & MaterialAnimationResultFlag.TextureMatrixScaleOne) != 0
            ? 1.0 : animationResult.ScaleT;

        double sinR = (animationResult.Flag & MaterialAnimationResultFlag.TextureMatrixRotationZero) != 0
            ? 0.0 : animationResult.RotationSin;
        double cosR = (animationResult.Flag & MaterialAnimationResultFlag.TextureMatrixRotationZero) != 0
            ? 1.0 : animationResult.RotationCos;

        double transS = (animationResult.Flag & MaterialAnimationResultFlag.TextureMatrixTranslationZero) != 0
            ? 0.0 : animationResult.TranslationS;
        double transT = (animationResult.Flag & MaterialAnimationResultFlag.TextureMatrixTranslationZero) != 0
            ? 0.0 : animationResult.TranslationT;

        double ssSin = scaleS * sinR;
        double ssCos = scaleS * cosR;
        double stSin = scaleT * sinR;
        double stCos = scaleT * cosR;

        matrix[0, 0] = ssCos;
        matrix[1, 1] = stCos;

        matrix[0, 1] = -stSin * ((double)animationResult.OriginalHeight / (double)animationResult.OriginalWidth);

        matrix[3, 0] = (((-ssSin - ssCos + scaleS) * animationResult.OriginalWidth * 8f) -
                     scaleS * transS * 16f * animationResult.OriginalWidth) / 16f;
        matrix[3, 1] = (((stSin - stCos - scaleT + 2f) * animationResult.OriginalHeight * 8f) +
                     scaleT * transT * 16f * animationResult.OriginalHeight) / 16f;

        matrix[1, 0] = ssSin * ((double)animationResult.OriginalWidth / (double)animationResult.OriginalHeight);
    }
}