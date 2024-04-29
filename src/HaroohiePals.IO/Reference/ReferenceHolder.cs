using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace HaroohiePals.IO.Reference;

[Browsable(false)]
public class ReferenceHolder<T> where T : IReferenceable<T>
{
    private readonly HashSet<Reference<T>> _references = new();

    [Browsable(false)]
    public bool HasReferences => _references.Count > 0;

    public void RegisterReference(Reference<T> reference)
    {
        _references.Add(reference);
    }

    public void ReleaseReference(Reference<T> reference)
    {
        reference.InvokeRemove();
        _references.Remove(reference);
    }

    public void ReleaseAllReferences()
    {
        while(_references.Count > 0)
            ReleaseReference(_references.First());
    }
}