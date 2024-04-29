using HaroohiePals.IO.Serialization;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;

namespace HaroohiePals.IO
{
    public class EndianBinaryReaderEx : EndianBinaryReader
    {
        public EndianBinaryReaderEx(Stream baseStream)
            : base(baseStream) { }

        public EndianBinaryReaderEx(Stream baseStream, Endianness endianness)
            : base(baseStream, endianness) { }

        public void ReadPadding(int alignment)
        {
            if ((BaseStream.Position % alignment) == 0)
                return;
            BaseStream.Position += alignment - (BaseStream.Position % alignment);
        }

        private Stack<long> Chunks = new Stack<long>();

        public void BeginChunk()
        {
            Chunks.Push(BaseStream.Position);
        }

        public void EndChunk()
        {
            Chunks.Pop();
        }

        public void EndChunk(long sectionSize)
        {
            JumpRelative(sectionSize);
            Chunks.Pop();
        }

        public long JumpRelative(long offset)
        {
            long curPos = BaseStream.Position;
            BaseStream.Position = GetChunkRelativePointer(offset);
            return curPos;
        }

        private long GetChunkRelativePointer(long offset)
        {
            if (Chunks.Count == 0)
                return offset;
            return Chunks.Peek() + offset;
        }

        public long GetRelativeOffset()
        {
            if (Chunks.Count == 0)
                return BaseStream.Position;

            return BaseStream.Position - Chunks.Peek();
        }

        public uint ReadSignature(uint expected)
        {
            uint signature = Read<uint>();
            if (signature != expected)
                throw new SignatureNotCorrectException(signature, expected, BaseStream.Position - 4);
            return signature;
        }

        public T ReadObject<T>() where T : new()
        {
            T result = new T();
            ReadObject(result);
            return result;
        }

