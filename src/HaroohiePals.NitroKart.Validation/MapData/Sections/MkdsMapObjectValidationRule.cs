using HaroohiePals.NitroKart.MapData.Intermediate;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections.MobjSettings;
using HaroohiePals.NitroKart.MapObj;
using HaroohiePals.NitroKart.Validation.MapData.Sections.MobjSettings;
using HaroohiePals.Validation;

namespace HaroohiePals.NitroKart.Validation.MapData.Sections;

internal class MkdsMapObjectValidationRule : MkdsMapDataEntryValidationRule<MkdsMapObject>
{
    public override string Name => "Map Object";

    private readonly IMkdsMapObjDatabase? _mobjDatabase;

    public MkdsMapObjectValidationRule()
    {

    }

    public MkdsMapObjectValidationRule(IMkdsMapObjDatabase mobjDatabase)
    {
        _mobjDatabase = mobjDatabase;
    }

    protected override IReadOnlyList<ValidationError> Validate(MkdsMapData mapData, MkdsMapObject entry)
    {
        var errors = new List<ValidationError>();

        var mobjDatabaseInfo = _mobjDatabase?.GetById(entry.ObjectId);

        bool isPathRequired = mobjDatabaseInfo?.IsPathRequired ?? false;
        bool isTimeTrialVisible = mobjDatabaseInfo?.IsTimeTrialVisible ?? true;

        ValidateReference(errors, entry, () => entry.Path, isRequired: isPathRequired);

        if (entry.TTVisible != isTimeTrialVisible)
            errors.Add(new MkdsMapObjectTimeTrialVisibleValidationError(this, entry, isTimeTrialVisible));

        switch (entry.Settings)
        {
            case Fireball2Settings fireball2Settings:
                var rule = new Fireball2SettingsValidationRule();
                errors.AddRange(rule.Validate((entry, fireball2Settings)));
                break;
            default:
                break;
        }

        return errors;
    }
}
