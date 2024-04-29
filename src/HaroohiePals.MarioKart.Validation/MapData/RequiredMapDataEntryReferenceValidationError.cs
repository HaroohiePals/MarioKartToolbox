using HaroohiePals.IO.Reference;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.Validation;
using System.Linq.Expressions;

namespace HaroohiePals.NitroKart.Validation.MapData.Sections;

public class RequiredMapDataEntryReferenceValidationError<TRef> : ValidationError
    where TRef : IReferenceable<TRef>, IMapDataEntry
{
    public RequiredMapDataEntryReferenceValidationError(IValidationRule rule, IMapDataEntry source, Expression<Func<Reference<TRef>?>> propertyExpression) 
        : base(rule, ErrorLevel.Error, $"Required reference: ", source, false)
    {
        if (propertyExpression.Body is MemberExpression memberExpression)
            Message += memberExpression.Member.Name;
    }
}
