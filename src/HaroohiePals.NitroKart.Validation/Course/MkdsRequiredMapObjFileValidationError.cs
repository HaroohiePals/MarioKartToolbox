using HaroohiePals.Validation;

namespace HaroohiePals.NitroKart.Validation.Course;

public class MkdsRequiredMapObjFileValidationError : ValidationError
{
    public MkdsRequiredMapObjFileValidationError(IValidationRule rule, string fileName, object source) 
        : base(rule, ErrorLevel.Error, $"Missing Required MapObj File: {fileName}", source, false)
    {
    }
}
