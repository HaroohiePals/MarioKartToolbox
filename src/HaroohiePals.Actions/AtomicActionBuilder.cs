namespace HaroohiePals.Actions;

internal class AtomicActionBuilder : IAtomicActionBuilder
{
    private readonly ActionStack _actionStack;
    private readonly List<IAction> _actions = new();

    private bool _finished = false;

    public AtomicActionBuilder(ActionStack actionStack)
    {
        _actionStack = actionStack;
    }

    public IAtomicActionBuilder Do(IAction action)
    {
        CheckFinished();

        action.Do();
        _actions.Add(action);

        return this;
    }

    public void Commit()
    {
        CheckFinished();

        if (_actions.Count > 0)
            _actionStack.Add(new BatchAction(_actions), false);

        _finished = true;
    }

    public void Cancel()
    {
        CheckFinished();

        if (_actions.Count > 0)
            new BatchAction(_actions).Undo();

        _finished = true;
    }

    private void CheckFinished()
    {
        if (_finished)
            throw new InvalidOperationException("The action has already finished.");
    }
}
