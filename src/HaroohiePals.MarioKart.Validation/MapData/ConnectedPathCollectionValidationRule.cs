using HaroohiePals.MarioKart.MapData;
using HaroohiePals.Validation;

namespace HaroohiePals.MarioKart.Validation.MapData;

public class ConnectedPathCollectionValidationRule<TMapData, TPath, TPoint, TPointValidationRule>
    : IValidationRule<(TMapData MapData, MapDataCollection<TPath> Collection)>
    where TMapData : IMapData
    where TPath : ConnectedPath<TPath, TPoint>, new()
    where TPoint : IMapDataEntry
    where TPointValidationRule : MapDataEntryValidationRule<TMapData, TPoint>, new()
{
    public string Name => "Connected Path";

    private readonly MapDataCollectionValidationRule<TMapData, TPath,
        ConnectedPathValidationRule<TMapData, TPath, TPoint, TPointValidationRule>> _pathValidationRule = new();

    public IReadOnlyList<ValidationError> Validate((TMapData MapData, MapDataCollection<TPath> Collection) obj)
    {
        var errors = new List<ValidationError>();

        errors.AddRange(_pathValidationRule.Validate(obj));

        for (int i = 0; i < obj.Collection.Count; i++)
        {
            var path = obj.Collection[i];

            if (path.Next.Count == 0)
                errors.Add(new ConnectedPathMissingReferenceValidationError(this, ErrorLevel.Error, nameof(path.Next), path));

            // The game uses the first path's previous path to find the last point.
            // Other paths can lack a previous path (e.g. enemy paths only used by recalculation areas)
            if (i == 0 && path.Previous.Count == 0)
                errors.Add(new ConnectedPathMissingReferenceValidationError(this, ErrorLevel.Error, nameof(path.Previous), path));
        }

        return errors;
    }
}
