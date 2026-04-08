#nullable enable
namespace HaroohiePals.Graphics3d.OpenGL.Renderers;

public class QuadRenderer(GLTexture texture, bool render2d, bool renderTranslucentPass) 
    : InstancedPointRenderer(
        new GLShader(Resources.Shaders.QuadVertex, Resources.Shaders.QuadFragment),
            render2d, renderTranslucentPass, 
            RendererUtil.GetVertexDataFromObj(Resources.Models.QuadObj), texture);