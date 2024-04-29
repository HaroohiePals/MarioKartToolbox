using System;
using System.Collections.Generic;

namespace HaroohiePals.IO.Reference;

public class ReferenceResolverCollection : IReferenceResolverCollection
{
    private const string NO_RESOLVER_FOUND_EXCEPTION_MESSAGE = "No suitable resolver found for Reference<{0}>";

    private readonly Dictionary<Type, object> _resolvers = new();

    public void RegisterResolver<T>(IReferenceResolver<T> resolver)
        where T : IReferenceable<T>
    {
        _resolvers[typeof(T)] = resolver;
    }

    public Reference<T> Resolve<T>(Reference<T> reference, Reference<T>.RemoveReferenceFunc removeFunc)
        where T : IReferenceable<T>
    {
        if (reference is null)
            throw new ArgumentNullException(nameof(reference));
        if (reference.IsResolved)
            return reference;
        if (!_resolvers.TryGetValue(typeof(T), out var resolver))
            throw new ReferenceResolveException(string.Format(NO_RESOLVER_FOUND_EXCEPTION_MESSAGE, typeof(T)));

        return ((IReferenceResolver<T>)resolver).Resolve(reference, removeFunc);
    }
}