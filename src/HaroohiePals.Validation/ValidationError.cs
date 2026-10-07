using HaroohiePals.Actions;
using System.Diagnostics.CodeAnalysis;

namespace HaroohiePals.Validation;

public abstract class ValidationError(
    IValidationRule rule,
    ErrorLevel level,
    string message,
    object? source,
    bool isFixable)
{
    public IValidationRule Rule { get; } = rule;
    public ErrorLevel Level { get; } = level;
    public string Message { get; protected init; } = message;
    public object? Source { get; } = source;
    public bool IsFixable { get; } = isFixable;

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
