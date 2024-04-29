namespace HaroohiePals.IO.Reference;

public interface IReferenceResolverCollection
{
    Reference<T> Resolve<T>(Reference<T> reference, Reference<T>.RemoveReferenceFunc removeFunc)
        where T : IReferenceable<T>;
}