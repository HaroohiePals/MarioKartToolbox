using System.Linq.Expressions;
using System.Reflection;

namespace HaroohiePals.Actions;

/// <summary>
/// Static class with extension methods for creating <see cref="Actions.SetPropertyAction"/>s.
/// </summary>
public static class SetPropertyActionExtensions
{
    private const string NO_MEMBER_ACCESS_EXCEPTION_MESSAGE = "Expression is not a member access.";
    private const string NO_PROPERTY_ACCESS_EXCEPTION_MESSAGE = "Expression is not a property member access.";
    private const string SUPPLIED_PARAMETER_NOT_USED_EXCEPTION_MESSAGE = "Expression does not use the supplied parameter.";

    /// <summary>
    /// Generates a <see cref="Actions.SetPropertyAction"/> that sets the property specified by
    /// <paramref name="property"/> to <paramref name="newValue"/>.
    /// </summary>
    /// <typeparam name="TObject">The type of the object containing the property.</typeparam>
    /// <typeparam name="TValue">The type of the value of the property.</typeparam>
    /// <param name="self">The object containing the property.</param>
    /// <param name="property">An expression specifying the property to set.</param>
    /// <param name="newValue">The new value for the property.</param>
    /// <returns>The generated <see cref="Actions.SetPropertyAction"/>.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="property"/> is not a valid
    /// property access expression.</exception>
    public static SetPropertyAction SetPropertyAction<TObject, TValue>(this TObject self,
        Expression<Func<TObject, TValue>> property, TValue newValue) where TObject : class
    {
        if (property.Body is not MemberExpression memberExpression)
            throw new ArgumentException(NO_MEMBER_ACCESS_EXCEPTION_MESSAGE, nameof(property));

        if (memberExpression.Expression != property.Parameters[0])
            throw new ArgumentException(SUPPLIED_PARAMETER_NOT_USED_EXCEPTION_MESSAGE, nameof(property));

        if (memberExpression.Member is not PropertyInfo propertyInfo)
            throw new ArgumentException(NO_PROPERTY_ACCESS_EXCEPTION_MESSAGE, nameof(property));
        
        return new SetPropertyAction(self, propertyInfo, newValue);
    }
}