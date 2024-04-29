using HaroohiePals.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace HaroohiePals.Graphics3d.OpenGL
{
    public sealed class GLTexture : Texture
    {
        private          bool      _disposed;
        private readonly GLContext _context;

        public readonly int           Handle;
        public readonly TextureTarget Target;

        public GLTexture(Rgba8Bitmap image, TextureWrapMode wrapS, TextureWrapMode wrapT)
        {
            _context = GLContext.Current;

            Handle = GL.GenTexture();
            Target = TextureTarget.Texture2D;

            SetWrapMode(wrapS, wrapT);
            SetFilterMode(TextureFilterMode.Nearest, TextureFilterMode.Nearest);

            Use();

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, image.Width, image.Height, 0,
                PixelFormat.Bgra, PixelType.UnsignedByte, image.Pixels);
        }

        public GLTexture(IntPtr data, int width, int height, PixelFormat pixelFormat, PixelType pixelType,
            TextureWrapMode wrapS, TextureWrapMode wrapT)
        {
            _context = GLContext.Current;

            Handle = GL.GenTexture();
            Target = TextureTarget.Texture2D;

            SetWrapMode(wrapS, wrapT);
            SetFilterMode(TextureFilterMode.Nearest, TextureFilterMode.Nearest);

            Use();

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, width, height, 0,
                pixelFormat, pixelType, data);
        }

        public GLTexture(PixelInternalFormat internalFormat, int width, int height, PixelFormat format, PixelType type,
            IntPtr pixels = default)
        {
            _context = GLContext.Current;

            Handle = GL.GenTexture();
            Target = TextureTarget.Texture2D;

            Use();

            GL.TexImage2D(TextureTarget.Texture2D, 0, internalFormat, width, height, 0, format, type, pixels);
        }

        public GLTexture(PixelInternalFormat internalFormat, int width, int height, PixelFormat format, PixelType type,
            byte[] pixels)
        {
            _context = GLContext.Current;

            Handle = GL.GenTexture();
            Target = TextureTarget.Texture2D;

            Use();

            GL.TexImage2D(TextureTarget.Texture2D, 0, internalFormat, width, height, 0, format, type, pixels);
        }

        public GLTexture(int samples, PixelInternalFormat internalFormat, int width, int height,
            bool fixedSampleLocations)
        {
            _context = GLContext.Current;

            Handle = GL.GenTexture();
            Target = TextureTarget.Texture2DMultisample;

            Use();

            GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, samples, internalFormat, width,
                height, fixedSampleLocations);
        }

        public override void Use()
        {
            if (_disposed)
                return;

            GL.BindTexture(Target, Handle);
        }

        private static OpenTK.Graphics.OpenGL4.TextureWrapMode WrapModeToGL(TextureWrapMode mode) => mode switch
        {
            TextureWrapMode.Clamp          => OpenTK.Graphics.OpenGL4.TextureWrapMode.ClampToEdge,
            TextureWrapMode.Repeat         => OpenTK.Graphics.OpenGL4.TextureWrapMode.Repeat,
            TextureWrapMode.MirroredRepeat => OpenTK.Graphics.OpenGL4.TextureWrapMode.MirroredRepeat,
            _                              => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };

        public override void SetWrapMode(TextureWrapMode wrapS, TextureWrapMode wrapT)
        {
            if (_disposed)
                return;

            Use();

            var glWrapS = WrapModeToGL(wrapS);
            var glWrapT = WrapModeToGL(wrapT);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)glWrapS);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)glWrapT);
        }

        public override void SetFilterMode(TextureFilterMode minFilter, TextureFilterMode magFilter)
        {
            if (_disposed)
                return;

            Use();

            var glMinFilter = minFilter switch
            {
                TextureFilterMode.Nearest => TextureMinFilter.Nearest,
                TextureFilterMode.Linear  => TextureMinFilter.Linear,
                _                         => throw new ArgumentOutOfRangeException(nameof(minFilter), minFilter, null)
            };

            var glMagFilter = magFilter switch
            {
                TextureFilterMode.Nearest => TextureMagFilter.Nearest,
                TextureFilterMode.Linear  => TextureMagFilter.Linear,
                _                         => throw new ArgumentOutOfRangeException(nameof(magFilter), magFilter, null)
            };

            GL.TexParameter(Target, TextureParameterName.TextureMinFilter, (int)glMinFilter);
            GL.TexParameter(Target, TextureParameterName.TextureMagFilter, (int)glMagFilter);
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing && (_context == null || GLContext.Current == _context))
                GL.DeleteTexture(Handle);
            else
                _context?.Delete(GLContext.GLObjectType.Texture, Handle);

            _disposed = true;
        }

        ~GLTexture() => Dispose(false);
    }
}