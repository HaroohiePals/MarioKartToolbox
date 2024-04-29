using HaroohiePals.Actions;
using HaroohiePals.Gui.Viewport;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.MarioKartToolbox.Application.Clipboard;
using HaroohiePals.MarioKartToolbox.Gui;
using HaroohiePals.MarioKartToolbox.Gui.View.CourseEditor;
using HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;
using HaroohiePals.NitroKart.Course;
using HaroohiePals.NitroKart.MapData.Intermediate;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using Moq;

namespace HaroohiePals.MarioKartToolbox.Test.Gui;

class MkdsMapDataClipboardManagerTests
{
    private static MkdsMapData ConstructTestMapData(int pathCount = 2, int pathPointCount = 5)
    {
        var mapData = new MkdsMapData(false);
        mapData.MgEnemyPaths = new MapDataCollection<MkdsMgEnemyPath>();

        for (int i = 0; i < pathCount; i++)
        {
            var path = new MkdsMgEnemyPath();
            for (int j = 0; j < pathPointCount; j++)
                path.Points.Add(new MkdsMgEnemyPoint { Position = new(i, j, 0) });
            mapData.MgEnemyPaths.Add(path);
        }

        for (int i = 0; i < pathCount; i++)
        {
            var path = new MkdsEnemyPath();
            for (int j = 0; j < pathPointCount; j++)
                path.Points.Add(new MkdsEnemyPoint { Position = new(i, j, 0) });
            mapData.EnemyPaths.Add(path);
        }

        for (int i = 0; i < pathCount; i++)
        {
            var path = new MkdsItemPath();
            for (int j = 0; j < pathPointCount; j++)
                path.Points.Add(new MkdsItemPoint { Position = new(i, j, 0) });
            mapData.ItemPaths.Add(path);
        }

        for (int i = 0; i < pathCount; i++)
        {
            var path = new MkdsCheckPointPath();
            for (int j = 0; j < pathPointCount; j++)
                path.Points.Add(new MkdsCheckPoint { Point1 = new(i, j) });
            mapData.CheckPointPaths.Add(path);
        }

        for (int i = 0; i < pathCount; i++)
        {
            var path = new NitroKart.MapData.Intermediate.Sections.MkdsPath();
            for (int j = 0; j < pathPointCount; j++)
                path.Points.Add(new MkdsPathPoint { Position = new(i, j, 0) });
            mapData.Paths.Add(path);
        }

        return mapData;
    }

    private static IMkdsCourse ConstructMockCourse()
    {
        var mock = new Mock<IMkdsCourse>();
        var mapData = ConstructTestMapData();

        mock.SetupGet(x => x.MapData).Returns(mapData);

        return mock.Object;
    }

    private static ICourseEditorContext ConstructMockCourseEditorContext()
    {
        var actionStack = new ActionStack();
        var sceneObjectHolder = new SceneObjectHolder();
        var course = ConstructMockCourse();

        var mock = new Mock<ICourseEditorContext>();
        mock.SetupGet(x => x.ActionStack).Returns(actionStack);
        mock.SetupGet(x => x.SceneObjectHolder).Returns(sceneObjectHolder);
        mock.SetupGet(x => x.Course).Returns(course);

        return mock.Object;
    }

    private static IMapDataClipboard ConstructMockMapDataClipboard()
    {
        var mock = new Mock<IMapDataClipboard>();
        var list = new List<IMapDataEntry>();

        mock.Setup(x => x.SetContents(It.IsAny<IEnumerable<IMapDataEntry>>()))
            .Callback((IEnumerable<IMapDataEntry> x) =>
            {
                list.Clear();
                list.AddRange(x);
            });

        mock.Setup(x => x.GetContents()).Returns(list);

        return mock.Object;
    }

