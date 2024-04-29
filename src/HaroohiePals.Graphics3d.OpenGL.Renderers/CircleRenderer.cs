namespace HaroohiePals.Graphics3d.OpenGL.Renderers;

public class CircleRenderer : MeshRenderer
{
    public CircleRenderer() : base(Resources.Models.QuadObj, null, Resources.Shaders.CircleVertex,
            Resources.Shaders.CircleFragment, true, true)
    {

    }
}