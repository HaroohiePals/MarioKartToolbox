using HaroohiePals.Graphics3d;
using HaroohiePals.IO;
using HaroohiePals.Nitro.G3;
using HaroohiePals.Nitro.NitroSystem.G3d.Animation;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;

namespace HaroohiePals.Nitro.NitroSystem.G3d;

public delegate void GetJointScaleFunction(JointAnimationResult result, G3dNodeData nodeData,
    Pointer<byte> sbc, RenderContext context);
public delegate void SendJointSrtFunction(JointAnimationResult result, RenderContext context);
public delegate void SendTextureSrtFunction(MaterialAnimationResult result, RenderContext context);

public abstract class RenderContext
{
    public RenderContext(GeometryEngineState geState)
    {
        GeState = geState;
        GlobalRenderState = new G3dGlobalRenderState();
        GlobalState = new G3dGlobalState();
        Sbc = new Sbc(this);
    }

    public abstract void ApplyTexParams(Texture texture);
    public abstract void RenderShp(G3dShape shp, DisplayListBuffer buffer);

    public GeometryEngineState GeState;

    public G3dRenderState RenderState;
    public G3dGlobalRenderState GlobalRenderState;
    public G3dGlobalState GlobalState;

    public Sbc Sbc;

    public GetJointScaleFunction[] GetJointScaleFuncArray =
    {
        CGTool.Basic.GetJointScale,
        CGTool.Maya.GetJointScale,
        null //Si3d
    };

    public SendJointSrtFunction[] SendJointSrtFuncArray =
    {
        CGTool.Basic.SendJointSrt,
        CGTool.Maya.SendJointSrt,
        null //Si3d
    };

    public SendTextureSrtFunction[] SendTexSrtFuncArray =
    {
        CGTool.Maya.SendTextureSrt,
        null, //Si3d,
        null, //3dsMax,
        null  //Xsi
    };
}