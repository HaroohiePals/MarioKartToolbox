namespace HaroohiePals.IO.Reference;

public interface IReferenceSerializerCollection
{
    TId Serialize<TRef, TId>(Reference<TRef> reference)
        where TRef : IReferenceable<TRef>;

    TId SerializeOrDefault<TRef, TId>(Reference<TRef> reference, TId defaultValue)
        where TRef : IReferenceable<TRef>;

    bool CanSerialize<TRef, TId>();
}