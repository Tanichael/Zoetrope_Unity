using UnityEngine;

public class RoomPhasePicture : RoomPhaseBase
{
    private RoomPhaseBase m_BefPhase;
    private bool m_IsPrepared;

    public override RoomPhase GetRoomPhase()
    {
        return RoomPhase.Picture;
    }

    public RoomPhasePicture(RoomPhaseMachine machine, MockRoomManager roomManager, RoomPhaseBase befPhase, IRoomCommander roomCommander) : base(machine, roomManager, roomCommander)
    {
        m_BefPhase = befPhase;
    }

    public override void OnEnterState()
    {
        Debug.Log("RoomPhase -> Picture");
    }

    public override void HandleUIEvent(RoomUIEvent roomUIEvent)
    {
        base.HandleUIEvent(roomUIEvent);
        if(roomUIEvent is PickCompleteEvent)
        {
            m_Machine.ChangePhase(new RoomPhaseTrim(m_Machine, m_RoomManager, m_BefPhase, m_RoomCommander));

        }
        if(roomUIEvent is EditCompleteButtonClickEvent)
        {
            if(m_RoomManager.SelectedSpace != null)
            {
                m_RoomManager.SelectedSpace.IsOutlineEnabled = true;
            }

            if(m_BefPhase != null)
            {
                if(m_BefPhase is RoomPhaseView)
                {
                    m_RoomManager.SelectedObject = null;
                    m_Machine.ChangePhase(new RoomPhaseView(m_Machine, m_RoomManager, m_RoomCommander));
                }
                else if(m_BefPhase is RoomPhaseSelected)
                {
                    m_RoomManager.SelectedObject.OnSelected(true);
                    m_Machine.ChangePhase(new RoomPhaseSelected(m_Machine, m_RoomManager, m_RoomCommander));
                }
                else if(m_BefPhase is RoomPhaseDetail)
                {
                    m_RoomManager.SelectedObject.OnDetail(true);
                    m_Machine.ChangePhase(new RoomPhaseDetail(m_Machine, m_RoomManager, m_RoomCommander));
                }
            }
        }
    }
}
