using HaroohiePals.Actions;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.Actions;
using HaroohiePals.NitroKart.Extensions;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;

class MapDataExplorerViewModel
{
    public ICourseEditorContext Context { get; }
    public bool ExpandSelection { get; set; }

    private readonly bool _insertInbetween = true;

    public MapDataExplorerViewModel(ICourseEditorContext context)
    {
        Context = context;
        Context.ExpandSelected += () => ExpandSelection = true;
    }

    public void HandlePaste(IMapDataEntry targetEntry)
    {
        if (!Context.MapDataClipboardManager.IsPasteRequested)
            return;

        var pasteObjects = Context.MapDataClipboardManager.GetPasteObjects();
        var groupByType = pasteObjects.GroupBy(x => x.GetType());

        int targetIndex = -1;
        IMapDataCollection targetCol = null;

        foreach (var group in groupByType)
        {
            var type = group.Key;

            pasteObjects = group.Cast<IMapDataEntry>().ToList();

            if (type == targetEntry.GetType())
            {
                targetCol = Context.Course.MapData.GetContainingCollection(targetEntry);
                targetIndex = targetCol.IndexOf(targetEntry) + 1;
                break;
            }

            var innerCol = targetEntry.GetInnerCollection();
            if (innerCol is not null)
            {
                targetCol = innerCol;
                targetIndex = targetCol.Count();
            }
        }

        if (targetCol != null)
            HandlePaste(pasteObjects, targetCol, targetIndex);
    }

    public void HandlePaste(IMapDataCollection targetCol)
    {
        if (!Context.MapDataClipboardManager.IsPasteRequested)
            return;

        var pasteObjects = Context.MapDataClipboardManager.GetPasteObjects();
        var groupByType = pasteObjects.GroupBy(x => x.GetType());

        foreach (var group in groupByType)
        {
            //todo: Check compatibility, right now I just let the following function handle an exception
            HandlePaste(group, targetCol, targetCol.Count());
        }
    }

    /// <summary>
    /// Convert Enemy Point to Item Points and viceversa
    /// </summary>
    /// <param name="pasteEntries"></param>
    /// <param name="targetCol"></param>
    /// <param name="outPasteEntries"></param>
    /// <returns></returns>
    private bool TryConvertPasteEntries(IEnumerable<IMapDataEntry> pasteEntries, IMapDataCollection targetCol, out IEnumerable<IMapDataEntry> outPasteEntries)
    {
        outPasteEntries = null;

        try
        {
            // Enemy Paths to Item Paths
            if (pasteEntries.All(x => x is MkdsEnemyPath) && targetCol is MapDataCollection<MkdsItemPath>)
            {
                var enemyPaths = pasteEntries.Cast<MkdsEnemyPath>();
                outPasteEntries = enemyPaths.ConvertToItemPaths();
                return true;
            }
            // Enemy Points to Item Points
            if (pasteEntries.All(x => x is MkdsEnemyPoint) && targetCol is MapDataCollection<MkdsItemPoint>)
            {
                outPasteEntries = pasteEntries.Cast<MkdsEnemyPoint>().Select(x => x.ToItemPoint()).ToList();
                return true;
            }
            // Item Paths to Enemy Paths
            if (pasteEntries.All(x => x is MkdsItemPath) && targetCol is MapDataCollection<MkdsEnemyPath>)
            {
                var itemPaths = pasteEntries.Cast<MkdsItemPath>();
                outPasteEntries = itemPaths.ConvertToEnemyPaths();
                return true;
            }
            // Item Points to Enemy Points
            if (pasteEntries.All(x => x is MkdsItemPoint) && targetCol is MapDataCollection<MkdsEnemyPoint>)
            {
                outPasteEntries = pasteEntries.Cast<MkdsItemPoint>().Select(x => x.ToEnemyPoint()).ToList();
                return true;
            }
        }
        catch
        {

        }

        return false;
    }

