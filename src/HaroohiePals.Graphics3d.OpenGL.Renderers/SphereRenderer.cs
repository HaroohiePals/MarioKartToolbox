namespace HaroohiePals.Graphics3d.OpenGL.Renderers;

public class SphereRenderer() : MeshRenderer(Resources.Models.SphereObj, 
    Resources.Models.BoxTexture,
    Resources.Shaders.BoxVertex,
    Resources.Shaders.BoxFragment,
    false, true);
