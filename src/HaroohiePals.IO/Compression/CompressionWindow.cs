#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace HaroohiePals.IO.Compression;

/// <summary>
/// Compression window for finding runs for LZ77-like compression algorithms.
/// The algorithm uses a linked hash table to keep track of encountered 3 byte sequences
/// to find suitable runs very quickly.
/// </summary>
ref struct CompressionWindow
{
    private const int KEY_BYTE_COUNT = 3;
    private const int WINDOW_SIZE = 0x1000;
    private const int INVALID_ADDRESS = -1;

    /// <summary>
    /// A span containing the source data.
    /// </summary>
    private readonly ReadOnlySpan<byte> _source;

    /// <summary>
    /// Given a key of the form ((byte0 << 10) ^ (byte1 << 5) ^ byte2) & 0x7FFF,
    /// points to an address that contains approximately that 3 byte sequence,
    /// or -1 when no such sequence exists.
    /// </summary>
    private readonly int[] _hashTable = new int[0x8000];

    /// <summary>
    /// Given an address modulo <see cref="WINDOW_SIZE"/>, points at a lower address containing
    /// approximately the same 3 byte sequence, or -1 when no such sequence exists.
    /// </summary>
    private readonly int[] _hashLinkTable;

    /// <summary>
    /// The current hash key.
    /// </summary>
    private uint _hashKey;

    /// <summary>
    /// The current source position.
    /// </summary>
    public int Position { get; private set; }

    /// <summary>
    /// The minimum run length that should be found.
    /// </summary>
    public readonly int MinimumRunLength { get; }

    /// <summary>
    /// The maximum run length that should be found.
    /// </summary>
    public readonly int MaximumRunLength { get; }

    public CompressionWindow(ReadOnlySpan<byte> src, int minimumRunLength, int maximumRunLength)
    {
        _source = src;
        MinimumRunLength = minimumRunLength;
        MaximumRunLength = maximumRunLength;
        _hashLinkTable = new int[WINDOW_SIZE];
        _hashKey = 0;

        // Set all entries to be invalid
        Array.Fill(_hashTable, INVALID_ADDRESS);
        Array.Fill(_hashLinkTable, INVALID_ADDRESS);

        // Load the first 3 bytes
        Position = -KEY_BYTE_COUNT;
        Slide(KEY_BYTE_COUNT);
    }

    /// <summary>
    /// Slides the window <see cref="Position"/> forward by <paramref name="count"/> bytes.
    /// </summary>
    /// <param name="count">The number of bytes to slide the window.</param>
    public void Slide(uint count)
    {
        for (int i = Position; i < Position + (int)count && i + KEY_BYTE_COUNT < _source.Length; i++)
        {
            _hashKey = (_hashKey << 5 ^ _source[i + KEY_BYTE_COUNT]) & 0x7FFF;
            _hashLinkTable[i + 1 & WINDOW_SIZE - 1] = _hashTable[_hashKey];
            _hashTable[_hashKey] = i + 1;
        }

        Position = Math.Min(Position + (int)count, _source.Length);
    }

    /// <summary>
    /// Tries to find a run for the current <see cref="Position"/>
    /// of a length between <see cref="MinimumRunLength"/> and <see cref="MaximumRunLength"/>.
    /// </summary>
    /// <param name="foundPosition">When successful, contains the position of the found run.</param>
    /// <param name="foundLength">When successful, contains the length of the found run.</param>
    /// <returns><see langword="true"/> when a run of a proper length is found,
    /// or <see langword="false"/> otherwise.</returns>
    public readonly bool TryFindRun(out int foundPosition, out int foundLength)
    {
        int bestLength = MinimumRunLength - 1;
        int bestPosition = -1;
        int maxLength = Math.Min(MaximumRunLength, _source.Length - Position);
        int searchPosition = _hashLinkTable[Position % WINDOW_SIZE];
        int targetPosition = Position;
        int minPos = Position - Math.Min(Position, WINDOW_SIZE);
        ref byte sourceRef = ref MemoryMarshal.GetReference(_source);
        while (searchPosition >= minPos)
        {
            int runLength = (int)CommonPrefixLength(
                ref Unsafe.Add(ref sourceRef, targetPosition),
                ref Unsafe.Add(ref sourceRef, searchPosition),
                (nuint)maxLength);
            if (runLength > bestLength)
            {
                bestLength = runLength;
                bestPosition = searchPosition;

                if (runLength >= maxLength)
                {
                    break;
                }
            }

            int newSearchPosition = _hashLinkTable[searchPosition % WINDOW_SIZE];
            if (newSearchPosition >= searchPosition)
            {
                break;
            }

            searchPosition = newSearchPosition;
        }

        foundPosition = bestPosition;
        foundLength = bestLength;

        return foundLength >= MinimumRunLength;
    }

    // Based on SpanHelpers.CommonPrefixLength, extended for 256 bit vectors
    private static nuint CommonPrefixLength(ref byte first, ref byte second, nuint length)
    {
        nuint i;

        // It is ordered this way to match the default branch predictor rules, to don't have too much
        // overhead for short input-lengths.
        if (!Vector128.IsHardwareAccelerated || length < (nuint)Vector128<byte>.Count)
        {
            // To have kind of fast path for small inputs, we handle as much elements needed
            // so that either we are done or can use the unrolled loop below.
            i = length % 4;

            if (i > 0)
            {
                if (first != second)
                {
                    return 0;
                }

                if (i > 1)
                {
                    if (Unsafe.Add(ref first, 1) != Unsafe.Add(ref second, 1))
                    {
                        return 1;
                    }

                    if (i > 2 && Unsafe.Add(ref first, 2) != Unsafe.Add(ref second, 2))
                    {
                        return 2;
                    }
                }
            }

            for (; (nint)i <= (nint)length - 4; i += 4)
            {
                if (Unsafe.Add(ref first, i + 0) != Unsafe.Add(ref second, i + 0)) goto Found0;
                if (Unsafe.Add(ref first, i + 1) != Unsafe.Add(ref second, i + 1)) goto Found1;
                if (Unsafe.Add(ref first, i + 2) != Unsafe.Add(ref second, i + 2)) goto Found2;
                if (Unsafe.Add(ref first, i + 3) != Unsafe.Add(ref second, i + 3)) goto Found3;
            }

            return length;
        Found0:
            return i;
        Found1:
            return i + 1;
        Found2:
            return i + 2;
        Found3:
            return i + 3;
        }

        Debug.Assert(length >= (uint)Vector128<byte>.Count);

        if (!Vector256.IsHardwareAccelerated || length < (nuint)Vector256<byte>.Count)
        {
            uint mask;
            nuint lengthToExamine = length - (nuint)Vector128<byte>.Count;

            Vector128<byte> maskVec;
            i = 0;

            while (i < lengthToExamine)
            {
                maskVec = Vector128.Equals(
                    Vector128.LoadUnsafe(ref first, i),
                    Vector128.LoadUnsafe(ref second, i));

                mask = maskVec.ExtractMostSignificantBits();
                if (mask != 0xFFFF)
                {
                    goto Found128;
                }

                i += (nuint)Vector128<byte>.Count;
            }

            // Do final compare as Vector128<byte>.Count from end rather than start
            i = lengthToExamine;
            maskVec = Vector128.Equals(
                Vector128.LoadUnsafe(ref first, i),
                Vector128.LoadUnsafe(ref second, i));

            mask = maskVec.ExtractMostSignificantBits();
            if (mask != 0xFFFF)
            {
                goto Found128;
            }

            return length;

        Found128:
            mask = ~mask;
            return i + uint.TrailingZeroCount(mask);
        }
        else
        {
            Debug.Assert(length >= (uint)Vector256<byte>.Count);

            uint mask;
            nuint lengthToExamine = length - (nuint)Vector256<byte>.Count;

            Vector256<byte> maskVec;
            i = 0;

            while (i < lengthToExamine)
            {
                maskVec = Vector256.Equals(
                    Vector256.LoadUnsafe(ref first, i),
                    Vector256.LoadUnsafe(ref second, i));

                mask = maskVec.ExtractMostSignificantBits();
                if (mask != 0xFFFFFFFFu)
                {
                    goto Found256;
                }

                i += (nuint)Vector256<byte>.Count;
            }

            // Do final compare as Vector256<byte>.Count from end rather than start
            i = lengthToExamine;
            maskVec = Vector256.Equals(
                Vector256.LoadUnsafe(ref first, i),
                Vector256.LoadUnsafe(ref second, i));

            mask = maskVec.ExtractMostSignificantBits();
            if (mask != 0xFFFFFFFFu)
            {
                goto Found256;
            }

            return length;

        Found256:
            mask = ~mask;
            return i + uint.TrailingZeroCount(mask);
        }
    }
}
