using HaroohiePals.Gui.View;
using HaroohiePals.Gui.View.Pane;
using HaroohiePals.Gui.View.PropertyGrid;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.MarioKartToolbox.Gui.View.PropertyGrid;
using HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;
using System.Linq;
using Vector2 = System.Numerics.Vector2;

namespace HaroohiePals.MarioKartToolbox.Gui.View.CourseEditor;

class PropertyGridPaneView : PaneView
{
    private readonly ICourseEditorContext _context;
    private readonly PropertyGridView _propertyGrid;

    private bool _selectionChanged = false;

    public PropertyGridPaneView(ICourseEditorContext context) : base("Properties", new Vector2(400, 600))
    {
        _context = context;

        _propertyGrid = new PropertyGridView(_context.ActionStack);

        _propertyGrid.RegisterMapDataEditors(_context.Course.MapData, _context.MapObjDatabase);
        _propertyGrid.RegisterCollisionEditors(_context.Course.MapData);

        _context.SceneObjectHolder.SelectionChanged += () => _selectionChanged = true;
    }

    public void Refresh()
    {
        _propertyGrid?.Refresh();
    }

    public void Clear()
    {
        _propertyGrid?.Clear();
    }

    public override void DrawContent()
    {
        _propertyGrid?.Draw();
    }

    public override void Update(UpdateArgs args)
    {
        if (_context.SceneObjectHolder != null && _propertyGrid != null && _selectionChanged)
        {
            var selectable = _context.SceneObjectHolder.GetSelection()
                .Where(x => x is not IMapDataCollection)
                .ToArray();

            if (selectable.Any())
                _propertyGrid.Select(selectable);
            else
                _propertyGrid.Clear();
        }

        _selectionChanged = false;
    }
}