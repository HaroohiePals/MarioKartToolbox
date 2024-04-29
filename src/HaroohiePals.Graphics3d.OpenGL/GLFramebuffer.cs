using OpenTK.Graphics.OpenGL4;
using System;

namespace HaroohiePals.Graphics3d.OpenGL
{
    public class GLFramebuffer : IDisposable
    {
        private          bool      _disposed;
        private readonly GLContext _context;

        public readonly int Handle;

        public GLFramebuffer()
        {
            _context = GLContext.Current;
            Handle   = GL.GenFramebuffer();
        }

        public void Bind(FramebufferTarget target)
        {
            GL.BindFramebuffer(target, Handle);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing && (_context == null || GLContext.Current == _context))
                GL.DeleteFramebuffer(Handle);
            else
                _context?.Delete(GLContext.GLObjectType.Framebuffer, Handle);

            _disposed = true;
        }

        ~GLFramebuffer() => Dispose(false);
    }
}