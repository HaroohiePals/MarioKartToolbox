using HaroohiePals.IO;
using HaroohiePals.Nitro.NitroSystem.G3d.Animation;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;
using OpenTK.Mathematics;
using System;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary.Animation.JointAnimation;

public sealed class G3dJointAnimation : G3dAnimation
{
    public G3dJointAnimation(EndianBinaryReaderEx reader)
    {
        reader.BeginChunk();
        Header = new G3dAnimationHeader(reader, 'J', "AC");
        NrFrames = reader.Read<ushort>();
        NrNodes = reader.Read<ushort>();
        AnimationFlags = (G3dJointAnimationFlags)reader.Read<uint>();
        uint pivotRotationsOffset = reader.Read<uint>();
        uint matrixRotationsOffset = reader.Read<uint>();
        var animationNodeOffsets = reader.Read<ushort>(NrNodes);
        reader.ReadPadding(4);
        long curPos = reader.BaseStream.Position;
        AnimationNodes = new G3dJointAnimationNode[NrNodes];
        int rot3MaxIdx = -1;
        int rot5MaxIdx = -1;
        for (int i = 0; i < NrNodes; i++)
        {
            reader.JumpRelative(animationNodeOffsets[i]);
            AnimationNodes[i] = new G3dJointAnimationNode(reader, NrFrames);

            if (AnimationNodes[i].Rotation is null)
            {
                continue;
            }

            if ((AnimationNodes[i].Flags & G3dJointAnimationNode.FLAGS_CONST_ROTATION) != 0)
            {
                uint dataIndex = AnimationNodes[i].Rotation.ConstIndex & G3dJointAnimationRotation.INDEX_DATA_MASK;
                if ((AnimationNodes[i].Rotation.ConstIndex & G3dJointAnimationRotation.INDEX_PIVOT) != 0)
                {
                    rot3MaxIdx = (int)Math.Max(rot3MaxIdx, dataIndex);
                }
                else
                {
                    rot5MaxIdx = (int)Math.Max(rot5MaxIdx, dataIndex);
                }
            }
            else
            {
                foreach (ushort index in AnimationNodes[i].Rotation.Indices)
                {
                    uint dataIndex = index & G3dJointAnimationRotation.INDEX_DATA_MASK;
                    if ((index & G3dJointAnimationRotation.INDEX_PIVOT) != 0)
                    {
                        rot3MaxIdx = (int)Math.Max(rot3MaxIdx, dataIndex);
                    }
                    else
                    {
                        rot5MaxIdx = (int)Math.Max(rot5MaxIdx, dataIndex);
                    }
                }
            }
        }

        PivotRotations = new G3dJointAnimationPivotRotation[rot3MaxIdx + 1];
        reader.JumpRelative(pivotRotationsOffset);
        for (int i = 0; i < rot3MaxIdx + 1; i++)
        {
            PivotRotations[i] = new G3dJointAnimationPivotRotation(reader);
        }

        MatrixRotations = new G3dJointAnimationMatrixRotation[rot5MaxIdx + 1];
        reader.JumpRelative(matrixRotationsOffset);
        for (int i = 0; i < rot5MaxIdx + 1; i++)
        {
            MatrixRotations[i] = new G3dJointAnimationMatrixRotation(reader);
        }

        reader.BaseStream.Position = curPos;
        reader.EndChunk();
    }

    public ushort NrNodes;
    public G3dJointAnimationFlags AnimationFlags;

    public G3dJointAnimationNode[] AnimationNodes;
    public G3dJointAnimationPivotRotation[] PivotRotations;
    public G3dJointAnimationMatrixRotation[] MatrixRotations;

    public override void InitializeAnimationObject(G3dAnimationObject animationObject, G3dModel model)
    {
        animationObject.AnimationFunction = (AnimateJointFunction)CalculateNsbcaAnimation;
        animationObject.MapData = new ushort[model.Info.NodeCount];

        for (int i = 0; i < NrNodes; i++)
        {
            animationObject.MapData[i] = (ushort)(
                (AnimationNodes[i].Flags & G3dJointAnimationNode.FLAGS_NODE_MASK) >> G3dJointAnimationNode.FLAGS_NODE_SHIFT |
                G3dAnimationObject.MapDataExist);
        }
    }

    public static void CalculateNsbcaAnimation(RenderContext context, JointAnimationResult result,
        G3dAnimationObject animationObject, uint dataIndex)
    {
        var animation = (G3dJointAnimation)animationObject.AnimationResource;
        double frame = Math.Clamp(animationObject.Frame, 0, animation.NrFrames - 1 / 4096.0);

        animation.GetJointSrtAnimation(context, (ushort)dataIndex, frame, result);
    }

