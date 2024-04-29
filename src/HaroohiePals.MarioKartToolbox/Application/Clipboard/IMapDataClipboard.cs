#nullable enable
using HaroohiePals.MarioKart.MapData;
using System.Collections.Generic;

namespace HaroohiePals.MarioKartToolbox.Application.Clipboard;

interface IMapDataClipboard
{
    void SetContents(IEnumerable<IMapDataEntry> entries);
    IReadOnlyCollection<IMapDataEntry> GetContents();
}