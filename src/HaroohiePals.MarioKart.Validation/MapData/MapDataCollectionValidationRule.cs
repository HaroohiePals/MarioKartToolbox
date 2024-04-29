using HaroohiePals.MarioKart.MapData;
using HaroohiePals.Validation;

namespace HaroohiePals.MarioKart.Validation.MapData;

public class MapDataCollectionValidationRule<TMapData, TEntry, TValidationRule> 
    : IValidationRule<(TMapData MapData, MapDataCollection<TEntry> Collection)>
    where TMapData : IMapData
    where TEntry : IMapDataEntry
    where TValidationRule : MapDataEntryValidationRule<TMapData, TEntry>, new()
{
    private readonly TValidationRule _entryValidationRule = new();
    public string Name => "Map Data Collection";

    public MapDataCollectionValidationRule()
    {

    }

    public MapDataCollectionValidationRule(TValidationRule entryValidationRule)
    {
        _entryValidationRule = entryValidationRule;
    }

    public IReadOnlyList<ValidationError> Validate((TMapData MapData, MapDataCollection<TEntry> Collection) obj)
    {
        var errors = new List<ValidationError>();

        foreach (var entry in obj.Collection)
            errors.AddRange(_entryValidationRule.Validate((obj.MapData, entry)));

        return errors;
    }
}
