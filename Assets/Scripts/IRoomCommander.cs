using System.Threading;
using Cysharp.Threading.Tasks;

public interface IRoomCommander
{
    public UniTask<bool> ExecuteCommandAsync(IRoomCommand command, CancellationToken token);
    public bool ExecuteUIEvent(RoomUIEvent uiEvent, RoomPhaseBase roomPhase);
}
