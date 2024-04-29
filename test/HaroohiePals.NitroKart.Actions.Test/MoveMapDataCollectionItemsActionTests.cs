using HaroohiePals.MarioKart.MapData;
using Moq;

namespace HaroohiePals.NitroKart.Actions.Test;

class MoveMapDataCollectionItemsActionTests
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

    private static TestCaseData ConstructSingleTestData(string testCaseName, int[] sourceIndices, int targetIndex, int[] expectedSequence)
    {
        var input = ConstructMockEntries(expectedSequence.Length);

        var sourceEntries = sourceIndices.Select(i => input[i]).ToList();
        var expected = new List<IMapDataEntry>();
        foreach (int index in expectedSequence)
            expected.Add(input[index]);

        return new TestCaseData(input, sourceEntries, targetIndex, expected).SetArgDisplayNames(testCaseName);
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

    private static IEnumerable<TestCaseData> ConstructInOwnCollectionTestData()
    {
        yield return ConstructSingleTestData("Move adjacent 3,4,5 to 10", new[] { 3, 4, 5 }, 10, new[] { 0, 1, 2, 6, 7, 8, 9, 3, 4, 5 });
        yield return ConstructSingleTestData("Move adjacent 3,4,5 to 0", new[] { 3, 4, 5 }, 0, new[] { 3, 4, 5, 0, 1, 2, 6, 7, 8, 9, });
        yield return ConstructSingleTestData("Move adjacent 3,4,5 to 2", new[] { 3, 4, 5 }, 2, new[] { 0, 1, 3, 4, 5, 2, 6, 7, 8, 9 });
        yield return ConstructSingleTestData("Move adjacent 3,4,5 to 3 (no change)", new[] { 3, 4, 5 }, 3, new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });

        yield return ConstructSingleTestData("Move 3,6,8 to 2", new[] { 3, 6, 8 }, 2, new[] { 0, 1, 3, 6, 8, 2, 4, 5, 7, 9 });
        yield return ConstructSingleTestData("Move 3,6,8 to 5", new[] { 3, 6, 8 }, 5, new[] { 0, 1, 2, 4, 3, 6, 8, 5, 7, 9 });
        yield return ConstructSingleTestData("Move 3,6,8 to 8", new[] { 3, 6, 8 }, 8, new[] { 0, 1, 2, 4, 5, 7, 3, 6, 8, 9 });
        yield return ConstructSingleTestData("Move 3,6,8 to 0", new[] { 3, 6, 8 }, 0, new[] { 3, 6, 8, 0, 1, 2, 4, 5, 7, 9 });
        yield return ConstructSingleTestData("Move 3,6,8 to 10", new[] { 3, 6, 8 }, 10, new[] { 0, 1, 2, 4, 5, 7, 9, 3, 6, 8 });
    }

    private static IEnumerable<TestCaseData> ConstructInOtherCollectionTestData()
    {
        //new[] { "S0", "S1", "S2", "S3", "S4", "S5", "S6", "S7", "S8", "S9" },
        //new[] { "T0", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9" });

        yield return ConstructSingleTestData("Move adjacent S3,4,5 after T9", 10, 10, new[] { 3, 4, 5 }, 10,
            new[] { "S0", "S1", "S2", "S6", "S7", "S8", "S9" },
            new[] { "T0", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "S3", "S4", "S5" });
        yield return ConstructSingleTestData("Move adjacent S3,4,5 before T0", 10, 10, new[] { 3, 4, 5 }, 0,
            new[] { "S0", "S1", "S2", "S6", "S7", "S8", "S9" },
            new[] { "S3", "S4", "S5", "T0", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9" });
        yield return ConstructSingleTestData("Move adjacent S3,4,5 before T2", 10, 10, new[] { 3, 4, 5 }, 2,
            new[] { "S0", "S1", "S2", "S6", "S7", "S8", "S9" },
            new[] { "T0", "T1", "S3", "S4", "S5", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9" });
        yield return ConstructSingleTestData("Move adjacent S3,4,5 before T6", 10, 10, new[] { 3, 4, 5 }, 6,
            new[] { "S0", "S1", "S2", "S6", "S7", "S8", "S9" },
            new[] { "T0", "T1", "T2", "T3", "T4", "T5", "S3", "S4", "S5", "T6", "T7", "T8", "T9" });

        yield return ConstructSingleTestData("Move S0,5,9 after T9", 10, 10, new[] { 0, 5, 9 }, 10,
            new[] { "S1", "S2", "S3", "S4", "S6", "S7", "S8" },
            new[] { "T0", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "S0", "S5", "S9" });
        yield return ConstructSingleTestData("Move S0,5,9 before T0", 10, 10, new[] { 0, 5, 9 }, 0,
            new[] { "S1", "S2", "S3", "S4", "S6", "S7", "S8" },
            new[] { "S0", "S5", "S9", "T0", "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9" });
        yield return ConstructSingleTestData("Move S0,5,9 before T5", 10, 10, new[] { 0, 5, 9 }, 5,
            new[] { "S1", "S2", "S3", "S4", "S6", "S7", "S8" },
            new[] { "T0", "T1", "T2", "T3", "T4", "S0", "S5", "S9", "T5", "T6", "T7", "T8", "T9" });
    }


    [TestCaseSource(nameof(ConstructInOwnCollectionTestData))]
    public static void Do_InOwnCollection_PerformsMove(IReadOnlyList<IMapDataEntry> inputSource, IReadOnlyList<IMapDataEntry> entries, int targetIndex, IReadOnlyList<IMapDataEntry> expected)
    {
        var sourceCol = new MapDataCollection<IMapDataEntry>();
        foreach (var element in inputSource)
            sourceCol.Add(element);

        var action = new MoveMkdsMapDataCollectionItemsAction<IMapDataEntry>(entries, sourceCol, targetIndex);
        action.Do();

        Assert.That(sourceCol, Is.EqualTo(expected));
    }

    [TestCaseSource(nameof(ConstructInOwnCollectionTestData))]
    public static void Undo_InOwnCollection_PerformsMove(IReadOnlyList<IMapDataEntry> inputSource, IReadOnlyList<IMapDataEntry> entries, int targetIndex, IReadOnlyList<IMapDataEntry> expected)
    {
        var sourceCol = new MapDataCollection<IMapDataEntry>();
        foreach (var element in inputSource)
            sourceCol.Add(element);

        var action = new MoveMkdsMapDataCollectionItemsAction<IMapDataEntry>(entries, sourceCol, targetIndex);
        action.Do();

        Assert.That(sourceCol, Is.EqualTo(expected));

        action.Undo();
        Assert.That(sourceCol, Is.EqualTo(inputSource));
    }

    [TestCaseSource(nameof(ConstructInOtherCollectionTestData))]
    public static void Do_InOtherCollection_PerformsMove(IReadOnlyList<IMapDataEntry> inputSource, IReadOnlyList<IMapDataEntry> inputTarget, IReadOnlyList<IMapDataEntry> entries, int targetIndex, IReadOnlyList<IMapDataEntry> expectedSource, IReadOnlyList<IMapDataEntry> expectedTarget)
    {
        var sourceCol = new MapDataCollection<IMapDataEntry>();
        foreach (var element in inputSource)
            sourceCol.Add(element);
        var targetCol = new MapDataCollection<IMapDataEntry>();
        foreach (var element in inputTarget)
            targetCol.Add(element);

        var action = new MoveMkdsMapDataCollectionItemsAction<IMapDataEntry>(entries, sourceCol, targetCol, targetIndex);
        action.Do();

        Assert.That(sourceCol, Is.EqualTo(expectedSource));
        Assert.That(targetCol, Is.EqualTo(expectedTarget));
    }

    [TestCaseSource(nameof(ConstructInOtherCollectionTestData))]
    public static void Undo_InOtherCollection_PerformsMove(IReadOnlyList<IMapDataEntry> inputSource, IReadOnlyList<IMapDataEntry> inputTarget, IReadOnlyList<IMapDataEntry> entries, int targetIndex, IReadOnlyList<IMapDataEntry> expectedSource, IReadOnlyList<IMapDataEntry> expectedTarget)
    {
        var sourceCol = new MapDataCollection<IMapDataEntry>();
        foreach (var element in inputSource)
            sourceCol.Add(element);
        var targetCol = new MapDataCollection<IMapDataEntry>();
        foreach (var element in inputTarget)
            targetCol.Add(element);

        var action = new MoveMkdsMapDataCollectionItemsAction<IMapDataEntry>(entries, sourceCol, targetCol, targetIndex);
        action.Do();

        Assert.That(sourceCol, Is.EqualTo(expectedSource));
        Assert.That(targetCol, Is.EqualTo(expectedTarget));

        action.Undo();

        Assert.That(sourceCol, Is.EqualTo(inputSource));
        Assert.That(targetCol, Is.EqualTo(inputTarget));
    }
}
