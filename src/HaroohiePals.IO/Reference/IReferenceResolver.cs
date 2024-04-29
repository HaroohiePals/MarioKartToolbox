namespace HaroohiePals.IO.Reference;

public interface IReferenceResolver<T> where T : IReferenceable<T>
{
    Reference<T> Resolve(Reference<T> reference, Reference<T>.RemoveReferenceFunc removeFunc);
}