        private object ReadFieldTypeDirect(FieldType type) => type switch
        {
            //The object cast is needed once to ensure the switch doesn't cast to double
            FieldType.U8     => (object)Read<byte>(),
            FieldType.S8     => Read<sbyte>(),
            FieldType.U16    => Read<ushort>(),
            FieldType.S16    => Read<short>(),
            FieldType.U32    => Read<uint>(),
            FieldType.S32    => Read<int>(),
            FieldType.U64    => Read<ulong>(),
            FieldType.S64    => Read<long>(),
            FieldType.Fx16   => ReadFx16(),
            FieldType.Fx32   => ReadFx32(),
            FieldType.Float  => Read<float>(),
            FieldType.Double => Read<double>(),
            _                => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        private Array ReadFieldTypeArrayDirect(FieldType type, int count) => type switch
        {
            FieldType.U8     => Read<byte>(count),
            FieldType.S8     => Read<sbyte>(count),
            FieldType.U16    => Read<ushort>(count),
            FieldType.S16    => Read<short>(count),
            FieldType.U32    => Read<uint>(count),
            FieldType.S32    => Read<int>(count),
            FieldType.U64    => Read<ulong>(count),
            FieldType.S64    => Read<long>(count),
            FieldType.Fx16   => ReadFx16s(count),
            FieldType.Fx32   => ReadFx32s(count),
            FieldType.Float  => Read<float>(count),
            FieldType.Double => Read<double>(count),
            _                => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        private void AlignForField(FieldInfo field, FieldAlignment alignment, FieldType type)
        {
            var alignAttribute = field.GetCustomAttribute<AlignAttribute>();
            if (alignAttribute != null)
                ReadPadding(alignAttribute.Alignment);
            else if (alignment == FieldAlignment.FieldSize)
                ReadPadding(SerializationUtil.GetTypeSize(type));
        }

        private void ReadPrimitive<T>(T target, FieldInfo field, FieldAlignment alignment)
        {
            var type = SerializationUtil.GetFieldPrimitiveType(field);

            //align if this is not a reference field
            if (field.GetCustomAttribute<ReferenceAttribute>() == null)
                AlignForField(field, alignment, type);

            long address = BaseStream.Position;

            object value = ReadFieldTypeDirect(type);
            object finalValue;
            if (field.FieldType == typeof(bool))
                finalValue = Convert.ChangeType(value, typeof(bool));
            else
                finalValue = SerializationUtil.Cast(value, field.FieldType);

            field.SetValue(target, finalValue);

            var constAttrib = field.GetCustomAttribute<ConstantAttribute>();
            if (constAttrib != null && !constAttrib.Value.Equals(finalValue))
                throw new InvalidDataException(
                    $"Const field \"{field.Name}\" in \"{typeof(T).Name}\" at address 0x{address:X} has an invalid value. Got: {finalValue:X}, expected: {constAttrib.Value:X}");
        }

        private void ReadArray<T>(T target, FieldInfo field, FieldAlignment alignment)
        {
            //align if this is not a reference field
            if (field.GetCustomAttribute<ReferenceAttribute>() == null)
            {
                var alignAttr = field.GetCustomAttribute<AlignAttribute>();
                if (alignAttr != null)
                    ReadPadding(alignAttr.Alignment);
            }

            var arrSizeAttr = field.GetCustomAttribute<ArraySizeAttribute>();
            if (arrSizeAttr == null)
                throw new SerializationException(
                    $"No array size attribute found for field \"{field.Name}\" in \"{typeof(T).Name}\"");
            int size = -1;
            if (arrSizeAttr.SizeField != null)
            {
                var sizeField = typeof(T).GetField(arrSizeAttr.SizeField);
                if (sizeField == null)
                    throw new SerializationException(
                        $"Array size field \"{arrSizeAttr.SizeField}\" not found in \"{typeof(T).Name}\"");
                size = (int)Convert.ChangeType(sizeField.GetValue(target), typeof(int));
            }
            else
                size = arrSizeAttr.FixedSize;

            var elementType = field.FieldType.GetElementType();

            Array value;

            if (elementType == typeof(Vector2d))
            {
                value = new Vector2d[size];
                for (int i = 0; i < size; i++)
                    value.SetValue(ReadVector2dDirect(field), i);
            }
            else if (elementType == typeof(Vector3d))
            {
                value = new Vector3d[size];
                for (int i = 0; i < size; i++)
                    value.SetValue(ReadVector3dDirect(field), i);
            }
            else if (SerializationUtil.HasPrimitiveArrayType(field))
            {
                var type = SerializationUtil.GetFieldPrimitiveType(field);

                //align if this is not a reference field
                if (field.GetCustomAttribute<ReferenceAttribute>() == null)
                    AlignForField(field, alignment, type);

                value = ReadFieldTypeArrayDirect(type, size);

                if (SerializationUtil.FieldTypeToType(type) != elementType)
                    throw new SerializationException("Conversion of array data not supported yet");
            }
            else if (elementType == typeof(string))
                throw new SerializationException();
            else
            {
                var readerConstructor = elementType.GetConstructor(new[] { typeof(EndianBinaryReader) });
                if (readerConstructor == null)
                    readerConstructor = elementType.GetConstructor(new[] { typeof(EndianBinaryReaderEx) });
                if (readerConstructor != null)
                {
                    value = Array.CreateInstance(elementType, size);
                    for (int i = 0; i < size; i++)
                        value.SetValue(readerConstructor.Invoke(new object[] { this }), i);
                }
                else
                    throw new SerializationException();
            }

            field.SetValue(target, value);
        }

        private Vector2d ReadVector2dDirect(FieldInfo field)
        {
            var type       = SerializationUtil.GetVectorPrimitiveType(field);
            var components = new double[2];
            for (int i = 0; i < 2; i++)
            {
                object value = ReadFieldTypeDirect(type);
                components[i] = (double)SerializationUtil.Cast(value, typeof(double));
            }

            return new Vector2d(components[0], components[1]);
        }

        private void ReadVector2<T>(T target, FieldInfo field, FieldAlignment alignment)
        {
            var type = SerializationUtil.GetVectorPrimitiveType(field);

            //align if this is not a reference field
            if (field.GetCustomAttribute<ReferenceAttribute>() == null)
                AlignForField(field, alignment, type);

            field.SetValue(target, ReadVector2dDirect(field));
        }

        private Vector3d ReadVector3dDirect(FieldInfo field)
        {
            var type       = SerializationUtil.GetVectorPrimitiveType(field);
            var components = new double[3];
            for (int i = 0; i < 3; i++)
            {
                object value = ReadFieldTypeDirect(type);
                components[i] = (double)SerializationUtil.Cast(value, typeof(double));
            }

            return new Vector3d(components[0], components[1], components[2]);
        }

        private void ReadVector3<T>(T target, FieldInfo field, FieldAlignment alignment)
        {
            var type = SerializationUtil.GetVectorPrimitiveType(field);

            //align if this is not a reference field
            if (field.GetCustomAttribute<ReferenceAttribute>() == null)
                AlignForField(field, alignment, type);

            field.SetValue(target, ReadVector3dDirect(field));
        }

        public void ReadObject<T>(T target)
        {
            var fields = SerializationUtil.GetFieldsInOrder<T>();

            var alignment = SerializationUtil.GetFieldAlignment<T>();
            foreach (var field in fields)
            {
                long curPos    = BaseStream.Position;
                var  refAttrib = field.GetCustomAttribute<ReferenceAttribute>();
                if (refAttrib != null)
                {
                    if (SerializationUtil.HasPrimitiveType(field))
                        throw new Exception("Reference field cannot be a primitive type");

                    AlignForField(field, alignment, refAttrib.PointerFieldType);
                    long   address = BaseStream.Position;
                    object val     = ReadFieldTypeDirect(refAttrib.PointerFieldType);
                    curPos = BaseStream.Position;
                    long ptr = (long)Convert.ChangeType(val, typeof(long));
                    switch (refAttrib.Type)
                    {
                        case ReferenceType.Absolute:
                            break;
                        case ReferenceType.ChunkRelative:
                            ptr = GetChunkRelativePointer(ptr);
                            break;
                        case ReferenceType.FieldRelative:
                            ptr += address;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    BaseStream.Position = ptr;
                }


                if (field.FieldType == typeof(Vector2d))
                    ReadVector2(target, field, alignment);
                else if (field.FieldType == typeof(Vector3d))
                    ReadVector3(target, field, alignment);
                else if (SerializationUtil.HasPrimitiveType(field))
                    ReadPrimitive(target, field, alignment);
                else if (field.FieldType == typeof(string))
                {
                    throw new SerializationException();
                }
                else if (field.FieldType.IsArray)
                    ReadArray(target, field, alignment);
                else
                {
                    AlignForField(field, alignment, FieldType.U8);
                    var readerConstructor = field.FieldType.GetConstructor(new[] { typeof(EndianBinaryReader) });
                    if (readerConstructor == null)
                        readerConstructor = field.FieldType.GetConstructor(new[] { typeof(EndianBinaryReaderEx) });
                    if (readerConstructor != null)
                    {
                        var obj = readerConstructor.Invoke(new object[] { this });
                        field.SetValue(target, obj);
                    }
                    else
                        throw new SerializationException();
                }

                if (refAttrib != null)
                    BaseStream.Position = curPos;
            }
        }
    }
}