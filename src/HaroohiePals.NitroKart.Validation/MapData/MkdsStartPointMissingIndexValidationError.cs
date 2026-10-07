using HaroohiePals.Actions;
using HaroohiePals.NitroKart.MapData.Intermediate;
using HaroohiePals.Validation;

namespace HaroohiePals.NitroKart.Validation.MapData;

class MkdsStartPointMissingIndexValidationError(
    IValidationRule rule,
    MkdsMapData mapData,
    IEnumerable<int> missingIndices)
    : ValidationError(rule, ErrorLevel.Error,
        $"Missing start point index: {string.Join(", ", missingIndices)} (battle start slots use indices 0-{MkdsStartPointCollectionValidationRule.MG_START_POINT_COUNT - 1})",
        mapData.StartPoints[0], true)
{
    protected override IAction Fix()
    {
        return new BatchAction(mapData.StartPoints
            .Take(MkdsStartPointCollectionValidationRule.MG_START_POINT_COUNT)
            .Select((x, i) => x.SetPropertyAction(y => y.Index, (short)i)));
    }
}
