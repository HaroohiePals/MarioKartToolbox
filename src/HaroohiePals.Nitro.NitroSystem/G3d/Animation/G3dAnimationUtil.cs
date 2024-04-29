using OpenTK.Mathematics;
using System;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Animation;

public delegate bool BlendMaterialAnimationFunction(MaterialAnimationResult result,
    G3dAnimationObject animationObject, uint materialId);
public delegate bool BlendJointAnimationFunction(RenderContext context,
    JointAnimationResult result, G3dAnimationObject animationObject, uint nodeId);
public delegate bool BlendVisibilityAnimationFunction(VisibilityAnimationResult result,
    G3dAnimationObject animationObject, uint nodeId);
public delegate void AnimateMaterialFunction(MaterialAnimationResult result,
    G3dAnimationObject animationObject, uint materialId);
public delegate void AnimateJointFunction(RenderContext context, JointAnimationResult result,
    G3dAnimationObject animationObject, uint nodeId);
public delegate void AnimateVisiblityFunction(VisibilityAnimationResult result,
    G3dAnimationObject animationObject, uint nodeId);

public static class G3dAnimationUtil
{
    public static bool BlendMaterialAnimation(MaterialAnimationResult result,
        G3dAnimationObject animationObject, uint matId)
    {
        if (animationObject is null)
        {
            return false;
        }

        bool returnValue = false;
        for (var p = animationObject; p is not null; p = p.Next)
        {
            uint dataIdx = p.MapData[matId];
            if ((dataIdx & G3dAnimationObject.MapDataExist) == 0)
            {
                continue;
            }

            var function = (AnimateMaterialFunction)p.AnimationFunction;
            if (function is not null)
            {
                function(result, p, dataIdx & G3dAnimationObject.MapDataDatafield);
                returnValue = true;
            }
        }

        return returnValue;
    }

    private static void BlendScaleVector(ref Vector3d v0, in Vector3d v1, double ratio, bool isV1One)
    {
        if (isV1One)
        {
            v0 += (ratio, ratio, ratio);
        }
        else
        {
            v0 += ratio * v1;
        }
    }

    public static bool BlendJointAnimation(RenderContext context, JointAnimationResult result,
        G3dAnimationObject animationObject, uint nodeId)
    {
        if (animationObject is null)
            return false;

        if (animationObject.Next is null)
        {
            return BlendJointAnimationSingle(context, result, animationObject, nodeId);
        }
        else
        {
            return BlendJointAnimationMulti(context, result, animationObject, nodeId);
        }
    }

    private static bool BlendJointAnimationSingle(RenderContext context, JointAnimationResult result,
        G3dAnimationObject animationObject, uint nodeId)
    {
        if (nodeId >= animationObject.MapData.Length)
        {
            return false;
        }

        uint dataIdx = animationObject.MapData[nodeId];
        if ((dataIdx & (G3dAnimationObject.MapDataExist | G3dAnimationObject.MapDataDisabled))
                == G3dAnimationObject.MapDataExist)
        {
            var function = (AnimateJointFunction)animationObject.AnimationFunction;
            if (function is not null)
            {
                function(context, result, animationObject, dataIdx & G3dAnimationObject.MapDataDatafield);
                return true;
            }
        }

        return false;
    }

    private static bool BlendJointAnimationMulti(RenderContext context, JointAnimationResult jntAnmResult,
        G3dAnimationObject animationObject, uint nodeId)
    {
        G3dAnimationObject lastAnmObj = null;
        double sumOfRatio = 0;
        int numBlend = 0;
        for (var p = animationObject; p is not null; p = p.Next)
        {
            if (nodeId < p.MapData.Length &&
                (p.MapData[nodeId] & (G3dAnimationObject.MapDataExist | G3dAnimationObject.MapDataDisabled))
                    == G3dAnimationObject.MapDataExist)
            {
                sumOfRatio += Math.Clamp(p.Ratio, 0, 1);
                lastAnmObj = p;
                numBlend++;
            }
        }

        if (sumOfRatio == 0)
        {
            return false;
        }

        if (numBlend == 1)
        {
            var function = (AnimateJointFunction)lastAnmObj.AnimationFunction;
            if (function is null)
            {
                return false;
            }

            uint dataIdx = lastAnmObj.MapData[nodeId];
            function(context, jntAnmResult, lastAnmObj, dataIdx & G3dAnimationObject.MapDataDatafield);
            return true;
        }

        BlendJointAnimationMultiCalculateResult(context, jntAnmResult, animationObject, nodeId, sumOfRatio);

        return true;
    }

