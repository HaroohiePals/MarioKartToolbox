namespace HaroohiePals.IO.Reference;

public interface IReferenceSerializer<TRef, TId> where TRef : IReferenceable<TRef>
{
    TId Serialize(Reference<TRef> reference);
}