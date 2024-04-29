using HaroohiePals.MarioKart.MapData;
using HaroohiePals.Validation;

namespace HaroohiePals.MarioKart.Validation.MapData;

public class ConnectedPathValidationRule<TMapData, TPath, TPoint, TPointValidationRule> 
    : MapDataEntryValidationRule<TMapData, TPath>
    where TMapData : IMapData
    where TPath : ConnectedPath<TPath, TPoint>, new()
    where TPoint : IMapDataEntry
    where TPointValidationRule : MapDataEntryValidationRule<TMapData, TPoint>, new()
{
    public override string Name => "Connected Path";

    private readonly TPointValidationRule _pointValidationRule = new();

    protected override IReadOnlyList<ValidationError> Validate(TMapData mapData, TPath entry)
    {
        var errors = new List<ValidationError>();
        foreach (var point in entry.Points)
            errors.AddRange(_pointValidationRule.Validate((mapData, point)));
        return errors;
    }
}
