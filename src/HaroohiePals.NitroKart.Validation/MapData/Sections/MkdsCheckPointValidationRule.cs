using HaroohiePals.NitroKart.MapData.Intermediate;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using HaroohiePals.Validation;

namespace HaroohiePals.NitroKart.Validation.MapData.Sections;

internal class MkdsCheckPointValidationRule : MkdsMapDataEntryValidationRule<MkdsCheckPoint>
{
    public override string Name => "Checkpoint";

    protected override IReadOnlyList<ValidationError> Validate(MkdsMapData mapData, MkdsCheckPoint entry)
    {
        var errors = new List<ValidationError>();

        ValidateReference(errors, entry, () => entry.Respawn, isRequired: true);

        return errors;
    }
}
