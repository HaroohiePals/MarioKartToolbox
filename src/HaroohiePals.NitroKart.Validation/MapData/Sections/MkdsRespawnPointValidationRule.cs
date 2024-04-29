using HaroohiePals.NitroKart.MapData.Intermediate;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using HaroohiePals.Validation;

namespace HaroohiePals.NitroKart.Validation.MapData.Sections;

internal class MkdsRespawnPointValidationRule : MkdsMapDataEntryValidationRule<MkdsRespawnPoint>
{
    public override string Name => "Respawn";

    protected override IReadOnlyList<ValidationError> Validate(MkdsMapData mapData, MkdsRespawnPoint entry)
    {
        var errors = new List<ValidationError>();

        ValidateReference(errors, entry, () => entry.ItemPoint, isRequired: !mapData.IsMgStage);
        ValidateReference(errors, entry, () => entry.EnemyPoint, isRequired: !mapData.IsMgStage);
        ValidateReference(errors, entry, () => entry.MgEnemyPoint, isRequired: mapData.IsMgStage);

        return errors;
    }
}