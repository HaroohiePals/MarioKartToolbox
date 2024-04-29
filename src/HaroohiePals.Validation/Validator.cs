namespace HaroohiePals.Validation;

public class Validator<TObj> : IValidator<TObj>
{
    public List<IValidationRule<TObj>> Rules { get; set; } = new List<IValidationRule<TObj>>();

    public IReadOnlyList<ValidationError> Validate(TObj obj)
    {
        var errors = new List<ValidationError>();

        foreach (var rule in Rules)
            errors.AddRange(rule.Validate(obj));

        return errors;
    }
}
