namespace HaroohiePals.Validation;

public interface IValidator<TObj>
{
    List<IValidationRule<TObj>> Rules { get; set; }
    IReadOnlyList<ValidationError> Validate(TObj obj);
}
