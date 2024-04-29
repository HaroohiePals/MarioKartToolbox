using HaroohiePals.IO.Reference;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using Moq;
using System.Reflection;

namespace HaroohiePals.NitroKart.Actions.Test;

class DeleteMapDataCollectionItemsActionTests
{
    private static IReadOnlyList<IMapDataEntry> ConstructMockEntries(int count, string? entryName = null)
    {
        var col = new List<IMapDataEntry>();

        for (int i = 0; i < count; i++)
        {
            var mockEntry = new Mock<IMapDataEntry>();
            mockEntry.Setup(x => x.ToString()).Returns($"{entryName}{i}");
            col.Add(mockEntry.Object);
        }

        return col;
    }

    private static TestCaseData ConstructSingleTestData(string testCaseName, int sequenceLength, int[] sourceIndices, int[] expectedSequence)
    {
        var input = ConstructMockEntries(sequenceLength);

        var sourceEntries = sourceIndices.Select(i => input[i]).ToList();
        var expected = new List<IMapDataEntry>();
        foreach (int index in expectedSequence)
            expected.Add(input[index]);

        return new TestCaseData(input, sourceEntries, expected).SetArgDisplayNames(testCaseName);
    }

    private static TestCaseData ConstructSingleTestData(string testCaseName, int sourceLength, int targetLength, int[] sourceIndices, int targetIndex, string[] expectedSourceSequence, string[] expectedTargetSequence)
    {
        const string SOURCE_ENTRY_NAME_PREFIX = "S";
        const string TARGET_ENTRY_NAME_PREFIX = "T";

        var source = ConstructMockEntries(sourceLength, SOURCE_ENTRY_NAME_PREFIX);
        var target = ConstructMockEntries(targetLength, TARGET_ENTRY_NAME_PREFIX);

        var sourceEntries = sourceIndices.Select(i => source[i]).ToList();
        var expectedSource = new List<IMapDataEntry>();
        var expectedTarget = new List<IMapDataEntry>();

        foreach (string element in expectedSourceSequence)
        {
            bool fromSource = element.StartsWith(SOURCE_ENTRY_NAME_PREFIX);
            int index = int.Parse(element.Remove(0, fromSource ? SOURCE_ENTRY_NAME_PREFIX.Length : TARGET_ENTRY_NAME_PREFIX.Length));
            expectedSource.Add(fromSource ? source[index] : target[index]);
        }
        foreach (string element in expectedTargetSequence)
        {
            bool fromSource = element.StartsWith(SOURCE_ENTRY_NAME_PREFIX);
            int index = int.Parse(element.Remove(0, fromSource ? SOURCE_ENTRY_NAME_PREFIX.Length : TARGET_ENTRY_NAME_PREFIX.Length));
            expectedTarget.Add(fromSource ? source[index] : target[index]);
        }

        return new TestCaseData(source, target, sourceEntries, targetIndex, expectedSource, expectedTarget).SetArgDisplayNames(testCaseName);
    }

