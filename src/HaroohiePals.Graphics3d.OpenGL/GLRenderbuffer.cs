using OpenTK.Graphics.OpenGL4;
using System;

namespace HaroohiePals.Graphics3d.OpenGL;

public class GLRenderbuffer : IDisposable
{
    private          bool      _disposed;
    private readonly GLContext _context;

    public readonly int Handle;

    public GLRenderbuffer()
    {
        _context = GLContext.Current;
        Handle   = GL.GenRenderbuffer();
    }

    public void Bind()
    {
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, Handle);
    }

    public void Storage(RenderbufferStorage internalFormat, int width, int height)
    {
        int curRenderBuf = GL.GetInteger(GetPName.RenderbufferBinding);
        {
            Bind();
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, internalFormat, width, height);
        }
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, curRenderBuf);
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
            GL.DeleteRenderbuffer(Handle);
        else
            _context?.Delete(GLContext.GLObjectType.Renderbuffer, Handle);

        _disposed = true;
    }

    ~GLRenderbuffer() => Dispose(false);
}