using HaroohiePals.IO;
using System;

namespace HaroohiePals.Nitro.Card;

public sealed class Blowfish
{
    public const int KEY_TABLE_LENGTH = 0x1048;
    public const int P_TABLE_ENTRY_COUNT = 18;
    public const int S_BOX_COUNT = 4;
    public const int S_BOX_ENTRY_COUNT = 256;

    private const string DATA_LENGTH_NOT_MULTIPLE_OF_8_EXCEPTION_MESSAGE = "Data length must be a multiple of 8.";
    private const string DESTINATION_BUFFER_TOO_SMALL_EXCEPTION_MESSAGE = "Not enough space in destination buffer.";

    private readonly uint[]   _pTable;
    private readonly uint[][] _sBoxes;

    public Blowfish(uint[] pTable, uint[][] sBoxes)
    {
        if (pTable.Length != P_TABLE_ENTRY_COUNT)
        {
            throw new ArgumentException($"Size of p table should be {P_TABLE_ENTRY_COUNT}", nameof(pTable));
        }

        if (sBoxes.Length != S_BOX_COUNT)
        {
            throw new ArgumentException($"Number of s boxes should be {S_BOX_COUNT}", nameof(sBoxes));
        }

        for (int i = 0; i < S_BOX_COUNT; i++)
        {
            if (sBoxes[i].Length != S_BOX_ENTRY_COUNT)
            {
                throw new ArgumentException($"Size of s box {i} should be {S_BOX_ENTRY_COUNT}", nameof(sBoxes));
            }
        }

        _pTable = pTable;
        _sBoxes = sBoxes;
    }

    public Blowfish(ReadOnlySpan<byte> keyTable)
    {
        if (keyTable == null)
        {
            throw new ArgumentNullException(nameof(keyTable));
        }

        if (keyTable.Length < KEY_TABLE_LENGTH)
        {
            throw new ArgumentException(nameof(keyTable));
        }

        _pTable    = IOUtil.ReadU32Le(keyTable, P_TABLE_ENTRY_COUNT);
        _sBoxes    = new uint[S_BOX_COUNT][];
        _sBoxes[0] = IOUtil.ReadU32Le(keyTable[0x48..], S_BOX_ENTRY_COUNT);
        _sBoxes[1] = IOUtil.ReadU32Le(keyTable[0x448..], S_BOX_ENTRY_COUNT);
        _sBoxes[2] = IOUtil.ReadU32Le(keyTable[0x848..], S_BOX_ENTRY_COUNT);
        _sBoxes[3] = IOUtil.ReadU32Le(keyTable[0xC48..], S_BOX_ENTRY_COUNT);
    }

    public void Encrypt(byte[] data, int offset, int length)
    {
        Encrypt(data.AsSpan(offset, length));
    }

    public void Encrypt(Span<byte> data)
    {
        ThrowIfDataLengthNotMultipleOf8(data.Length, nameof(data));

        for (int i = 0; i < data.Length; i += 8)
        {
            ulong val = Encrypt(IOUtil.ReadU64Le(data[i..]));
            IOUtil.WriteU64Le(data[i..], val);
        }
    }

    public ulong Encrypt(ulong val)
    {
        uint y = (uint)(val & 0xFFFFFFFF);
        uint x = (uint)(val >> 32);
        for (int i = 0; i < 16; i++)
        {
            uint z = _pTable[i] ^ x;
            uint a = _sBoxes[0][(z >> 24) & 0xFF];
            uint b = _sBoxes[1][(z >> 16) & 0xFF];
            uint c = _sBoxes[2][(z >> 8) & 0xFF];
            uint d = _sBoxes[3][z & 0xFF];
            x = (d + (c ^ (b + a))) ^ y;
            y = z;
        }

        return (x ^ _pTable[16]) | (ulong)(y ^ _pTable[17]) << 32;
    }

    public void Decrypt(byte[] src, int srcOffset, int length, byte[] dst, int dstOffset)
    {
        Decrypt(src.AsSpan(srcOffset, length), dst.AsSpan(dstOffset, length));
    }

    public void Decrypt(Span<byte> data)
    {
        Decrypt(data, data);
    }

    public void Decrypt(ReadOnlySpan<byte> src, Span<byte> dst)
    {
        ThrowIfDataLengthNotMultipleOf8(src.Length, nameof(src));

        if (dst.Length < src.Length)
        {
            throw new ArgumentException(DESTINATION_BUFFER_TOO_SMALL_EXCEPTION_MESSAGE, nameof(dst));
        }

        for (int i = 0; i < src.Length; i += 8)
        {
            ulong val = Decrypt(IOUtil.ReadU64Le(src[i..]));
            IOUtil.WriteU64Le(dst[i..], val);
        }
    }

    public ulong Decrypt(ulong val)
    {
        uint y = (uint)(val & 0xFFFFFFFF);
        uint x = (uint)(val >> 32);
        for (int i = 17; i >= 2; i--)
        {
            uint z = _pTable[i] ^ x;
            uint a = _sBoxes[0][(z >> 24) & 0xFF];
            uint b = _sBoxes[1][(z >> 16) & 0xFF];
            uint c = _sBoxes[2][(z >> 8) & 0xFF];
            uint d = _sBoxes[3][z & 0xFF];
            x = (d + (c ^ (b + a))) ^ y;
            y = z;
        }

        return (x ^ _pTable[1]) | (ulong)(y ^ _pTable[0]) << 32;
    }

    private void ThrowIfDataLengthNotMultipleOf8(int dataLength, string paramName)
    {
        if ((dataLength & 7) != 0)
        {
            throw new ArgumentException(DATA_LENGTH_NOT_MULTIPLE_OF_8_EXCEPTION_MESSAGE, paramName);
        }
    }
}