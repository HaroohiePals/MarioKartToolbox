using HaroohiePals.NitroKart.MapData.Intermediate;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using HaroohiePals.Validation;

namespace HaroohiePals.NitroKart.Validation.MapData.Sections;

class MkdsStartPointValidationRule : MkdsMapDataEntryValidationRule<MkdsStartPoint>
{
    private const double MEPO_DETECTION_RANGE = 200.0;

    private const double DETECTION_RANGE_MARGIN = 8.0;

    public override string Name => "Start Point";

    protected override IReadOnlyList<ValidationError> Validate(MkdsMapData mapData, MkdsStartPoint entry)
    {
        var errors = new List<ValidationError>();

        if (!mapData.IsMgStage || mapData.MgEnemyPaths is null || entry.Index is < 0 or > 7)
            return errors;

        double distance = mapData.MgEnemyPaths
            .SelectMany(x => x.Points)
            .Select(x => (x.Position - entry.Position).Length)
            .DefaultIfEmpty(double.PositiveInfinity)
            .Min();

        if (distance >= MEPO_DETECTION_RANGE - DETECTION_RANGE_MARGIN)
            errors.Add(new MkdsStartPointMgEnemyPointDistanceValidationError(this, entry));

        return errors;
    }
}
