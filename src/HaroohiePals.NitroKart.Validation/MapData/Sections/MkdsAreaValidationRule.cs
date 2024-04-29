using HaroohiePals.NitroKart.MapData.Intermediate;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using HaroohiePals.Validation;

namespace HaroohiePals.NitroKart.Validation.MapData.Sections;

internal class MkdsAreaValidationRule : MkdsMapDataEntryValidationRule<MkdsArea>
{
    public override string Name => "Area";

    protected override IReadOnlyList<ValidationError> Validate(MkdsMapData mapData, MkdsArea entry)
    {
        var errors = new List<ValidationError>();

        ValidateReference(errors, entry, () => entry.EnemyPoint, isRequired: false);
        ValidateReference(errors, entry, () => entry.MgEnemyPoint, isRequired: false);
        ValidateReference(errors, entry, () => entry.Camera, isRequired: false);

        return errors;
    }
}
