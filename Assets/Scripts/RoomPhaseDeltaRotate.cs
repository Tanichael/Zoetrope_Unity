public class RoomPhaseDeltaRotate : RoomPhaseBase
{
    public RoomPhaseDeltaRotate(RoomPhaseMachine machine, MockRoomManager roomManager, IRoomCommander roomCommander) : base(machine, roomManager, roomCommander)
    {

    }

    public override RoomPhase GetRoomPhase()
    {
        return RoomPhase.DeltaRotate;
    }

    public override void HandleUIEvent(RoomUIEvent roomUIEvent)
    {
        base.HandleUIEvent(roomUIEvent);
        if(roomUIEvent is BackButtonClickEvent)
        {
            if (m_RoomManager.SelectedObject != null)
            {
                if (m_RoomManager.SelectedFloorObject != null && m_RoomManager.SelectedFloorObject == m_RoomManager.SelectedObject)
                {
                    m_Machine.ChangePhase(new RoomPhaseDetail(m_Machine, m_RoomManager, m_RoomCommander));
                }
                else
                {
                    m_Machine.ChangePhase(new RoomPhaseSelected(m_Machine, m_RoomManager, m_RoomCommander));
                }
            }
        }

        if(roomUIEvent is CompleteDeltaRotateButtoClickEvent)
        {
            if (m_RoomManager.SelectedObject != null)
            {
                if (m_RoomManager.SelectedFloorObject != null && m_RoomManager.SelectedFloorObject == m_RoomManager.SelectedObject)
                {
                    m_Machine.ChangePhase(new RoomPhaseDetail(m_Machine, m_RoomManager, m_RoomCommander));
                }
                else
                {
                    m_Machine.ChangePhase(new RoomPhaseSelected(m_Machine, m_RoomManager, m_RoomCommander));
                }
            }
        }
    }
}
