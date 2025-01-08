using System;
using System.Buffers;
using System.Buffers.Binary;

namespace HaroohiePals.IO.Compression;

/// <summary>
/// Class implementing the Yaz0 compression algorithm.
/// </summary>
public sealed class Yaz0CompressionAlgorithm : ICompressionAlgorithm
{
    private const int MINIMUM_RUN_LENGTH = 3;
    private const int MAXIMUM_RUN_LENGTH = 273;
    private const int HEADER_LENGTH = 16;
    private const int MAXIMUM_BLOCK_SIZE = 1 + 8 * 3;

    /// <summary>
    /// When <see langword="true"/> the compression algorithm will try if skipping a byte results
    /// in a better run length. This improves the compression ratio, but makes compression about twice as slow.
    /// When <see langword="false"/> look ahead is not used, resulting in faster compression.
    /// </summary>
    public bool IsCompressionLookAheadEnabled { get; }

    public Yaz0CompressionAlgorithm()
        : this(isCompressionLookAheadEnabled: true) { }

    public Yaz0CompressionAlgorithm(bool isCompressionLookAheadEnabled)
    {
        IsCompressionLookAheadEnabled = isCompressionLookAheadEnabled;
    }

    /// <inheritdoc cref="ICompressionAlgorithm.Compress(ReadOnlySpan{byte})"/>
    public byte[] Compress(ReadOnlySpan<byte> sourceData)
    {
        var window = new CompressionWindow(sourceData, MINIMUM_RUN_LENGTH, MAXIMUM_RUN_LENGTH);
        var destination = new ArrayBufferWriter<byte>(sourceData.Length);

        var headerSpan = destination.GetSpan(HEADER_LENGTH);
        "Yaz0"u8.CopyTo(headerSpan);
        BinaryPrimitives.WriteInt32BigEndian(headerSpan[4..], sourceData.Length);
        headerSpan[8..HEADER_LENGTH].Fill(0);
        destination.Advance(HEADER_LENGTH);

        int position = 0;
        int blockSize = 1;
        int bit = 8;
        byte blockHeader = 0;
        var blockBuffer = destination.GetSpan(MAXIMUM_BLOCK_SIZE);
        int foundNextPosition = -1;
        int foundNextLength = 0;
        while (position < sourceData.Length)
        {
            blockHeader <<= 1;

            int foundPosition = foundNextPosition;
            int foundLength = foundNextLength;
            if (foundLength >= MINIMUM_RUN_LENGTH ||
                window.TryFindRun(out foundPosition, out foundLength))
            {
                if (IsCompressionLookAheadEnabled)
                {
                    // look ahead
                    window.Slide(1);
                    if (foundLength < MAXIMUM_RUN_LENGTH &&
                        window.TryFindRun(out foundNextPosition, out foundNextLength) &&
                        foundLength < foundNextLength)
                    {
                        // better run found if we skip one byte
                        foundLength = 0;
                    }
                    else
                    {
                        foundNextLength = 0;
                    }
                }
            }
            else
            {
                foundLength = 0;
            }

            if (foundLength >= MINIMUM_RUN_LENGTH)
            {
                int offset = position - foundPosition - 1;

                if (foundLength < 18)
                {
                    blockBuffer[blockSize++] = (byte)((byte)((foundLength - 2) << 4) | (byte)(offset >> 8));
                    blockBuffer[blockSize++] = (byte)offset;
                }
                else
                {
                    blockBuffer[blockSize++] = (byte)(offset >> 8);
                    blockBuffer[blockSize++] = (byte)offset;
                    blockBuffer[blockSize++] = (byte)((byte)foundLength - 18);
                }

                position += foundLength;
            }
            else
            {
                blockHeader |= 1;
                blockBuffer[blockSize++] = sourceData[position++];
            }

            window.Slide((uint)(position - window.Position));

            if (--bit == 0)
            {
                blockBuffer[0] = blockHeader;
                destination.Advance(blockSize);
                blockBuffer = destination.GetSpan(MAXIMUM_BLOCK_SIZE);

                blockHeader = 0;
                blockSize = 1;
                bit = 8;
            }
        }

        blockBuffer[0] = (byte)(blockHeader << bit);
        destination.Advance(blockSize);

        return destination.WrittenSpan.ToArray();
    }

    /// <inheritdoc cref="ICompressionAlgorithm.Decompress(ReadOnlySpan{byte})"/>
    public byte[] Decompress(ReadOnlySpan<byte> compressedData)
    {
        uint outputSize = BinaryPrimitives.ReadUInt32BigEndian(compressedData[4..]);
        var destination = new byte[outputSize];
        int sourceOffset = HEADER_LENGTH;
        int destinationOffset = 0;
        while (true)
        {
            byte header = compressedData[sourceOffset++];
            for (int i = 0; i < 8; i++)
            {
                if ((header & 0x80) != 0)
                {
                    destination[destinationOffset++] = compressedData[sourceOffset++];
                }
                else
                {
                    byte b = compressedData[sourceOffset++];
                    int offs = ((b & 0xF) << 8 | compressedData[sourceOffset++]) + 1;
                    int length = (b >> 4) + 2;
                    if (length == 2)
                    {
                        length = compressedData[sourceOffset++] + 18;
                    }

                    for (int j = 0; j < length; j++)
                    {
                        destination[destinationOffset] = destination[destinationOffset - offs];
                        destinationOffset++;
                    }
                }

                if (destinationOffset >= outputSize)
                {
                    return destination;
                }

                header <<= 1;
            }
        }
    }
}
