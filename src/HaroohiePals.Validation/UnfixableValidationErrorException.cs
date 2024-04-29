namespace HaroohiePals.Validation;

public class UnfixableValidationErrorException : Exception
{
    public UnfixableValidationErrorException() : base("This validation error doesn't provide a fix.")
    {

    }
}