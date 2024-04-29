using HaroohiePals.Nitro.NitroSystem.G3d.Animation;
using OpenTK.Mathematics;

namespace HaroohiePals.Nitro.NitroSystem.G3d;

public sealed class G3dGlobalRenderState
{
    public G3dGlobalRenderState()
    {
        for (int i = 0; i < G3dConfig.MaxMaterialCount; i++)
        {
            MaterialCache[i] = new MaterialAnimationResult();
        }

        for (int i = 0; i < G3dConfig.MaxJointCount; i++)
        {
            ScaleCache[i] = new ScaleCacheEntry();
            EnvelopeCache[i]   = new EnvelopeCacheEntry();
        }
    }

    public readonly MaterialAnimationResult[] MaterialCache = new MaterialAnimationResult[G3dConfig.MaxMaterialCount];
    public readonly ScaleCacheEntry[] ScaleCache = new ScaleCacheEntry[G3dConfig.MaxJointCount];
    public readonly EnvelopeCacheEntry[] EnvelopeCache = new EnvelopeCacheEntry[G3dConfig.MaxJointCount];

    public class ScaleCacheEntry
    {
        public Vector3d Scale;
        public Vector3d InverseScale;
    }

    public class EnvelopeCacheEntry
    {
        public Matrix4d PositionMtx;
        public Matrix3d DirectionMtx;
    }
}
