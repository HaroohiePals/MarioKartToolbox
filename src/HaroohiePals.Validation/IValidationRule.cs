namespace HaroohiePals.Validation;

public interface IValidationRule
{
    string Name { get; }
}

public interface IValidationRule<TObj> : IValidationRule
{
    IReadOnlyList<ValidationError> Validate(TObj obj);
}