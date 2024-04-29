using HaroohiePals.Gui.View;
using HaroohiePals.Gui.Viewport;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;
using HaroohiePals.MarioKartToolbox.Gui.Viewport;
using HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups.KCollision;
using System;
using System.Linq;

namespace HaroohiePals.MarioKartToolbox.Gui.View.CourseEditor;

internal abstract class CourseViewportView : IView, IDisposable
{
    protected string _title;
    protected RenderGroupScene _scene;
    protected InteractiveViewportPanel _viewportPanel;
    protected readonly ICourseEditorContext Context;

    protected KclPrismRenderGroup _perspectiveKclPrismGroup;

    private bool _shouldUpdateDrawTool = false;
    private bool _cancelOperationAdded = false;

    protected CourseViewportView(string title, ICourseEditorContext context)
    {
        _title = title;
        Context = context;

        Context.SceneObjectHolder.SelectionChanged += () => _shouldUpdateDrawTool = true;

        Context.Course.CourseFileUpdated += CourseFileUpdated;
    }

    private void CourseFileUpdated(bool isTex, string path)
    {
        if (!isTex && path.ToLower().EndsWith(".kcl"))
            AddOrUpdateKclPrismRenderGroup();
    }

    public virtual void Dispose()
    {
        if (_scene is IDisposable d)
            d.Dispose();
    }

    public void UpdateDrawTool()
    {
        _viewportPanel.DrawTool = null;

        var lastSelected = Context.SceneObjectHolder.GetSelection().LastOrDefault();

        if (lastSelected != null)
        {
            if (lastSelected is IMapDataCollection col)
                _viewportPanel.DrawTool = MapDataCollectionDrawTool.CreateTool(Context.Course, col);
            else if (lastSelected is IMapDataEntry entry)
                _viewportPanel.DrawTool = MapDataCollectionDrawTool.CreateTool(Context.Course, entry);
        }

        _shouldUpdateDrawTool = false;
    }

    public abstract bool Draw();

    public virtual void Update(UpdateArgs args)
    {
        if (_shouldUpdateDrawTool)
            UpdateDrawTool();

        if (_viewportPanel.IsGizmoStarted)
        {
            if (!_cancelOperationAdded)
            {
                _cancelOperationAdded = true;
                Context.StartOperation(_viewportPanel.CancelGizmoTransform);
            }
        }
        else if (_cancelOperationAdded)
        {
            _cancelOperationAdded = false;
            Context.ClearOperation(_viewportPanel.CancelGizmoTransform);
        }
    }

    protected void AddOrUpdateKclPrismRenderGroup()
    {
        bool enabled = false;

        if (_perspectiveKclPrismGroup != null)
        {
            enabled = _perspectiveKclPrismGroup.Enabled;
            _perspectiveKclPrismGroup.Dispose();
            _scene.RenderGroups.Remove(_perspectiveKclPrismGroup);
        }

        if (Context.Course.Collision is null)
            return;

        _perspectiveKclPrismGroup =
            new KclPrismRenderGroup(new KclViewerContext(Context.Course.Collision), Context.Course.MapData.StageInfo);
        _perspectiveKclPrismGroup.Enabled = enabled;
        _scene.RenderGroups.Add(_perspectiveKclPrismGroup);
    }
}