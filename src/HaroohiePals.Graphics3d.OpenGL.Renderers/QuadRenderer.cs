#nullable enable
namespace HaroohiePals.Graphics3d.OpenGL.Renderers;

public class QuadRenderer(GLTexture texture) : InstancedPointRenderer(
    new GLShader(Resources.Shaders.QuadVertex, Resources.Shaders.QuadFragment),
    false, true, RendererUtil.GetVertexDataFromObj(Resources.Models.QuadObj), texture);