    private static MkdsMapData ConstructTestMapData(bool isMgStage = false)
    {
        var mapData = new MkdsMapData(isMgStage);

        var mapObject = new MkdsMapObject();
        var path = new MkdsPath();
        var respawnPoint = new MkdsRespawnPoint();

        var checkpoint = new MkdsCheckPoint();
        var checkpointPathA = new MkdsCheckPointPath { Points = { checkpoint } };
        var checkpointPathB = new MkdsCheckPointPath();
        var checkpointPathC = new MkdsCheckPointPath();
        checkpointPathA.Next.Add(new WeakMapDataReference<MkdsCheckPointPath>(checkpointPathB));
        checkpointPathA.Next.Add(new WeakMapDataReference<MkdsCheckPointPath>(checkpointPathC));
        checkpointPathA.Previous.Add(new WeakMapDataReference<MkdsCheckPointPath>(checkpointPathB));
        checkpointPathA.Previous.Add(new WeakMapDataReference<MkdsCheckPointPath>(checkpointPathC));

        checkpointPathB.Next.Add(new WeakMapDataReference<MkdsCheckPointPath>(checkpointPathA));
        checkpointPathC.Next.Add(new WeakMapDataReference<MkdsCheckPointPath>(checkpointPathA));
        checkpointPathB.Previous.Add(new WeakMapDataReference<MkdsCheckPointPath>(checkpointPathA));
        checkpointPathC.Previous.Add(new WeakMapDataReference<MkdsCheckPointPath>(checkpointPathA));

        mapData.CheckPointPaths.Add(checkpointPathA);
        mapData.CheckPointPaths.Add(checkpointPathB);
        mapData.CheckPointPaths.Add(checkpointPathC);

        var itemPoint = new MkdsItemPoint();
        var itemPathA = new MkdsItemPath { Points = { itemPoint } };
        var itemPathB = new MkdsItemPath();
        var itemPathC = new MkdsItemPath();
        itemPathA.Next.Add(new WeakMapDataReference<MkdsItemPath>(itemPathB));
        itemPathA.Next.Add(new WeakMapDataReference<MkdsItemPath>(itemPathC));
        itemPathA.Previous.Add(new WeakMapDataReference<MkdsItemPath>(itemPathB));
        itemPathA.Previous.Add(new WeakMapDataReference<MkdsItemPath>(itemPathC));

        itemPathB.Next.Add(new WeakMapDataReference<MkdsItemPath>(itemPathA));
        itemPathC.Next.Add(new WeakMapDataReference<MkdsItemPath>(itemPathA));
        itemPathB.Previous.Add(new WeakMapDataReference<MkdsItemPath>(itemPathA));
        itemPathC.Previous.Add(new WeakMapDataReference<MkdsItemPath>(itemPathA));

        mapData.ItemPaths.Add(itemPathA);
        mapData.ItemPaths.Add(itemPathB);
        mapData.ItemPaths.Add(itemPathC);

        if (isMgStage)
        {
            var mgEnemyPathA = new MkdsMgEnemyPath();
            mgEnemyPathA.Points.AddRange(Enumerable.Range(0, 4).Select(x => new MkdsMgEnemyPoint { Position = new(0, x, 0) }));
            var mgEnemyPathB = new MkdsMgEnemyPath();
            mgEnemyPathB.Points.AddRange(Enumerable.Range(0, 4).Select(x => new MkdsMgEnemyPoint { Position = new(1, x, 1) }));
            var mgEnemyPathC = new MkdsMgEnemyPath();
            mgEnemyPathC.Points.AddRange(Enumerable.Range(0, 4).Select(x => new MkdsMgEnemyPoint { Position = new(2, x, 2) }));

            mgEnemyPathA.Next.Add(new WeakMapDataReference<MkdsMgEnemyPoint>(mgEnemyPathB.Points[0]));
            mgEnemyPathA.Next.Add(new WeakMapDataReference<MkdsMgEnemyPoint>(mgEnemyPathC.Points[0]));
            mgEnemyPathB.Next.Add(new WeakMapDataReference<MkdsMgEnemyPoint>(mgEnemyPathC.Points[0]));
            mgEnemyPathB.Next.Add(new WeakMapDataReference<MkdsMgEnemyPoint>(mgEnemyPathA.Points[0]));
            mgEnemyPathC.Next.Add(new WeakMapDataReference<MkdsMgEnemyPoint>(mgEnemyPathB.Points[0]));
            mgEnemyPathC.Next.Add(new WeakMapDataReference<MkdsMgEnemyPoint>(mgEnemyPathA.Points[0]));

            mgEnemyPathA.Previous.Add(new WeakMapDataReference<MkdsMgEnemyPoint>(mgEnemyPathB.Points[^1]));
            mgEnemyPathA.Previous.Add(new WeakMapDataReference<MkdsMgEnemyPoint>(mgEnemyPathC.Points[^1]));
            mgEnemyPathB.Previous.Add(new WeakMapDataReference<MkdsMgEnemyPoint>(mgEnemyPathC.Points[^1]));
            mgEnemyPathB.Previous.Add(new WeakMapDataReference<MkdsMgEnemyPoint>(mgEnemyPathA.Points[^1]));
            mgEnemyPathC.Previous.Add(new WeakMapDataReference<MkdsMgEnemyPoint>(mgEnemyPathB.Points[^1]));
            mgEnemyPathC.Previous.Add(new WeakMapDataReference<MkdsMgEnemyPoint>(mgEnemyPathA.Points[^1]));

            mapData.MgEnemyPaths.Add(mgEnemyPathA);
            mapData.MgEnemyPaths.Add(mgEnemyPathB);
            mapData.MgEnemyPaths.Add(mgEnemyPathC);

            respawnPoint.MgEnemyPoint = new WeakMapDataReference<MkdsMgEnemyPoint>(mgEnemyPathA.Points[0]);
        }
        else
        {
            var enemyPoint = new MkdsEnemyPoint();
            var enemyPathA = new MkdsEnemyPath { Points = { enemyPoint } };
            var enemyPathB = new MkdsEnemyPath();
            var enemyPathC = new MkdsEnemyPath();
            enemyPathA.Next.Add(new WeakMapDataReference<MkdsEnemyPath>(enemyPathB));
            enemyPathA.Next.Add(new WeakMapDataReference<MkdsEnemyPath>(enemyPathC));
            enemyPathA.Previous.Add(new WeakMapDataReference<MkdsEnemyPath>(enemyPathB));
            enemyPathA.Previous.Add(new WeakMapDataReference<MkdsEnemyPath>(enemyPathC));

            enemyPathB.Next.Add(new WeakMapDataReference<MkdsEnemyPath>(enemyPathA));
            enemyPathC.Next.Add(new WeakMapDataReference<MkdsEnemyPath>(enemyPathA));
            enemyPathB.Previous.Add(new WeakMapDataReference<MkdsEnemyPath>(enemyPathA));
            enemyPathC.Previous.Add(new WeakMapDataReference<MkdsEnemyPath>(enemyPathA));

            mapData.EnemyPaths.Add(enemyPathA);
            mapData.EnemyPaths.Add(enemyPathB);
            mapData.EnemyPaths.Add(enemyPathC);

            respawnPoint.EnemyPoint = new WeakMapDataReference<MkdsEnemyPoint>(enemyPoint);
        }

        mapObject.Path = new WeakMapDataReference<MkdsPath>(path);
        checkpoint.Respawn = new WeakMapDataReference<MkdsRespawnPoint>(respawnPoint);

        mapData.MapObjects.Add(mapObject);
        mapData.Paths.Add(path);
        mapData.RespawnPoints.Add(respawnPoint);

        //Resolve
        var resolverCollection = new ReferenceResolverCollection();
        var referenceResolver = new MkdsWeakMapDataReferenceResolver(mapData);
        resolverCollection.RegisterResolver<MkdsPath>(referenceResolver);
        resolverCollection.RegisterResolver<MkdsRespawnPoint>(referenceResolver);
        resolverCollection.RegisterResolver<MkdsCheckPointPath>(referenceResolver);
        resolverCollection.RegisterResolver<MkdsItemPoint>(referenceResolver);
        resolverCollection.RegisterResolver<MkdsItemPath>(referenceResolver);
        resolverCollection.RegisterResolver<MkdsEnemyPoint>(referenceResolver);
        resolverCollection.RegisterResolver<MkdsEnemyPath>(referenceResolver);
        resolverCollection.RegisterResolver<MkdsMgEnemyPoint>(referenceResolver);
        resolverCollection.RegisterResolver<MkdsCamera>(referenceResolver);

        mapData.MapObjects?.ResolveReferences(resolverCollection);
        mapData.Paths?.ResolveReferences(resolverCollection);
        mapData.RespawnPoints?.ResolveReferences(resolverCollection);
        mapData.CannonPoints?.ResolveReferences(resolverCollection);
        mapData.CheckPointPaths?.ResolveReferences(resolverCollection);
        mapData.ItemPaths?.ResolveReferences(resolverCollection);
        mapData.EnemyPaths?.ResolveReferences(resolverCollection);
        mapData.MgEnemyPaths?.ResolveReferences(resolverCollection);
        mapData.Areas?.ResolveReferences(resolverCollection);
        mapData.Cameras?.ResolveReferences(resolverCollection);

        return mapData;
    }

