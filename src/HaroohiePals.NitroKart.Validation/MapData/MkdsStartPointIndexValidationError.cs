using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using HaroohiePals.Validation;

namespace HaroohiePals.NitroKart.Validation.MapData;

class MkdsStartPointIndexValidationError(IValidationRule rule, MkdsStartPoint source, string message)
    : ValidationError(rule, ErrorLevel.Warning, message, source, false);
