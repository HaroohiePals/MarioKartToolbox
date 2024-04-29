using HaroohiePals.IO.Reference;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.Validation.MapData.Sections;
using HaroohiePals.Validation;
using System.Linq.Expressions;

namespace HaroohiePals.MarioKart.Validation.MapData;

public abstract class MapDataEntryValidationRule<TMapData, TEntry> : IValidationRule<(TMapData MapData, TEntry Entry)>
    where TEntry : IMapDataEntry
{
    public abstract string Name { get; }

    public IReadOnlyList<ValidationError> Validate((TMapData MapData, TEntry Entry) obj)
        => Validate(obj.MapData, obj.Entry);

    protected abstract IReadOnlyList<ValidationError> Validate(TMapData mapData, TEntry entry);

    protected void ValidateReference<TRef>(ICollection<ValidationError> errors, TEntry entry,
        Expression<Func<Reference<TRef>?>> propertyExpression, bool isRequired)
        where TRef : IReferenceable<TRef>, IMapDataEntry
    {
        var reference = propertyExpression.Compile().Invoke();

        if (reference?.IsResolved is false)
            errors.Add(new InvalidMapDataEntryReferenceValidationError<TEntry, TRef>(this, entry, propertyExpression));

        if (isRequired && reference is null)
            errors.Add(new RequiredMapDataEntryReferenceValidationError<TRef>(this, entry, propertyExpression));
    }
}
