using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections.MobjSettings;
using HaroohiePals.Validation;

namespace HaroohiePals.NitroKart.Validation.MapData.Sections.MobjSettings;

internal class Fireball2SettingsValidationRule : IMkdsMobjSettingsValidationRule<Fireball2Settings>
{
    public string Name => "Fireball2 Settings";

    public IReadOnlyList<ValidationError> Validate((MkdsMapObject, Fireball2Settings) obj)
    {
        var error = new List<ValidationError>();

        if (obj.Item2.NrArms < 1 || obj.Item2.NrArms > 20)
            error.Add(new MkdsMobjSettingsValidationError(this, "Number of arms should be at least 1 and at most 20", obj.Item1));

        if (obj.Item2.FireballsPerArm < 1 || obj.Item2.FireballsPerArm > 20)
            error.Add(new MkdsMobjSettingsValidationError(this, "Number of fireballs per arm should be at least 1 and at most 20", obj.Item1));

        return error;
    }
}
