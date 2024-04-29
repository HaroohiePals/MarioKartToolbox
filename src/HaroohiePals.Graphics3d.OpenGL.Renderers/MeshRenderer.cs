using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace HaroohiePals.Graphics3d.OpenGL.Renderers;

public class MeshRenderer : InstancedPointRenderer
{
    public MeshRenderer(byte[] objModel, byte[] texture, string vertexShader, string fragmentShader, bool render2d = false, bool renderTranslucentPass = false) : base(
        new GLShader(vertexShader, fragmentShader),
        render2d, renderTranslucentPass, RendererUtil.GetVertexDataFromObj(objModel), texture)
    {

    }

    protected sealed override GLTexture SetupTexture(byte[] texData)
    {
        using (var boxText = Image.Load<L8>(texData))
        {
            var data = new byte[boxText.Width * boxText.Height];
            boxText.CopyPixelDataTo(data);
            var texture = new GLTexture(PixelInternalFormat.R8, boxText.Width, boxText.Height, PixelFormat.Red,
                PixelType.UnsignedByte, data);
            texture.Use();
            texture.SetWrapMode(TextureWrapMode.Clamp, TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureLodBias, -2.0f);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            return texture;
        }
    }
}