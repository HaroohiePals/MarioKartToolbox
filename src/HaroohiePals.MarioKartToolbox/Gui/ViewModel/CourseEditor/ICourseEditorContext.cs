using HaroohiePals.Actions;
using HaroohiePals.Gui.Viewport;
using HaroohiePals.NitroKart.Course;
using HaroohiePals.NitroKart.MapObj;
using HaroohiePals.Validation;
using System;

namespace HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;

interface ICourseEditorContext
{
    ActionStack ActionStack { get; }
    IMkdsCourse Course { get; }
    IMkdsMapObjDatabase MapObjDatabase { get; }
    IValidator<IMkdsCourse> CourseValidator { get; }
    MapDataClipboardManager MapDataClipboardManager { get; }
    SceneObjectHolder SceneObjectHolder { get; }
    TimelineManager TimelineManager { get; }

    event Action ExpandSelected;
    event Action FrameSelection;

    bool CancelOperations();
    void ClearOperation(Action cancelOperation);
    void ClearOperations();
    void DeleteSelected();
    void PerformExpandSelected();
    void PerformFrameSelection();
    void StartOperation(Action cancelOperation);

    void RequestAction(CourseEditorRequestableAction action);
    void ClearActionRequest();
    bool IsActionRequested(CourseEditorRequestableAction action);
}