    private static IEnumerable<TestCaseData> ConstructCopyFromPathTestData()
    {
        //Single Path Tests
        yield return new TestCaseData(
            new (int, int)[] { (0, 0), (0, 1), (0, 2), (0, 3), (0, 4) },
            new (int, int)[] { (0, 0), (0, 1), (0, 2), (0, 3), (0, 4) })
            .SetArgDisplayNames("Single Path: Ascending");
        yield return new TestCaseData(
            new (int, int)[] { (0, 4), (0, 3), (0, 2), (0, 1), (0, 0) },
            new (int, int)[] { (0, 0), (0, 1), (0, 2), (0, 3), (0, 4) })
            .SetArgDisplayNames("Single Path: Descending");
        yield return new TestCaseData(
            new (int, int)[] { (0, 4), (0, 0), (0, 1), (0, 2), (0, 3) },
            new (int, int)[] { (0, 0), (0, 1), (0, 2), (0, 3), (0, 4) })
            .SetArgDisplayNames("Single Path: Shift selected from bottom");
        yield return new TestCaseData(
            new (int, int)[] { (0, 4), (0, 0), (0, 2), (0, 1), (0, 3) },
            new (int, int)[] { (0, 0), (0, 1), (0, 2), (0, 3), (0, 4) })
            .SetArgDisplayNames("Single Path: Hand picked");

        //Multiple Paths Test
        yield return new TestCaseData(
            new (int, int)[] { (0, 0), (0, 1), (0, 2), (0, 3), (0, 4), (1, 0), (1, 1), (1, 2), (1, 3), (1, 4) },
            new (int, int)[] { (0, 0), (0, 1), (0, 2), (0, 3), (0, 4), (1, 0), (1, 1), (1, 2), (1, 3), (1, 4) })
            .SetArgDisplayNames("Two Paths: Ascending");
        yield return new TestCaseData(
            new (int, int)[] { (1, 4), (1, 3), (1, 2), (1, 1), (1, 0), (0, 4), (0, 3), (0, 2), (0, 1), (0, 0) },
            new (int, int)[] { (0, 0), (0, 1), (0, 2), (0, 3), (0, 4), (1, 0), (1, 1), (1, 2), (1, 3), (1, 4) })
            .SetArgDisplayNames("Two Paths: Descending");
        yield return new TestCaseData(
            new (int, int)[] { (0, 3), (1, 0), (1, 4), (0, 2), (1, 2), (1, 1), (0, 0), (1, 3), (0, 4), (0, 1) },
            new (int, int)[] { (0, 0), (0, 1), (0, 2), (0, 3), (0, 4), (1, 0), (1, 1), (1, 2), (1, 3), (1, 4) })
            .SetArgDisplayNames("Two Paths: Hand picked");
    }

    [TestCaseSource(nameof(ConstructCopyFromPathTestData))]
    public static void Copy_OnMgEnemyPoints_ReturnsCorrectOrder((int, int)[] selectOrder, (int, int)[] expectedOrder)
    {
        //Arrange
        var context = ConstructMockCourseEditorContext();
        var clipboard = ConstructMockMapDataClipboard();
        var mapData = context.Course.MapData;

        foreach ((int, int) pathEntryIndices in selectOrder)
            context.SceneObjectHolder.AddToSelection(mapData.MgEnemyPaths[pathEntryIndices.Item1].Points[pathEntryIndices.Item2]);

        var expectedContent = new List<IMapDataEntry>();

        foreach ((int, int) pathEntryIndices in expectedOrder)
            expectedContent.Add(mapData.MgEnemyPaths[pathEntryIndices.Item1].Points[pathEntryIndices.Item2]);

        var manager = new MapDataClipboardManager(context, clipboard);

        //Act
        manager.Copy();

        //Assert
        var resultContent = clipboard.GetContents().Cast<IMapDataEntry>();

        Assert.That(resultContent, Is.EqualTo(expectedContent));
    }

