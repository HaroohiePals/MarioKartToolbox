using HaroohiePals.NitroKart.MapData.Intermediate;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using HaroohiePals.Validation;

namespace HaroohiePals.NitroKart.Validation.MapData.Sections;

internal class MkdsItemPointValidationRule : MkdsMapDataEntryValidationRule<MkdsItemPoint>
{
    public override string Name => "Item Point";

    protected override IReadOnlyList<ValidationError> Validate(MkdsMapData mapData, MkdsItemPoint entry)
    {
        var errors = new List<ValidationError>();

        var rule = new MkdsPointWifiCoordOutOfRangeValidationRule(Name);
        errors.AddRange(rule.Validate(entry));

        return errors;
    }
}