    private static IEnumerable<TestCaseData> ConstructReferenceTestData()
    {
        var mapData = ConstructTestMapData();
        yield return new TestCaseData(mapData, mapData.MapObjects[0], mapData.MapObjects, nameof(MkdsMapObject.Path), mapData.Paths[0], mapData.Paths).SetArgDisplayNames("MapObject -> Path");
        mapData = ConstructTestMapData();
        yield return new TestCaseData(mapData, mapData.CheckPointPaths[0].Points[0], mapData.CheckPointPaths[0].Points, nameof(MkdsCheckPoint.Respawn), mapData.RespawnPoints[0], mapData.RespawnPoints).SetArgDisplayNames("Checkpoint -> Respawn point");
    }

    private static TestCaseData ConstructSingleConnectedPathTestCase<TPath, TPoint>(string displayName, MkdsMapData mapData, MapDataCollection<TPath> pathCollection, int checkIndex, int deleteIndex, int[] expectedNextIndices, int[] expectedPrevIndices)
        where TPath : ConnectedPath<TPath, TPoint>, new()
        where TPoint : IMapDataEntry
    {
        var expectedNext = expectedNextIndices.Select(x => pathCollection[x]).ToList();
        var expectedPrev = expectedPrevIndices.Select(x => pathCollection[x]).ToList();

        return new TestCaseData(mapData, pathCollection, pathCollection[checkIndex], pathCollection[deleteIndex], expectedNext, expectedPrev).SetArgDisplayNames(displayName);
    }

