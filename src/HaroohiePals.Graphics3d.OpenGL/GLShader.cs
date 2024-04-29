using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace HaroohiePals.Graphics3d.OpenGL
{
    public class GLShader : IDisposable
    {
        private bool      _disposed;
        private readonly GLContext _context;

        private readonly Dictionary<string, int> AttribLocs  = new();
        private readonly Dictionary<string, int> UniformLocs = new();

        public readonly int Handle;

        public GLShader(string vertexSource, string fragmentSource)
        {
            _context = GLContext.Current;

            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexSource);

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentSource);

            // Compile shader

            GL.CompileShader(vertexShader);

            string infoLogVert = GL.GetShaderInfoLog(vertexShader);
            if (infoLogVert != string.Empty)
                throw new ShaderCompilationException(infoLogVert);

            GL.CompileShader(fragmentShader);

            string infoLogFrag = GL.GetShaderInfoLog(fragmentShader);

            if (infoLogFrag != string.Empty)
                throw new ShaderCompilationException(infoLogFrag);

            // Create handle

            Handle = GL.CreateProgram();

            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);

            GL.LinkProgram(Handle);

            // Detach shaders

            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }

        public int GetAttribLocation(string name)
        {
            if (AttribLocs.TryGetValue(name, out int loc))
                return loc;

            loc = GL.GetAttribLocation(Handle, name);

            if (loc == -1)
                throw new ArgumentException("Invalid attribute name specified", nameof(name));

            AttribLocs[name] = loc;

            return loc;
        }

        public int GetUniformLocation(string name)
        {
            if (UniformLocs.TryGetValue(name, out int loc))
                return loc;

            loc = GL.GetUniformLocation(Handle, name);

            // According to OpenGL specifications, Uniforms can be optimized and a -1 location can be ignored
            //if (loc == -1)
            //    throw new ArgumentException($"Invalid uniform name specified: \"{name}\"", nameof(name));

            UniformLocs[name] = loc;

            return loc;
        }

        public void SetInt(int uniformLoc, int value)
            => GL.Uniform1(uniformLoc, value);

        public void SetInt(string name, int value)
            => SetInt(GetUniformLocation(name), value);

        public void SetIntArray(int uniformLoc, ReadOnlySpan<int> value)
            => GL.Uniform1(uniformLoc, value.Length, ref MemoryMarshal.GetReference(value));

        public void SetIntArray(string name, ReadOnlySpan<int> value)
            => SetIntArray(GetUniformLocation(name), value);

        public void SetUint(int uniformLoc, uint value)
            => GL.Uniform1(uniformLoc, value);

        public void SetUint(string name, uint value)
            => SetUint(GetUniformLocation(name), value);

        public void SetFloat(int uniformLoc, float value)
            => GL.Uniform1(uniformLoc, value);

        public void SetFloat(string name, float value)
            => SetFloat(GetUniformLocation(name), value);

        public void SetVector2(int uniformLoc, Vector2 vector)
            => GL.Uniform2(uniformLoc, vector);

        public void SetVector2(string name, Vector2 vector)
            => SetVector2(GetUniformLocation(name), vector);

        public void SetVector3(int uniformLoc, Vector3 vector)
            => GL.Uniform3(uniformLoc, vector);

        public void SetVector3(string name, Vector3 vector)
            => SetVector3(GetUniformLocation(name), vector);

        public void SetVector4(int uniformLoc, Vector4 vector)
            => GL.Uniform4(uniformLoc, vector);

        public void SetVector4(string name, Vector4 vector)
            => SetVector4(GetUniformLocation(name), vector);

        public void SetMatrix4(int uniformLoc, Matrix4 matrix, bool transpose = false)
            => GL.UniformMatrix4(uniformLoc, transpose, ref matrix);

        public void SetMatrix4(string name, Matrix4 matrix, bool transpose = false)
            => SetMatrix4(GetUniformLocation(name), matrix, transpose);

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
                GL.DeleteProgram(Handle);
            else
                _context?.Delete(GLContext.GLObjectType.Program, Handle);

            _disposed = true;
        }

        ~GLShader() => Dispose(false);
    }

    public class ShaderCompilationException : Exception
    {
        public ShaderCompilationException(string message) : base(message) { }
    }
}