using HaroohiePals.Validation;

namespace HaroohiePals.NitroKart.Validation.MapData.Sections.MobjSettings;

internal class MkdsMobjSettingsValidationError : ValidationError
{
    public MkdsMobjSettingsValidationError(IValidationRule rule, string message, object source) 
        : base(rule, ErrorLevel.Error, $"Invalid MapObj Setting: {message}", source, false)
    {
    }
}
