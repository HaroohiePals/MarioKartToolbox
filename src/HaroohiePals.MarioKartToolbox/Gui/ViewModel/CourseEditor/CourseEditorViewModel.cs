using HaroohiePals.Gui.View.Modal;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.MarioKartToolbox.Application.Clipboard;
using HaroohiePals.MarioKartToolbox.Gui.View.CourseEditor;
using HaroohiePals.NitroKart.Course;
using HaroohiePals.NitroKart.MapObj;
using HaroohiePals.NitroKart.Validation.Course;
using HaroohiePals.Validation;
using System;
using System.Linq;

namespace HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;

class CourseEditorViewModel(
    IMkdsCourse course,
    IModalService modalService,
    IMkdsCourseValidatorFactory courseValidatorFactory,
    IMkdsMapObjDatabase mobjDatabase,
    IMapDataClipboard mapDataClipboard)
{
    public ICourseEditorContext Context { get; } = new CourseEditorContext(course, courseValidatorFactory, mobjDatabase, mapDataClipboard);

    public void SaveFile()
    {
        try
        {
            Context.Course.Save();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public void FixFatalErrors()
    {
        var errors = Context.CourseValidator.Validate(Context.Course);

        foreach (var error in errors.Where(e => e.Level == ErrorLevel.Fatal))
        {
            //The actions are executed immediately in this case
            if (error.TryFix(out var action))
                action.Do();
        }
    }

    public bool PerformRedo()
    {
        if (modalService.IsAnyModalOpen || !Context.ActionStack.CanRedo)
            return false;

        Context.CancelOperations();

        Context.ActionStack.Redo();

        return true;
    }

    public bool PerformUndo(out bool selectionCleared)
    {
        selectionCleared = false;

        if (Context.CancelOperations())
            return false;

        if (modalService.IsAnyModalOpen || !Context.ActionStack.CanUndo)
            return false;

        bool clearSelection = Context.ActionStack.IsLastActionCreateOrDelete;
        Context.ActionStack.Undo();

        if (clearSelection)
        {
            Context.SceneObjectHolder.ClearSelection();
            selectionCleared = true;
        }

        return true;
    }

    public void ShowMapDataGenerator()
    {
        modalService.ShowModal(new MapDataGeneratorModalView(new MapDataGeneratorViewModel(Context)));
    }

    public void ShowGlobalMapGenerator()
    {
        modalService.ShowModal(new GenerateGlobalMapModalView(new GenerateGlobalMapViewModel(Context)));
    }

    public void ShowImportCollision()
    {
        modalService.ShowModal(new CollisionImportModalView(Context));
    }

    public void ShowImportCourseModel()
    {
        Console.WriteLine("Not implemented");
    }

    public void ShowImportCourseModelV()
    {
        Console.WriteLine("Not implemented");
    }

    public void ClearRequests()
    {
        if (Context.MapDataClipboardManager.IsPasteRequested)
            Context.MapDataClipboardManager.ClearPasteRequest();

        Context.ClearActionRequest();
    }

    private void ClearSelectionAfterAction()
    {
        if (Context.ActionStack.IsLastActionCreateOrDelete)
            Context.SceneObjectHolder.ClearSelection();
    }

    public void Copy() => Context.MapDataClipboardManager.Copy();

    public void Cut()
    {
        Context.MapDataClipboardManager.Cut();
        ClearSelectionAfterAction();
    }

    public void Paste() => Context.MapDataClipboardManager.Paste();
    public void Insert() => Context.RequestAction(CourseEditorRequestableAction.Insert);
    public void SelectAll() => Context.RequestAction(CourseEditorRequestableAction.SelectAll);

    public void DeleteSelected()
    {
        Context.CancelOperations();
        Context.DeleteSelected();
        ClearSelectionAfterAction();
    }

    public void Update(double time)
    {
        Context.TimelineManager.Update(time);
    }

    public bool CanUndo => Context.ActionStack.CanUndo;
    public bool CanRedo => Context.ActionStack.CanRedo;

    public bool CanCopy => true;
    public bool CanCut => true;

    public bool CanAdd => Context.SceneObjectHolder.GetSelection().Any();
    public bool CanDelete => Context.SceneObjectHolder.GetSelection().OfType<IMapDataEntry>().Any();
}