    private void HandlePaste(IEnumerable<IMapDataEntry> pasteEntries, IMapDataCollection targetCol, int targetIndex = 0)
    {
        if (!Context.MapDataClipboardManager.IsPasteRequested)
            return;

        try
        {
            if (TryConvertPasteEntries(pasteEntries, targetCol, out var outPasteEntries))
                pasteEntries = outPasteEntries;

            Context.ActionStack.Add(InsertMkdsMapDataCollectionItemsActionFactory.Create(Context.Course.MapData,
                pasteEntries, targetCol, targetIndex));

            Context.SceneObjectHolder.ClearSelection();
            foreach (var pasteObject in pasteEntries)
                Context.SceneObjectHolder.AddToSelection(pasteObject);

            Context.MapDataClipboardManager.ClearPasteRequest();
            ExpandSelection = true;
        }
        catch
        {
            Console.WriteLine($"Cannot paste onto {targetCol}: {string.Join(" - ", pasteEntries)}");
        }
    }

    public bool CanDrop(IMapDataEntry source, IMapDataEntry target)
    {
        var selection = Context.SceneObjectHolder.GetSelection().OfType<IMapDataEntry>();

        bool isCompatible = source.GetType() == target.GetType() ||
            Context.Course.MapData.GetContainingCollection(source).GetType() == target.GetInnerCollection()?.GetType();
        bool selectionIsSameType = selection.All(x => x.GetType() == source.GetType());
        bool targetNotInSelection = selection.All(x => x != target);

        return isCompatible && selectionIsSameType && targetNotInSelection;
    }

    public void DropSelected(IMapDataCollection targetCol, int? targetIndex = null)
    {
        var selection = Context.SceneObjectHolder.GetSelection().OfType<IMapDataEntry>();
        MoveItems(selection, targetCol, targetIndex ?? targetCol.Cast<IMapDataEntry>().Count());
    }

    public void DropSelected(IMapDataEntry targetEntry, bool moveAfter = false)
    {
        var selection = Context.SceneObjectHolder.GetSelection().OfType<IMapDataEntry>();

        var targetCol = Context.Course.MapData.GetContainingCollection(targetEntry);
        int targetIndex = targetCol.IndexOf(targetEntry) + (moveAfter ? 1 : 0);

        MoveItems(selection, targetCol, targetIndex);
    }

    private void MoveItems(IEnumerable<IMapDataEntry> itemsToMove, IMapDataCollection targetCol, int targetIndex)
    {
        if (targetCol == null || itemsToMove == null || itemsToMove.Count() == 0)
            return;

        var actions = new List<IAction>();

        var groupedSelection = itemsToMove.GroupBy(Context.Course.MapData.GetContainingCollection);

        foreach (var group in groupedSelection)
        {
            IMapDataCollection sourceCol = group.Key;

            if (targetCol.GetType() != sourceCol.GetType())
                continue;

            var orderedEntries = sourceCol.Cast<IMapDataEntry>().Where(x => group.Contains(x));

            actions.Add(
                MoveMkdsMapDataCollectionItemsActionFactory.Create(orderedEntries, sourceCol, targetCol, targetIndex));
        }

        if (actions.Count() > 0)
            Context.ActionStack.Add(new BatchAction(actions));
    }

    public void HandleInsert(IMapDataEntry targetEntry)
    {
        if (!Context.IsActionRequested(CourseEditorRequestableAction.Insert))
            return;

        var innerCollection = targetEntry.GetInnerCollection();

        var targetCol = innerCollection ?? Context.Course.MapData.GetContainingCollection(targetEntry);

        int targetIndex = innerCollection is not null ? -1 : targetCol.IndexOf(targetEntry) + 1;

        HandleInsert(targetCol, targetIndex);
    }

    private IMapDataEntry ConstructInsertEntry(IMapDataCollection targetCol, int targetIndex)
    {
        int count = targetCol.Count();
        var entry = targetCol.ConstructEntry();

        if (_insertInbetween && count > 2 && targetIndex != count)
        {
            if (entry is IPoint point)
            {
                var prevPoint = targetCol.Cast<IPoint>().ElementAt(targetIndex - 1);
                var nextPoint = targetCol.Cast<IPoint>().ElementAt(targetIndex);
                point.Position = (prevPoint.Position + nextPoint.Position) / 2;
            }
            else if (entry is MkdsCheckPoint cpoi)
            {
                var prevCpoi = targetCol.Cast<MkdsCheckPoint>().ElementAt(targetIndex - 1);
                var nextPoi = targetCol.Cast<MkdsCheckPoint>().ElementAt(targetIndex);
                cpoi.Point1 = (prevCpoi.Point1 + nextPoi.Point1) / 2;
                cpoi.Point2 = (prevCpoi.Point2 + nextPoi.Point2) / 2;
                if (prevCpoi.Respawn?.Target is not null)
                    cpoi.Respawn = new WeakMapDataReference<MkdsRespawnPoint>(prevCpoi.Respawn?.Target);
            }
        }

        return entry;
    }