    private static TestCaseData ConstructSingleMgEnemyPathTestCase(string displayName, MkdsMapData mapData, int checkIndex, int[] deleteIndices, string[] expectedNextIndices, string[] expectedPrevIndices)
    {
        var expectedNext = expectedNextIndices.Select(x => mapData.MgEnemyPaths[int.Parse(x.Split('_')[0])].Points[int.Parse(x.Split('_')[1])]).ToList();
        var expectedPrev = expectedPrevIndices.Select(x => mapData.MgEnemyPaths[int.Parse(x.Split('_')[0])].Points[int.Parse(x.Split('_')[1])]).ToList();

        return new TestCaseData(mapData, mapData.MgEnemyPaths[checkIndex], deleteIndices.Select(x => mapData.MgEnemyPaths[x]), expectedNext, expectedPrev).SetArgDisplayNames(displayName);
    }

    private static TestCaseData ConstructSingleMgEnemyPointTestCase(string displayName, MkdsMapData mapData, int checkIndex, int deleteFromPathIndex, int deleteIndex, string[] expectedNextIndices, string[] expectedPrevIndices)
    {
        var expectedNext = expectedNextIndices.Select(x => mapData.MgEnemyPaths[int.Parse(x.Split('_')[0])].Points[int.Parse(x.Split('_')[1])]).ToList();
        var expectedPrev = expectedPrevIndices.Select(x => mapData.MgEnemyPaths[int.Parse(x.Split('_')[0])].Points[int.Parse(x.Split('_')[1])]).ToList();

        return new TestCaseData(mapData, mapData.MgEnemyPaths[checkIndex], mapData.MgEnemyPaths[deleteFromPathIndex], mapData.MgEnemyPaths[deleteFromPathIndex].Points[deleteIndex], expectedNext, expectedPrev).SetArgDisplayNames(displayName);
    }

    private static IEnumerable<TestCaseData> ConstructCheckPointPathTestData()
    {
        var mapData = ConstructTestMapData();
        yield return ConstructSingleConnectedPathTestCase<MkdsCheckPointPath, MkdsCheckPoint>("Check 0 after removing 1", mapData, mapData.CheckPointPaths, 0, 1, new[] { 2 }, new[] { 2 });
        mapData = ConstructTestMapData();
        yield return ConstructSingleConnectedPathTestCase<MkdsCheckPointPath, MkdsCheckPoint>("Check 0 after removing 2", mapData, mapData.CheckPointPaths, 0, 2, new[] { 1 }, new[] { 1 });
        mapData = ConstructTestMapData();
        yield return ConstructSingleConnectedPathTestCase<MkdsCheckPointPath, MkdsCheckPoint>("Check 1 after removing 0", mapData, mapData.CheckPointPaths, 1, 0, new int[] { }, new int[] { });
        mapData = ConstructTestMapData();
        yield return ConstructSingleConnectedPathTestCase<MkdsCheckPointPath, MkdsCheckPoint>("Check 2 after removing 0", mapData, mapData.CheckPointPaths, 2, 0, new int[] { }, new int[] { });
    }

    private static IEnumerable<TestCaseData> ConstructEnemyPathTestData()
    {
        var mapData = ConstructTestMapData();
        yield return ConstructSingleConnectedPathTestCase<MkdsEnemyPath, MkdsEnemyPoint>("Check 0 after removing 1", mapData, mapData.EnemyPaths, 0, 1, new[] { 2 }, new[] { 2 });
        mapData = ConstructTestMapData();
        yield return ConstructSingleConnectedPathTestCase<MkdsEnemyPath, MkdsEnemyPoint>("Check 0 after removing 2", mapData, mapData.EnemyPaths, 0, 2, new[] { 1 }, new[] { 1 });
        mapData = ConstructTestMapData();
        yield return ConstructSingleConnectedPathTestCase<MkdsEnemyPath, MkdsEnemyPoint>("Check 1 after removing 0", mapData, mapData.EnemyPaths, 1, 0, new int[] { }, new int[] { });
        mapData = ConstructTestMapData();
        yield return ConstructSingleConnectedPathTestCase<MkdsEnemyPath, MkdsEnemyPoint>("Check 2 after removing 0", mapData, mapData.EnemyPaths, 2, 0, new int[] { }, new int[] { });
    }

