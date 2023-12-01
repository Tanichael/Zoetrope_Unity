using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

public class RoomCommandDelete : IRoomCommand
{
    private IRoomCommander m_RoomCommander;
    private RoomPhaseMachine m_Machine;
    private MockRoomManager m_RoomManager;
    private RoomUIEvent m_UIEvent;
    private List<RoomObject> m_DeleteObjects = new List<RoomObject>();
    private List<SaveDataUnit> m_DeleteData = new List<SaveDataUnit>();

    public RoomCommandDelete(MockRoomManager roomManager, RoomUIEvent uiEvent, RoomPhaseMachine machine, IRoomCommander roomCommander)
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
        if (m_UIEvent is DeleteButtonClickEvent)
        {
            m_DeleteObjects.Add(m_RoomManager.SelectedObject);
            m_DeleteData.Add(new SaveDataUnit(m_RoomManager.SelectedObject));
            Queue<RoomObject> tempObjQue = new Queue<RoomObject>();
            tempObjQue.Enqueue(m_RoomManager.SelectedObject);
            while (tempObjQue.Count > 0)
            {
                RoomObject parentObject = tempObjQue.Peek();
                tempObjQue.Dequeue();
                foreach (var childObject in parentObject.ChildRoomObjectList)
                {
                    m_DeleteObjects.Add(childObject);
                    m_DeleteData.Add(new SaveDataUnit(childObject));
                    tempObjQue.Enqueue(childObject);
                }
            }

            m_RoomManager.RemoveFamily(m_RoomManager.SelectedObject);

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
            return UniTask.FromResult(true);
        }
        return UniTask.FromResult(false);
    }

    public bool Undo()
    {
        UnityEngine.Debug.Log("Undo Delete");
        RoomObject selectedObject = null;
        
        foreach (var deleteData in m_DeleteData)
        {
            RoomObject roomObject = m_RoomManager.InstantiateRoomObject(deleteData);

            if (deleteData == m_DeleteData[0])
            {
                selectedObject = roomObject;
            }
        }
        m_RoomManager.SelectedObject = selectedObject;
        m_Machine.ChangePhase(new RoomPhaseSelected(m_Machine, m_RoomManager, m_RoomCommander));

        return true;
    }
}