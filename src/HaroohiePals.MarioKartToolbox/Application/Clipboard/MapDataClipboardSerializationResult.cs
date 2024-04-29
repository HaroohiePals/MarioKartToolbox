#nullable enable
using HaroohiePals.MarioKart.MapData;
using System.Collections.Generic;

namespace HaroohiePals.MarioKartToolbox.Application.Clipboard;

class MapDataClipboardSerializationResult
{
    public string ClipboardString { get; }

    public string UniqueId { get; }

    public IReadOnlyDictionary<string, IMapDataEntry> LocalMapping { get; }

    public MapDataClipboardSerializationResult(string clipboardString, string uniqueId, IReadOnlyDictionary<string, IMapDataEntry> localMapping)
    {
        ClipboardString = clipboardString;
        UniqueId = uniqueId;
        LocalMapping = localMapping;
    }
}