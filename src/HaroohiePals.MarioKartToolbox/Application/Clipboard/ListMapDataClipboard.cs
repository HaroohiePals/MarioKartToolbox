using HaroohiePals.MarioKart.MapData;
using System.Collections.Generic;
using System.Linq;

namespace HaroohiePals.MarioKartToolbox.Application.Clipboard;

class ListMapDataClipboard : IMapDataClipboard
{
    private List<IMapDataEntry> _entries = new();

    public IReadOnlyCollection<IMapDataEntry> GetContents() => _entries.Select(x => x.Clone()).ToList();

    public void SetContents(IEnumerable<IMapDataEntry> entries)
    {
        _entries = entries.Select(x => x.Clone()).ToList();
    }
}