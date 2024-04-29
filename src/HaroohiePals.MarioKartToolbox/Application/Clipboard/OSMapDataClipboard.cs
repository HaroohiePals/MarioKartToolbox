#nullable enable
using HaroohiePals.MarioKart.MapData;
using System;
using System.Collections.Generic;
using TextCopy;

namespace HaroohiePals.MarioKartToolbox.Application.Clipboard;

class OSMapDataClipboard : IMapDataClipboard
{
    private readonly IClipboard _clipboard;
    private readonly IMapDataClipboardSerializer _mapDataClipboardSerializer;

    private MapDataClipboardSerializationResult? _lastSerializationResult;

    public OSMapDataClipboard(IClipboard clipboard, IMapDataClipboardSerializer mapDataClipboardSerializer)
    {
        _clipboard = clipboard;
        _mapDataClipboardSerializer = mapDataClipboardSerializer;
    }

    public void SetContents(IEnumerable<IMapDataEntry> entries)
    {
        var result = _mapDataClipboardSerializer.Serialize(entries);
        _clipboard.SetText(result.ClipboardString);
        _lastSerializationResult = result;
    }

    public IReadOnlyCollection<IMapDataEntry> GetContents()
    {
        string? clipboardString = _clipboard.GetText();

        if (clipboardString is null)
            return Array.Empty<IMapDataEntry>();

        return _mapDataClipboardSerializer.Deserialize(clipboardString, _lastSerializationResult);
    }
}