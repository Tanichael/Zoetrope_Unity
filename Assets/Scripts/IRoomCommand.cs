using System.Threading;
using Cysharp.Threading.Tasks;

public interface IRoomCommand
{
    //public bool Execute();
    public UniTask<bool> ExecuteAsync(CancellationToken token);
    public bool Undo();
    public bool CanUndo();
}
