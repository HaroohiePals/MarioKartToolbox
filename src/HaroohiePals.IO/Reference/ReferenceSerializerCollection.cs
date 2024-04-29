using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace HaroohiePals.IO.Reference;

public class ReferenceSerializerCollection : IReferenceSerializerCollection
{
    private const string CANNOT_SERIALIZE_UNRESOLVED_EXCEPTION_MESSAGE = "Cannot serialize unresolved reference";
    private const string NO_SERIALIZER_FOUND_EXCEPTION_MESSAGE = "No suitable serializer found from Reference<{0}> to {1}";

    private readonly Dictionary<(Type, Type), object> _serializers = new();

    public void RegisterSerializer<TRef, TId>(IReferenceSerializer<TRef, TId> serializer)
        where TRef : IReferenceable<TRef>
    {
        _serializers[(typeof(TRef), typeof(TId))] = serializer; 
    }

    public TId Serialize<TRef, TId>(Reference<TRef> reference)
        where TRef : IReferenceable<TRef>
    {
        if (reference is null)
            throw new ArgumentNullException(nameof(reference));
        if (!reference.IsResolved)
            throw new ArgumentException(CANNOT_SERIALIZE_UNRESOLVED_EXCEPTION_MESSAGE, nameof(reference));
        if (!_serializers.TryGetValue((typeof(TRef), typeof(TId)), out var serializer))
            throw new SerializationException(string.Format(NO_SERIALIZER_FOUND_EXCEPTION_MESSAGE, typeof(TRef), typeof(TId)));

        return ((IReferenceSerializer<TRef, TId>)serializer).Serialize(reference);
    }

    public TId SerializeOrDefault<TRef, TId>(Reference<TRef> reference, TId defaultValue)
        where TRef : IReferenceable<TRef>
    {
        if (reference is null)
            return defaultValue;
        if (!reference.IsResolved)
            throw new ArgumentException(CANNOT_SERIALIZE_UNRESOLVED_EXCEPTION_MESSAGE, nameof(reference));
        if (!_serializers.TryGetValue((typeof(TRef), typeof(TId)), out var serializer))
            throw new SerializationException(string.Format(NO_SERIALIZER_FOUND_EXCEPTION_MESSAGE, typeof(TRef), typeof(TId)));

        return ((IReferenceSerializer<TRef, TId>)serializer).Serialize(reference);
    }

    public bool CanSerialize<TRef, TId>() => _serializers.ContainsKey((typeof(TRef), typeof(TId)));
}