    [TestCaseSource(nameof(ConstructCopyFromPathTestData))]
    public static void Copy_OnEnemyPoints_ReturnsCorrectOrder((int, int)[] selectOrder, (int, int)[] expectedOrder)
    {
        //Arrange
        var context = ConstructMockCourseEditorContext();
        var clipboard = ConstructMockMapDataClipboard();
        var mapData = context.Course.MapData;

        foreach ((int, int) pathEntryIndices in selectOrder)
            context.SceneObjectHolder.AddToSelection(mapData.EnemyPaths[pathEntryIndices.Item1].Points[pathEntryIndices.Item2]);

        var expectedContent = new List<IMapDataEntry>();

        foreach ((int, int) pathEntryIndices in expectedOrder)
            expectedContent.Add(mapData.EnemyPaths[pathEntryIndices.Item1].Points[pathEntryIndices.Item2]);

        var manager = new MapDataClipboardManager(context, clipboard);

        //Act
        manager.Copy();

        //Assert
        var resultContent = clipboard.GetContents().Cast<IMapDataEntry>();
        
        Assert.That(resultContent, Is.EqualTo(expectedContent));
    }

    [TestCaseSource(nameof(ConstructCopyFromPathTestData))]
    public static void Copy_OnItemPoints_ReturnsCorrectOrder((int, int)[] selectOrder, (int, int)[] expectedOrder)
    {
        //Arrange
        var context = ConstructMockCourseEditorContext();
        var clipboard = ConstructMockMapDataClipboard();
        var mapData = context.Course.MapData;

        foreach ((int, int) pathEntryIndices in selectOrder)
            context.SceneObjectHolder.AddToSelection(mapData.ItemPaths[pathEntryIndices.Item1].Points[pathEntryIndices.Item2]);

        var expectedContent = new List<IMapDataEntry>();

        foreach ((int, int) pathEntryIndices in expectedOrder)
            expectedContent.Add(mapData.ItemPaths[pathEntryIndices.Item1].Points[pathEntryIndices.Item2]);

        var manager = new MapDataClipboardManager(context, clipboard);

        //Act
        manager.Copy();

        //Assert
        var resultContent = clipboard.GetContents().Cast<IMapDataEntry>();

        Assert.That(resultContent, Is.EqualTo(expectedContent));
    }
    
    [TestCaseSource(nameof(ConstructCopyFromPathTestData))]
    public static void Copy_OnCheckPointPoints_ReturnsCorrectOrder((int, int)[] selectOrder, (int, int)[] expectedOrder)
    {
        //Arrange
        var context = ConstructMockCourseEditorContext();
        var clipboard = ConstructMockMapDataClipboard();
        var mapData = context.Course.MapData;

        foreach ((int, int) pathEntryIndices in selectOrder)
            context.SceneObjectHolder.AddToSelection(mapData.CheckPointPaths[pathEntryIndices.Item1].Points[pathEntryIndices.Item2]);

        var expectedContent = new List<IMapDataEntry>();

        foreach ((int, int) pathEntryIndices in expectedOrder)
            expectedContent.Add(mapData.CheckPointPaths[pathEntryIndices.Item1].Points[pathEntryIndices.Item2]);

        var manager = new MapDataClipboardManager(context, clipboard);

        //Act
        manager.Copy();

        //Assert
        var resultContent = clipboard.GetContents().Cast<IMapDataEntry>();

        Assert.That(resultContent, Is.EqualTo(expectedContent));
    }

    [TestCaseSource(nameof(ConstructCopyFromPathTestData))]
    public static void Copy_OnPathPoints_ReturnsCorrectOrder((int, int)[] selectOrder, (int, int)[] expectedOrder)
    {
        //Arrange
        var context = ConstructMockCourseEditorContext();
        var clipboard = ConstructMockMapDataClipboard();
        var mapData = context.Course.MapData;

        foreach ((int, int) pathEntryIndices in selectOrder)
            context.SceneObjectHolder.AddToSelection(mapData.Paths[pathEntryIndices.Item1].Points[pathEntryIndices.Item2]);

        var expectedContent = new List<IMapDataEntry>();

        foreach ((int, int) pathEntryIndices in expectedOrder)
            expectedContent.Add(mapData.Paths[pathEntryIndices.Item1].Points[pathEntryIndices.Item2]);

        var manager = new MapDataClipboardManager(context, clipboard);

        //Act
        manager.Copy();

        //Assert
        var resultContent = clipboard.GetContents().Cast<IMapDataEntry>();

        Assert.That(resultContent, Is.EqualTo(expectedContent));
    }
}
