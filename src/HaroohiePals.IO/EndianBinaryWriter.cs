using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace HaroohiePals.IO
{
    public class EndianBinaryWriter : IDisposable
    {
        protected bool   _disposed;
        private   byte[] _buffer;

        public Stream     BaseStream { get; private set; }
        public Endianness Endianness { get; set; }

        public static Endianness SystemEndianness =>
            BitConverter.IsLittleEndian ? Endianness.LittleEndian : Endianness.BigEndian;

        private bool Reverse => SystemEndianness != Endianness;

        public EndianBinaryWriter(Stream baseStream, Endianness endianness = Endianness.LittleEndian)
        {
            if (baseStream == null)
                throw new ArgumentNullException(nameof(baseStream));
            if (!baseStream.CanWrite)
                throw new ArgumentException(nameof(baseStream));

            BaseStream = baseStream;
            Endianness = endianness;
        }

        ~EndianBinaryWriter()
        {
            Dispose(false);
        }

        private void WriteBuffer(int bytes, int stride)
        {
            if (Reverse && stride > 1)
            {
                for (int i = 0; i < bytes; i += stride)
                    Array.Reverse(_buffer, i, stride);
            }

            BaseStream.Write(_buffer, 0, bytes);
        }

        private void CreateBuffer(int size)
        {
            if (_buffer == null || _buffer.Length < size)
                _buffer = new byte[size];
        }

        public void Write(byte value)
        {
            CreateBuffer(1);
            _buffer[0] = value;
            WriteBuffer(1, 1);
        }

        public void Write(char value, Encoding encoding)
        {
            int size;

            size = GetEncodingSize(encoding);
            CreateBuffer(size);
            Array.Copy(encoding.GetBytes(new string(value, 1)), 0, _buffer, 0, size);
            WriteBuffer(size, size);
        }

        public void Write(char[] value, int offset, int count, Encoding encoding)
        {
            int size;

            size = GetEncodingSize(encoding);
            CreateBuffer(size * count);
            Array.Copy(encoding.GetBytes(value, offset, count), 0, _buffer, 0, count * size);
            WriteBuffer(size * count, size);
        }

        private static int GetEncodingSize(Encoding encoding)
        {
            if (encoding == Encoding.UTF8 || encoding == Encoding.ASCII)
                return 1;
            else if (encoding == Encoding.Unicode || encoding == Encoding.BigEndianUnicode)
                return 2;

            return 1;
        }

        public void Write(string value, Encoding encoding, bool nullTerminated)
        {
            Write(value.ToCharArray(), 0, value.Length, encoding);
            if (nullTerminated)
                Write('\0', encoding);
        }

        public unsafe void Write<T>(T value) where T : unmanaged
        {
            int size = sizeof(T);
            CreateBuffer(size);
            MemoryMarshal.Write(_buffer, ref value);
            WriteBuffer(size, size);
        }

        public unsafe void Write<T>(T[] value) where T : unmanaged
            => Write<T>(value.AsSpan());

        public unsafe void Write<T>(T[] value, int offset, int count) where T : unmanaged
            => Write<T>(value.AsSpan(offset, count));

        public unsafe void Write<T>(ReadOnlySpan<T> value) where T : unmanaged
        {
            int size = sizeof(T);

            if (!Reverse || size == 1)
            {
                BaseStream.Write(MemoryMarshal.Cast<T, byte>(value));
                return;
            }

            CreateBuffer(size * value.Length);
            MemoryMarshal.Cast<T, byte>(value).CopyTo(_buffer);
            WriteBuffer(size * value.Length, size);
        }

        public void WriteFx16(double value)
        {
            Write((short)System.Math.Round(value * 4096d));
        }

        public void WriteFx16s(ReadOnlySpan<double> values)
        {
            for (int i = 0; i < values.Length; i++)
                WriteFx16(values[i]);
        }

        public void WriteFx32(double value)
        {
            Write((int)System.Math.Round(value * 4096d));
        }

        public void WriteFx32s(ReadOnlySpan<double> values)
        {
            for (int i = 0; i < values.Length; i++)
                WriteFx32(values[i]);
        }

        public void Close()
        {
            Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                BaseStream?.Close();

            _buffer   = null;
            _disposed = true;
        }
    }
}