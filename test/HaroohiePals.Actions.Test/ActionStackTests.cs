using FluentAssertions;
using HaroohiePals.Actions;
using Moq;

namespace HaroohiePals.Gui.Test.Actions;

public class ActionStackTests
{
    [Fact]
    public void Add_WithExecute_AddsToStackAndExecutes()
    {
        const int STACK_CAPACITY = 10;
        var       actionMock     = new Mock<IAction>();
        var       actionStack    = new ActionStack(STACK_CAPACITY);
        var       action         = actionMock.Object;

        actionStack.Add(action);

        actionMock.Verify(mockAction => mockAction.Do(), Times.Once());
        actionMock.VerifyNoOtherCalls();
        actionStack.Capacity.Should().Be(STACK_CAPACITY);
        actionStack.Count.Should().Be(1);
        actionStack.UndoActionsCount.Should().Be(1);
        actionStack.RedoActionsCount.Should().Be(0);
        actionStack.CanUndo.Should().BeTrue();
        actionStack.CanRedo.Should().BeFalse();
        actionStack.Peek().Should().ContainSingle().Which.Should().Be(action);
    }

    [Fact]
    public void Add_WithoutExecute_AddsToStackAndDoesNotExecute()
    {
        const int STACK_CAPACITY = 10;
        var       actionMock     = new Mock<IAction>();
        var       actionStack    = new ActionStack(STACK_CAPACITY);
        var       action         = actionMock.Object;

        actionStack.Add(action, false);

        actionMock.Verify(mockAction => mockAction.Do(), Times.Never());
        actionMock.VerifyNoOtherCalls();
        actionStack.Capacity.Should().Be(STACK_CAPACITY);
        actionStack.Count.Should().Be(1);
        actionStack.UndoActionsCount.Should().Be(1);
        actionStack.RedoActionsCount.Should().Be(0);
        actionStack.CanUndo.Should().BeTrue();
        actionStack.CanRedo.Should().BeFalse();
        actionStack.Peek().Should().ContainSingle().Which.Should().Be(action);
    }

    [Fact]
    public void Add_MoreThanCapacity_DeletesOldest()
    {
        const int STACK_CAPACITY = 10;
        var       actions        = new IAction[STACK_CAPACITY * 2];
        for (int i = 0; i < actions.Length; i++)
        {
            var actionMock = new Mock<IAction>();
            actions[i] = actionMock.Object;
        }

        var actionStack = new ActionStack(STACK_CAPACITY);

        for (int i = 0; i < actions.Length; i++)
        {
            actionStack.Add(actions[i], false);
        }

        actionStack.Capacity.Should().Be(STACK_CAPACITY);
        actionStack.Count.Should().Be(STACK_CAPACITY);
        actionStack.UndoActionsCount.Should().Be(STACK_CAPACITY);
        actionStack.RedoActionsCount.Should().Be(0);
        actionStack.CanUndo.Should().BeTrue();
        actionStack.CanRedo.Should().BeFalse();
        actionStack.Peek().Should().Equal(actions[STACK_CAPACITY..]);
    }

    [Fact]
    public void Undo_PerformsUndoAndMovesToRedo()
    {
        const int STACK_CAPACITY = 10;
        var       actionMock     = new Mock<IAction>();
        var       actionStack    = new ActionStack(STACK_CAPACITY);
        var       action         = actionMock.Object;

        actionStack.Add(action);

        actionMock.Verify(mockAction => mockAction.Do(), Times.Once());
        actionMock.VerifyNoOtherCalls();

        actionStack.Undo();

        actionMock.Verify(mockAction => mockAction.Undo(), Times.Once());
        actionMock.VerifyNoOtherCalls();

        actionStack.Capacity.Should().Be(STACK_CAPACITY);
        actionStack.Count.Should().Be(1);
        actionStack.UndoActionsCount.Should().Be(0);
        actionStack.RedoActionsCount.Should().Be(1);
        actionStack.CanUndo.Should().BeFalse();
        actionStack.CanRedo.Should().BeTrue();
        actionStack.Peek().Should().ContainSingle().Which.Should().Be(action);
    }

    [Fact]
    public void Undo_ThrowsExceptionWhenEmpty()
    {
        const int STACK_CAPACITY = 10;
        var       actionStack    = new ActionStack(STACK_CAPACITY);

        var action = () => actionStack.Undo();

        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void TryUndo_ReturnsFalseWhenEmpty()
    {
        const int STACK_CAPACITY = 10;
        var       actionStack    = new ActionStack(STACK_CAPACITY);

        actionStack.TryUndo().Should().BeFalse();
    }

    [Fact]
    public void Redo_PerformsRedoAndMovesToUndo()
    {
        const int STACK_CAPACITY = 10;
        var       actionMock     = new Mock<IAction>();
        var       actionStack    = new ActionStack(STACK_CAPACITY);
        var       action         = actionMock.Object;

        actionStack.Add(action);

        actionMock.Verify(mockAction => mockAction.Do(), Times.Once());
        actionMock.VerifyNoOtherCalls();

        actionStack.Undo();

        actionMock.Verify(mockAction => mockAction.Undo(), Times.Once());
        actionMock.VerifyNoOtherCalls();

        actionStack.Redo();

        actionMock.Verify(mockAction => mockAction.Do(), Times.Exactly(2));
        actionMock.VerifyNoOtherCalls();

        actionStack.Capacity.Should().Be(STACK_CAPACITY);
        actionStack.Count.Should().Be(1);
        actionStack.UndoActionsCount.Should().Be(1);
        actionStack.RedoActionsCount.Should().Be(0);
        actionStack.CanUndo.Should().BeTrue();
        actionStack.CanRedo.Should().BeFalse();
        actionStack.Peek().Should().ContainSingle().Which.Should().Be(action);
    }

    [Fact]
    public void Redo_ThrowsExceptionWhenEmpty()
    {
        const int STACK_CAPACITY = 10;
        var       actionStack    = new ActionStack(STACK_CAPACITY);

        var action = () => actionStack.Redo();

        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void TryRedo_ReturnsFalseWhenEmpty()
    {
        const int STACK_CAPACITY = 10;
        var       actionStack    = new ActionStack(STACK_CAPACITY);

        actionStack.TryRedo().Should().BeFalse();
    }
}