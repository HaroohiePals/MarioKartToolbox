using HaroohiePals.Graphics3d.OpenGL;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace HaroohiePals.Nitro.G3.OpenGL
{
    public sealed unsafe class GLGeStateUniformBuffer : IDisposable
    {
        private readonly int            _baseAlign;
        private          byte[]         _buffer;
        private          int            _offset;
        private readonly GLBuffer<byte> _glBuffer;

        private readonly int _uniformIdx;

        public GLGeStateUniformBuffer(int uniformIdx)
        {
            _uniformIdx = uniformIdx;
            GL.GetInteger(GetPName.UniformBufferOffsetAlignment, out _baseAlign);
            _buffer = new byte[1 << (BitOperations.Log2((uint)_baseAlign) + 1)];

            _glBuffer = new GLBuffer<byte>();
            Bind();
            GL.BufferData(BufferTarget.UniformBuffer, _buffer.Length, _buffer, BufferUsageHint.DynamicDraw);
        }

        public void CommitGeState(GeometryEngineState state)
        {
            //we assume offset is always aligned
            int offset        = _offset;
            int requiredSpace = GeStateUniform.Size;
            if (requiredSpace % _baseAlign != 0)
                requiredSpace += _baseAlign - (requiredSpace % _baseAlign);

            if (_offset + requiredSpace > _buffer.Length)
            {
                //resize
                int newSize = _buffer.Length * 2;
                while (_offset + requiredSpace > newSize)
                    newSize *= 2;
                Array.Resize(ref _buffer, newSize);

                _glBuffer.BufferData(_buffer, BufferUsageHint.DynamicDraw);
                // _sizeChanged = true;
            }

            _offset += requiredSpace;

            state.ToUniform(ref MemoryMarshal.AsRef<GeStateUniform>(_buffer.AsSpan(offset)));

            GL.BindBufferRange(BufferRangeTarget.UniformBuffer, _uniformIdx, _glBuffer.Handle, (IntPtr)offset,
                GeStateUniform.Size);
        }

        public void Reset()
        {
            _offset = 0;
        }

        public void Bind()
        {
            _glBuffer.Bind(BufferTarget.UniformBuffer);
        }

        public void Flush()
        {
            Bind();

            GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)0, _offset, _buffer);

            Reset();
        }

        public void Dispose()
        {
            _glBuffer.Dispose();
        }
    }
}