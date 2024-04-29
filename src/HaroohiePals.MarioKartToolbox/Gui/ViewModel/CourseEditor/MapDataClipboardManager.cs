using HaroohiePals.MarioKart.MapData;
using HaroohiePals.MarioKartToolbox.Application.Clipboard;
using HaroohiePals.NitroKart.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;

class MapDataClipboardManager
{
    private readonly ICourseEditorContext _context;
    private readonly IMapDataClipboard _mapDataClipboard;

    public bool IsPasteRequested { get; private set; } = false;

    public MapDataClipboardManager(ICourseEditorContext context, IMapDataClipboard mapDataClipboard)
    {
        _context = context;
        _mapDataClipboard = mapDataClipboard;
    }

    private IEnumerable<IMapDataEntry> GetSelection()
    {
        var unorderedPoints = _context.SceneObjectHolder.GetSelection().OfType<IMapDataEntry>();
        return unorderedPoints.OrderBy(x => x.GetSortableIndex(_context.Course.MapData)).ToList();
    }


    public void Cut()
    {
        Copy();
        _context.DeleteSelected();
    }

    public void Copy() => _mapDataClipboard.SetContents(GetSelection());

    public void Paste() => IsPasteRequested = true;

    public void ClearPasteRequest() => IsPasteRequested = false;

    public IEnumerable<T> GetPasteObjects<T>() where T : IMapDataEntry
        => _mapDataClipboard.GetContents().OfType<T>();

    public IEnumerable<IMapDataEntry> GetPasteObjects() => GetPasteObjects<IMapDataEntry>();
}
