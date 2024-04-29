using HaroohiePals.MarioKart.MapData;
using HaroohiePals.MarioKart.Validation.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections.MobjSettings;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using HaroohiePals.NitroKart.Validation.MapData.Sections.MobjSettings;
using HaroohiePals.Validation;

namespace HaroohiePals.NitroKart.Validation.MapData.Sections;

internal abstract class MkdsMapDataEntryValidationRule<TEntry> : MapDataEntryValidationRule<MkdsMapData, TEntry>
    where TEntry : IMapDataEntry
{

}