    private static void BlendJointAnimationMultiCalculateResult(RenderContext context, JointAnimationResult endResult,
        G3dAnimationObject animationObject, uint nodeId, double sumOfRatio)
    {
        var partialResult = new JointAnimationResult();
        var keepAxisX = Vector3d.Zero;
        var keepAxisZ = Vector3d.Zero;

        endResult.Clear();
        endResult.Flag = (JointAnimationResultFlag)(-1);

        int i1 = 0;
        for (var p = animationObject; p is not null; p = p.Next, i1++)
        {
            if (nodeId >= p.MapData.Length)
            {
                continue;
            }

            uint dataIdx = p.MapData[nodeId];
            if ((dataIdx & (G3dAnimationObject.MapDataExist | G3dAnimationObject.MapDataDisabled))
                    != G3dAnimationObject.MapDataExist ||
                p.Ratio <= 0)
            {
                continue;
            }

            var function = (AnimateJointFunction)p.AnimationFunction;
            if (function is null)
            {
                continue;
            }

            function(context, partialResult, p, dataIdx & G3dAnimationObject.MapDataDatafield);

            if (i1 == 0)
            {
                keepAxisX = partialResult.Rotation.Row0;
                keepAxisZ = partialResult.Rotation.Row2;
            }

            BlendJointAnimationMultiAddToResult(endResult, partialResult, p.Ratio / sumOfRatio);
        }

        BlendJointAnimationMultiCompleteMatrix(endResult, keepAxisX, keepAxisZ);
    }

    private static void BlendJointAnimationMultiAddToResult(JointAnimationResult dstJntAnmResult,
        JointAnimationResult srcJntAnmResult, double ratio)
    {
        BlendScaleVector(ref dstJntAnmResult.Scale, srcJntAnmResult.Scale, ratio,
            (srcJntAnmResult.Flag & JointAnimationResultFlag.ScaleOne) != 0);
        BlendScaleVector(ref dstJntAnmResult.ScaleEx0, srcJntAnmResult.ScaleEx0, ratio,
            (srcJntAnmResult.Flag & JointAnimationResultFlag.ScaleEx0One) != 0);

        BlendScaleVector(ref dstJntAnmResult.ScaleEx1, srcJntAnmResult.ScaleEx1, ratio,
            (srcJntAnmResult.Flag & JointAnimationResultFlag.ScaleEx1One) != 0);

        if ((srcJntAnmResult.Flag & JointAnimationResultFlag.TranslationZero) == 0)
        {
            dstJntAnmResult.Translation += ratio * srcJntAnmResult.Translation;
        }

        if ((srcJntAnmResult.Flag & JointAnimationResultFlag.RotationZero) == 0)
        {
            dstJntAnmResult.Rotation.Row0 += ratio * srcJntAnmResult.Rotation.Row0;
            dstJntAnmResult.Rotation.Row1 += ratio * srcJntAnmResult.Rotation.Row1;
        }
        else
        {
            dstJntAnmResult.Rotation[0, 0] += ratio;
            dstJntAnmResult.Rotation[1, 1] += ratio;
        }

        dstJntAnmResult.Flag &= srcJntAnmResult.Flag;
    }

    private static void BlendJointAnimationMultiCompleteMatrix(JointAnimationResult jntAnmResult,
        Vector3d keepAxisX, Vector3d keepAxisZ)
    {
        jntAnmResult.Rotation.Row2 = Vector3d.Cross(jntAnmResult.Rotation.Row0, jntAnmResult.Rotation.Row1);

        if (jntAnmResult.Rotation[0, 0] == 0 && jntAnmResult.Rotation[0, 1] == 0 && jntAnmResult.Rotation[0, 2] == 0)
        {
            jntAnmResult.Rotation.Row0 = keepAxisX;
        }
        else
        {
            jntAnmResult.Rotation.Row0 = jntAnmResult.Rotation.Row0.Normalized();
        }

        if (jntAnmResult.Rotation[2, 0] == 0 && jntAnmResult.Rotation[2, 1] == 0 && jntAnmResult.Rotation[2, 2] == 0)
        {
            jntAnmResult.Rotation.Row2 = keepAxisZ;
        }
        else
        {
            jntAnmResult.Rotation.Row2 = jntAnmResult.Rotation.Row2.Normalized();
        }

        jntAnmResult.Rotation.Row1 = Vector3d.Cross(jntAnmResult.Rotation.Row2, jntAnmResult.Rotation.Row0);
    }

