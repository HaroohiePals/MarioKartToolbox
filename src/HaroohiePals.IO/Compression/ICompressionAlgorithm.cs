using System;

namespace HaroohiePals.IO.Compression;

/// <summary>
/// Interface for compression algorithms.
/// </summary>
public interface ICompressionAlgorithm
{
    /// <summary>
    /// Compresses the given <paramref name="sourceData"/>.
    /// </summary>
    /// <param name="sourceData">A span containing the source data.</param>
    /// <returns>A byte array containing the compressed data.</returns>
    byte[] Compress(ReadOnlySpan<byte> sourceData);

    /// <summary>
    /// Decompresses the given <paramref name="compressedData"/>.
    /// </summary>
    /// <param name="compressedData">A span containing the compressed data.</param>
    /// <returns>A byte array containing the decompressed data.</returns>
    byte[] Decompress(ReadOnlySpan<byte> compressedData);
}
