using HaroohiePals.NitroKart.MapData.Intermediate;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using HaroohiePals.Validation;

namespace HaroohiePals.NitroKart.Validation.MapData.Sections;

internal class MkdsEnemyPointValidationRule : MkdsMapDataEntryValidationRule<MkdsEnemyPoint>
{
    public override string Name => "Enemy Point";

    protected override IReadOnlyList<ValidationError> Validate(MkdsMapData mapData, MkdsEnemyPoint entry)
    {
        var errors = new List<ValidationError>();

        var rule = new MkdsPointWifiCoordOutOfRangeValidationRule(Name);
        errors.AddRange(rule.Validate(entry));

        return errors;
    }
}