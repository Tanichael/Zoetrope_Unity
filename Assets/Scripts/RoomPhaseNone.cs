using UnityEngine;

public class RoomPhaseNone : RoomPhaseBase
{
    public override RoomPhase GetRoomPhase()
    {
        return RoomPhase.None;
    }

    public RoomPhaseNone(RoomPhaseMachine machine, MockRoomManager roomManager, IRoomCommander roomCommander) : base(machine, roomManager, roomCommander)
    {

    }

    public override void OnEnterState()
    {
        Debug.Log("RoomPhase -> None");
    }

    public override void OnUpdate()
    {
        if(m_Machine.TouchManager.GetCanSelect())
        {
            Camera currentCamera = m_Machine.CurrentCameraController.Camera;
            if(currentCamera != null)
            {
                Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
                int layerMask = (1 << 0) + (1 << 11) + (1 << 12) + (1 << 13);
                RoomSingleton.Instance.ObjectSelector.TapSelect(ray, layerMask);
            }
        }

        if(m_RoomManager.SelectedObject != null)
        {
            if(m_RoomManager.SelectedObject.IsInControl)
            {
                m_Machine.ChangePhase(new RoomPhaseControl(m_Machine, m_RoomManager, m_RoomCommander));
            }
            else if(m_RoomManager.SelectedObject.IsSelected)
            {
                m_Machine.ChangePhase(new RoomPhaseSelected(m_Machine, m_RoomManager, m_RoomCommander));
            }
        }
    }

    public override void HandleUIEvent(RoomUIEvent roomUIEvent)
    {
        base.HandleUIEvent(roomUIEvent);
        if(roomUIEvent is EndEditButtonClickEvent)
        {
            m_Machine.TransitPhase(new RoomPhaseView(m_Machine, m_RoomManager, m_RoomCommander));
        }
    }
}
