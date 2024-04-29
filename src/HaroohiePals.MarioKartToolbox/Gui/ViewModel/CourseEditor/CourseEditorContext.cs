using HaroohiePals.Actions;
using HaroohiePals.Gui.Viewport;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.MarioKartToolbox.Application.Clipboard;
using HaroohiePals.NitroKart.Actions;
using HaroohiePals.NitroKart.Course;
using HaroohiePals.NitroKart.MapObj;
using HaroohiePals.NitroKart.Validation.Course;
using HaroohiePals.Validation;
using System;
using System.Linq;

namespace HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;

class CourseEditorContext : ICourseEditorContext
{
    public IMkdsCourse Course { get; }
    public ActionStack ActionStack { get; } = new();
    public SceneObjectHolder SceneObjectHolder { get; } = new();
    public MapDataClipboardManager MapDataClipboardManager { get; }
    public TimelineManager TimelineManager { get; } = new();
    public IMkdsMapObjDatabase MapObjDatabase { get; }
    public IValidator<IMkdsCourse> CourseValidator { get; }

    public event Action ExpandSelected;
    public event Action FrameSelection;

    private event Action _cancelOperations;

    private CourseEditorRequestableAction _requestedAction = CourseEditorRequestableAction.None;

    public CourseEditorContext(IMkdsCourse course, IMkdsCourseValidatorFactory courseValidatorFactory, IMkdsMapObjDatabase mobjDatabase, IMapDataClipboard mapDataClipboard)
    {
        Course = course;
        MapObjDatabase = mobjDatabase;
        MapDataClipboardManager = new(this, mapDataClipboard);
        CourseValidator = courseValidatorFactory.CreateMkdsCourseValidator();
    }

    public void PerformExpandSelected()
    {
        ExpandSelected?.Invoke();
    }

    public void PerformFrameSelection()
    {
        FrameSelection?.Invoke();
    }

    public void DeleteSelected()
    {
        var selection = SceneObjectHolder.GetSelection().OfType<IMapDataEntry>();

        if (selection.Count() == 0)
            return;

        ActionStack.Add(DeleteMkdsMapDataCollectionItemsActionFactory.Create(Course.MapData, selection));
    }

    public void StartOperation(Action cancelOperation) => _cancelOperations += cancelOperation;
    public void ClearOperation(Action cancelOperation) => _cancelOperations -= cancelOperation;
    public void ClearOperations() => _cancelOperations = null;

    public bool CancelOperations()
    {
        bool anyCancelled = _cancelOperations != null;

        _cancelOperations?.Invoke();
        ClearOperations();

        return anyCancelled;
    }

    public void RequestAction(CourseEditorRequestableAction action)
        => _requestedAction = action;

    public void ClearActionRequest()
        => _requestedAction = CourseEditorRequestableAction.None;

    public bool IsActionRequested(CourseEditorRequestableAction action)
        => _requestedAction == action;
}