using UnityEngine;

public class RoomPhaseTrim : RoomPhaseBase
{
    private RoomPhaseBase m_BefPhase;

    public override RoomPhase GetRoomPhase()
    {
        return RoomPhase.Trim;
    }

    public RoomPhaseTrim(RoomPhaseMachine machine, MockRoomManager roomManager, RoomPhaseBase befPhase, IRoomCommander roomCommander) : base(machine, roomManager, roomCommander)
    {
        m_BefPhase = befPhase;
    }

    public override void OnEnterState()
    {
        Debug.Log("RoomPhase -> Trim");
    }

    public override void OnUpdate()
    {
        
    }

    public override void OnExitState()
    {
        
    }

    public override void HandleUIEvent(RoomUIEvent roomUIEvent)
    {
        base.HandleUIEvent(roomUIEvent);
        if(roomUIEvent is CompleteTrimButtonClickEvent)
        {
            CompleteTrimButtonClickEvent trimEvent = roomUIEvent as CompleteTrimButtonClickEvent;
            //AcrylStandの場合メッシュの生成が必要
            m_RoomManager.SelectedObject.SetTexture(trimEvent.TrimmedTexture, trimEvent.SetMaterialEvent);
            m_Machine.ChangePhase(new RoomPhasePicture(m_Machine, m_RoomManager, m_BefPhase, m_RoomCommander));
        }
    }
}
