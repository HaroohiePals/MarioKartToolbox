using OpenTK.Graphics.OpenGL4;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HaroohiePals.Graphics3d.OpenGL
{
    public static class GLUtil
    {
        public static void SetupVertexAttribPointers<T>(int indexOffset = 0, int divisor = 0)
            where T : struct
        {
            var type   = typeof(T);
            int stride = Unsafe.SizeOf<T>();
            var fields = type.GetFields();
            foreach (var field in fields)
            {
                var attribAttribute  = field.GetCustomAttribute<GLVertexAttribAttribute>();
                var attribIAttribute = field.GetCustomAttribute<GLVertexAttribIAttribute>();

                if (attribAttribute == null && attribIAttribute == null)
                    continue;

                if (attribAttribute != null && attribIAttribute != null)
                    throw new Exception("Field cannot have more than one vertex attrib attribute");

                var fieldOffset = Marshal.OffsetOf<T>(field.Name);

                if (attribAttribute != null)
                {
                    VertexAttribPointerType fieldType;
                    if (field.FieldType.IsPrimitive)
                    {
                        if (field.FieldType == typeof(sbyte))
                            fieldType = VertexAttribPointerType.Byte;
                        else if (field.FieldType == typeof(byte))
                            fieldType = VertexAttribPointerType.UnsignedByte;
                        else if (field.FieldType == typeof(short))
                            fieldType = VertexAttribPointerType.Short;
                        else if (field.FieldType == typeof(ushort))
                            fieldType = VertexAttribPointerType.UnsignedShort;
                        else if (field.FieldType == typeof(int))
                            fieldType = VertexAttribPointerType.Int;
                        else if (field.FieldType == typeof(uint))
                            fieldType = VertexAttribPointerType.UnsignedInt;
                        else if (field.FieldType == typeof(float))
                            fieldType = VertexAttribPointerType.Float;
                        else if (field.FieldType == typeof(double))
                            fieldType = VertexAttribPointerType.Double;
                        else
                            throw new Exception("Unsupported primitive type");

                        GL.VertexAttribPointer(attribAttribute.Index + indexOffset, 1, fieldType,
                            attribAttribute.Normalized, stride,
                            fieldOffset);
                        GL.VertexAttribDivisor(attribAttribute.Index + indexOffset, divisor);
                        GL.EnableVertexAttribArray(attribAttribute.Index + indexOffset);
                    }
                    else if (field.FieldType == typeof(OpenTK.Mathematics.Matrix4) ||
                             field.FieldType == typeof(System.Numerics.Matrix4x4))
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            GL.VertexAttribPointer(attribAttribute.Index + indexOffset + i, 4,
                                VertexAttribPointerType.Float, attribAttribute.Normalized, stride,
                                fieldOffset + sizeof(float) * i * 4);
                            GL.VertexAttribDivisor(attribAttribute.Index + indexOffset + i, divisor);
                            GL.EnableVertexAttribArray(attribAttribute.Index + indexOffset + i);
                        }
                    }
                    else
                    {
                        int fieldSize;
                        if (field.FieldType == typeof(OpenTK.Mathematics.Vector2) ||
                            field.FieldType == typeof(System.Numerics.Vector2))
                        {
                            fieldType = VertexAttribPointerType.Float;
                            fieldSize = 2;
                        }
                        else if (field.FieldType == typeof(OpenTK.Mathematics.Vector3) ||
                                 field.FieldType == typeof(System.Numerics.Vector3))
                        {
                            fieldType = VertexAttribPointerType.Float;
                            fieldSize = 3;
                        }
                        else if (field.FieldType == typeof(OpenTK.Mathematics.Vector4) ||
                                 field.FieldType == typeof(OpenTK.Mathematics.Color4) ||
                                 field.FieldType == typeof(System.Numerics.Vector4))
                        {
                            fieldType = VertexAttribPointerType.Float;
                            fieldSize = 4;
                        }
                        else
                            throw new Exception("Unsupported type");

                        GL.VertexAttribPointer(attribAttribute.Index + indexOffset, fieldSize, fieldType,
                            attribAttribute.Normalized, stride,
                            fieldOffset);
                        GL.VertexAttribDivisor(attribAttribute.Index + indexOffset, divisor);
                        GL.EnableVertexAttribArray(attribAttribute.Index + indexOffset);
                    }
                }
                else
                {
                    VertexAttribIntegerType fieldType;
                    if (field.FieldType == typeof(sbyte))
                        fieldType = VertexAttribIntegerType.Byte;
                    else if (field.FieldType == typeof(byte))
                        fieldType = VertexAttribIntegerType.UnsignedByte;
                    else if (field.FieldType == typeof(short))
                        fieldType = VertexAttribIntegerType.Short;
                    else if (field.FieldType == typeof(ushort))
                        fieldType = VertexAttribIntegerType.UnsignedShort;
                    else if (field.FieldType == typeof(int))
                        fieldType = VertexAttribIntegerType.Int;
                    else if (field.FieldType == typeof(uint))
                        fieldType = VertexAttribIntegerType.UnsignedInt;
                    else
                        throw new Exception("Unsupported primitive type");

                    GL.VertexAttribIPointer(attribIAttribute.Index + indexOffset, 1, fieldType, stride, fieldOffset);
                    GL.VertexAttribDivisor(attribIAttribute.Index + indexOffset, divisor);
                    GL.EnableVertexAttribArray(attribIAttribute.Index + indexOffset);
                }
            }
        }
    }
}