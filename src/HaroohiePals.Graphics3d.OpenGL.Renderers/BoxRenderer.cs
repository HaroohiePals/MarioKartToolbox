namespace HaroohiePals.Graphics3d.OpenGL.Renderers;

public class BoxRenderer(bool gpuFixWorkaround = false) : MeshRenderer(Resources.Models.CubeObj,
    Resources.Models.BoxTexture,
    Resources.Shaders.BoxVertex,
    gpuFixWorkaround ? Resources.Shaders.BoxIntelFragment : Resources.Shaders.BoxFragment,
    false);