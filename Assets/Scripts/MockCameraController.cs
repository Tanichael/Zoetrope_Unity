/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

//Android専用のカスタマイズ
public class MockCameraController : CameraController
{
    [SerializeField] private float m_MinFov = 20f;
    [SerializeField] private float m_MaxFov = 80f;
    [SerializeField] private float m_MinMargin = 10f;
    [SerializeField] private float m_MaxMargin = 20f;
    [SerializeField] private float m_FovThreshold = 77f;

    private MockCameraZoomManager m_ZoomManager;
    private MockCameraRotateManager m_RotateManager;
    private MockCameraMoveManager m_MoveManager;

    private Vector3 m_InitCenter;
    private Vector3 m_RotateCenter;
    private float m_InitRadius;
    private float m_RoomWidth;
    private float m_RoomHeight;
    private float m_RoomDepth;
    *//*    private float m_RotateTheta;
        private float m_RotatePhi;
    *//*
    private float m_InitTheta;
    private float m_InitPhi;
    private float m_FocusTheta;
    private float m_FocusPhi;
    private bool m_IsViewChanging;

    private void Start()
    {
        m_Camera.fieldOfView = m_MaxFov;

        float roomWidth = m_RoomMasterData.RoomSizeWidth;
        float roomHeight = m_RoomMasterData.RoomSizeHeight;
        float roomDepth = m_RoomMasterData.RoomSizeDepth;
        m_RotateCenter = new Vector3(roomWidth / 2, roomHeight / 2, -roomDepth / 2);
        m_InitCenter = m_RotateCenter;
        m_InitRadius = Vector3.Magnitude(m_Camera.transform.position - m_InitCenter);

        m_RoomWidth = m_RoomMasterData.RoomSizeWidth;
        m_RoomHeight = m_RoomMasterData.RoomSizeHeight;
        m_RoomDepth = m_RoomMasterData.RoomSizeDepth;

        Vector3 positionVec = m_Camera.transform.position - m_RotateCenter;
*//*        m_RotateTheta = Vector3.Angle(Vector3.up, positionVec);
        m_RotatePhi = Vector3.Angle(Vector3.right, new Vector3(positionVec.x, 0, positionVec.z));

        m_InitPos = positionVec;*//*
        m_InitTheta = Vector3.Angle(Vector3.up, positionVec);
        m_InitPhi = Vector3.Angle(Vector3.right, new Vector3(positionVec.x, 0, positionVec.z));

        m_FocusTheta = 70f;
        m_FocusPhi = 70f;

        m_ZoomManager = new MockCameraZoomManager(m_MinFov, m_MaxFov, m_MinMargin, m_MaxMargin);
        m_RotateManager = new MockCameraRotateManager(m_FovThreshold, m_Camera.transform.position);
        m_MoveManager = new MockCameraMoveManager(m_FovThreshold, m_Camera.transform.position, m_MaxFov, m_RotateCenter);
    }

    private void Update()
    {
        switch (m_RoomPhase)
        {
            case RoomPhase.Control:
            case RoomPhase.Picture:
            case RoomPhase.Trim:
                return;

            default:
                break;
        }

        float nextFov = m_Camera.fieldOfView;
        Vector3 nextPos = m_Camera.transform.position;

        if (m_ViewType == ViewType.EDIT)
        {
            //オブジェクト操作中はカメラ操作を制限
            if (!m_IsViewChanging)
            {
                //UIの位置はタップしても動かないようにする
                if (Input.mousePosition.y < Screen.height - (Screen.width / 4f + Screen.width / 3.8f + Screen.width / 10f))
                {
                    nextFov = m_ZoomManager.Zoom(m_Camera.fieldOfView);

                    nextPos = m_RotateManager.Rotate(m_Camera.transform.position, m_Camera.fieldOfView, m_RotateCenter);
                    Vector3 newPos;
                    newPos = m_MoveManager.Move(m_Camera, m_Camera.transform.position, m_Camera.fieldOfView, m_RotateCenter);
                    m_RotateCenter += newPos - m_Camera.transform.position;
                    m_Camera.transform.position = newPos;
                }
            }
        }

        m_Camera.transform.position = nextPos;
        m_Camera.fieldOfView = nextFov;
        m_Camera.transform.LookAt(m_RotateCenter);
    }

    public override void SetRoomPhase(RoomPhaseBase phase)
    {
        m_RoomPhase = phase.GetRoomPhase();
    }

    public override void SetRoomPhase(RoomPhase phase)
    {
        m_RoomPhase = phase;
    }

