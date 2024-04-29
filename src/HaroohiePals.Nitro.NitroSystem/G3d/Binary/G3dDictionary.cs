#nullable enable

using HaroohiePals.IO;
using System;
using System.Collections.ObjectModel;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary;

/// <summary>
/// Class representing a G3D dictionary containing data with corresponding names.
/// </summary>
/// <typeparam name="TData">The type of the data contained in the dictionary.</typeparam>
public sealed class G3dDictionary<TData> : KeyedCollection<string, G3dDictionaryEntry<TData>>
    where TData : notnull, IG3dDictionaryData, new()
{
    private const string NAME_TOO_LONG_EXCEPTION_MESSAGE = "Names can be at most 16 characters long.";
    private const int NAME_LENGTH = 16;

    public new TData this[string name] => base[name].Data;

    public G3dDictionary() { }

    public G3dDictionary(EndianBinaryReaderEx reader)
    {
        G3dDictionarySerializer.ReadG3dDictionary<TData>(reader, this);
    }

    public void Add(string name, TData data)
    {
        Add(new G3dDictionaryEntry<TData>(name, data));
    }

    public void Insert(int index, string name, TData data)
    {
        Insert(index, new G3dDictionaryEntry<TData>(name, data));
    }

    public int IndexOf(string name)
    {
        return TryGetValue(name, out var value) ? IndexOf(value) : -1;
    }

    public void Write(EndianBinaryWriterEx writer)
    {
        G3dDictionarySerializer.WriteG3dDictionary(writer, this);
    }

    protected override string GetKeyForItem(G3dDictionaryEntry<TData> item)
    {
        return item.Name;
    }

    protected override void InsertItem(int index, G3dDictionaryEntry<TData> item)
    {
        ThrowIfNameTooLong(item.Name);
        base.InsertItem(index, item);
    }

    protected override void SetItem(int index, G3dDictionaryEntry<TData> item)
    {
        ThrowIfNameTooLong(item.Name);
        base.SetItem(index, item);
    }

    private void ThrowIfNameTooLong(string name)
    {
        if (name.Length > NAME_LENGTH)
        {
            throw new ArgumentException(NAME_TOO_LONG_EXCEPTION_MESSAGE, nameof(name));
        }
    }
}
