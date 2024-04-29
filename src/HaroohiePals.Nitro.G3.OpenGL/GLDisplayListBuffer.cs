using HaroohiePals.Graphics3d.OpenGL;
using OpenTK.Graphics.OpenGL4;
using System;

namespace HaroohiePals.Nitro.G3.OpenGL
{
    public class GLDisplayListBuffer : DisplayListBuffer
    {
        private readonly GLBuffer<NitroVertexData> _vertexBuffer;
        private readonly GLBuffer<uint>       _elementBuffer;
        private readonly GLVertexArray        _vertexArray;

        public GLDisplayListBuffer(ReadOnlySpan<byte> dl)
            : base(dl)
        {
            _vertexArray = new GLVertexArray();
            _vertexArray.Bind();

            _vertexBuffer = new GLBuffer<NitroVertexData>(_vtxData, BufferUsageHint.StaticDraw);
            _vertexBuffer.Bind(BufferTarget.ArrayBuffer);

            _elementBuffer = new GLBuffer<uint>(_idxData, BufferUsageHint.StaticDraw);
            _elementBuffer.Bind(BufferTarget.ElementArrayBuffer);

            GLNitroVertexData.SetupVertexAttribPointers();

            GL.BindVertexArray(0);
        }

        public override void Bind()
        {
            _vertexArray.Bind();
        }

        public override void Draw()
        {
            Bind();
            GL.DrawElements(PrimitiveType.Triangles, _idxData.Length, DrawElementsType.UnsignedInt, 0);

            GL.BindVertexArray(0);
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            _vertexBuffer.Dispose();
            _elementBuffer.Dispose();
            _vertexArray.Dispose();
        }
    }
}