using System;

namespace HaroohiePals.Graphics3d.OpenGL
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class GLVertexAttribAttribute : Attribute
    {
        public readonly int  Index;
        public readonly bool Normalized;

        public GLVertexAttribAttribute(int index, bool normalized = false)
        {
            Index      = index;
            Normalized = normalized;
        }
    }
}
