using System;

namespace HaroohiePals.Graphics3d
{
    public abstract class Texture : IDisposable
    {
        public abstract void Use();
        public abstract void SetWrapMode(TextureWrapMode wrapS, TextureWrapMode wrapT);
        public abstract void SetFilterMode(TextureFilterMode minFilter, TextureFilterMode magFilter);

        protected abstract void Dispose(bool disposing);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}