using HaroohiePals.Graphics;
using HaroohiePals.Graphics3d;
using HaroohiePals.Graphics3d.OpenGL;
using HaroohiePals.Nitro.G3;
using HaroohiePals.Nitro.G3.OpenGL;
using System;

namespace HaroohiePals.Nitro.NitroSystem.G3d.OpenGL
{
    public class GLG3dModelManager : G3dModelManager
    {
        public override DisplayListBuffer CreateDisplayListBuffer(ReadOnlySpan<byte> dl)
            => new GLDisplayListBuffer(dl);

        public override Texture CreateTexture(Rgba8Bitmap bitmap)
            => new GLTexture(bitmap, TextureWrapMode.Repeat, TextureWrapMode.Repeat);
    }
}