using HaroohiePals.IO;
using System;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary;

/// <summary>
/// Interface for the data of a <see cref="G3dDictionary{TData}"/>.
/// </summary>
public interface IG3dDictionaryData
{
    /// <summary>
    /// Size of this dictionary data in bytes.
    /// </summary>
    static abstract ushort DataSize { get; }

    /// <summary>
    /// Reads the data using the given <paramref name="reader"/>.
    /// </summary>
    /// <param name="reader">The reader to read the data with.</param>
    void Read(EndianBinaryReaderEx reader);

    /// <summary>
    /// Writes the data using the given <paramref name="writer"/>.
    /// </summary>
    /// <param name="writer">The writer to write the data with.</param>
    void Write(EndianBinaryWriterEx writer)
        => throw new NotImplementedException();
}
