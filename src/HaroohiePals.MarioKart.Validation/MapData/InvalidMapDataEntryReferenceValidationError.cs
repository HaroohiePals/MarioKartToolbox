using HaroohiePals.Actions;
using HaroohiePals.IO.Reference;
using HaroohiePals.MarioKart.Actions;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.Validation;
using System.Linq.Expressions;

namespace HaroohiePals.MarioKart.Validation.MapData;

public class InvalidMapDataEntryReferenceValidationError<TEntry, TRef> : ValidationError
    where TEntry : IMapDataEntry
    where TRef : IReferenceable<TRef>, IMapDataEntry
{
    private readonly string _propertyName = "";
    private readonly Reference<TRef>? _oldRef;

    private readonly TEntry _entry;

    public InvalidMapDataEntryReferenceValidationError(IValidationRule rule, TEntry source, Expression<Func<Reference<TRef>?>> propertyExpression)
        : base(rule, ErrorLevel.Fatal, $"Invalid reference: ", source, true)
    {
        _entry = source;

        _oldRef = propertyExpression.Compile().Invoke();
        if (propertyExpression.Body is MemberExpression memberExpression)
            _propertyName = memberExpression.Member.Name;

        Message += _propertyName;
    }

    protected override IAction Fix()
        => new SetMapDataEntryReferenceAction<TRef>(_entry, _propertyName, _oldRef, null);
}
