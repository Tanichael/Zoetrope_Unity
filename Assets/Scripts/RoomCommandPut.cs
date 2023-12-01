using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class RoomCommandPut : IRoomCommand
{
    private IRoomCommander m_RoomCommander;
    private RoomPhaseMachine m_Machine;
    private MockRoomManager m_RoomManager;
    private RoomUIEvent m_UIEvent;
    private RoomObject m_PutObject;

    public RoomCommandPut(MockRoomManager roomManager, RoomUIEvent uiEvent, RoomPhaseMachine machine, IRoomCommander roomCommander)
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
        if (m_UIEvent is PutItemEvent)
        {
            PutItemEvent putItemEvent = m_UIEvent as PutItemEvent;
            RoomObject roomObject = m_RoomManager.Put(putItemEvent.Data);
            if (roomObject == null)
            {
                //どこにも置けなかった場合
                return UniTask.FromResult(false);
            }
            else
            {
                //置くことができた場合
                m_RoomManager.SelectedObject = roomObject;
                m_PutObject = roomObject;
                m_Machine.ChangePhase(new RoomPhaseSelected(m_Machine, m_RoomManager, m_RoomCommander));
                return UniTask.FromResult(true);
            }
        }
        else
        {
            return UniTask.FromResult(false);
        }
    }

    public bool Undo()
    {
        Debug.Log("Undo Put");
        //if(m_RoomManager.SelectedObject != null)
        if(m_PutObject != null)
        {
            //m_RoomManager.RemoveFamily(m_RoomManager.SelectedObject);
            m_RoomManager.RemoveFamily(m_PutObject);

            if (m_RoomManager.SelectedFloorObject == null)
            {
                m_RoomManager.SelectedObject = null;
                m_Machine.ChangePhase(new RoomPhaseNone(m_Machine, m_RoomManager, m_RoomCommander));
            }
            else
            {
                m_RoomManager.SelectedObject = m_RoomManager.SelectedFloorObject;
                m_Machine.ChangePhase(new RoomPhaseEmptySpace(m_Machine, m_RoomManager, m_RoomCommander));
            }
            return true;
        }
        else
        {
            Debug.Log("put object is null");
        }
        return false;
    }
}