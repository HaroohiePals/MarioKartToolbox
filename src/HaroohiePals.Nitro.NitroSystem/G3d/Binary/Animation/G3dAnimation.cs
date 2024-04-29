using HaroohiePals.Nitro.NitroSystem.G3d.Animation;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary;

public abstract class G3dAnimation
{
    public G3dAnimationHeader Header;
    public ushort NrFrames;

    public abstract void InitializeAnimationObject(G3dAnimationObject animationObject, G3dModel model);
}
