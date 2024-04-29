using FluentAssertions;
using HaroohiePals.Actions;

namespace HaroohiePals.Gui.Test.Actions;

public class SetPropertyActionExtensionsTests
{
    private class Foo
    {
        public int Bar { get; set; }
    }

    [Fact]
    public void SetPropertyAction_ValidExpression_GeneratesSetPropertyAction()
    {
        const int TEST_OLD_VALUE = 1;
        const int TEST_NEW_VALUE = 2;
        var       testFoo        = new Foo { Bar = TEST_OLD_VALUE };

        var action = testFoo.SetPropertyAction(o => o.Bar, TEST_NEW_VALUE);

        testFoo.Bar.Should().Be(TEST_OLD_VALUE);
        action.Do();
        testFoo.Bar.Should().Be(TEST_NEW_VALUE);
        action.Undo();
        testFoo.Bar.Should().Be(TEST_OLD_VALUE);
    }

    [Fact]
    public void SetPropertyAction_InvalidExpression_Throws()
    {
        var testFoo = new Foo();

        var action = () => testFoo.SetPropertyAction(o => 1, 0);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SetPropertyAction_ExpressionWithCapture_Throws()
    {
        var testFoo = new Foo();

        var action = () => testFoo.SetPropertyAction(o => testFoo.Bar, 0);

        action.Should().Throw<ArgumentException>();
    }
}