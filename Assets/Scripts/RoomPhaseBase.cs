public abstract class RoomPhaseBase
{
    protected RoomPhaseMachine m_Machine;
    protected MockRoomManager m_RoomManager;
    protected IRoomCommander m_RoomCommander;

    public RoomPhaseBase(RoomPhaseMachine machine, MockRoomManager roomManager, IRoomCommander roomCommander)
    {
        m_Machine = machine;
        m_RoomManager = roomManager;
        m_RoomCommander = roomCommander;
    }

    public virtual void OnEnterState() { }
    public virtual void OnUpdate() { }
    public virtual void OnExitState() { }
    public virtual void HandleUIEvent(RoomUIEvent roomUIEvent)
    {
        m_RoomCommander.ExecuteUIEvent(roomUIEvent, this);
    }

    public abstract RoomPhase GetRoomPhase();
}