    private bool GetRotationDataByIndex(out Matrix3d rotation, uint info)
    {
        uint index = info & G3dJointAnimationRotation.INDEX_DATA_MASK;
        if ((info & G3dJointAnimationRotation.INDEX_PIVOT) != 0)
        {
            rotation = PivotRotations[index].GetRotationMatrix();
            return false;
        }
        else
        {
            rotation = MatrixRotations[index].GetRotationMatrix();
            return true;
        }
    }

    private Matrix3d GetRotationDataFrame(double frame, G3dJointAnimationRotation data)
    {
        uint intFrame = (uint)Math.Floor(frame);
        if (data.Step == AnimationStep.Step1)
        {
            return GetRotationDataFrameNoInterpolation(data, intFrame);
        }
        else if (data.Step == AnimationStep.Step2)
        {
            if ((intFrame & 1) == 0)
            {
                return GetRotationDataFrameNoInterpolation(data, intFrame >> 1);
            }
            else if (intFrame > data.LastInterpolationFrame)
            {
                return GetRotationDataFrameNoInterpolation(data, (data.LastInterpolationFrame >> 1) + 1);
            }
            else
            {
                return GetRotationDataFrameInterpolate2(data, intFrame >> 1);
            }
        }
        else
        {
            if ((intFrame & 3) == 0)
            {
                return GetRotationDataFrameNoInterpolation(data, intFrame >> 2);
            }
            else if (intFrame > data.LastInterpolationFrame)
            {
                return GetRotationDataFrameNoInterpolation(data, (data.LastInterpolationFrame >> 2) + (intFrame & 3));
            }
            else if ((intFrame & 1) == 0)
            {
                return GetRotationDataFrameInterpolate2(data, intFrame >> 2);
            }

            bool doCross = false;
            uint idx, idxSub;
            if ((intFrame & 2) != 0)
            {
                idxSub = intFrame >> 2;
                idx = idxSub + 1;
            }
            else
            {
                idx = intFrame >> 2;
                idxSub = idx + 1;
            }

            doCross |= GetRotationDataByIndex(out var rotation0, data.Indices[idx]);
            doCross |= GetRotationDataByIndex(out var rotation1, data.Indices[idxSub]);

            rotation0.Row0 = (rotation0.Row0 * 3 + rotation1.Row0).Normalized();
            rotation0.Row1 = (rotation0.Row1 * 3 + rotation1.Row1).Normalized();

            if (!doCross)
            {
                rotation0.Row2 = (rotation0.Row2 * 3 + rotation1.Row2).Normalized();
            }
            else
            {
                rotation0.Row2 = Vector3d.Cross(rotation0.Row0, rotation0.Row1);
            }

            return rotation0;
        }
    }

    private Matrix3d GetRotationDataFrameNoInterpolation(G3dJointAnimationRotation data, uint idx)
    {
        if (GetRotationDataByIndex(out var rotation, data.Indices[idx]))
        {
            rotation.Row2 = Vector3d.Cross(rotation.Row0, rotation.Row1);
        }
        else
        {
            rotation.Row2 = rotation.Row2.Normalized();
        }

        return rotation;
    }

    private Matrix3d GetRotationDataFrameInterpolate2(G3dJointAnimationRotation data, uint idx)
    {
        bool doCross = false;
        doCross |= GetRotationDataByIndex(out var rotation, data.Indices[idx]);
        doCross |= GetRotationDataByIndex(out var tmp, data.Indices[idx + 1]);
        rotation.Row0 = (rotation.Row0 + tmp.Row0).Normalized();
        rotation.Row1 = (rotation.Row1 + tmp.Row1).Normalized();

        if (!doCross)
        {
            rotation.Row2 = (rotation.Row2 + tmp.Row2).Normalized();
        }
        else
        {
            rotation.Row2 = Vector3d.Cross(rotation.Row0, rotation.Row1);
        }

        return rotation;
    }

