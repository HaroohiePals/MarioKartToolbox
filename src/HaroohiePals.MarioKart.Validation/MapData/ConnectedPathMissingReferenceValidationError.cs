using HaroohiePals.MarioKart.MapData;
using HaroohiePals.Validation;

namespace HaroohiePals.MarioKart.Validation.MapData;

public class ConnectedPathMissingReferenceValidationError(
    IValidationRule rule,
    ErrorLevel level,
    string propertyName,
    IMapDataEntry source)
    : ValidationError(rule, level, $"Missing path reference: {propertyName}", source, false);
