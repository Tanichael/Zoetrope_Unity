using System.Threading;
using Cysharp.Threading.Tasks;

public class RoomCommandRemoveAll : IRoomCommand
{
    private IRoomCommander m_RoomCommander;
    private RoomPhaseMachine m_Machine;
    private MockRoomManager m_RoomManager;
    private RoomUIEvent m_UIEvent;

    public RoomCommandRemoveAll(MockRoomManager roomManager, RoomUIEvent uiEvent, RoomPhaseMachine machine, IRoomCommander roomCommander)
    {
        m_RoomManager = roomManager;
        m_UIEvent = uiEvent;
        m_Machine = machine;
        m_RoomCommander = roomCommander;
    }

    public bool CanUndo()
    {
        return false;
    }

    public UniTask<bool> ExecuteAsync(CancellationToken token)
    {
        if (m_UIEvent is RemoveAllButtonClickEvent)
        {
            /*m_Machine.OnRoomEvent.Invoke(new RoomEventLogTwoAnswer("本当にすべて削除しますか？"));*/
            m_Machine.FireRoomEvent(new RoomEventLogTwoAnswer("本当にすべて削除しますか？", new RemoveAllYesButtonClickEvent(), new RemoveAllNoButtonClickEvent()));
            return UniTask.FromResult(true);
        }
        else
        {
            return UniTask.FromResult(false);
        }
    }

    public bool Undo()
    {
        UnityEngine.Debug.Log("Undo RemoveAll");
        return true;
    }
}

public class RoomCommandRemoveAllYes : IRoomCommand
{
    private IRoomCommander m_RoomCommander;
    private RoomPhaseMachine m_Machine;
    private MockRoomManager m_RoomManager;
    private RoomUIEvent m_UIEvent;
    private SaveData m_SaveData;

    public RoomCommandRemoveAllYes(MockRoomManager roomManager, RoomUIEvent uiEvent, RoomPhaseMachine machine, IRoomCommander roomCommander)
    {
        m_RoomManager = roomManager;
        m_UIEvent = uiEvent;
        m_Machine = machine;
        m_RoomCommander = roomCommander;
    }

    public bool CanUndo()
    {
        return true;
    }

    public UniTask<bool> ExecuteAsync(CancellationToken token)
    {
        if (m_UIEvent is RemoveAllYesButtonClickEvent)
        {
            m_SaveData = RoomSingleton.Instance.RoomSaveManager.GetRoomState(m_RoomManager);

            m_RoomManager.RemoveAll();
            m_RoomManager.SelectedObject = null;
            m_RoomManager.SelectedFloorObject = null;
            m_RoomManager.SelectedSpace = null;
            m_Machine.ChangePhase(new RoomPhaseNone(m_Machine, m_RoomManager, m_RoomCommander));
            m_Machine.FireRoomEvent(new RoomEventLogEnd(""));
            return UniTask.FromResult(true);
        }
        else
        {
            return UniTask.FromResult(false);
        }
    }

    public bool Undo()
    {
        UnityEngine.Debug.Log("Undo RemoveAllYes");
        m_RoomManager.Init(m_SaveData, null, null, 0f, null);

        return true;
    }
}

public class RoomCommandRemoveAllNo : IRoomCommand
{
    private IRoomCommander m_RoomCommander;
    private RoomPhaseMachine m_Machine;
    private MockRoomManager m_RoomManager;
    private RoomUIEvent m_UIEvent;
    private SaveData m_SaveData;

    public RoomCommandRemoveAllNo(MockRoomManager roomManager, RoomUIEvent uiEvent, RoomPhaseMachine machine, IRoomCommander roomCommander)
    {
        m_RoomManager = roomManager;
        m_UIEvent = uiEvent;
        m_Machine = machine;
        m_RoomCommander = roomCommander;
    }

    public bool CanUndo()
    {
        return false;
    }

    public UniTask<bool> ExecuteAsync(CancellationToken token)
    {
        if (m_UIEvent is RemoveAllNoButtonClickEvent)
        {
            UnityEngine.Debug.Log("Do not execute remove all");
            m_Machine.FireRoomEvent(new RoomEventLogEnd(""));
            return UniTask.FromResult(true);
        }
        else
        {
            return UniTask.FromResult(false);
        }
    }

    public bool Undo()
    {
        UnityEngine.Debug.Log("Undo RemoveAllNo");
        return true;
    }
}