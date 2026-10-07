using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using HaroohiePals.Validation;

namespace HaroohiePals.NitroKart.Validation.MapData.Sections;

class MkdsStartPointMgEnemyPointDistanceValidationError(
    IValidationRule rule,
    MkdsStartPoint source)
    : ValidationError(rule, ErrorLevel.Error, "No battle enemy point within detection range", source, false);
