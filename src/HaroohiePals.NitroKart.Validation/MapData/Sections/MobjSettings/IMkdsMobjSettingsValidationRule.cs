using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections.MobjSettings;
using HaroohiePals.Validation;

namespace HaroohiePals.NitroKart.Validation.MapData.Sections.MobjSettings;

internal interface IMkdsMobjSettingsValidationRule<TSettings> : IValidationRule<(MkdsMapObject, TSettings)>
    where TSettings : MkdsMobjSettings
{
}
