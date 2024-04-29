using HaroohiePals.MarioKart.MapData;
using Moq;

namespace HaroohiePals.MarioKart.Test.MapData;

class MapDataCollectionTests
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

    private static IEnumerable<TestCaseData> ConstructMoveInOwnCollectionTestData(string testCaseName, int sourceIndex, int targetIndex, int[] expectedSequence)
    {
        var input = ConstructMockEntries(expectedSequence.Length);

        var entry = input[sourceIndex];
        var expected = new List<IMapDataEntry>();
        foreach (int index in expectedSequence)
            expected.Add(input[index]);

        yield return new TestCaseData(input, entry, targetIndex, expected).SetArgDisplayNames(testCaseName);
    }

    private static IEnumerable<TestCaseData> ConstructMoveInOtherCollectionTestData(string testCaseName, int sourceLength, int targetLength, int sourceIndex, int targetIndex, string[] expectedSourceSequence, string[] expectedTargetSequence)
    {
        const string SOURCE_ENTRY_NAME_PREFIX = "S";
        const string TARGET_ENTRY_NAME_PREFIX = "T";

        var source = ConstructMockEntries(sourceLength, SOURCE_ENTRY_NAME_PREFIX);
        var target = ConstructMockEntries(targetLength, TARGET_ENTRY_NAME_PREFIX);

        var entry = source[sourceIndex];
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

        yield return new TestCaseData(source, target, entry, targetIndex, expectedSource, expectedTarget).SetArgDisplayNames(testCaseName);
    }


    [TestCaseSource(nameof(ConstructMoveInOwnCollectionTestData), new object[] { "Move 0 to 3", 0, 3, new[] { 1, 2, 0, 3, 4 } })]
    [TestCaseSource(nameof(ConstructMoveInOwnCollectionTestData), new object[] { "Move 0 to 4", 0, 4, new[] { 1, 2, 3, 0, 4 } })]
    [TestCaseSource(nameof(ConstructMoveInOwnCollectionTestData), new object[] { "Move 0 to 5", 0, 5, new[] { 1, 2, 3, 4, 0 } })]
    [TestCaseSource(nameof(ConstructMoveInOwnCollectionTestData), new object[] { "Move 4 to 0", 4, 0, new[] { 4, 0, 1, 2, 3 } })]
    [TestCaseSource(nameof(ConstructMoveInOwnCollectionTestData), new object[] { "Move 4 to 1", 4, 1, new[] { 0, 4, 1, 2, 3 } })]
    [TestCaseSource(nameof(ConstructMoveInOwnCollectionTestData), new object[] { "Move 4 to 4 (no change)", 4, 4, new[] { 0, 1, 2, 3, 4 } })]
    [TestCaseSource(nameof(ConstructMoveInOwnCollectionTestData), new object[] { "Move 4 to 5 (no change)", 4, 4, new[] { 0, 1, 2, 3, 4 } })]
    [TestCaseSource(nameof(ConstructMoveInOwnCollectionTestData), new object[] { "Move 0 to 0 (no change)", 0, 0, new[] { 0, 1, 2, 3, 4 } })]
    public static void Move_InOwnCollection_PerformsMove(IReadOnlyList<IMapDataEntry> input, IMapDataEntry entry, int targetIndex, IReadOnlyList<IMapDataEntry> expected)
    {
        var col = new MapDataCollection<IMapDataEntry>();
        foreach (var element in input)
            col.Add(element);

        col.Move(targetIndex, entry);

        Assert.That(col, Is.EqualTo(expected));
    }

    [TestCaseSource(nameof(ConstructMoveInOtherCollectionTestData), new object[] { "Move S3 before T1", 5, 5, 3, 1,
        new[] { "S0", "S1", "S2", "S4" }, new[] { "T0", "S3", "T1", "T2", "T3", "T4" } })]
    [TestCaseSource(nameof(ConstructMoveInOtherCollectionTestData), new object[] { "Move S0 before T0", 5, 5, 0, 0,
        new[] { "S1", "S2", "S3", "S4" }, new[] { "S0", "T0", "T1", "T2", "T3", "T4" } })]
    [TestCaseSource(nameof(ConstructMoveInOtherCollectionTestData), new object[] { "Move S0 after T4", 5, 5, 0, 5,
        new[] { "S1", "S2", "S3", "S4" }, new[] { "T0", "T1", "T2", "T3", "T4", "S0" } })]
    [TestCaseSource(nameof(ConstructMoveInOtherCollectionTestData), new object[] { "Move S4 before T0", 5, 5, 4, 0,
        new[] { "S0", "S1", "S2", "S3" }, new[] { "S4", "T0", "T1", "T2", "T3", "T4" } })]
    [TestCaseSource(nameof(ConstructMoveInOtherCollectionTestData), new object[] { "Move S4 after T4", 5, 5, 4, 5,
        new[] { "S0", "S1", "S2", "S3" }, new[] { "T0", "T1", "T2", "T3", "T4", "S4" } })]
    public static void Move_InOtherCollection_PerformsMove(IReadOnlyList<IMapDataEntry> sourceInput, IReadOnlyList<IMapDataEntry> targetInput, IMapDataEntry sourceEntry, int targetIndex, IReadOnlyList<IMapDataEntry> expectedSource, IReadOnlyList<IMapDataEntry> expectedTarget)
    {
        var sourceCol = new MapDataCollection<IMapDataEntry>();
        var targetCol = new MapDataCollection<IMapDataEntry>();

        foreach (var element in sourceInput)
            sourceCol.Add(element);
        foreach (var element in targetInput)
            targetCol.Add(element);

        sourceCol.Move(targetIndex, sourceEntry, targetCol);

        Assert.That(sourceCol, Is.EqualTo(expectedSource));
        Assert.That(targetCol, Is.EqualTo(expectedTarget));
    }
}
