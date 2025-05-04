namespace HaroohiePals.Graphics3d.OpenGL.Renderers;

public class CircleRenderer() : MeshRenderer(Resources.Models.QuadObj, null, Resources.Shaders.CircleVertex,
    Resources.Shaders.CircleFragment, true, true);