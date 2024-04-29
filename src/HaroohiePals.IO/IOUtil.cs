using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HaroohiePals.IO
{
    public static class IOUtil
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ReadS16Le(byte[] data, int offset)
            => BinaryPrimitives.ReadInt16LittleEndian(data.AsSpan(offset));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ReadS16Le(ReadOnlySpan<byte> span)
            => BinaryPrimitives.ReadInt16LittleEndian(span);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short[] ReadS16Le(byte[] data, int offset, int count)
            => ReadS16Le(data.AsSpan(offset), count);

        public static short[] ReadS16Le(ReadOnlySpan<byte> data, int count)
        {
            var res = MemoryMarshal.Cast<byte, short>(data.Slice(0, count * 2)).ToArray();
            if (!BitConverter.IsLittleEndian)
            {
                for (int i = 0; i < count; i++)
                    res[i] = BinaryPrimitives.ReverseEndianness(res[i]);
            }

            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteS16Le(byte[] data, int offset, short value)
            => BinaryPrimitives.WriteInt16LittleEndian(data.AsSpan(offset), value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteS16Le(Span<byte> span, short value)
            => BinaryPrimitives.WriteInt16LittleEndian(span, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteS16Le(byte[] data, int offset, ReadOnlySpan<short> values)
            => WriteS16Le(data.AsSpan(offset), values);

        public static void WriteS16Le(Span<byte> data, ReadOnlySpan<short> values)
        {
            var dst = MemoryMarshal.Cast<byte, short>(data);
            values.CopyTo(dst);
            if (!BitConverter.IsLittleEndian)
            {
                for (int i = 0; i < values.Length; i++)
                    dst[i] = BinaryPrimitives.ReverseEndianness(dst[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ReadS16Be(byte[] data, int offset)
            => BinaryPrimitives.ReadInt16BigEndian(data.AsSpan(offset));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ReadS16Be(ReadOnlySpan<byte> span)
            => BinaryPrimitives.ReadInt16BigEndian(span);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort[] ReadU16Le(byte[] data, int offset, int count)
            => ReadU16Le(data.AsSpan(offset), count);

        public static ushort[] ReadU16Le(ReadOnlySpan<byte> data, int count)
        {
            var res = MemoryMarshal.Cast<byte, ushort>(data.Slice(0, count * 2)).ToArray();
            if (!BitConverter.IsLittleEndian)
            {
                for (int i = 0; i < count; i++)
                    res[i] = BinaryPrimitives.ReverseEndianness(res[i]);
            }

            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReadU16Le(byte[] data, int offset)
            => BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(offset));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReadU16Le(ReadOnlySpan<byte> span)
            => BinaryPrimitives.ReadUInt16LittleEndian(span);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteU16Le(byte[] data, int offset, ushort value)
            => BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(offset), value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteU16Le(Span<byte> span, ushort value)
            => BinaryPrimitives.WriteUInt16LittleEndian(span, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteU16Le(byte[] data, int offset, ReadOnlySpan<ushort> values)
            => WriteU16Le(data.AsSpan(offset), values);

        public static void WriteU16Le(Span<byte> data, ReadOnlySpan<ushort> values)
        {
            var dst = MemoryMarshal.Cast<byte, ushort>(data);
            values.CopyTo(dst);
            if (!BitConverter.IsLittleEndian)
            {
                for (int i = 0; i < values.Length; i++)
                    dst[i] = BinaryPrimitives.ReverseEndianness(dst[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReadU16Be(byte[] data, int offset)
            => BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(offset));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReadU16Be(ReadOnlySpan<byte> span)
            => BinaryPrimitives.ReadUInt16BigEndian(span);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteU16Be(byte[] data, int offset, ushort value)
            => BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(offset), value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteU16Be(Span<byte> span, ushort value)
            => BinaryPrimitives.WriteUInt16BigEndian(span, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadU24Le(byte[] data, int offset)
            => (uint)(data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadU24Le(ReadOnlySpan<byte> span)
            => (uint)(span[0] | (span[1] << 8) | (span[2] << 16));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadU32Le(byte[] data, int offset)
            => BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(offset));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadU32Le(ReadOnlySpan<byte> span)
            => BinaryPrimitives.ReadUInt32LittleEndian(span);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint[] ReadU32Le(byte[] data, int offset, int count)
            => ReadU32Le(data.AsSpan(offset), count);

        public static uint[] ReadU32Le(ReadOnlySpan<byte> data, int count)
        {
            var res = MemoryMarshal.Cast<byte, uint>(data.Slice(0, count * 4)).ToArray();
            if (!BitConverter.IsLittleEndian)
            {
                for (int i = 0; i < count; i++)
                    res[i] = BinaryPrimitives.ReverseEndianness(res[i]);
            }

            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadU32Be(byte[] data, int offset)
            => BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(offset));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadU32Be(ReadOnlySpan<byte> span)
            => BinaryPrimitives.ReadUInt32BigEndian(span);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteU32Le(byte[] data, int offset, uint value)
            => BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(offset), value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteU32Le(Span<byte> span, uint value)
            => BinaryPrimitives.WriteUInt32LittleEndian(span, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteU32Le(byte[] data, int offset, ReadOnlySpan<uint> values)
            => WriteU32Le(data.AsSpan(offset), values);

        public static void WriteU32Le(Span<byte> data, ReadOnlySpan<uint> values)
        {
            var dst = MemoryMarshal.Cast<byte, uint>(data);
            values.CopyTo(dst);
            if (!BitConverter.IsLittleEndian)
            {
                for (int i = 0; i < values.Length; i++)
                    dst[i] = BinaryPrimitives.ReverseEndianness(dst[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ReadU64Le(byte[] data, int offset)
            => BinaryPrimitives.ReadUInt64LittleEndian(data.AsSpan(offset));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ReadU64Le(ReadOnlySpan<byte> span)
            => BinaryPrimitives.ReadUInt64LittleEndian(span);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ReadU64Be(byte[] data, int offset)
            => BinaryPrimitives.ReadUInt64BigEndian(data.AsSpan(offset));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ReadU64Be(ReadOnlySpan<byte> span)
            => BinaryPrimitives.ReadUInt64BigEndian(span);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteU64Le(byte[] data, int offset, ulong value)
            => BinaryPrimitives.WriteUInt64LittleEndian(data.AsSpan(offset), value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteU64Le(Span<byte> span, ulong value)
            => BinaryPrimitives.WriteUInt64LittleEndian(span, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteU64Be(byte[] data, int offset, ulong value)
            => BinaryPrimitives.WriteUInt64BigEndian(data.AsSpan(offset), value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteU64Be(Span<byte> span, ulong value)
            => BinaryPrimitives.WriteUInt64BigEndian(span, value);
    }
}