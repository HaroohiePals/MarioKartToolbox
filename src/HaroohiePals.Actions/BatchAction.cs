namespace HaroohiePals.Actions;

public class BatchAction : IAction
{
    private readonly IAction[] _actions;

    public bool IsCreateDelete { get; }

    public BatchAction(IEnumerable<IAction> actions)
    {
        _actions       = actions.ToArray();
        IsCreateDelete = _actions.Any(x => x.IsCreateDelete);
    }

    public void Do()
    {
        foreach (var x in _actions)
            x.Do();
    }

    public void Undo()
    {
        for (int i = _actions.Length - 1; i >= 0; i--)
            _actions[i].Undo();
    }

    public override string ToString() => "{ " + string.Join(", ", _actions.Select(a => a.ToString())) + " }";
}