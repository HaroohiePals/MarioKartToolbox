using OpenTK.Graphics.OpenGL4;
using System;

namespace HaroohiePals.Graphics3d.OpenGL
{
    public class GLVertexArray : IDisposable
    {
        private          bool      _disposed;
        private readonly GLContext _context;

        public readonly int Handle;

        public GLVertexArray()
        {
            _context = GLContext.Current;
            Handle   = GL.GenVertexArray();
        }

        public void Bind()
        {
            GL.BindVertexArray(Handle);
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
                GL.DeleteVertexArray(Handle);
            else
                _context?.Delete(GLContext.GLObjectType.VertexArray, Handle);

            _disposed = true;
        }

        ~GLVertexArray() => Dispose(false);
    }
}