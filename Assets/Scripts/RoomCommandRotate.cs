using System.Threading;
using Cysharp.Threading.Tasks;

public class RoomCommandRotate : IRoomCommand
{
    private IRoomCommander m_RoomCommander;
    private RoomPhaseMachine m_Machine;
    private MockRoomManager m_RoomManager;
    private RoomUIEvent m_UIEvent;
    private RoomObject m_RotateObject;
    private RoomObject m_RotateFloorObject;
    private EmptySpace m_RotateSpace;

    public RoomCommandRotate(MockRoomManager roomManager, RoomUIEvent uiEvent, RoomPhaseMachine machine, IRoomCommander roomCommander)
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
        if (m_UIEvent is RotateButtonClickEvent)
        {
            m_RotateObject = m_RoomManager.SelectedObject;
            m_RotateFloorObject = m_RoomManager.SelectedFloorObject;
            m_RotateSpace = m_RoomManager.SelectedSpace != null ? m_RoomManager.SelectedSpace.EmptySpace : null;

            m_RoomManager.RotateObject(m_RotateObject, m_RotateFloorObject, m_RotateSpace);
            return UniTask.FromResult(true);
        }
        else
        {
            return UniTask.FromResult(false);
        }
    }

    public bool Undo()
    {
        UnityEngine.Debug.Log("Undo Rotate");
        for(int i = 0; i < 3; i++)
        {
            m_RoomManager.RotateObject(m_RotateObject, m_RotateFloorObject, m_RotateSpace);
        }

        return true;
    }
}
