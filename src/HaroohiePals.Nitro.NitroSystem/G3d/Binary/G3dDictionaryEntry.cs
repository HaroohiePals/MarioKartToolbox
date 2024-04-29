#nullable enable

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary;

/// <summary>
/// Record representing an entry of a <see cref="G3dDictionary{TData}"/>, consisting
/// of a <paramref name="Name"/> and <paramref name="Data"/> of type <typeparamref name="TData"/>.
/// </summary>
/// <typeparam name="TData">The type of the data in the dictionary.</typeparam>
/// <param name="Name">The name of the entry.</param>
/// <param name="Data">The data of the entry.</param>
public sealed record G3dDictionaryEntry<TData>(string Name, TData Data)
    where TData : notnull, IG3dDictionaryData, new();
