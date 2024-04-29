namespace HaroohiePals.IO.Reference;

public static class IReferenceableExtensions
{
    public static Reference<T> GetReference<T>(this IReferenceable<T> referenceable,
        Reference<T>.RemoveReferenceFunc removeFunc) where T : IReferenceable<T>
        => referenceable.GetReference(removeFunc);

    public static void ReleaseAllReferences<T>(this IReferenceable<T> referenceable) where T : IReferenceable<T>
        => referenceable.ReleaseAllReferences();
}