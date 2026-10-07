using HaroohiePals.Validation;

namespace HaroohiePals.NitroKart.Validation.MapData.Sections.MobjSettings;

class MkdsMobjSettingsValidationError(IValidationRule rule, string message, object source)
    : ValidationError(rule, ErrorLevel.Error, $"Invalid MapObj Setting: {message}", source, false);
