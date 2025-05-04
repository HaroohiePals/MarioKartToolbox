using HaroohiePals.Graphics3d.OpenGL.Renderers.Resources;
using OpenTK.Graphics.OpenGL4;

namespace HaroohiePals.Graphics3d.OpenGL.Renderers;

public class DotRenderer(float size = 8f, byte[] texData = null)
    : InstancedPointRenderer(new GLShader(Shaders.DotVertex, Shaders.DotFragment), true, true, new VertexData[1],
        texData)
{
    protected override void RenderInstanced(int instanceCount)
    {
        GL.PointSize(size);
        GL.PointParameter(PointParameterName.PointSpriteCoordOrigin, (int)PointSpriteCoordOriginParameter.UpperLeft);
        GL.DrawArraysInstanced(PrimitiveType.Points, 0, 1, instanceCount);
    }
}
