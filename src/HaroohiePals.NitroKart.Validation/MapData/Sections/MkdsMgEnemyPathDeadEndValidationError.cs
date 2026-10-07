using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using HaroohiePals.Validation;

namespace HaroohiePals.NitroKart.Validation.MapData.Sections;

class MkdsMgEnemyPathDeadEndValidationError(
    IValidationRule rule,
    MkdsMgEnemyPath source)
    : ValidationError(rule, ErrorLevel.Error, "Isolated battle enemy point without links, CPUs reaching it will crash", source, false);
