using HaroohiePals.Actions;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.Validation;
using System.Collections.Generic;

namespace HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;

class ValidationPaneViewModel
{
    private int _prevActionStackCount = 0;
    private bool _shouldCheckErrors = true;

    public ICourseEditorContext Context { get; }
    public IEnumerable<ValidationError> Errors { get; private set; }

    public bool ShowFatal { get; set; } = true;
    public bool ShowErrors { get; set; } = true;
    public bool ShowWarning { get; set; } = true;

    public ValidationPaneViewModel(ICourseEditorContext context)
    {
        Context = context;
    }

    public void Update()
    {
        if (Context.ActionStack.UndoActionsCount != _prevActionStackCount)
            _shouldCheckErrors = true;

        _prevActionStackCount = Context.ActionStack.UndoActionsCount;

        if (!_shouldCheckErrors)
            return;

        UpdateErrors();

        _shouldCheckErrors = false;
    }

    public void UpdateErrors()
    {
        Errors = Context.CourseValidator.Validate(Context.Course);
    }

    public void FixAllErrors()
    {
        var actions = new List<IAction>();

        foreach (var error in Errors)
        {
            if (error.TryFix(out var action))
                actions.Add(action);
        }

        if (actions.Count > 0)
            Context.ActionStack.Add(new BatchAction(actions));

        UpdateErrors();
    }

    public void FixSingleError(ValidationError error)
    {
        if (error.TryFix(out var action))
            Context.ActionStack.Add(action);

        UpdateErrors();
    }

    public void SelectMapDataEntry(IMapDataEntry entry)
    {
        Context.SceneObjectHolder.SetSelection(entry);
        Context.PerformExpandSelected();
    }
}