    private Matrix3d GetRotationDataLinear(double frame, G3dJointAnimationRotation data)
    {
        Matrix3d rotation;
        uint idx0, idx1;
        int step;

        uint intFrame = (uint)Math.Floor(frame);
        if (intFrame == NrFrames - 1)
        {
            if (data.Step == AnimationStep.Step1)
                idx0 = intFrame;
            else if (data.Step == AnimationStep.Step2)
                idx0 = (intFrame >> 1) + (intFrame & 1);
            else
                idx0 = (intFrame >> 2) + (intFrame & 3);

            if ((AnimationFlags & G3dJointAnimationFlags.EndToStartInterpolation) == 0)
            {
                if (GetRotationDataByIndex(out rotation, data.Indices[idx0]))
                    rotation.Row2 = Vector3d.Cross(rotation.Row0, rotation.Row1);
                else
                    rotation.Row2 = rotation.Row2.Normalized();

                return rotation;
            }

            idx1 = 0;
            step = 1;
        }
        else if (data.Step == AnimationStep.Step1)
        {
            idx0 = intFrame;
            idx1 = idx0 + 1;
            step = 1;
        }
        else if (data.Step == AnimationStep.Step2)
        {
            if (intFrame >= data.LastInterpolationFrame)
            {
                idx0 = data.LastInterpolationFrame >> 1;
                idx1 = idx0 + 1;
                step = 1;
            }
            else
            {
                idx0 = intFrame >> 1;
                idx1 = idx0 + 1;
                step = 2;
            }
        }
        else
        {
            if (intFrame >= data.LastInterpolationFrame)
            {
                idx0 = (intFrame >> 2) + (intFrame & 3);
                idx1 = idx0 + 1;
                step = 1;
            }
            else
            {
                idx0 = intFrame >> 2;
                idx1 = idx0 + 1;
                step = 4;
            }
        }

        bool doCross = false;
        doCross |= GetRotationDataByIndex(out var r0, data.Indices[idx0]);
        doCross |= GetRotationDataByIndex(out var r1, data.Indices[idx1]);

        double remainder = frame % step;
        rotation = new Matrix3d
        {
            Row0 = (r0.Row0 * step + (r1.Row0 - r0.Row0) * remainder).Normalized(),
            Row1 = (r0.Row1 * step + (r1.Row1 - r0.Row1) * remainder).Normalized()
        };

        if (!doCross)
        {
            rotation.Row2 = (r0.Row2 * step + (r1.Row2 - r0.Row2) * remainder).Normalized();
        }
        else
        {
            rotation.Row2 = Vector3d.Cross(rotation.Row0, rotation.Row1);
        }

        return rotation;
    }

    private void GetJointSrtAnimation(RenderContext context, uint dataIndex, double frame, JointAnimationResult jointAnimationResult)
    {
        var scale = Vector3d.Zero;
        var inverseScale = Vector3d.Zero;

        var animationNode = AnimationNodes[dataIndex];
        uint flags = animationNode.Flags;

        if ((flags & G3dJointAnimationNode.FLAGS_IDENTITY) != 0)
        {
            jointAnimationResult.Flag = JointAnimationResultFlag.ScaleOne
                        | JointAnimationResultFlag.RotationZero
                        | JointAnimationResultFlag.TranslationZero;
        }
        else
        {
            uint idxNode = context.RenderState.SbcData[1];
            bool isDecimalFrame = frame % 1.0 == 0 && (AnimationFlags & G3dJointAnimationFlags.Interpolation) != 0;

            jointAnimationResult.Flag = 0;

            if ((flags & (G3dJointAnimationNode.FLAGS_IDENTITY_TRANSLATION | G3dJointAnimationNode.FLAGS_BASE_TRANSLATION)) == 0)
            {
                jointAnimationResult.Translation.X = CalculateTranslation(frame, animationNode.TranslationX,
                    (flags & G3dJointAnimationNode.FLAGS_CONST_TRANSLATION_X) != 0, isDecimalFrame);
                jointAnimationResult.Translation.Y = CalculateTranslation(frame, animationNode.TranslationY,
                    (flags & G3dJointAnimationNode.FLAGS_CONST_TRANSLATION_Y) != 0, isDecimalFrame);
                jointAnimationResult.Translation.Z = CalculateTranslation(frame, animationNode.TranslationZ,
                    (flags & G3dJointAnimationNode.FLAGS_CONST_TRANSLATION_Z) != 0, isDecimalFrame);
            }
            else if ((flags & G3dJointAnimationNode.FLAGS_IDENTITY_TRANSLATION) != 0)
            {
                jointAnimationResult.Flag |= JointAnimationResultFlag.TranslationZero;
            }
            else
            {
                context.RenderState.NodeResource.Data[idxNode].GetTranslation(jointAnimationResult);
            }

            if ((flags & (G3dJointAnimationNode.FLAGS_IDENTITY_ROTATION | G3dJointAnimationNode.FLAGS_BASE_ROTATION)) == 0)
            {
                if ((flags & G3dJointAnimationNode.FLAGS_CONST_ROTATION) == 0)
                {
                    if (isDecimalFrame)
                        jointAnimationResult.Rotation = GetRotationDataLinear(frame, animationNode.Rotation);
                    else
                        jointAnimationResult.Rotation = GetRotationDataFrame(frame, animationNode.Rotation);
                }
                else
                {
                    if (GetRotationDataByIndex(out jointAnimationResult.Rotation, animationNode.Rotation.ConstIndex))
                        jointAnimationResult.Rotation.Row2 = Vector3d.Cross(jointAnimationResult.Rotation.Row0, jointAnimationResult.Rotation.Row1);
                }
            }
            else if ((flags & G3dJointAnimationNode.FLAGS_IDENTITY_ROTATION) != 0)
            {
                jointAnimationResult.Flag |= JointAnimationResultFlag.RotationZero;
            }
            else
            {
                context.RenderState.NodeResource.Data[idxNode].GetRotation(jointAnimationResult);
            }

            if ((flags & (G3dJointAnimationNode.FLAGS_IDENTITY_SCALE | G3dJointAnimationNode.FLAGS_BASE_SCALE)) == 0)
            {
                (scale.X, inverseScale.X) = CalculateScale(frame, animationNode.ScaleX,
                    (flags & G3dJointAnimationNode.FLAGS_CONST_SCALE_X) != 0, isDecimalFrame);
                (scale.Y, inverseScale.Y) = CalculateScale(frame, animationNode.ScaleY,
                    (flags & G3dJointAnimationNode.FLAGS_CONST_SCALE_Y) != 0, isDecimalFrame);
                (scale.Z, inverseScale.Z) = CalculateScale(frame, animationNode.ScaleZ,
                    (flags & G3dJointAnimationNode.FLAGS_CONST_SCALE_Z) != 0, isDecimalFrame);
            }
            else if ((flags & G3dJointAnimationNode.FLAGS_IDENTITY_SCALE) != 0)
            {
                jointAnimationResult.Flag |= JointAnimationResultFlag.ScaleOne;
            }
            else
            {
                context.RenderState.GetJointScale(jointAnimationResult, context.RenderState.NodeResource.Data[idxNode],
                    context.RenderState.SbcData, context);
                return;
            }
        }

        var nodeData = new G3dNodeData
        {
            Flags = (jointAnimationResult.Flag & JointAnimationResultFlag.ScaleOne) != 0
                ? G3dNodeData.FLAGS_SCALE_ONE : (ushort)0,
            Scale = scale,
            InverseScale = inverseScale
        };

        context.RenderState.GetJointScale(jointAnimationResult, nodeData, context.RenderState.SbcData, context);
    }

