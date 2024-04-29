using System;

namespace HaroohiePals.Graphics3d.OpenGL
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class GLVertexAttribIAttribute : Attribute
    {
        public readonly int  Index;

        public GLVertexAttribIAttribute(int index)
        {
            Index      = index;
        }
    }
}
