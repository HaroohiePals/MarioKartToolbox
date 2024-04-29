using System.ComponentModel;

namespace HaroohiePals.IO.Reference;

/// <summary>
/// Interface for objects of type <typeparamref name="T"/> that can be referenced.
/// </summary>
/// <typeparam name="T">The referencable type.</typeparam>
public interface IReferenceable<T> where T : IReferenceable<T>
{
    [Browsable(false)]
    protected ReferenceHolder<T> ReferenceHolder { get; }

    public Reference<T> GetReference(Reference<T>.RemoveReferenceFunc removeFunc)
    {
        var reference = new Reference<T>((T)this, removeFunc);
        ReferenceHolder.RegisterReference(reference);
        return reference;
    }

    void ReleaseReference(Reference<T> reference)
    {
        if (reference == null || !reference.Target.Equals(this))
            return;

        ReferenceHolder.ReleaseReference(reference);
    }

    void ReleaseAllReferences() => ReferenceHolder.ReleaseAllReferences();

    [Browsable(false)]
    bool HasReferences => ReferenceHolder.HasReferences;
}