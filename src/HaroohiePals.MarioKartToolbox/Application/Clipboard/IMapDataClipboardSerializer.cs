#nullable enable
using HaroohiePals.MarioKart.MapData;
using System.Collections.Generic;

namespace HaroohiePals.MarioKartToolbox.Application.Clipboard;

interface IMapDataClipboardSerializer
{
    MapDataClipboardSerializationResult Serialize(IEnumerable<IMapDataEntry> entries);

    IReadOnlyCollection<IMapDataEntry> Deserialize(string clipboardString,
        MapDataClipboardSerializationResult? lastClipboardSerializationResult);
}