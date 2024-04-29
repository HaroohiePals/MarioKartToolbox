using OpenTK.Graphics.OpenGL4;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HaroohiePals.Graphics3d.OpenGL
{
    public class GLBuffer<T> : IDisposable where T : struct
    {
        private          bool      _disposed;
        private readonly GLContext _context;

        public readonly int Handle;

        public GLBuffer()
        {
            Handle   = GL.GenBuffer();
            _context = GLContext.Current;
        }

        public GLBuffer(ReadOnlySpan<T> data, BufferUsageHint usage)
            : this()
        {
            BufferData(data, usage);
        }

        public GLBuffer(int size, BufferUsageHint usage)
            : this()
        {
            BufferData(size, usage);
        }

        public void Bind(BufferTarget target)
        {
            GL.BindBuffer(target, Handle);
        }

        public void BufferData(ReadOnlySpan<T> data, BufferUsageHint usage)
        {
            int arrBuf = GL.GetInteger(GetPName.ArrayBufferBinding);
            Bind(BufferTarget.ArrayBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, Unsafe.SizeOf<T>() * data.Length,
                ref MemoryMarshal.GetReference(data), usage);
            GL.BindBuffer(BufferTarget.ArrayBuffer, arrBuf);
        }

        public void BufferData(int size, BufferUsageHint usage)
        {
            int arrBuf = GL.GetInteger(GetPName.ArrayBufferBinding);
            Bind(BufferTarget.ArrayBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, Unsafe.SizeOf<T>() * size, IntPtr.Zero, usage);
            GL.BindBuffer(BufferTarget.ArrayBuffer, arrBuf);
        }

        public void BufferSubData(int offset, ReadOnlySpan<T> data)
        {
            int arrBuf = GL.GetInteger(GetPName.ArrayBufferBinding);
            Bind(BufferTarget.ArrayBuffer);
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)(offset * Unsafe.SizeOf<T>()),
                Unsafe.SizeOf<T>() * data.Length, ref MemoryMarshal.GetReference(data));
            GL.BindBuffer(BufferTarget.ArrayBuffer, arrBuf);
        }

        public void BufferSubData(int offset, IntPtr data, int count)
        {
            int arrBuf = GL.GetInteger(GetPName.ArrayBufferBinding);
            Bind(BufferTarget.ArrayBuffer);
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)(offset * Unsafe.SizeOf<T>()),
                Unsafe.SizeOf<T>() * count, data);
            GL.BindBuffer(BufferTarget.ArrayBuffer, arrBuf);
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
                GL.DeleteBuffer(Handle);
            else
                _context?.Delete(GLContext.GLObjectType.Buffer, Handle);

            _disposed = true;
        }

        ~GLBuffer() => Dispose(false);
    }
}