using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace HaroohiePals.IO
{
    public class EndianBinaryReader : IDisposable
    {
        private bool   _disposed;
        private byte[] _buffer;

        public Stream     BaseStream { get; }
        public Endianness Endianness { get; }

        public static Endianness SystemEndianness =>
            BitConverter.IsLittleEndian ? Endianness.LittleEndian : Endianness.BigEndian;

        private bool Reverse => SystemEndianness != Endianness;

        public EndianBinaryReader(Stream baseStream, Endianness endianness = Endianness.LittleEndian)
        {
            if (baseStream == null)
                throw new ArgumentNullException(nameof(baseStream));
            if (!baseStream.CanRead)
                throw new ArgumentException(nameof(baseStream));

            BaseStream = baseStream;
            Endianness = endianness;
        }

        ~EndianBinaryReader()
        {
            Dispose(false);
        }

        private void FillBuffer(int bytes, int stride)
        {
            if (_buffer == null || _buffer.Length < bytes)
                _buffer = new byte[bytes];

            BaseStream.Read(_buffer, 0, bytes);

            if (Reverse && stride > 1)
            {
                for (int i = 0; i < bytes; i += stride)
                    Array.Reverse(_buffer, i, stride);
            }
        }

        public char ReadChar(Encoding encoding)
        {
            int size;

            size = GetEncodingSize(encoding);
            FillBuffer(size, size);
            return encoding.GetChars(_buffer, 0, size)[0];
        }

        public char[] ReadChars(Encoding encoding, int count)
        {
            int size;

            size = GetEncodingSize(encoding);
            FillBuffer(size * count, size);
            return encoding.GetChars(_buffer, 0, size * count);
        }

        private static int GetEncodingSize(Encoding encoding)
        {
            if (encoding == Encoding.UTF8 || encoding == Encoding.ASCII)
                return 1;
            else if (encoding == Encoding.Unicode || encoding == Encoding.BigEndianUnicode)
                return 2;

            return 1;
        }

        public string ReadStringNT(Encoding encoding)
        {
            string text;

            text = "";

            do
            {
                text += ReadChar(encoding);
            } while (!text.EndsWith("\0", StringComparison.Ordinal));

            return text.Remove(text.Length - 1);
        }

        public string ReadString(Encoding encoding, int count)
        {
            return new string(ReadChars(encoding, count));
        }

        public unsafe T Read<T>() where T : unmanaged
        {
            int size = sizeof(T);
            FillBuffer(size, size);
            return MemoryMarshal.Read<T>(_buffer);
        }

        public unsafe T[] Read<T>(int count) where T : unmanaged
        {
            int size       = sizeof(T);
            var result     = new T[count];
            var byteResult = MemoryMarshal.Cast<T, byte>(result);
            BaseStream.Read(byteResult);

            if (Reverse && size > 1)
            {
                for (int i = 0; i < size * count; i += size)
                    byteResult.Slice(i, size).Reverse();
            }

            return result;
        }

        public double ReadFx16()
        {
            return Read<short>() / 4096d;
        }

        public double[] ReadFx16s(int count)
        {
            var result = new double[count];
            for (int i = 0; i < count; i++)
                result[i] = ReadFx16();

            return result;
        }

        public double ReadFx32()
        {
            return Read<int>() / 4096d;
        }

        public double[] ReadFx32s(int count)
        {
            var result = new double[count];
            for (int i = 0; i < count; i++)
                result[i] = ReadFx32();

            return result;
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

        private void Dispose(bool disposing)
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