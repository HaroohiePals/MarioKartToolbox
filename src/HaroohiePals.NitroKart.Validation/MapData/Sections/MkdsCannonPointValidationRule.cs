using HaroohiePals.NitroKart.MapData.Intermediate;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using HaroohiePals.Validation;

namespace HaroohiePals.NitroKart.Validation.MapData.Sections;

internal class MkdsCannonPointValidationRule : MkdsMapDataEntryValidationRule<MkdsCannonPoint>
{
    public override string Name => "CannonPoint";

    protected override IReadOnlyList<ValidationError> Validate(MkdsMapData mapData, MkdsCannonPoint entry)
    {
        var errors = new List<ValidationError>();

        ValidateReference(errors, entry, () => entry.MgEnemyPoint, isRequired: false);

        return errors;
    }
}
