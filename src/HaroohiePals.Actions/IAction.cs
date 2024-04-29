namespace HaroohiePals.Actions;

public interface IAction
{
    bool IsCreateDelete { get; }
    void Undo();
    void Do();
}