    public static double CalculateAnimationValueFrame(uint frame, double[] data, AnimationStep step, uint lastInterp)
        => step switch
    {
        AnimationStep.Step1 => data[frame],
        AnimationStep.Step2 => CalculareAnimationValueFrameStep2(frame, data, lastInterp),
        AnimationStep.Step4 => CalculateAnimationValueFrameStep4(frame, data, lastInterp),
        _ => throw new ArgumentOutOfRangeException(nameof(step), step, null),
    };

    private static double CalculareAnimationValueFrameStep2(uint frame, double[] data, uint lastInterp)
    {
        if ((frame & 1) == 0)
        {
            return data[frame >> 1];
        }

        if (frame > lastInterp)
        {
            return data[(lastInterp >> 1) + 1];
        }

        return (data[frame >> 1] + data[(frame >> 1) + 1]) / 2.0;
    }

    private static double CalculateAnimationValueFrameStep4(uint frame, double[] data, uint lastInterp)
    {
        if ((frame & 3) == 0)
        {
            return data[frame >> 2];
        }

        if (frame > lastInterp)
        {
            return data[(lastInterp >> 2) + (frame & 3)];
        }

        if ((frame & 1) == 0)
        {
            return (data[frame >> 2] + data[(frame >> 2) + 1]) / 2.0;
        }

        double v0, v3;
        if ((frame & 2) != 0)
        {
            v3 = data[frame >> 2];
            v0 = data[(frame >> 2) + 1];
        }
        else
        {
            v0 = data[frame >> 2];
            v3 = data[(frame >> 2) + 1];
        }

        return (3 * v0 + v3) / 4.0;
    }

    public static double CalculateAnimationValueLinear(double frame, double[] data, AnimationStep step,
        uint lastInterp, uint nrFrames, bool endToStart)
    {
        uint intFrame = (uint)Math.Floor(frame);
        if (intFrame == nrFrames - 1)
        {
            return CalculateAnimationValueLinearLastFrame(frame, data, step, endToStart);
        }

        return step switch
        {
            AnimationStep.Step1 => CalculateAnimationValueLinearStep1(frame, data),
            AnimationStep.Step2 => CalculateAnimationValueLinearStep2(frame, data, lastInterp),
            AnimationStep.Step4 => CalculateAnimationValueLinearStep4(frame, data, lastInterp),
            _ => throw new ArgumentOutOfRangeException(nameof(step), step, null)
        };
    }

    private static double CalculateAnimationValueLinearStep1(double frame, double[] data)
    {
        uint intFrame = (uint)Math.Floor(frame);
        double v0 = data[intFrame];
        double v1 = data[intFrame + 1];

        return v0 + (v1 - v0) * (frame % 1.0);
    }

    private static double CalculateAnimationValueLinearStep2(double frame, double[] data, uint lastInterp)
    {
        uint intFrame = (uint)Math.Floor(frame);
        if (intFrame >= lastInterp)
        {
            double v0 = data[lastInterp >> 1];
            double v1 = data[(lastInterp >> 1) + 1];

            return v0 + (v1 - v0) * (frame % 1.0);
        }
        else
        {
            double v0 = data[intFrame >> 1];
            double v1 = data[(intFrame >> 1) + 1];

            return (v0 * 2.0 + (v1 - v0) * (frame % 2.0)) / 2.0;
        }
    }

    private static double CalculateAnimationValueLinearStep4(double frame, double[] data, uint lastInterp)
    {
        uint intFrame = (uint)Math.Floor(frame);
        if (intFrame >= lastInterp)
        {
            uint idx = (intFrame >> 2) + (intFrame & 3);
            double v0 = data[idx];
            double v1 = data[idx + 1];

            return v0 + (v1 - v0) * (frame % 1.0);
        }
        else
        {
            double v0 = data[intFrame >> 2];
            double v1 = data[(intFrame >> 2) + 1];

            return (v0 * 4.0 + (v1 - v0) * (frame % 4.0)) / 4.0;
        }
    }

    private static double CalculateAnimationValueLinearLastFrame(double frame, double[] data,
        AnimationStep step, bool endToStart)
    {
        uint intFrame = (uint)Math.Floor(frame);
        uint idx = step switch
        {
            AnimationStep.Step1 => intFrame,
            AnimationStep.Step2 => (intFrame >> 1) + (intFrame & 1),
            AnimationStep.Step4 => (intFrame >> 2) + (intFrame & 3),
            _ => throw new ArgumentOutOfRangeException(nameof(step), step, null)
        };

        if (endToStart)
        {
            double remainder = frame % 1.0;
            double v0 = data[idx];
            double v1 = data[0];

            return v0 + (v1 - v0) * remainder;
        }

        return data[idx];
    }
}