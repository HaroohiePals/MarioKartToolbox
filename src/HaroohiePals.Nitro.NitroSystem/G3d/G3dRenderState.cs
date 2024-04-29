using HaroohiePals.IO;
using HaroohiePals.Nitro.NitroSystem.G3d.Animation;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;
using System;

namespace HaroohiePals.Nitro.NitroSystem.G3d;

public sealed class G3dRenderState
{
    private readonly SbcCallbackFunction[] _callbackFunctions = new SbcCallbackFunction[Sbc.CommandNum];
    private readonly SbcCallbackTiming[] _callbackTimings = new SbcCallbackTiming[Sbc.CommandNum];

    public Pointer<byte> SbcData;

    public G3dRenderObject RenderObject;
    public G3dRenderStateFlag Flag;

    public byte CurrentNode;
    public byte CurrentMaterial;
    public byte CurrentNodeDescription;

    public MaterialAnimationResult MaterialAnimation;
    public JointAnimationResult JointAnimation;
    public VisibilityAnimationResult VisibilityAnimation;

    public readonly bool[] IsMaterialCached = new bool[G3dConfig.MaxMaterialCount];
    public readonly bool[] IsScaleCacheOne = new bool[G3dConfig.MaxJointCount];
    public readonly bool[] IsEnvelopeCached = new bool[G3dConfig.MaxJointCount];

    public G3dNodeSet NodeResource;
    public G3dMaterialSet MaterialResource;
    public G3dShapeSet ShapeResource;
    public double PosScale;
    public double InversePosScale;
    public GetJointScaleFunction GetJointScale;
    public SendJointSrtFunction SendJointSrt;
    public SendTextureSrtFunction SendTexSrt;

    public readonly MaterialAnimationResult TmpMatAnmResult = new();
    public readonly JointAnimationResult TmpJntAnmResult = new();
    public readonly VisibilityAnimationResult TmpVisAnmResult = new();

    public void Clear()
    {
        SbcData = null;
        RenderObject = null;
        Flag = 0;
        Array.Clear(_callbackFunctions, 0, _callbackFunctions.Length);
        Array.Clear(_callbackTimings, 0, _callbackTimings.Length);
        CurrentNode = 0;
        CurrentMaterial = 0;
        CurrentNodeDescription = 0;
        MaterialAnimation = null;
        JointAnimation = null;
        VisibilityAnimation = null;
        Array.Clear(IsMaterialCached, 0, IsMaterialCached.Length);
        Array.Clear(IsScaleCacheOne, 0, IsScaleCacheOne.Length);
        Array.Clear(IsEnvelopeCached, 0, IsEnvelopeCached.Length);
        NodeResource = null;
        MaterialResource = null;
        ShapeResource = null;
        PosScale = 0;
        InversePosScale = 0;
        GetJointScale = null;
        SendJointSrt = null;
        SendTexSrt = null;

        TmpMatAnmResult.Clear();
        TmpJntAnmResult.Clear();
        TmpVisAnmResult.Clear();
    }

    public SbcCallbackTiming GetCallbackTiming(SbcCommand cmd)
        => _callbackFunctions[(int)cmd] != null ? _callbackTimings[(int)cmd] : SbcCallbackTiming.None;

    public bool PerformCallbackA(RenderContext context, SbcCommand cmd)
    {
        if (GetCallbackTiming(cmd) == SbcCallbackTiming.TimingA)
        {
            Flag &= ~G3dRenderStateFlag.Skip;
            _callbackFunctions[(int)cmd](context);
            return (Flag & G3dRenderStateFlag.Skip) != 0;
        }

        return false;
    }

    public bool PerformCallbackB(RenderContext context, SbcCommand cmd)
    {
        if (GetCallbackTiming(cmd) == SbcCallbackTiming.TimingB)
        {
            Flag &= ~G3dRenderStateFlag.Skip;
            _callbackFunctions[(int)cmd](context);
            return (Flag & G3dRenderStateFlag.Skip) != 0;
        }

        return false;
    }

    public bool PerformCallbackC(RenderContext context, SbcCommand command)
    {
        if (GetCallbackTiming(command) == SbcCallbackTiming.TimingC)
        {
            Flag &= ~G3dRenderStateFlag.Skip;
            _callbackFunctions[(int)command](context);
            return (Flag & G3dRenderStateFlag.Skip) != 0;
        }

        return false;
    }

    public void SetCallback(SbcCallbackFunction function, SbcCommand command, SbcCallbackTiming timing)
    {
        _callbackFunctions[(int)command] = function;
        _callbackTimings[(int)command] = timing;
    }

    public void ResetCallback(SbcCommand command)
    {
        _callbackFunctions[(int)command] = null;
        _callbackTimings[(int)command] = SbcCallbackTiming.None;
    }
}