    private double CalculateTranslation(double frame,
        G3dJointAnimationTranslation translationAnimation, bool isConstant, bool isDecimalFrame)
    {
        if (isConstant)
        {
            return translationAnimation.ConstTranslation;
        }
        else if (isDecimalFrame)
        {
            return G3dAnimationUtil.CalculateAnimationValueLinear(
                frame, translationAnimation.Translation, translationAnimation.Step,
                translationAnimation.LastInterpolationFrame, NrFrames,
                (AnimationFlags & G3dJointAnimationFlags.EndToStartInterpolation) != 0);
        }
        else
        {
            return G3dAnimationUtil.CalculateAnimationValueFrame((uint)Math.Floor(frame),
                translationAnimation.Translation, translationAnimation.Step, translationAnimation.LastInterpolationFrame);
        }
    }

    private (double scale, double inverseScale) CalculateScale(double frame,
        G3dJointAnimationScale scaleAnimation, bool isConstant, bool isDecimalFrame)
    {
        if (isConstant)
        {
            return (scaleAnimation.ConstScale, scaleAnimation.ConstInverseScale);
        }
        else if (isDecimalFrame)
        {
            return (
                G3dAnimationUtil.CalculateAnimationValueLinear(
                    frame, scaleAnimation.Scale, scaleAnimation.Step, scaleAnimation.LastInterpolationFrame,
                    NrFrames, (AnimationFlags & G3dJointAnimationFlags.EndToStartInterpolation) != 0),
                G3dAnimationUtil.CalculateAnimationValueLinear(
                    frame, scaleAnimation.InverseScale, scaleAnimation.Step, scaleAnimation.LastInterpolationFrame,
                    NrFrames, (AnimationFlags & G3dJointAnimationFlags.EndToStartInterpolation) != 0));
        }
        else
        {
            uint intFrame = (uint)Math.Floor(frame);
            return (
                G3dAnimationUtil.CalculateAnimationValueFrame(
                    intFrame, scaleAnimation.Scale, scaleAnimation.Step, scaleAnimation.LastInterpolationFrame),
                G3dAnimationUtil.CalculateAnimationValueFrame(
                    intFrame, scaleAnimation.InverseScale, scaleAnimation.Step, scaleAnimation.LastInterpolationFrame));
        }
    }
}