    private static IEnumerable<TestCaseData> ConstructItemPathTestData()
    {
        var mapData = ConstructTestMapData();
        yield return ConstructSingleConnectedPathTestCase<MkdsItemPath, MkdsItemPoint>("Check 0 after removing 1", mapData, mapData.ItemPaths, 0, 1, new[] { 2 }, new[] { 2 });
        mapData = ConstructTestMapData();
        yield return ConstructSingleConnectedPathTestCase<MkdsItemPath, MkdsItemPoint>("Check 0 after removing 2", mapData, mapData.ItemPaths, 0, 2, new[] { 1 }, new[] { 1 });
        mapData = ConstructTestMapData();
        yield return ConstructSingleConnectedPathTestCase<MkdsItemPath, MkdsItemPoint>("Check 1 after removing 0", mapData, mapData.ItemPaths, 1, 0, new int[] { }, new int[] { });
        mapData = ConstructTestMapData();
        yield return ConstructSingleConnectedPathTestCase<MkdsItemPath, MkdsItemPoint>("Check 2 after removing 0", mapData, mapData.ItemPaths, 2, 0, new int[] { }, new int[] { });
    }

    private static IEnumerable<TestCaseData> ConstructMgEnemyPathTestData()
    {
        var mapData = ConstructTestMapData(true);
        yield return ConstructSingleMgEnemyPathTestCase("Check 0 after removing 1", mapData, 0, new[] { 1 }, new[] { "2_0" }, new[] { "2_3" });
        mapData = ConstructTestMapData(true);
        yield return ConstructSingleMgEnemyPathTestCase("Check 0 after removing 1 and 2", mapData, 0, new[] { 1, 2 }, new string[] { }, new string[] { });
        mapData = ConstructTestMapData(true);
        yield return ConstructSingleMgEnemyPathTestCase("Check 1 after removing 0", mapData, 1, new[] { 0 }, new[] { "2_0" }, new[] { "2_3" });
        mapData = ConstructTestMapData(true);
        yield return ConstructSingleMgEnemyPathTestCase("Check 2 after removing 0", mapData, 2, new[] { 0 }, new[] { "1_0" }, new[] { "1_3" });
    }

    private static IEnumerable<TestCaseData> ConstructMgEnemyPointTestData()
    {
        var mapData = ConstructTestMapData(true);
        yield return ConstructSingleMgEnemyPointTestCase("Check 0 after removing first element of 1", mapData, 0, 1, 0, new[] { "2_0" }, new[] { "1_3", "2_3" });
        mapData = ConstructTestMapData(true);
        yield return ConstructSingleMgEnemyPointTestCase("Check 0 after removing last element of 1", mapData, 0, 1, 3, new[] { "1_0", "2_0" }, new[] { "2_3" });
        mapData = ConstructTestMapData(true);
        yield return ConstructSingleMgEnemyPointTestCase("Check 1 after removing first element of 0", mapData, 1, 0, 0, new[] { "2_0" }, new[] { "2_3", "0_3" });
        mapData = ConstructTestMapData(true);
        yield return ConstructSingleMgEnemyPointTestCase("Check 1 after removing last element of 0", mapData, 1, 0, 3, new[] { "2_0", "0_0" }, new[] { "2_3" });
    }

