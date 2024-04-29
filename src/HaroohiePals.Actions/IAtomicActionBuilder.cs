namespace HaroohiePals.Actions;

public interface IAtomicActionBuilder
{
    void Cancel();
    void Commit();
    IAtomicActionBuilder Do(IAction action);
}