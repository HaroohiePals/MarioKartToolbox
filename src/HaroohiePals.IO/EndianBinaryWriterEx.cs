using HaroohiePals.IO.Serialization;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;

namespace HaroohiePals.IO
{
    public class EndianBinaryWriterEx : EndianBinaryWriter
    {
        public EndianBinaryWriterEx(Stream baseStream)
            : base(baseStream) { }

        public EndianBinaryWriterEx(Stream baseStream, Endianness endianness)
            : base(baseStream, endianness) { }

        public enum ChunkSizeType
        {
            U8,
            U16,
            U32,
            U64
        }

        private class Chunk
        {
            public long          StartAddress;
            public bool          HasSize;
            public int           SizeOffset;
            public ChunkSizeType SizeType;
            public int           SizeDelta;
        }

        private readonly Stack<Chunk> _chunks = new();

        public void BeginChunk() => BeginChunk(null);

        public void BeginChunk(ChunkPointer chunkPointer)
        {
            if (chunkPointer != null)
                WriteChunkPointer(chunkPointer);
            _chunks.Push(new Chunk() { StartAddress = BaseStream.Position, HasSize = false });
        }

        public void BeginChunk(int sizeOffset, ChunkSizeType sizeType = ChunkSizeType.U32)
            => BeginChunk(null, sizeOffset, sizeType);

        public void BeginChunk(ChunkPointer chunkPointer, int sizeOffset, ChunkSizeType sizeType = ChunkSizeType.U32)
        {
            if (chunkPointer != null)
                WriteChunkPointer(chunkPointer);
            _chunks.Push(new Chunk
            {
                StartAddress = BaseStream.Position,
                HasSize      = true,
                SizeOffset   = sizeOffset,
                SizeType     = sizeType,
                SizeDelta    = 0
            });
        }

