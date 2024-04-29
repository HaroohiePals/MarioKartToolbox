using HaroohiePals.Nitro.G3;
using HaroohiePals.Nitro.NitroSystem.G3d.Animation;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;

namespace HaroohiePals.Nitro.NitroSystem.G3d;

public sealed class G3dRenderObject
{
    public G3dRenderObject(G3dModel model)
    {
        BlendMaterialFunction = G3dAnimationUtil.BlendMaterialAnimation;
        BlendJointFunction = G3dAnimationUtil.BlendJointAnimation;
        BlendVisibilityFunction = null; // todo: Not yet implemented

        ModelResource = model;

        MaterialTextureInfos = new MaterialTextureInfo[ModelResource.Materials.Materials.Length];
    }

    public G3dRenderObjectFlag Flag;

    public G3dModel ModelResource;
    public G3dAnimationObject MaterialAnimations;
    public BlendMaterialAnimationFunction BlendMaterialFunction;
    public G3dAnimationObject JointAnimations;
    public BlendJointAnimationFunction BlendJointFunction;
    public G3dAnimationObject VisibilityAnimations;
    public BlendVisibilityAnimationFunction BlendVisibilityFunction;

    public SbcCallbackFunction CallbackFunction;
    public SbcCommand CallbackCmd;
    public SbcCallbackTiming CallbackTiming;

    public SbcCallbackFunction CallbackInitFunction;

    public object User;

    public byte[] UserSbc;

    public JointAnimationResult[] RecordedJointAnimations;
    public MaterialAnimationResult[] RecordedMaterialAnimations;

    public readonly bool[] MaterialAnimationMayExist = new bool[G3dConfig.MaxMaterialCount];
    public readonly bool[] JointAnimationMayExist = new bool[G3dConfig.MaxJointCount];
    public readonly bool[] VisibilityAnimationMayExist = new bool[G3dConfig.MaxJointCount];

    public void SetFlag(G3dRenderObjectFlag flag) => Flag |= flag;
    public void ResetFlag(G3dRenderObjectFlag flag) => Flag &= ~flag;
    public bool TestFlag(G3dRenderObjectFlag flag) => (Flag & flag) == flag;

    public G3dTextureSet Textures;
    public MaterialTextureInfo[] MaterialTextureInfos;
    public DisplayListBuffer[] ShapeProxies;

    public void AddAnimationObject(G3dAnimationObject animationObject)
    {
        switch (animationObject.AnimationResource.Header.Category0)
        {
            case 'M': // NSBMA
            {
                UpdateAnimationExistence(MaterialAnimationMayExist, animationObject);
                G3dAnimationObject.AddLink(ref MaterialAnimations, animationObject);
                break;
            }
            case 'J': // NSBCA
            {
                UpdateAnimationExistence(JointAnimationMayExist, animationObject);
                G3dAnimationObject.AddLink(ref JointAnimations, animationObject);
                break;
            }
            case 'V': // NSBVA
            {
                UpdateAnimationExistence(VisibilityAnimationMayExist, animationObject);
                G3dAnimationObject.AddLink(ref VisibilityAnimations, animationObject);
                break;
            }
        }
    }

    public void RemoveAnimationObject(G3dAnimationObject animationObject)
    {
        if (G3dAnimationObject.RemoveLink(ref MaterialAnimations, animationObject) ||
            G3dAnimationObject.RemoveLink(ref JointAnimations, animationObject) ||
            G3dAnimationObject.RemoveLink(ref VisibilityAnimations, animationObject))
        {
            SetFlag(G3dRenderObjectFlag.HintObsolete);
        }
    }

    public static void UpdateAnimationExistence(bool[] vec, G3dAnimationObject anmObj)
    {
        for (var p = anmObj; p is not null; p = p.Next)
        {
            for (int i = 0; i < p.MapData.Length; ++i)
            {
                if ((p.MapData[i] & G3dAnimationObject.MapDataExist) != 0)
                {
                    vec[i] = true;
                }
            }
        }
    }
}
