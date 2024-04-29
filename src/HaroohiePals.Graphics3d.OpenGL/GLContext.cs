using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Concurrent;

namespace HaroohiePals.Graphics3d.OpenGL
{
    public class GLContext
    {
        public enum GLObjectType
        {
            Buffer,
            Framebuffer,
            Program,
            Renderbuffer,
            Shader,
            Texture,
            VertexArray
        }

        [ThreadStatic]
        public static GLContext Current;

        private readonly ConcurrentQueue<(GLObjectType type, int handle)> _garbage = new();

        public void Delete(GLObjectType type, int handle)
        {
            _garbage.Enqueue((type, handle));
        }

        public void CollectGarbage()
        {
            while (_garbage.TryDequeue(out var entry))
            {
                switch (entry.type)
                {
                    case GLObjectType.Buffer:
                        GL.DeleteBuffer(entry.handle);
                        break;
                    case GLObjectType.Framebuffer:
                        GL.DeleteFramebuffer(entry.handle);
                        break;
                    case GLObjectType.Program:
                        GL.DeleteProgram(entry.handle);
                        break;
                    case GLObjectType.Renderbuffer:
                        GL.DeleteRenderbuffer(entry.handle);
                        break;
                    case GLObjectType.Shader:
                        GL.DeleteShader(entry.handle);
                        break;
                    case GLObjectType.Texture:
                        GL.DeleteTexture(entry.handle);
                        break;
                    case GLObjectType.VertexArray:
                        GL.DeleteVertexArray(entry.handle);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}