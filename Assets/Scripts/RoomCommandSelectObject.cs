using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class RoomCommandSelectObject : IRoomCommand
{
    private IRoomCommander m_RoomCommander;
    private RoomObject m_NextObject;
    private RoomPhaseMachine m_Machine;
    private MockRoomManager m_RoomManager;
    private RoomObject m_BefObject;

    public RoomCommandSelectObject(MockRoomManager roomManager, RoomObject nextObject, RoomPhaseMachine machine, IRoomCommander roomCommander)
    {
        m_RoomManager = roomManager;
        m_NextObject = nextObject;
        m_Machine = machine;
        m_RoomCommander = roomCommander;
    }

    public bool CanUndo()
    {
        return true;
    }

    public UniTask<bool> ExecuteAsync(CancellationToken token)
    {
        m_BefObject = m_RoomManager.SelectedObject;
        m_RoomManager.SetSelectedObject(m_NextObject);
        return UniTask.FromResult(true);
    }

    public bool Undo()
    {
        Debug.Log("Undo select");
        if(m_BefObject != null)
        {
            m_RoomManager.SetSelectedObject(m_BefObject);
        }
        else
        {
#if UNITY_EDITOR
            Debug.Log("Bef object is null");
#endif
        }

        return true;
    }

}
