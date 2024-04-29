using HaroohiePals.NitroKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using HaroohiePals.Validation;

namespace HaroohiePals.NitroKart.Validation.MapData.Sections;

internal class MkdsCameraValidationRule : MkdsMapDataEntryValidationRule<MkdsCamera>
{
    public override string Name => "Camera";

    protected override IReadOnlyList<ValidationError> Validate(MkdsMapData mapData, MkdsCamera entry)
    {
        var errors = new List<ValidationError>();

        ValidateReference(errors, entry, () => entry.Path, isRequired: 
            entry.Type is MkdsCameraType.RouteLookAtDriver or MkdsCameraType.RouteLookAtTargets);
        ValidateReference(errors, entry, () => entry.NextCamera, isRequired: false);

        return errors;
    }

}
