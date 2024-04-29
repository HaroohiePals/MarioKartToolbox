using HaroohiePals.Graphics3d.OpenGL;
using HaroohiePals.Nitro.G3.OpenGL;
using HaroohiePals.Nitro.NitroSystem.G3d.OpenGL.Resources;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace HaroohiePals.Nitro.NitroSystem.G3d.OpenGL;

public class GLG3dModelShader : GLShader
{
    public GLG3dModelShader() : base(Shaders.G3dModelVertex, Shaders.G3dModelFragment) { }
    public GLGeStateUniformBuffer GetUniformBuffer() => new GLGeStateUniformBuffer(GL.GetUniformBlockIndex(Handle, "uboData"));

    public void SetViewMatrix(Matrix4 view) => SetMatrix4("view", view);
    public void SetModelMatrix(Matrix4 model) => SetMatrix4("model", model);
    public void SetProjectionMatrix(Matrix4 projection) => SetMatrix4("projection", projection);
    public void SetPickingId(uint pickingId) => SetUint("pickingId", pickingId);
}
