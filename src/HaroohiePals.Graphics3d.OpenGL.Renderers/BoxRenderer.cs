namespace HaroohiePals.Graphics3d.OpenGL.Renderers;

public class BoxRenderer : MeshRenderer
{
    public BoxRenderer(bool gpuFixWorkaround = false) 
        : base(Resources.Models.CubeObj, Resources.Models.BoxTexture,
                Resources.Shaders.BoxVertex,
                gpuFixWorkaround ?
                    Resources.Shaders.BoxIntelFragment :
                    Resources.Shaders.BoxFragment,
            false)
    {
    }
}