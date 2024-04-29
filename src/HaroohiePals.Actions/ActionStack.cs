namespace HaroohiePals.Actions;

/// <summary>
/// Class for keeping track of performed actions with the ability to undo and redo.
/// </summary>
public sealed class ActionStack
{
    private const string CAPACITY_AT_LEAST_ONE_EXCEPTION_MESSAGE = "Capacity should be at least 1.";
    private const string UNDO_STACK_EMPTY_EXCEPTION_MESSAGE      = "The undo stack is empty.";
    private const string REDO_STACK_EMPTY_EXCEPTION_MESSAGE      = "The redo stack is empty.";

    private readonly LinkedList<IAction> _undo = new();
    private readonly Stack<IAction>      _redo = new();

    /// <summary>
    /// The maximum amount of actions that can be stored in the stack.
    /// </summary>
    public int Capacity { get; }

    /// <summary>
    /// The total number of actions currently stored in the stack.
    /// </summary>
    public int Count => _undo.Count + _redo.Count;

    /// <summary>
    /// The number of redo actions currently stored in the stack.
    /// </summary>
    public int RedoActionsCount => _redo.Count;

    /// <summary>
    /// The number of undo actions currently stored in the stack.
    /// </summary>
    public int UndoActionsCount => _undo.Count;

    /// <summary>
    /// Whether an undo action can currently be performed.
    /// </summary>
    public bool CanUndo => _undo.Count > 0;

    /// <summary>
    /// Whether a redo action can currently be performed.
    /// </summary>
    public bool CanRedo => _redo.Count > 0;

    /// <summary>
    /// Whether the last performed action is a create or delete action.
    /// </summary>
    public bool IsLastActionCreateOrDelete => _undo.Count > 0 ? _undo.Last.Value.IsCreateDelete : false;

    /// <summary>
    /// Creates a new action stack with the given <paramref name="capacity"/>.
    /// </summary>
    /// <param name="capacity">The maximum amount of actions that can be stored in the stack.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="capacity"/> is less than 1.</exception>
    public ActionStack(int capacity = 100)
    {
        if (capacity < 1)
            throw new ArgumentOutOfRangeException(nameof(capacity), capacity, CAPACITY_AT_LEAST_ONE_EXCEPTION_MESSAGE);

        Capacity = capacity;
    }

    /// <summary>
    /// Gets a copy of the current contents of the action stack.
    /// </summary>
    /// <returns>A copy of the contents of the undo and redo stack concatted.</returns>
    public IReadOnlyList<IAction> Peek() => _undo.Concat(_redo).ToList();

    /// <summary>
    /// Adds a new action to the stack.
    /// The action is performed if <paramref name="execute"/> is <see langword="true"/>.
    /// Otherwise it is expected that the action was already performed.
    /// </summary>
    /// <param name="action">The action to add.</param>
    /// <param name="execute">Whether the action should be performed.</param>
    public void Add(IAction action, bool execute = true)
    {
        _redo.Clear();
        _undo.AddLast(action);

        if (_undo.Count > Capacity)
            _undo.RemoveFirst();

        if (execute)
            action.Do();
    }

    /// <summary>
    /// Undoes the last <paramref name="times"/> performed actions.
    /// </summary>
    /// <param name="times">The amount of actions to undo.</param>
    /// <exception cref="InvalidOperationException">Thrown in there were not enough actions to undo.</exception>
    public void Undo(int times)
    {
        for (var i = 0; i < times; i++)
            Undo();
    }

    /// <summary>
    /// Undoes the last performed action.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown in there is no action to undo.</exception>
    public void Undo()
    {
        if (!CanUndo)
            throw new InvalidOperationException(UNDO_STACK_EMPTY_EXCEPTION_MESSAGE);

        var lastAction = _undo.Last.Value;
        _undo.RemoveLast();
        _redo.Push(lastAction);
        lastAction.Undo();
    }

    /// <summary>
    /// Tries to undo the last performed action.
    /// </summary>
    /// <returns><see langword="true"/> if an action was undone, or <see langword="false"/> otherwise.</returns>
    public bool TryUndo()
    {
        if (!CanUndo)
            return false;

        Undo();
        return true;
    }

    /// <summary>
    /// Redoes the last <paramref name="times"/> undone actions.
    /// </summary>
    /// <param name="times">The amount of actions to redo.</param>
    /// <exception cref="InvalidOperationException">Thrown in there were not enough actions to redo.</exception>
    public void Redo(int times)
    {
        for (var i = 0; i < times; i++)
            Redo();
    }

    /// <summary>
    /// Redoes the last undone action.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown in there is no action to redo.</exception>
    public void Redo()
    {
        if (!CanRedo)
            throw new InvalidOperationException(REDO_STACK_EMPTY_EXCEPTION_MESSAGE);

        var newLastAction = _redo.Pop();
        _undo.AddLast(newLastAction);
        newLastAction.Do();
    }

    /// <summary>
    /// Tries to redo the last undone action.
    /// </summary>
    /// <returns><see langword="true"/> if an action was redone, or <see langword="false"/> otherwise.</returns>
    public bool TryRedo()
    {
        if (!CanRedo)
            return false;

        Redo();
        return true;
    }

    /// <summary>
    /// Begins the creation of a set of atomically performed actions.
    /// </summary>
    /// <returns>An <see cref="IAtomicActionBuilder"/> to build the set of atomic actions.</returns>
    public IAtomicActionBuilder PerformAtomic() => new AtomicActionBuilder(this);
}