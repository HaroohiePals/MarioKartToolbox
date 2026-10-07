using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using HaroohiePals.Validation;

namespace HaroohiePals.NitroKart.Validation.MapData;

class MkdsStartPointCountValidationError(IValidationRule rule, ErrorLevel level, MkdsStartPoint? source, string message)
    : ValidationError(rule, level, message, source, false);
