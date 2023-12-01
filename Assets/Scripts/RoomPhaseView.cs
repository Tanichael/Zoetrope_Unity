using UnityEngine;

public class RoomPhaseView : RoomPhaseBase
{
    private AvatarController m_AvatarController;
    private RoomObject m_SuspectObject;
    private AvatarController m_SuspectAvatar;

    public override RoomPhase GetRoomPhase()
    {
        return RoomPhase.View;
    }

    public RoomPhaseView(RoomPhaseMachine machine, MockRoomManager roomManager, IRoomCommander roomCommander) : base(machine, roomManager, roomCommander)
    {

    }

    public override void OnEnterState()
    {
        base.OnEnterState();
        Debug.Log("RoomPhase -> View");
        m_RoomManager.AvatarController.gameObject.SetActive(true);
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        Camera currentCamera = m_Machine.CurrentCameraController.Camera;
        if(currentCamera != null)
        {
            Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
            if(Input.GetMouseButtonDown(0) && m_Machine.TouchManager.GetCanSelect())
            {
                int layerMask = (1 << 0);
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
                {
                    RoomObject roomObject = hit.transform.gameObject.GetComponent<RoomObject>();
                    if(roomObject != null && roomObject.Data.Type == RoomObjectType.ITEM)
                    {
                        m_SuspectObject = roomObject;
                    }

                    AvatarController avatarController = hit.transform.gameObject.GetComponent<AvatarController>();
                    if(avatarController != null)
                    {
                        m_SuspectAvatar = avatarController;
                    }
                }
            }
            if (Input.GetMouseButtonUp(0))
            {
                //EmptySpace‚É“–‚½‚ç‚È‚¢‚æ‚¤‚É‚·‚é
                int layerMask = (1 << 0);
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
                {
                    RoomObject roomObject = hit.transform.gameObject.GetComponent<RoomObject>();

                    if(roomObject != null && roomObject == m_SuspectObject)
                    {
                        Debug.Log("hit roomobject: " + roomObject.gameObject.name);
                        m_RoomManager.SelectedObject = roomObject;
                        m_Machine.ChangePhase(new RoomPhasePicture(m_Machine, m_RoomManager, this, m_RoomCommander));
                    }

                    AvatarController avatarController = hit.transform.gameObject.GetComponent<AvatarController>();
                    if(avatarController != null && avatarController == m_SuspectAvatar)
                    {
                        if(m_AvatarController != null)
                        {
                            m_AvatarController.ChangeAvatarAnimation();
                        }
                        else
                        {
                            m_AvatarController = avatarController;
                            m_AvatarController.ChangeAvatarAnimation();
                        }
                    }
                }
                m_SuspectObject = null;
                m_SuspectAvatar = null;
            }

        }
    }

    public override void OnExitState()
    {
        base.OnExitState();
        m_RoomManager.AvatarController.gameObject.SetActive(false);

        if (m_AvatarController != null)
        {
            m_AvatarController.SetAnimationIdle();
        }
    }

    public override void HandleUIEvent(RoomUIEvent roomUIEvent)
    {
        base.HandleUIEvent(roomUIEvent);
        if(roomUIEvent is RoomEditButtonClickEvent)
        {
            m_RoomManager.SelectedObject = null;
            m_Machine.ChangePhase(new RoomPhaseNone(m_Machine, m_RoomManager, m_RoomCommander));
        }

        if(roomUIEvent is VirtualCamButtonClickEvent)
        {
            m_RoomManager.SelectedObject = null;
            m_Machine.ChangePhase(new RoomPhaseVirtualCam(m_Machine, m_RoomManager, m_RoomCommander));
        }
    }
}