    public void HandleInsert(IMapDataCollection targetCol, int targetIndex = -1)
    {
        if (!Context.IsActionRequested(CourseEditorRequestableAction.Insert))
            return;

        try
        {
            if (targetIndex == -1)
                targetIndex = targetCol.Count();

            var entry = ConstructInsertEntry(targetCol, targetIndex);

            var action = InsertMkdsMapDataCollectionItemsActionFactory.Create(Context.Course.MapData, entry, targetCol, targetIndex);

            Context.ActionStack.Add(action);

            Context.SceneObjectHolder.ClearSelection();
            Context.SceneObjectHolder.AddToSelection(entry);

            ExpandSelection = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        Context.ClearActionRequest();
    }


    public bool HandleSelect(object obj, bool resetSelection = false, bool allowUnselect = true)
    {
        if (resetSelection)
        {
            Context.SceneObjectHolder.ClearSelection();
            for (int i = -1; i <= 2; i++)
                Context.SceneObjectHolder.AddToSelection(obj, i);
        }
        else if (!Context.SceneObjectHolder.IsSelected(obj))
        {
            for (int i = -1; i <= 2; i++)
                Context.SceneObjectHolder.AddToSelection(obj, i);
        }
        else if (allowUnselect)
        {
            Context.SceneObjectHolder.RemoveFromSelection(obj);
            return true;
        }

        return false;
    }

    //public bool CanMoveUp()
    //{
    //    if (_viewModel.SceneObjectHolder.GetSelection().Any(x => x is not IMapDataEntry))
    //        return false;

    //    var first = GetAdjacentSelectedNodes().OrderBy(x => x.Index).FirstOrDefault();

    //    if (first == null || first.Index == 0)
    //        return false;

    //    return true;
    //}

    //public bool CanMoveDown()
    //{
    //    if (_viewModel.SceneObjectHolder.GetSelection().Any(x => x is not IMapDataEntry))
    //        return false;

    //    var first = GetAdjacentSelectedNodes().OrderByDescending(x => x.Index).FirstOrDefault();

    //    if (first == null || first.Index == first.Parent?.Children.Count - 1)
    //        return false;

    //    return true;
    //}

    //public void PerformMoveDown()
    //{
    //    ResetPendingActions();

    //    var nodes = GetAdjacentSelectedNodes().OrderByDescending(x => x.Index).ToList();

    //    var actions = new List<IAction>();

    //    foreach (var node in nodes)
    //        actions.Add(new MoveMapDataCollectionItemsAction(node.Parent?.Collection, node.Parent?.Collection,
    //            node.Data, node.Index + 1));

    //    if (actions.Count > 0)
    //    {
    //        _actionStack.Add(new BatchAction(actions));
    //        _viewModel.SceneObjectHolder.ClearSelection();
    //        foreach (var node in nodes)
    //            _viewModel.SceneObjectHolder.AddToSelection(node.Data);
    //    }
    //}

    //public void PerformMoveUp()
    //{
    //    ResetPendingActions();

    //    var nodes = GetAdjacentSelectedNodes().OrderBy(x => x.Index).ToList();

    //    var actions = new List<IAction>();

    //    foreach (var node in nodes)
    //        actions.Add(new MoveMapDataCollectionItemsAction(node.Parent?.Collection, node.Parent?.Collection,
    //            node.Data, node.Index - 1));

    //    if (actions.Count > 0)
    //    {
    //        _actionStack.Add(new BatchAction(actions));
    //        _viewModel.SceneObjectHolder.ClearSelection();
    //        foreach (var node in nodes)
    //            _viewModel.SceneObjectHolder.AddToSelection(node.Data);
    //    }
    //}

    //#endregion
}