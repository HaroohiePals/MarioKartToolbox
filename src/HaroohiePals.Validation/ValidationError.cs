using HaroohiePals.Actions;
using System.Diagnostics.CodeAnalysis;

namespace HaroohiePals.Validation;

public abstract class ValidationError
{
    public IValidationRule Rule { get; }
    public ErrorLevel Level { get; }
    public string Message { get; protected set; }
    public object Source { get; }
    public bool IsFixable { get; }

    public ValidationError(IValidationRule rule, ErrorLevel level, string message, object source, bool isFixable)
    {
        Rule = rule;
        Level = level;
        Message = message;
        Source = source;
        IsFixable = isFixable;
    }

    protected virtual IAction Fix() => throw new UnfixableValidationErrorException();

    /// <summary>
    /// Returns the fix action if the error is fixable
    /// </summary>
    /// <param name="action"></param>
    /// <returns>Actions to fix the error</returns>
    public bool TryFix([NotNullWhen(returnValue: true)] out IAction? action)
    {
        action = null;

        if (!IsFixable)
            return false;

        try
        {
            action = Fix();
            return true;
        }
        catch (UnfixableValidationErrorException)
        {
            return false;
        }
    }
}