    private static bool ReferenceHolderHasReferences(IMapDataEntry entry)
    {
        var referenceHolder = entry.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Instance).First(x => x.Name.Contains("ReferenceHolder")).GetValue(entry);
        return referenceHolder?.GetType().GetProperty("HasReferences")?.GetValue(referenceHolder) is true;
    }

    private static IMapDataEntry? GetReferenceTarget(IMapDataEntry entry, string propertyName)
    {
        var reference = entry.GetType().GetProperty(propertyName)?.GetValue(entry);
        return (IMapDataEntry?)reference?.GetType().GetProperty("Target")?.GetValue(reference);
    }

    private static IEnumerable<TestCaseData> ConstructTestData()
    {
        yield return ConstructSingleTestData("Remove 1st element", 10, new[] { 0 }, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
        yield return ConstructSingleTestData("Remove last element", 10, new[] { 9 }, new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 });
        yield return ConstructSingleTestData("Remove 3, 4, 5", 10, new[] { 3, 4, 5 }, new[] { 0, 1, 2, 6, 7, 8, 9 });
        yield return ConstructSingleTestData("Remove 2, 6, 8", 10, new[] { 2, 6, 8 }, new[] { 0, 1, 3, 4, 5, 7, 9 });
        yield return ConstructSingleTestData("Remove all elements", 10, new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, new int[] { });
    }

    [TestCaseSource(nameof(ConstructTestData))]
    public static void Do_InOwnCollection_PerformsDelete(IReadOnlyList<IMapDataEntry> inputSource, IReadOnlyList<IMapDataEntry> entries, IReadOnlyList<IMapDataEntry> expected)
    {
        var col = new MapDataCollection<IMapDataEntry>();
        foreach (var element in inputSource)
            col.Add(element);

        var action = new DeleteMkdsMapDataCollectionItemsAction<IMapDataEntry>(null, entries, col);
        action.Do();

        Assert.That(col, Is.EqualTo(expected));
    }

    [TestCaseSource(nameof(ConstructTestData))]
    public static void Undo_InOwnCollection_PerformsDelete(IReadOnlyList<IMapDataEntry> inputSource, IReadOnlyList<IMapDataEntry> entries, IReadOnlyList<IMapDataEntry> expected)
    {
        var col = new MapDataCollection<IMapDataEntry>();
        foreach (var element in inputSource)
            col.Add(element);

        var action = new DeleteMkdsMapDataCollectionItemsAction<IMapDataEntry>(null, entries, col);
        action.Do();

        Assert.That(col, Is.EqualTo(expected));

        action.Undo();
        Assert.That(col, Is.EqualTo(inputSource));
    }

    [TestCaseSource(nameof(ConstructReferenceTestData))]
    public static void Do_UnsetInternalReferences_PerformsDelete(MkdsMapData mapData, IMapDataEntry entry, IMapDataCollection collection, string referencePropName, IMapDataEntry referencedEntry, IMapDataCollection referencedCollection)
    {
        var action = DeleteMkdsMapDataCollectionItemsActionFactory.Create(mapData, entry, collection);
        action.Do();

        Assert.That(ReferenceHolderHasReferences(referencedEntry) is false);
    }

    [TestCaseSource(nameof(ConstructReferenceTestData))]
    public static void Do_UnsetExternalReferences_PerformsDelete(MkdsMapData mapData, IMapDataEntry entry, IMapDataCollection collection, string referencePropName, IMapDataEntry referencedEntry, IMapDataCollection referencedCollection)
    {
        var action = DeleteMkdsMapDataCollectionItemsActionFactory.Create(mapData, referencedEntry, referencedCollection);
        action.Do();

        Assert.That(GetReferenceTarget(entry, referencePropName) is null);
    }

    [TestCaseSource(nameof(ConstructReferenceTestData))]
    public static void Undo_UnsetInternalReferences_PerformsDelete(MkdsMapData mapData, IMapDataEntry entry, IMapDataCollection collection, string referencePropName, IMapDataEntry referencedEntry, IMapDataCollection referencedCollection)
    {
        var action = DeleteMkdsMapDataCollectionItemsActionFactory.Create(mapData, entry, collection);
        action.Do();
        action.Undo();

        Assert.That(ReferenceHolderHasReferences(referencedEntry) is true);
    }

    [TestCaseSource(nameof(ConstructReferenceTestData))]
    public static void Undo_UnsetExternalReferences_PerformsDelete(MkdsMapData mapData, IMapDataEntry entry, IMapDataCollection collection, string referencePropName, IMapDataEntry referencedEntry, IMapDataCollection referencedCollection)
    {
        var action = DeleteMkdsMapDataCollectionItemsActionFactory.Create(mapData, referencedEntry, referencedCollection);
        action.Do();
        action.Undo();

        Assert.That(GetReferenceTarget(entry, referencePropName) == referencedEntry);
    }

    [TestCaseSource(nameof(ConstructCheckPointPathTestData))]
    public static void Do_OnCheckPointPaths_PerformsDelete(MkdsMapData mapData, IMapDataCollection pathCollection, MkdsCheckPointPath checkEntry, MkdsCheckPointPath deleteEntry, IEnumerable<MkdsCheckPointPath> expectedNext, IEnumerable<MkdsCheckPointPath> expectedPrev)
    {
        var action = DeleteMkdsMapDataCollectionItemsActionFactory.Create(mapData, deleteEntry, pathCollection);
        action.Do();

        var next = checkEntry.Next.Select(x => x.Target);
        var prev = checkEntry.Previous.Select(x => x.Target);

        Assert.That(expectedNext, Is.EqualTo(next));
        Assert.That(expectedPrev, Is.EqualTo(prev));
    }

    [TestCaseSource(nameof(ConstructCheckPointPathTestData))]
    public static void Undo_OnCheckPointPaths_PerformsDelete(MkdsMapData mapData, IMapDataCollection pathCollection, MkdsCheckPointPath checkEntry, MkdsCheckPointPath deleteEntry, IEnumerable<MkdsCheckPointPath> expectedNext, IEnumerable<MkdsCheckPointPath> expectedPrev)
    {
        var action = DeleteMkdsMapDataCollectionItemsActionFactory.Create(mapData, deleteEntry, pathCollection);

        var startNext = checkEntry.Next.Select(x => x.Target);
        var startPrev = checkEntry.Previous.Select(x => x.Target);

        action.Do();
        action.Undo();

        var next = checkEntry.Next.Select(x => x.Target);
        var prev = checkEntry.Previous.Select(x => x.Target);

        Assert.That(next, Is.EqualTo(startNext));
        Assert.That(prev, Is.EqualTo(startPrev));
    }

    [TestCaseSource(nameof(ConstructItemPathTestData))]
    public static void Do_OnItemPaths_PerformsDelete(MkdsMapData mapData, IMapDataCollection pathCollection, MkdsItemPath checkEntry, MkdsItemPath deleteEntry, IEnumerable<MkdsItemPath> expectedNext, IEnumerable<MkdsItemPath> expectedPrev)
    {
        var action = DeleteMkdsMapDataCollectionItemsActionFactory.Create(mapData, deleteEntry, pathCollection);
        action.Do();

        var next = checkEntry.Next.Select(x => x.Target);
        var prev = checkEntry.Previous.Select(x => x.Target);

        Assert.That(expectedNext, Is.EqualTo(next));
        Assert.That(expectedPrev, Is.EqualTo(prev));
    }

    [TestCaseSource(nameof(ConstructItemPathTestData))]
    public static void Undo_OnItemPaths_PerformsDelete(MkdsMapData mapData, IMapDataCollection pathCollection, MkdsItemPath checkEntry, MkdsItemPath deleteEntry, IEnumerable<MkdsItemPath> expectedNext, IEnumerable<MkdsItemPath> expectedPrev)
    {
        var action = DeleteMkdsMapDataCollectionItemsActionFactory.Create(mapData, deleteEntry, pathCollection);

        var startNext = checkEntry.Next.Select(x => x.Target);
        var startPrev = checkEntry.Previous.Select(x => x.Target);

        action.Do();
        action.Undo();

        var next = checkEntry.Next.Select(x => x.Target);
        var prev = checkEntry.Previous.Select(x => x.Target);

        Assert.That(next, Is.EqualTo(startNext));
        Assert.That(prev, Is.EqualTo(startPrev));
    }

    [TestCaseSource(nameof(ConstructEnemyPathTestData))]
    public static void Do_OnEnemyPaths_PerformsDelete(MkdsMapData mapData, IMapDataCollection pathCollection, MkdsEnemyPath checkEntry, MkdsEnemyPath deleteEntry, IEnumerable<MkdsEnemyPath> expectedNext, IEnumerable<MkdsEnemyPath> expectedPrev)
    {
        var action = DeleteMkdsMapDataCollectionItemsActionFactory.Create(mapData, deleteEntry, pathCollection);
        action.Do();

        var next = checkEntry.Next.Select(x => x.Target);
        var prev = checkEntry.Previous.Select(x => x.Target);

        Assert.That(expectedNext, Is.EqualTo(next));
        Assert.That(expectedPrev, Is.EqualTo(prev));
    }

    [TestCaseSource(nameof(ConstructEnemyPathTestData))]
    public static void Undo_OnEnemyPaths_PerformsDelete(MkdsMapData mapData, IMapDataCollection pathCollection, MkdsEnemyPath checkEntry, MkdsEnemyPath deleteEntry, IEnumerable<MkdsEnemyPath> expectedNext, IEnumerable<MkdsEnemyPath> expectedPrev)
    {
        var action = DeleteMkdsMapDataCollectionItemsActionFactory.Create(mapData, deleteEntry, pathCollection);

        var startNext = checkEntry.Next.Select(x => x.Target);
        var startPrev = checkEntry.Previous.Select(x => x.Target);

        action.Do();
        action.Undo();

        var next = checkEntry.Next.Select(x => x.Target);
        var prev = checkEntry.Previous.Select(x => x.Target);

        Assert.That(next, Is.EqualTo(startNext));
        Assert.That(prev, Is.EqualTo(startPrev));
    }

    [TestCaseSource(nameof(ConstructMgEnemyPathTestData))]
    public static void Do_OnMgEnemyPaths_PerformsDelete(MkdsMapData mapData, MkdsMgEnemyPath checkEntry, IEnumerable<MkdsMgEnemyPath> deleteEntries, IEnumerable<MkdsMgEnemyPoint> expectedNext, IEnumerable<MkdsMgEnemyPoint> expectedPrev)
    {
        var action = DeleteMkdsMapDataCollectionItemsActionFactory.Create(mapData, deleteEntries, mapData.MgEnemyPaths);

        action.Do();

        var next = checkEntry.Next.Select(x => x.Target);
        var prev = checkEntry.Previous.Select(x => x.Target);

        Assert.That(expectedNext, Is.EqualTo(next));
        Assert.That(expectedPrev, Is.EqualTo(prev));
    }

    [TestCaseSource(nameof(ConstructMgEnemyPathTestData))]
    public static void Undo_OnMgEnemyPaths_PerformsDelete(MkdsMapData mapData, MkdsMgEnemyPath checkEntry, IEnumerable<MkdsMgEnemyPath> deleteEntries, IEnumerable<MkdsMgEnemyPoint> expectedNext, IEnumerable<MkdsMgEnemyPoint> expectedPrev)
    {
        var action = DeleteMkdsMapDataCollectionItemsActionFactory.Create(mapData, deleteEntries, mapData.MgEnemyPaths);

        var startNext = checkEntry.Next.Select(x => x.Target);
        var startPrev = checkEntry.Previous.Select(x => x.Target);

        action.Do();
        action.Undo();

        var next = checkEntry.Next.Select(x => x.Target);
        var prev = checkEntry.Previous.Select(x => x.Target);

        Assert.That(startNext, Is.EqualTo(next));
        Assert.That(startPrev, Is.EqualTo(prev));
    }

    [TestCaseSource(nameof(ConstructMgEnemyPointTestData))]
    public static void Do_OnMgEnemyPoints_PerformsDelete(MkdsMapData mapData, MkdsMgEnemyPath checkEntry, MkdsMgEnemyPath deleteFromPath, MkdsMgEnemyPoint deleteEntry, IEnumerable<MkdsMgEnemyPoint> expectedNext, IEnumerable<MkdsMgEnemyPoint> expectedPrev)
    {
        var action = DeleteMkdsMapDataCollectionItemsActionFactory.Create(mapData, deleteEntry, deleteFromPath.Points);

        action.Do();

        var next = checkEntry.Next.Select(x => x.Target);
        var prev = checkEntry.Previous.Select(x => x.Target);

        Assert.That(expectedNext, Is.EqualTo(next));
        Assert.That(expectedPrev, Is.EqualTo(prev));
    }

    [TestCaseSource(nameof(ConstructMgEnemyPointTestData))]
    public static void Undo_OnMgEnemyPoints_PerformsDelete(MkdsMapData mapData, MkdsMgEnemyPath checkEntry, MkdsMgEnemyPath deleteFromPath, MkdsMgEnemyPoint deleteEntry, IEnumerable<MkdsMgEnemyPoint> expectedNext, IEnumerable<MkdsMgEnemyPoint> expectedPrev)
    {
        var action = DeleteMkdsMapDataCollectionItemsActionFactory.Create(mapData, deleteEntry, deleteFromPath.Points);

        var startNext = checkEntry.Next.Select(x => x.Target);
        var startPrev = checkEntry.Previous.Select(x => x.Target);

        action.Do();
        action.Undo();

        var next = checkEntry.Next.Select(x => x.Target);
        var prev = checkEntry.Previous.Select(x => x.Target);

        Assert.That(startNext, Is.EqualTo(next));
        Assert.That(startPrev, Is.EqualTo(prev));
    }

    [Test]
    public static void Do_UnsetEnemyPathPointsReferences_PerformsDelete()
    {
        var mapData = ConstructTestMapData();

        var action = DeleteMkdsMapDataCollectionItemsActionFactory.Create(mapData, mapData.EnemyPaths[0], mapData.EnemyPaths);

        action.Do();

        Assert.That(mapData.RespawnPoints[0].EnemyPoint, Is.Null);
    }


    [Test]
    public static void Undo_UnsetEnemyPathPointsReferences_PerformsDelete()
    {
        var mapData = ConstructTestMapData();

        var action = DeleteMkdsMapDataCollectionItemsActionFactory.Create(mapData, mapData.EnemyPaths[0], mapData.EnemyPaths);

        var enemyPoint = mapData.RespawnPoints[0].EnemyPoint.Target;

        action.Do();

        action.Undo();

        Assert.That(mapData.RespawnPoints[0].EnemyPoint.Target, Is.EqualTo(enemyPoint));
    }


    [Test]
    public static void Do_UnsetMgEnemyPathPointsReferences_PerformsDelete()
    {
        var mapData = ConstructTestMapData(true);

        var action = DeleteMkdsMapDataCollectionItemsActionFactory.Create(mapData, mapData.MgEnemyPaths[0], mapData.MgEnemyPaths);

        action.Do();

        Assert.That(mapData.RespawnPoints[0].MgEnemyPoint, Is.Null);
    }

    [Test]
    public static void Undo_UnsetMgEnemyPathPointsReferences_PerformsDelete()
    {
        var mapData = ConstructTestMapData(true);

        var action = DeleteMkdsMapDataCollectionItemsActionFactory.Create(mapData, mapData.MgEnemyPaths[0], mapData.MgEnemyPaths);

        var mgEnemyPoint = mapData.RespawnPoints[0].MgEnemyPoint.Target;

        action.Do();

        action.Undo();

        Assert.That(mapData.RespawnPoints[0].MgEnemyPoint.Target, Is.EqualTo(mgEnemyPoint));
    }
}