    public override async UniTask ChangeViewType(RoomPhaseBase roomPhase, RoomObject roomObject)
    {
        Debug.Log("Camera Move Start");
        RoomPhase phase = roomPhase.GetRoomPhase();
        switch (phase)
        {
            case RoomPhase.None:
            case RoomPhase.Selected:
            case RoomPhase.EmptySpace:
                m_ViewType = ViewType.EDIT;
                break;

            case RoomPhase.Detail:
                m_ViewType = ViewType.FURNITURE;
                break;

            case RoomPhase.View:
                m_ViewType = ViewType.VIEW;
                break;

            default:
                m_ViewType = ViewType.EDIT;
                break;
        }

        await ChangeViewType(m_ViewType, roomObject);
        Debug.Log("Camera Move End");
    }

    public override async UniTask ChangeViewType(ViewType viewType, RoomObject roomObject)
    {
        //if (m_ViewType == viewType) return;

        m_ViewType = viewType;
        RoomObject target = roomObject;
        if (viewType == ViewType.EDIT)
        {
            m_RotateCenter = m_InitCenter;
            target = null;
        }

        if (viewType == ViewType.FURNITURE)
        {
            Vector3 targetCenter;
            Vector3 idxPos = m_RoomMasterData.GetPosition(roomObject.RoomIndex);
            idxPos.z *= -1f;
            Vector3 centerPos = roomObject.GetFamilySize(roomObject.PutType) / 2;
            centerPos.z *= -1f;
            targetCenter = m_RoomMasterData.RoomUnit * (idxPos + centerPos + new Vector3(m_RoomWidth / 2, m_RoomHeight / 2, -m_RoomDepth / 2));
            m_RotateCenter = targetCenter;
            target = roomObject;
        }

        await MoveToTargetAsync(target);
    }

    public override async UniTask MoveToTargetAsync(RoomObject roomObject)
    {
        m_IsViewChanging = true;
        Vector3 targetPos;
        float targetFOV;
        Vector3 targetCenter;
        Vector3 targetDirection;
        Vector3 tempDirection = m_Camera.transform.forward;
        if (roomObject != null)
        {
            Vector3 idxPos = m_RoomMasterData.GetPosition(roomObject.RoomIndex);
            idxPos.z *= -1f;
            Vector3 centerPos = roomObject.GetFamilySize(roomObject.PutType) / 2;
            centerPos.z *= -1f;
            targetCenter = m_RoomMasterData.RoomUnit * (idxPos + centerPos + new Vector3(m_RoomWidth / 2, m_RoomHeight / 2, -m_RoomDepth / 2));
            targetFOV = Mathf.Atan(0.3f * roomObject.GetFamilySize(roomObject.PutType).y * m_RoomMasterData.RoomUnit / m_RoomHeight * Mathf.Tan(m_MaxFov * Mathf.PI / 180f)) * 180f / Mathf.PI;
            if (roomObject.PutType == PutType.NORMAL)
            {
                targetPos = CalcCameraPosFromAngles(targetCenter, m_InitRadius, m_FocusTheta, m_FocusPhi);
            }
            else if (roomObject.PutType == PutType.REVERSE)
            {
                targetPos = CalcCameraPosFromAngles(targetCenter, m_InitRadius, m_FocusTheta, 90f - m_FocusPhi);
            }
            else
            {
                targetPos = CalcCameraPosFromAngles(targetCenter, m_InitRadius, m_FocusTheta, m_FocusPhi);
            }
            targetDirection = targetCenter - targetPos;
        }
        else
        {
            targetCenter = m_InitCenter;
            targetFOV = m_MaxFov;
            targetPos = CalcCameraPosFromAngles(targetCenter, m_InitRadius, m_InitTheta, m_InitPhi);
            targetDirection = targetCenter - targetPos;
        }

        while (Vector3.Distance(m_Camera.transform.position, targetPos) > 0.03f || Mathf.Abs(targetFOV - m_Camera.fieldOfView) > 1f)
        {
            m_Camera.transform.position = Vector3.Lerp(m_Camera.transform.position, targetPos, 5f * Time.deltaTime);
            m_Camera.fieldOfView = Mathf.Lerp(m_Camera.fieldOfView, targetFOV, 5f * Time.deltaTime);
            tempDirection = Vector3.Lerp(tempDirection, targetDirection, 5f * Time.deltaTime);
            m_Camera.transform.LookAt(m_Camera.transform.position + tempDirection);
            await UniTask.DelayFrame(1);
        }
        //OnMoveComplete.Invoke(m_ViewType);
        m_IsViewChanging = false;
    }

    Vector3 CalcCameraPosFromAngles(Vector3 center, float radius, float theta, float phi)
    {
        float thetaRad = theta * Mathf.PI / 180f;
        float phiRad = phi * Mathf.PI / 180f;
        Vector3 offset = radius * new Vector3(Mathf.Sin(thetaRad) * Mathf.Cos(phiRad), Mathf.Cos(thetaRad), Mathf.Sin(thetaRad) * (-1) * Mathf.Sin(phiRad));
        return center + offset;
    }
}
*/