        public void EndChunk()
        {
            var s = _chunks.Pop();
            if (s.HasSize)
            {
                ulong length = (ulong)(BaseStream.Position - s.StartAddress + s.SizeDelta);
                long  curpos = BaseStream.Position;
                BaseStream.Position = s.StartAddress + s.SizeOffset;
                switch (s.SizeType)
                {
                    case ChunkSizeType.U8:
                        Write((byte)length);
                        break;
                    case ChunkSizeType.U16:
                        Write((ushort)length);
                        break;
                    case ChunkSizeType.U32:
                        Write((uint)length);
                        break;
                    case ChunkSizeType.U64:
                        Write((ulong)length);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                BaseStream.Position = curpos;
            }
        }

        public enum PointerType
        {
            Global,
            ChunkRelative,
            FieldRelative
        }

        public class ChunkPointer
        {
            public long PointerAddress;
            public long PointerBase;
        }

        private void WriteChunkPointer(ChunkPointer pointer)
        {
            long curpos = BaseStream.Position;
            BaseStream.Position = pointer.PointerAddress;
            Write((uint)(curpos - pointer.PointerBase));
            BaseStream.Position = curpos;
        }

        public ChunkPointer WriteChunkPointer(PointerType type)
        {
            long address = BaseStream.Position;
            long ptrBase = 0;
            switch (type)
            {
                case PointerType.ChunkRelative:
                    ptrBase = _chunks.Peek().StartAddress;
                    break;
                case PointerType.FieldRelative:
                    ptrBase = address;
                    break;
            }

            Write(0);
            return new ChunkPointer() { PointerAddress = address, PointerBase = ptrBase };
        }

        public void WriteChunkSize(ChunkSizeType sizeType = ChunkSizeType.U32)
        {
            long curPos = BaseStream.Position;
            switch (sizeType)
            {
                case ChunkSizeType.U8:
                    Write((byte)0);
                    break;
                case ChunkSizeType.U16:
                    Write((ushort)0);
                    break;
                case ChunkSizeType.U32:
                    Write(0);
                    break;
                case ChunkSizeType.U64:
                    Write((ulong)0);
                    break;
            }

            if (_chunks.Count == 0)
                return;
            var curChunk = _chunks.Peek();
            curChunk.HasSize    = true;
            curChunk.SizeOffset = (int)(curPos - curChunk.StartAddress);
            curChunk.SizeType   = sizeType;
        }

        public void WriteChunkSize16()
            => WriteChunkSize(ChunkSizeType.U16);

        public long GetCurposRelative()
            => BaseStream.Position - _chunks.Peek().StartAddress;

        public void WriteCurposRelative(int offset, int delta = 0)
        {
            long curpos = JumpRelative(offset);
            if (_chunks.Count == 0)
                Write((uint)(curpos + delta));
            else
                Write((uint)(curpos - _chunks.Peek().StartAddress + delta));
            BaseStream.Position = curpos;
        }

        public void WriteCurposRelativeU16(int offset, int delta = 0)
        {
            long curpos = JumpRelative(offset);
            if (_chunks.Count == 0)
                Write((ushort)(curpos + delta));
            else
                Write((ushort)(curpos - _chunks.Peek().StartAddress + delta));
            BaseStream.Position = curpos;
        }

        public void WritePadding(int alignment, byte value = 0)
        {
            while ((BaseStream.Position % alignment) != 0)
                Write(value);
        }

        public long JumpRelative(long offset)
        {
            long curPos = BaseStream.Position;
            if (_chunks.Count == 0)
                BaseStream.Position = offset;
            else
                BaseStream.Position = _chunks.Peek().StartAddress + offset;
            return curPos;
        }

        private void AlignForField(FieldInfo field, FieldAlignment alignment, FieldType type)
        {
            var alignAttribute = field.GetCustomAttribute<AlignAttribute>();
            if (alignAttribute != null)
                WritePadding(alignAttribute.Alignment);
            else if (alignment == FieldAlignment.FieldSize)
                WritePadding(SerializationUtil.GetTypeSize(type));
        }

        private void WriteFieldTypeDirect(FieldType type, object value)
        {
            switch (type)
            {
                case FieldType.U8:
                    Write((byte)value);
                    break;
                case FieldType.S8:
                    Write((sbyte)value);
                    break;
                case FieldType.U16:
                    Write((ushort)value);
                    break;
                case FieldType.S16:
                    Write((short)value);
                    break;
                case FieldType.U32:
                    Write((uint)value);
                    break;
                case FieldType.S32:
                    Write((int)value);
                    break;
                case FieldType.U64:
                    Write((ulong)value);
                    break;
                case FieldType.S64:
                    Write((long)value);
                    break;
                case FieldType.Fx16:
                    WriteFx16((double)value);
                    break;
                case FieldType.Fx32:
                    WriteFx32((double)value);
                    break;
                case FieldType.Float:
                    Write((float)value);
                    break;
                case FieldType.Double:
                    Write((double)value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private void WriteFieldTypeArrayDirect(FieldType type, Array value)
        {
            switch (type)
            {
                case FieldType.U8:
                    Write((byte[])value);
                    break;
                case FieldType.S8:
                    Write((sbyte[])value);
                    break;
                case FieldType.U16:
                    Write((ushort[])value);
                    break;
                case FieldType.S16:
                    Write((short[])value);
                    break;
                case FieldType.U32:
                    Write((uint[])value);
                    break;
                case FieldType.S32:
                    Write((int[])value);
                    break;
                case FieldType.U64:
                    Write((ulong[])value);
                    break;
                case FieldType.S64:
                    Write((long[])value);
                    break;
                case FieldType.Fx16:
                    WriteFx16s((double[])value);
                    break;
                case FieldType.Fx32:
                    WriteFx32s((double[])value);
                    break;
                case FieldType.Float:
                    Write((float[])value);
                    break;
                case FieldType.Double:
                    Write((double[])value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private Dictionary<object, List<(long address, Chunk chunk, ReferenceAttribute refInfo)>> _references = new();
        private Dictionary<object, long> _refObjAddresses = new();

        private void WritePrimitive<T>(T obj, FieldInfo field, FieldAlignment alignment)
        {
            var fieldType = SerializationUtil.GetFieldPrimitiveType(field);
            var trueType  = SerializationUtil.FieldTypeToType(fieldType);

            AlignForField(field, alignment, fieldType);

            var    constAttrib = field.GetCustomAttribute<ConstantAttribute>();
            object fieldValue  = constAttrib != null ? constAttrib.Value : field.GetValue(obj);

            object finalValue;
            if (field.FieldType == typeof(bool))
                finalValue = SerializationUtil.Cast((bool)fieldValue ? 1 : 0, trueType);
            else
                finalValue = SerializationUtil.Cast(fieldValue, trueType);

            var chunkSizeAttr = field.GetCustomAttribute<ChunkSizeAttribute>();
            if (chunkSizeAttr != null && _chunks.Count > 0)
            {
                var chunkSizeType = fieldType switch
                {
                    FieldType.U8  => ChunkSizeType.U8,
                    FieldType.U16 => ChunkSizeType.U16,
                    FieldType.U32 => ChunkSizeType.U32,
                    FieldType.U64 => ChunkSizeType.U64,
                    _             => throw new ArgumentOutOfRangeException("Invalid type for chunk size")
                };

                var chunk = _chunks.Peek();
                chunk.HasSize    = true;
                chunk.SizeOffset = (int)GetCurposRelative();
                chunk.SizeType   = chunkSizeType;
                chunk.SizeDelta  = chunkSizeAttr.Difference;
                finalValue       = SerializationUtil.Cast(0, trueType);
            }

            WriteFieldTypeDirect(fieldType, finalValue);
        }

        private void WriteArray<T>(T obj, FieldInfo field, FieldAlignment alignment)
        {
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
                size = (int)Convert.ChangeType(sizeField.GetValue(obj), typeof(int));
            }
            else
                size = arrSizeAttr.FixedSize;

            if (size < 0)
                throw new SerializationException("Array size invalid");

            var elementType = field.FieldType.GetElementType();

            var value = field.GetValue(obj) as Array;

            if (value == null)
                throw new SerializationException("Field value is null");

            if (value.Length != size)
                throw new SerializationException("Array size invalid");

            if (elementType == typeof(Vector2d))
            {
                for (int i = 0; i < size; i++)
                    WriteVector2dDirect(field, (Vector2d)value.GetValue(i));
            }
            else if (elementType == typeof(Vector3d))
            {
                for (int i = 0; i < size; i++)
                    WriteVector3dDirect(field, (Vector3d)value.GetValue(i));
            }
            else if (SerializationUtil.HasPrimitiveArrayType(field))
            {
                var type = SerializationUtil.GetFieldPrimitiveType(field);

                //align if this is not a reference field
                if (field.GetCustomAttribute<ReferenceAttribute>() == null)
                    AlignForField(field, alignment, type);

                if (SerializationUtil.FieldTypeToType(type) != elementType)
                    throw new SerializationException("Conversion of array data not supported yet");

                WriteFieldTypeArrayDirect(type, value);
            }
            else if (elementType == typeof(string))
                throw new SerializationException();
            else
            {
                AlignForField(field, alignment, FieldType.U8);
                var writeMethod = elementType.GetMethod("Write", new[] { typeof(EndianBinaryWriter) });
                if (writeMethod == null)
                    writeMethod = elementType.GetMethod("Write", new[] { typeof(EndianBinaryWriterEx) });
                if (writeMethod != null)
                {
                    for (int i = 0; i < size; i++)
                        writeMethod.Invoke(value.GetValue(i), new object[] { this });
                }
                else
                    throw new SerializationException();
            }
        }

        private void WriteVector2dDirect(FieldInfo field, Vector2d value)
        {
            var type     = SerializationUtil.GetVectorPrimitiveType(field);
            var trueType = SerializationUtil.FieldTypeToType(type);
            WriteFieldTypeDirect(type, SerializationUtil.Cast(value.X, trueType));
            WriteFieldTypeDirect(type, SerializationUtil.Cast(value.Y, trueType));
        }

        private void WriteVector2<T>(T obj, FieldInfo field, FieldAlignment alignment)
        {
            var type = SerializationUtil.GetVectorPrimitiveType(field);

            //align if this is not a reference field
            if (field.GetCustomAttribute<ReferenceAttribute>() == null)
                AlignForField(field, alignment, type);

            WriteVector2dDirect(field, (Vector2d)field.GetValue(obj));
        }

        private void WriteVector3dDirect(FieldInfo field, Vector3d value)
        {
            var type     = SerializationUtil.GetVectorPrimitiveType(field);
            var trueType = SerializationUtil.FieldTypeToType(type);
            WriteFieldTypeDirect(type, SerializationUtil.Cast(value.X, trueType));
            WriteFieldTypeDirect(type, SerializationUtil.Cast(value.Y, trueType));
            WriteFieldTypeDirect(type, SerializationUtil.Cast(value.Z, trueType));
        }

        private void WriteVector3<T>(T obj, FieldInfo field, FieldAlignment alignment)
        {
            var type = SerializationUtil.GetVectorPrimitiveType(field);

            //align if this is not a reference field
            if (field.GetCustomAttribute<ReferenceAttribute>() == null)
                AlignForField(field, alignment, type);

            WriteVector3dDirect(field, (Vector3d)field.GetValue(obj));
        }

        public void WriteObject<T>(T obj)
        {
            var fields = SerializationUtil.GetFieldsInOrder<T>();

            var alignment = SerializationUtil.GetFieldAlignment<T>();
            foreach (var field in fields)
            {
                var refAttrib = field.GetCustomAttribute<ReferenceAttribute>();
                if (refAttrib != null)
                {
                    if (SerializationUtil.HasPrimitiveType(field))
                        throw new SerializationException("Reference field cannot be a primitive type");

                    AlignForField(field, alignment, refAttrib.PointerFieldType);

                    object refValue = field.GetValue(obj);
                    if (refValue != null)
                    {
                        if (!_references.ContainsKey(refValue))
                            _references.Add(refValue, new List<(long, Chunk, ReferenceAttribute)>());
                        _references[refValue].Add((BaseStream.Position, _chunks.Count == 0 ? null : _chunks.Peek(),
                            refAttrib));
                    }

                    var fieldType = SerializationUtil.FieldTypeToType(refAttrib.PointerFieldType);
                    WriteFieldTypeDirect(refAttrib.PointerFieldType, Convert.ChangeType(0, fieldType));
                    continue;
                }

                if (field.FieldType == typeof(Vector2d))
                    WriteVector2(obj, field, alignment);
                else if (field.FieldType == typeof(Vector3d))
                    WriteVector3(obj, field, alignment);
                else if (SerializationUtil.HasPrimitiveType(field))
                    WritePrimitive(obj, field, alignment);
                else if (field.FieldType == typeof(string))
                {
                    throw new SerializationException();
                }
                else if (field.FieldType.IsArray)
                    WriteArray(obj, field, alignment);
                else
                {
                    var value = field.GetValue(obj);
                    if (value == null)
                        throw new SerializationException("Field value is null");

                    var writeMethod = field.FieldType.GetMethod("Write", new[] { typeof(EndianBinaryWriter) });
                    if (writeMethod == null)
                        writeMethod = field.FieldType.GetMethod("Write", new[] { typeof(EndianBinaryWriterEx) });
                    if (writeMethod != null)
                        writeMethod.Invoke(value, new object[] { this });
                    else
                    {
                        throw new SerializationException();
                    }
                }
            }
        }

        public void StartOfRefObject(object obj)
        {
            _refObjAddresses.Add(obj, BaseStream.Position);
        }

        public void FlushReferences()
        {
            foreach (var reference in _references)
            {
                if (!_refObjAddresses.ContainsKey(reference.Key))
                    throw new Exception("Referenced object has not been written");

                long address = _refObjAddresses[reference.Key];

                foreach (var pointer in reference.Value)
                {
                    long ptr = pointer.refInfo.Type switch
                    {
                        ReferenceType.Absolute      => address,
                        ReferenceType.ChunkRelative => address - (pointer.chunk?.StartAddress ?? 0),
                        ReferenceType.FieldRelative => address - pointer.address,
                        _                           => throw new ArgumentOutOfRangeException()
                    };

                    var fieldType = SerializationUtil.FieldTypeToType(pointer.refInfo.PointerFieldType);
                    BaseStream.Position = pointer.address;
                    WriteFieldTypeDirect(pointer.refInfo.PointerFieldType, Convert.ChangeType(ptr, fieldType));
                }
            }

            _references.Clear();
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
                FlushReferences();

            base.Dispose(disposing);
        }
    }
}