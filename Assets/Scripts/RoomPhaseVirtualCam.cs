using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RoomPhaseVirtualCam : RoomPhaseBase
{
    private RoomObjectAvator avator;
    private AvatarController avatarController;

    bool manipulateRotation;
    Vector3 initpos;
    bool lookAtCamera;
    bool arMode;
    bool m_EditingAvatar;
    bool edit_Placement=true; //true->place false->depth of field

    GameObject arOrigin;
    StudioNonARCameraController nonARController;
    GameObject nonARCamera;

    float tapTime;
    Volume ppVolume;
    DepthOfField dOF=null;


    public override RoomPhase GetRoomPhase()
    {
        return RoomPhase.VirtualCam;
    }

    public RoomPhaseVirtualCam(RoomPhaseMachine machine, MockRoomManager roomManager, IRoomCommander roomCommander) : base(machine, roomManager, roomCommander)
    {

    }

    public override void OnEnterState()
    {
        Debug.Log("RoomPhase -> VirtualCam");
        base.OnEnterState();
        m_RoomManager.AvatarController.gameObject.SetActive(true);
        avator = GameObject.FindObjectOfType<RoomObjectAvator>();
        avatarController = avator.GetComponent<AvatarController>();
        arOrigin = GameObject.FindObjectOfType<UnityEngine.XR.ARFoundation.ARSessionOrigin>(true).gameObject;
        nonARController = GameObject.FindObjectOfType<StudioNonARCameraController>(true);
        nonARCamera = nonARController.Camera.gameObject;
        ppVolume = GameObject.FindObjectOfType<Volume>();
        ppVolume?.profile.TryGet(out dOF);
        arMode = false;
        if (arMode)
            SwitchToAR();
        else
            SwitchToNonAR();
    }

    public override void OnExitState()
    {
        base.OnExitState();
        arOrigin.SetActive(false);
        /*nonARController.gameObject.SetActive(false);
        nonARCamera.SetActive(false);*/
    }

    public override void OnUpdate()
    {
        base.OnUpdate();


        var cam = Camera.main;
        //set avatar head IK
        if (lookAtCamera)
        {
            avatarController.SetLookAtWeightAndPos(1f, cam.transform.position);
        }

        var ui = new List<RaycastResult>();
        EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = Input.mousePosition }, ui);
        if(ui.Count>0)
            return;

        //avator rotate/placement init
        if (Input.GetMouseButtonDown(0))
        {
            tapTime = Time.time;
            if (m_EditingAvatar)
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                int layerMask = 1 << 15;
                if (Physics.Raycast(ray, out RaycastHit hit, 4f, layerMask))
                {
                    manipulateRotation = true;
                    initpos = Input.mousePosition;
                }
                else
                    manipulateRotation = false;
            }
        }

        //avatar rotate
        if (manipulateRotation&&Input.GetMouseButton(0))
        {
            var vec=Input.mousePosition-initpos;
            avatarController.AddRotationY(vec.x/Screen.width*2);
            initpos = Input.mousePosition;
        }

        //avatar placement
        if (Input.GetMouseButtonUp(0)&&!manipulateRotation)
        {
                manipulateRotation = false;

            if (Time.time - tapTime > 0.3f)
                return;

            if (edit_Placement)
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                int roMask = 1 << 0;
                if (Physics.Raycast(ray, out RaycastHit hit, 100f, roMask))
                {
                    Debug.Log(hit.collider.gameObject.name);
                    if (hit.collider.TryGetComponent(out IAvatarInteractable interactable))
                    {
                        avator.transform.position = hit.collider.transform.position + interactable.GetOffset();
                        avator.transform.rotation = hit.collider.transform.rotation;
                        avatarController.SetAvatarAnimation(interactable.GetAnimType());
                        return;
                    }
                }

                int layerMask = 1 << 16;
                if (Physics.Raycast(ray, out hit, 100f, layerMask))
                {
                    Debug.Log(hit.collider.gameObject.name);
                    avator.transform.position = hit.point;
                    var angles = Quaternion.LookRotation(hit.point - cam.transform.position).eulerAngles;
                    angles.x = 0;
                    angles.z = 0;
                    avator.transform.eulerAngles = angles;
                    avatarController.SetAvatarAnimation(10);
                }
            }
            else
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray,out RaycastHit hit, 100f))
                {
                    dOF.focusDistance.value = hit.distance;
                }
            }

        }

    }

    public override void HandleUIEvent(RoomUIEvent roomUIEvent)
    {
        base.HandleUIEvent(roomUIEvent);

        if(roomUIEvent is VirtualCamReturnViewButtonClickEvent)
        {
            m_Machine.ChangePhase(new RoomPhaseView(m_Machine,m_RoomManager,m_RoomCommander));
        }

        if(roomUIEvent is VirtualCamAvatarHeadIKButtonClickEvent)
        {
            lookAtCamera = !lookAtCamera;
            if (!lookAtCamera)
                avatarController.SetLookAtWeightAndPos(0, Camera.main.transform.position);
        }

        if(roomUIEvent is VirtualCamARToggleButtonClickEvent)
        {
            if (arMode)
                SwitchToNonAR();
            else
                SwitchToAR();
            arMode = !arMode;

        }

        if(roomUIEvent is VirtualCamStartAvatarEditButtonClickEvent)
        {
            m_EditingAvatar = true;
        }

        if(roomUIEvent is VirtualCamEndAvatarEditButtonClickEvent)
        {
            m_EditingAvatar = false;
        }

        if(roomUIEvent is VirtualCamPlacementModeButtonClickEvent)
        {
            edit_Placement = true;
        }
        if(roomUIEvent is VirtualCamFocusModeButtonClickEvent)
        {
            edit_Placement=false;
        }
    }

    void SwitchToAR()
    {
        GameObject.FindObjectOfType<UnityEngine.XR.ARFoundation.ARSession>().Reset();
        arOrigin.transform.position = nonARCamera.transform.position;
        arOrigin.transform.rotation = nonARCamera.transform.rotation;
        arOrigin.SetActive(true);
        nonARCamera.SetActive(false);
        nonARController.gameObject.SetActive(false);
    }

    void SwitchToNonAR()
    {
        nonARCamera.transform.position = arOrigin.transform.GetChild(0).position;
        nonARCamera.transform.rotation = arOrigin.transform.GetChild(0).rotation;
        var angle = nonARCamera.transform.eulerAngles;
        angle.z = 0;
        nonARCamera.transform.eulerAngles = angle;
        arOrigin.SetActive(false);
        nonARCamera.SetActive(true);
        nonARController.gameObject.SetActive(true);
    }
}

