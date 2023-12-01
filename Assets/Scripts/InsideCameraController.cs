using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// ref: https://zenn.dev/daichi_gamedev/articles/526869bb674495
/// </summary>
public class InsideCameraController : CameraController
{
    private readonly float ms_RoomZoomMinBound = 5f;
    private readonly float ms_RoomZoomMaxBound = 85f;
    private readonly float ms_RoomFieldOfViewThreshold = 50f;
    private readonly float ms_RoomZoomFrontMargin = 12f;
    private readonly float ms_RoomZoomBackMargin = 3f;

    //[SerializeField] private Camera m_Camera;
    [SerializeField] private float m_ZoomSpeed = 1f; //ズームのスピード
    [SerializeField] private float m_ZoomMinBound = 5f;
    [SerializeField] private float m_ZoomMaxBound = 85f;
    [SerializeField] private float m_FieldOfViewThreshold = 75f;
    [SerializeField] private Vector3 m_CameraEndPoint = new Vector3(0f, 0f, 0f);

    public Action<ViewType> OnMoveComplete;

    private Vector3 m_RoomCenter;

    private RoomObjectData m_FixedTableData;
    private RoomObjectData m_FixedChairData;
    private float m_FixedOffset;
    private float m_FixedFov;

    private float m_FocusTheta;
    private float m_FocusPhi;
    private float m_InitRadius;
    private bool m_IsViewChanging;
    private Vector3 m_InitPos;
    private float m_InitTheta;
    private float m_InitPhi;
    private Vector3 m_InitCenter;

    #region VARIABLES
    //カメラズーム用変数
    private float m_ZoomFrontMargin = 20f;
    private float m_ZoomBackMargin = 3f;
    private Vector3 m_CameraStartPoint;
    private Vector3 m_CameraOffset;
    private float m_DragStartTime;
    private Vector3 m_DragStartPos;
    private bool m_IsDrag;
    private bool m_IsMove;
    private float m_MoveTarget;
    private float m_DeltaDistance;
    private float m_ZoomFov;

    //カメラ回転用変数
    private float m_RotateRadius;
    private float m_RotateTheta; //0~360タイプの角度
    private float m_RotatePhi; //0~360タイプの角度
    private float m_RotateMargin = 5f;
    private float m_RotateMinTheta = 10f;
    private float m_RotateMaxTheta = 170f;
    private float m_RotateMinPhi = -136f;
    private float m_RotateMaxPhi = 225f;
    private float m_IsMoveRotate;
    private float m_RotateTargetTheta;
    private float m_RotateTargetPhi;
    private Vector3 m_RotateTarget;

    private Vector2 m_RotateBefPos;
    private bool m_IsRotate;
    private Vector2 m_RotateDiff;
    private float m_RoomWidth = 3f;
    private float m_RoomDepth = 4f;
    private float m_RoomHeight = 3f;
    private float m_nowAngleX = 0f;
    private float m_nowAngleY = 0f;
    private float m_BefAngleX = 0f;
    private float m_BefAngleY = 0f;
    private float m_MinAngleX = -30f;
    private float m_MaxAngleX = 30f;
    private float m_MinAngleY = -30f;
    private float m_MaxAngleY = 30f;
    private float m_RotateSpeed = 0.3f;
    private float m_RotateTargetX;
    private float m_RotateTargetY;
    private bool m_IsMoveRotateX;
    private bool m_IsMoveRotateY;

    private float m_MoveInertiaTheta;
    private float m_MoveInertiaPhi;

    private bool m_IsMovingLock;
    private bool m_IsRotatingLock;

    public Vector3 m_RotateCenter; //カメラの回転運動の中心
    public Vector3 RotateCenter
    {
        get => m_RotateCenter;
        set
        {
            m_RotateCenter = value;
            m_RotateRadius = Vector3.Distance(m_RotateCenter, m_CameraStartPoint);
        }
    }

    //カメラ移動用変数
    private bool m_IsMoving;
    private Vector2 m_MoveStartPos;
    private Vector2 m_MovedOffset = new Vector2(0f, 0f);
    private bool m_IsInertia;
    private Vector2 m_InertiaOffset;
    private Vector3 m_InertiaTarget;
    private float m_OffsetRange = 2f;

    private Vector3 m_RotateCenterTargetPos;

    #endregion

    private void OnEnable()
    {
        Debug.Log("this is inside camera controller");
    }

    private void Start()
    {
        Debug.Log("inside start");
        if(m_Camera == null)
        {
            m_Camera = Camera.main;
        }
        m_CameraStartPoint = m_Camera.transform.position;
        m_CameraOffset = m_CameraEndPoint - m_CameraStartPoint;
        m_IsDrag = false;
        m_IsRotate = false;
        m_Camera.fieldOfView = ms_RoomZoomMaxBound;

        m_RoomHeight = m_RoomMasterData.RoomSizeHeight;
        m_RoomWidth = m_RoomMasterData.RoomSizeWidth;
        m_RoomDepth = m_RoomMasterData.RoomSizeDepth;

        m_RotateCenter = new Vector3(m_RoomWidth / 2f, 1.274f, -m_RoomDepth / 2f); //いったんこのあたりにする
        m_Camera.transform.LookAt(m_RotateCenter);
        m_RotateRadius = Vector3.Distance(m_RotateCenter, m_CameraStartPoint); //半径算出
        m_InitRadius = m_RotateRadius;

        Vector3 positionVec = m_CameraStartPoint - m_RotateCenter;
        m_RotateTheta = Vector3.Angle(Vector3.up, positionVec);
        m_RotatePhi = Vector3.Angle(Vector3.right, new Vector3(positionVec.x, 0, positionVec.z));
        m_InitPos = positionVec;
        m_InitTheta = m_RotateTheta;
        m_InitPhi = m_RotatePhi;
        m_MoveInertiaTheta = m_RotateTheta;
        m_MoveInertiaPhi = m_RotatePhi;

        m_FocusTheta = 65f;
        m_FocusPhi = 12f;
        m_InitCenter = m_RotateCenter;
        m_RoomCenter = m_RotateCenter;

        m_ZoomMaxBound = ms_RoomZoomMaxBound;
        m_ZoomMinBound = ms_RoomZoomMinBound;
        m_FieldOfViewThreshold = ms_RoomFieldOfViewThreshold;
        m_ZoomFrontMargin = ms_RoomZoomFrontMargin;
        m_ZoomBackMargin = ms_RoomZoomBackMargin;

        m_IsMovingLock = false;
        m_IsRotatingLock = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_Camera == null) return;

        if (Input.GetMouseButtonDown(2))
        {
            //m_DebugText.gameObject.SetActive(!m_DebugText.gameObject.activeSelf);
        }

        switch (m_RoomPhase)
        {
            case RoomPhase.Control:
            case RoomPhase.Picture:
            case RoomPhase.Trim:
            case RoomPhase.DeltaRotate:
                return;

            default:
                break;
        }

        //以下カメラ操作
        if (m_IsViewChanging) return;

        //CalcCameraAngles();
        if (m_ViewType == ViewType.EDIT)
        {
            //UIの位置はタップしても動かないようにする
            if (!EventSystem.current.IsPointerOverGameObject(-1) && !EventSystem.current.IsPointerOverGameObject(0))
            {
                CameraZoom();
                CameraRotate();
                //CameraMove();
            }
        }

        if (m_ViewType == ViewType.FURNITURE)
        {
            //UIの位置はタップしても動かないようにする
            if (!EventSystem.current.IsPointerOverGameObject(-1) && !EventSystem.current.IsPointerOverGameObject(0))
            {
                CameraZoom();
                CameraRotate();
                //CameraMove();
            }
        }

        if (m_ViewType == ViewType.VIEW)
        {
            //UIの位置はタップしても動かないようにする
            if (!EventSystem.current.IsPointerOverGameObject(-1) && !EventSystem.current.IsPointerOverGameObject(0))
            {
                CameraZoom();
                CameraRotate();
                //CameraMove();
            }
        }

        //Debug.Log("center " + m_RotateCenter);
        /*m_Camera.transform.position = m_RotateCenter;
        m_Camera.transform.LookAt(CalcCameraPosFromAngles(m_RotateCenter, m_RotateRadius, 180f - m_RotateTheta, m_RotatePhi + 180f));*/

        //範囲外に出ていたら修正する処理

        if(m_Camera != null)
        {
            m_Camera.transform.position = CalcCameraPosFromAngles(m_RotateCenter, m_RotateRadius, m_RotateTheta, m_RotatePhi);

            //壁との衝突があれば修正する
            /*int layerMask = (1 << 6) + (1 << 12) + (1 << 13);
            if (Physics.SphereCast(m_RotateCenter, 0.1f, (m_Camera.transform.position - m_RotateCenter).normalized, out RaycastHit hit, m_RotateRadius, layerMask))
            {
                m_Camera.transform.position = m_RotateCenter + (m_Camera.transform.position - m_RotateCenter).normalized * hit.distance;
            }*/

            /*float posX = Mathf.Clamp(m_Camera.transform.position.x, 0.1f, m_RoomWidth - 0.1f);
            float posY = Mathf.Clamp(m_Camera.transform.position.y, 0.1f, m_RoomHeight - 0.1f);
            float posZ = Mathf.Clamp(m_Camera.transform.position.z, -m_RoomDepth + 0.1f, 0.1f);

            m_CameraParent.transform.position = new Vector3(posX, posY, posZ);*/
            //CalcCameraAngles();

            m_Camera.transform.LookAt(m_RotateCenter);
            /*if(Mathf.Abs(m_Camera.fieldOfView - m_ZoomFov) > 0.1f)
            {
                m_Camera.fieldOfView = m_ZoomFov;
            }
            m_ZoomFov = m_Camera.fieldOfView;*/
        }
    }

    public override Vector3 SetProfilePosition(RoomObjectData table, RoomObjectData chair, float offset)
    {
        Vector3 center = Vector3.zero;
        m_FixedTableData = table;
        m_FixedChairData = chair;
        m_FixedOffset = offset;
        //center = m_RoomMasterData.RoomUnit * new Vector3(table.GetObjWidth(PutType.REVERSE) / 2f, table.GetObjHeight(PutType.REVERSE) * 2f / 3f, -(table.GetObjDepth(PutType.REVERSE) / 2f + chair.GetObjDepth(PutType.REVERSE) / 4f));
        center = m_RoomMasterData.RoomUnit * new Vector3(table.GetObjWidth(PutType.REVERSE) / 2f, table.GetObjHeight(PutType.REVERSE) * 2f / 3f, -(table.GetObjDepth(PutType.REVERSE) / 2f + chair.GetObjDepth(PutType.REVERSE) / 4f));
        center += (offset - (float)(table.GetObjWidth(PutType.REVERSE)) * m_RoomMasterData.RoomUnit / 2f) * new Vector3(1f, 0f, 0f);
        m_RotateCenter = center;
        m_RotateTheta = 70f;
        m_RotatePhi = 40f;
        m_FixedFov = 22f;
        m_Camera.fieldOfView = 22f;
        m_ZoomFov = m_FixedFov;
        return CalcCameraPosFromAngles(m_RotateCenter, m_RotateRadius, m_RotateTheta, m_RotatePhi);
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
        ViewType nextViewType = ViewType.EDIT;

        switch (phase)
        {
            case RoomPhase.None:
                nextViewType = ViewType.EDIT;
                break;

            case RoomPhase.Selected:
                if (roomObject == null)
                {
                    nextViewType = ViewType.EDIT;
                }
                else
                {
                    nextViewType = ViewType.FURNITURE;
                }
                break;

            case RoomPhase.Detail:
            case RoomPhase.EmptySpace:
                nextViewType = ViewType.FURNITURE;
                break;

            case RoomPhase.View:
                nextViewType = ViewType.VIEW;
                break;

            default:
                nextViewType = ViewType.EDIT;
                break;
        }

        await ChangeViewType(nextViewType, roomObject);
        Debug.Log("Camera Move End");
    }

    public override void HandleRoomEvent(RoomEvent roomEvent)
    {
        base.HandleRoomEvent(roomEvent);
        if (roomEvent is RoomEventLogTwoAnswer)
        {
            m_IsViewChanging = true;
        }
        if (roomEvent is RoomEventLogEnd)
        {
            m_IsViewChanging = false;
        }
    }

    public override async UniTask ChangeViewType(ViewType viewType, RoomObject roomObject)
    {
        m_ViewType = viewType;
        RoomObject target = roomObject;
        if (viewType == ViewType.EDIT)
        {
            m_InitCenter = m_RoomCenter;
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
            //targetCenter = m_RoomMasterData.RoomUnit * (idxPos + centerPos + new Vector3(m_RoomWidth / 2, m_RoomHeight / 2, -m_RoomDepth / 2));
            targetCenter = m_RoomMasterData.RoomUnit * (idxPos + centerPos);
            m_RotateCenter = targetCenter;
            m_InitCenter = m_RotateCenter;
            target = roomObject;
        }

        if (viewType == ViewType.VIEW)
        {
            target = null;
        }

        m_IsMoveRotateX = false;
        m_IsMoveRotateY = false;
        m_IsInertia = false;
        m_IsMove = false;

        await MoveToTargetAsync(viewType, target);
    }

    public override async UniTask MoveToTargetAsync(ViewType viewType, RoomObject roomObject)
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
            float fovBaseSize = Mathf.Max(roomObject.GetFamilySize(roomObject.PutType).y, Mathf.Max(roomObject.GetFamilySize(roomObject.PutType).x, roomObject.GetFamilySize(roomObject.PutType).z));
            /*targetFOV = 2f * Mathf.Atan(fovBaseSize * m_RoomMasterData.RoomUnit / (m_RoomHeight / 4f) * Mathf.Tan(ms_RoomZoomMaxBound / 2f * Mathf.PI / 180f)) * 180f / Mathf.PI;
            m_ZoomMaxBound = 10f + 2f * Mathf.Atan(fovBaseSize * m_RoomMasterData.RoomUnit / (m_RoomHeight / 4f) * Mathf.Tan(ms_RoomZoomMaxBound / 2f * Mathf.PI / 180f)) * 180f / Mathf.PI;
            m_ZoomMinBound = 2f * Mathf.Atan(fovBaseSize * m_RoomMasterData.RoomUnit / (m_RoomHeight / 4f) * Mathf.Tan(ms_RoomZoomMinBound / 2f * Mathf.PI / 180f)) * 180f / Mathf.PI;

            m_FieldOfViewThreshold = Mathf.Atan(fovBaseSize * m_RoomMasterData.RoomUnit / m_RoomHeight * Mathf.Tan(ms_RoomFieldOfViewThreshold * Mathf.PI / 180f)) * 180f / Mathf.PI;
            m_ZoomFrontMargin = 5f + Mathf.Atan(fovBaseSize * m_RoomMasterData.RoomUnit / m_RoomHeight * Mathf.Tan(ms_RoomZoomFrontMargin * Mathf.PI / 180f)) * 180f / Mathf.PI;
            m_ZoomBackMargin = Mathf.Atan(fovBaseSize * m_RoomMasterData.RoomUnit / m_RoomHeight * Mathf.Tan(ms_RoomZoomBackMargin * Mathf.PI / 180f)) * 180f / Mathf.PI;*/

            m_RotateRadius *= fovBaseSize * m_RoomMasterData.RoomUnit / (m_RoomHeight / 5f);

            int x = m_RoomMasterData.GetWidth(roomObject.RoomIndex);
            int y = m_RoomMasterData.GetHeight(roomObject.RoomIndex);
            int z = m_RoomMasterData.GetDepth(roomObject.RoomIndex);
            int roomSize = (int)(m_RoomWidth / m_RoomMasterData.RoomUnit);

            if(y >= roomSize / 2f)
            {
                m_RotateTheta = 180f - m_FocusTheta;
            }
            else
            {
                m_RotateTheta = m_FocusTheta;
            }
            if (roomObject.PutType == PutType.NORMAL)
            {
                if(z <= roomSize / 2f)
                {
                    m_RotatePhi = m_FocusPhi;
                }
                else if(x + z <= roomSize)
                {
                    m_RotatePhi = -m_FocusPhi;
                }
                else if(x <= roomSize / 2f)
                {
                    m_RotatePhi = -90f + m_FocusPhi;
                }
                else
                {
                    m_RotatePhi = -90f - m_FocusPhi;
                }
            }
            else if (roomObject.PutType == PutType.REVERSE)
            {
                if(x <= roomSize / 2f)
                {
                    m_RotatePhi = 90f - m_FocusPhi;
                }
                else if(x + z <= roomSize)
                {
                    m_RotatePhi = 90f + m_FocusPhi;
                }
                else if(z <= roomSize / 2f)
                {
                    m_RotatePhi = 180f - m_FocusPhi;
                }
                else
                {
                    m_RotatePhi = 180f + m_FocusPhi;
                }
            }
            else
            {
                targetPos = CalcCameraPosFromAngles(targetCenter, m_RotateRadius, m_FocusTheta, m_FocusPhi);
                m_RotateTheta = m_FocusTheta;
                m_RotatePhi = m_FocusPhi;
            }
            targetPos = CalcCameraPosFromAngles(targetCenter, m_RotateRadius, m_RotateTheta, m_RotatePhi);
            targetDirection = targetCenter - targetPos;
        }
        else
        {
            if (viewType == ViewType.EDIT)
            {
                m_ZoomMaxBound = ms_RoomZoomMaxBound;
                m_ZoomMinBound = ms_RoomZoomMinBound;
                m_FieldOfViewThreshold = ms_RoomFieldOfViewThreshold;
                m_ZoomFrontMargin = ms_RoomZoomFrontMargin;
                m_ZoomBackMargin = ms_RoomZoomBackMargin;

                targetCenter = m_RoomCenter;
                m_RotateTheta = m_InitTheta;
                m_RotatePhi = m_InitPhi;
                m_RotateRadius = m_InitRadius;
                targetFOV = m_ZoomMaxBound;
                targetPos = CalcCameraPosFromAngles(targetCenter, m_RotateRadius, m_InitTheta, m_InitPhi);
                targetDirection = targetCenter - targetPos;
            }
            else if (viewType == ViewType.VIEW)
            {
                targetPos = m_CameraStartPoint;
                targetFOV = m_ZoomMaxBound;
                targetDirection = m_RotateCenter - targetPos;
            }
            else
            {
                targetPos = SetProfilePosition(m_FixedTableData, m_FixedTableData, m_FixedOffset);
                targetFOV = m_FixedFov;
                targetDirection = m_RotateCenter - targetPos;
            }
        }

        //while (Vector3.Distance(m_Camera.transform.position, targetPos) > 0.01f || Mathf.Abs(targetFOV - m_Camera.fieldOfView) > 1f)
        while (Vector3.Distance(m_Camera.transform.position, targetPos) > 0.01f)
        {
            m_Camera.transform.position = Vector3.Lerp(m_Camera.transform.position, targetPos, 5f * Time.deltaTime);
            //m_Camera.fieldOfView = Mathf.Lerp(m_Camera.fieldOfView, targetFOV, 5f * Time.deltaTime);
            tempDirection = Vector3.Lerp(tempDirection, targetDirection, 5f * Time.deltaTime);
            m_Camera.transform.LookAt(m_Camera.transform.position + tempDirection);
            await UniTask.DelayFrame(1);
        }
        //m_ZoomFov = targetFOV;
        //CalcCameraAngles();
        m_IsViewChanging = false;
    }

    void CameraRotate()
    {
        if (m_Camera.fieldOfView >= m_FieldOfViewThreshold) //Aのとき
        {
            //一本指の回転の実装
            if (!m_IsRotate)
            {
                if (InputProvider.GetIsDragging())
                {
                    m_IsRotate = true;
                    m_IsMoveRotateX = false;
                    m_IsMoveRotateY = false;
                    m_RotateBefPos = InputProvider.GetDragPosition();
                }
            }
            else
            {
                if (InputProvider.GetIsDragging())
                {
                    m_RotateDiff = InputProvider.GetDragDeltaPosition();
                    m_RotateBefPos = InputProvider.GetDragPosition();

                    //float phi = Mathf.Clamp(m_RotatePhi + m_RotateDiff.x * m_RotateSpeed, m_RotateMinPhi, m_RotateMaxPhi);

                    float phi = m_RotatePhi + m_RotateDiff.x * m_RotateSpeed;
                    phi = (phi + 360f) % 360f;
                    float theta = Mathf.Clamp(m_RotateTheta + m_RotateDiff.y * m_RotateSpeed, m_RotateMinTheta, m_RotateMaxTheta);

                    //範囲指定
                    Vector3 futurePos = CalcCameraPosFromAngles(m_RotateCenter, m_RotateRadius, theta, phi);
                    if (futurePos.x >= 0.1f && futurePos.x < m_RoomWidth - 0.1f && futurePos.y >= 0.1f && futurePos.y < m_RoomHeight - 0.1f && futurePos.z <= -0.1f && futurePos.z > -m_RoomDepth + 0.1f)
                    {
                        m_RotateTheta = theta;
                        m_RotatePhi = phi;
                    }
                    //m_Camera.transform.LookAt(m_RotateCenter);
                }
                else if (InputProvider.GetIsDragEnd())
                {
                    m_IsRotate = false;

                    m_IsMoveRotateX = true;
                    m_IsMoveRotateY = true;

                    m_RotateDiff = InputProvider.GetDragDeltaPosition();

                    m_RotateTargetTheta = Mathf.Clamp(m_RotateTheta + 7f * m_RotateDiff.y * m_RotateSpeed, m_RotateMinTheta - 3f, m_RotateMaxTheta + 5f);
                    //m_RotateTargetPhi = Mathf.Clamp(m_RotatePhi + 7f * m_RotateDiff.x * m_RotateSpeed, m_RotateMinPhi - 3f, m_RotateMaxPhi + 5f);
                    m_RotateTargetPhi = m_RotatePhi + 7f * m_RotateDiff.x * m_RotateSpeed;
                }
            }

            if (m_IsMoveRotateX || m_IsMoveRotateY)
            {
                /*if (m_RotateTargetPhi > m_RotateMaxPhi && m_RotatePhi > m_RotateMaxPhi)
                {
                    m_RotateTargetPhi = m_RotateMaxPhi;
                }
                else if (m_RotateTargetPhi < m_RotateMinPhi && m_RotatePhi < m_RotateMinPhi)
                {
                    m_RotateTargetPhi = m_RotateMinPhi;
                }

                if (m_RotateTargetTheta > m_RotateMaxTheta && m_RotateTheta > m_RotateMaxTheta)
                {
                    m_RotateTargetTheta = m_RotateMaxTheta;
                }
                else if (m_RotateTargetTheta < m_RotateMinTheta && m_RotateTheta < m_RotateMinTheta)
                {
                    m_RotateTargetTheta = m_RotateMinTheta;
                }*/

                float theta = Mathf.Lerp(m_RotateTheta, m_RotateTargetTheta, 0.1f);
                float phi = Mathf.Lerp(m_RotatePhi, m_RotateTargetPhi, 0.1f);

                //m_Camera.transform.LookAt(m_RotateCenter);
                Vector3 futurePos = CalcCameraPosFromAngles(m_RotateCenter, m_RotateRadius, theta, phi);
                if (futurePos.x < 0.1f || futurePos.x >= m_RoomWidth - 0.1f || futurePos.y < 0.1f || futurePos.y >= m_RoomHeight - 0.1f || futurePos.z > -0.1f || futurePos.z <= -m_RoomDepth + 0.1f)
                {
                    m_RotateTargetTheta = m_RotateTheta;
                    m_RotateTargetPhi = m_RotatePhi;
                }
                else
                {
                    m_RotateTheta = theta;
                    m_RotatePhi = phi;
                }

                if (Mathf.Abs(m_RotatePhi - m_RotateTargetPhi) <= 0.03f)
                {
                    m_IsMoveRotateX = false;
                    m_RotatePhi = m_RotateTargetPhi;
                }
                if (Mathf.Abs(m_RotateTheta - m_RotateTargetTheta) <= 0.03f)
                {
                    m_IsMoveRotateY = false;
                    m_RotateTheta = m_RotateTargetTheta;
                }
            }
        }

        else if (m_Camera.fieldOfView < m_FieldOfViewThreshold)
        {
            if (!m_IsRotate)
            {
                if (InputProvider.GetIsDragging())
                {
                    m_IsRotate = true;
                    m_IsMoveRotateX = false;
                    m_IsMoveRotateY = false;
                    //m_Camera.transform.position = CalcCameraPosFromAngles(m_RotateCenter, m_RotateRadius, m_RotateTheta, m_RotatePhi);
                    m_RotateBefPos = InputProvider.GetDragPosition();
                }
            }
            else
            {
                if (InputProvider.GetIsDragging())
                {
                    m_RotateDiff = InputProvider.GetDragDeltaPosition();
                    m_RotateBefPos = InputProvider.GetDragPosition();

                    float phi = m_RotatePhi + m_RotateDiff.x * m_RotateSpeed;
                    phi = (phi + 360f) % 360f;
                    float theta = Mathf.Clamp(m_RotateTheta + m_RotateDiff.y * m_RotateSpeed, m_RotateMinTheta, m_RotateMaxTheta);
                    
                    Vector3 futurePos = CalcCameraPosFromAngles(m_RotateCenter, m_RotateRadius, theta, phi);
                    if (futurePos.x >= 0.1f && futurePos.x < m_RoomWidth - 0.1f && futurePos.y >= 0.1f && futurePos.y < m_RoomHeight - 0.1f && futurePos.z <= -0.1f && futurePos.z > -m_RoomDepth + 0.1f)
                    {
                        m_RotateTheta = theta;
                        m_RotatePhi = phi;
                    }
                }
                else if (InputProvider.GetIsDragEnd())
                {
                    m_IsRotate = false;

                    m_IsMoveRotateX = true;
                    m_IsMoveRotateY = true;

                    m_RotateDiff = InputProvider.GetDragDeltaPosition();

                    m_RotateTargetTheta = Mathf.Clamp(m_RotateTheta + 7f * m_RotateDiff.y * m_RotateSpeed, m_RotateMinTheta - 3f, m_RotateMaxTheta + 5f);
                    m_RotateTargetPhi = m_RotatePhi + 7f * m_RotateDiff.x * m_RotateSpeed;
                }
            }

            if (m_IsMoveRotateX || m_IsMoveRotateY)
            {
                /*if (m_RotateTargetPhi > m_RotateMaxPhi && m_RotatePhi > m_RotateMaxPhi)
                {
                    m_RotateTargetPhi = m_RotateMaxPhi;
                }
                else if (m_RotateTargetPhi < m_RotateMinPhi && m_RotatePhi < m_RotateMinPhi)
                {
                    m_RotateTargetPhi = m_RotateMinPhi;
                }

                if (m_RotateTargetTheta > m_RotateMaxTheta && m_RotateTheta > m_RotateMaxTheta)
                {
                    m_RotateTargetTheta = m_RotateMaxTheta;
                }
                else if (m_RotateTargetTheta < m_RotateMinTheta && m_RotateTheta < m_RotateMinTheta)
                {
                    m_RotateTargetTheta = m_RotateMinTheta;
                }*/

                float theta = Mathf.Lerp(m_RotateTheta, m_RotateTargetTheta, 0.1f);
                float phi = Mathf.Lerp(m_RotatePhi, m_RotateTargetPhi, 0.1f);

                //m_Camera.transform.LookAt(m_RotateCenter);
                Vector3 futurePos = CalcCameraPosFromAngles(m_RotateCenter, m_RotateRadius, theta, phi);
                if (futurePos.x < 0.1f || futurePos.x >= m_RoomWidth - 0.1f || futurePos.y < 0.1f || futurePos.y >= m_RoomHeight - 0.1f || futurePos.z > -0.1f || futurePos.z <= -m_RoomDepth + 0.1f)
                {
                    m_RotateTargetTheta = m_RotateTheta;
                    m_RotateTargetPhi = m_RotatePhi;
                }
                else
                {
                    m_RotateTheta = theta;
                    m_RotatePhi = phi;
                }
                //m_Camera.transform.LookAt(m_RotateCenter);

                if (Mathf.Abs(m_RotatePhi - m_RotateTargetPhi) <= 3f)
                {
                    m_IsMoveRotateX = false;
                }
                if (Mathf.Abs(m_RotateTheta - m_RotateTargetTheta) <= 3f)
                {
                    m_IsMoveRotateY = false;
                }
            }
        }
    }

    void CameraMove()
    {
        Transform cameraTransform = m_Camera.transform;
        Vector3 positionVec;
        //m_OffsetRange = Mathf.Max(0f, (m_FieldOfViewThreshold - m_Camera.fieldOfView) / 20f);
        m_OffsetRange = Mathf.Max(0f, (ms_RoomFieldOfViewThreshold - m_Camera.fieldOfView) / 25f);
        positionVec = cameraTransform.position - CalcCameraPosFromAngles(m_InitCenter, m_RotateRadius, m_RotateTheta, m_RotatePhi);
        m_MovedOffset = new Vector2(Vector3.Dot(positionVec, cameraTransform.right), Vector3.Dot(positionVec, cameraTransform.up));

        float adjustRange = m_OffsetRange + 0.02f;
        //縮小時の調整
        /*if (!m_IsMoving && !m_IsInertia && m_MovedOffset.x < -adjustRange || m_MovedOffset.x > adjustRange || m_MovedOffset.y < -adjustRange || m_MovedOffset.y > adjustRange)
        {
            m_IsInertia = true;
            m_InertiaOffset.x = Mathf.Max(-m_OffsetRange, m_MovedOffset.x);
            m_InertiaOffset.x = Mathf.Min(m_OffsetRange, m_InertiaOffset.x);
            m_InertiaOffset.y = Mathf.Max(-m_OffsetRange, m_MovedOffset.y);
            m_InertiaOffset.y = Mathf.Min(m_OffsetRange, m_InertiaOffset.y);
            m_InertiaTarget = m_InitCenter + cameraTransform.right * m_InertiaOffset.x + cameraTransform.up * m_InertiaOffset.y;
        }*/
        if (m_IsInertia) //慣性の動き
        {
            float wallRange = m_OffsetRange - 0.1f;
            /*if (m_InertiaOffset.x > m_OffsetRange && m_MovedOffset.x > wallRange)
            {
                m_InertiaOffset.x = m_OffsetRange;
                m_InertiaTarget = m_InitCenter + cameraTransform.right * m_InertiaOffset.x + cameraTransform.up * m_InertiaOffset.y;
            }
            else if (m_InertiaOffset.x < -m_OffsetRange && m_MovedOffset.x < -wallRange)
            {
                m_InertiaOffset.x = -m_OffsetRange;
                m_InertiaTarget = m_InitCenter + cameraTransform.right * m_InertiaOffset.x + cameraTransform.up * m_InertiaOffset.y;
            }
            if (m_InertiaOffset.y > m_OffsetRange && m_MovedOffset.y > wallRange)
            {
                m_InertiaOffset.y = m_OffsetRange;
                m_InertiaTarget = m_InitCenter + cameraTransform.right * m_InertiaOffset.x + cameraTransform.up * m_InertiaOffset.y;
            }
            else if (m_InertiaOffset.y < -m_OffsetRange && m_MovedOffset.y < -wallRange)
            {
                m_InertiaOffset.y = -m_OffsetRange;
                m_InertiaTarget = m_InitCenter + cameraTransform.right * m_InertiaOffset.x + cameraTransform.up * m_InertiaOffset.y;
            }*/
            if (Vector3.Distance(m_RotateCenter, m_InertiaTarget) <= 0.1f)
            {
                m_IsInertia = false;
            }
            else
            {
                m_RotateCenter = Vector3.Lerp(m_RotateCenter, m_InertiaTarget, 0.1f);
            }
            positionVec = cameraTransform.position - CalcCameraPosFromAngles(m_InitCenter, m_RotateRadius, m_RotateTheta, m_RotatePhi);
            m_MovedOffset = new Vector2(Vector3.Dot(positionVec, cameraTransform.right), Vector3.Dot(positionVec, cameraTransform.up));
        }
        //if (m_Camera.fieldOfView < m_FieldOfViewThreshold) //Bのとき
        if (true)
        {
            Vector2 moveDelta = new Vector2();
            if (!m_IsMoving)
            {
                /*if (!m_IsRotate && InputProvider.GetIsMultiDragging())
                {
                    m_IsMoving = true;
                    m_IsInertia = false;
                    m_MoveStartPos = InputProvider.GetMultiDragPosition();
                }*/
                if (InputProvider.GetIsMultiDragging())
                {
                    m_IsRotate = false;
                    m_IsMoving = true;
                    m_IsInertia = false;
                    m_MoveStartPos = InputProvider.GetMultiDragPosition();
                }
            }
            else
            {
                if (InputProvider.GetIsMultiDragging()) //ドラッグ時の動き
                {
                    m_IsMoving = true;
                    m_IsInertia = false;
                    //指と実際の視点の動きがずれる 吸いつくように動くにはどうすればいいか？
                    moveDelta = -1f * InputProvider.GetMultiDragDeltaPosition() / 100f * Mathf.Sin(Mathf.PI / 180f * m_Camera.fieldOfView) / Mathf.Sin(Mathf.PI / 180f * m_ZoomMaxBound);
                    m_MoveStartPos = InputProvider.GetMultiDragPosition();
                    /*Vector3 positionVec = cameraTransform.position - CalcCameraPosFromAngles(m_RotateTheta, m_RotatePhi);
                    m_MovedOffset = new Vector2(Vector3.Dot(positionVec, cameraTransform.right), Vector3.Dot(positionVec, cameraTransform.up));*/
                    /*moveDelta.x = Mathf.Clamp(moveDelta.x, -m_OffsetRange - m_MovedOffset.x, m_OffsetRange - m_MovedOffset.x);
                    moveDelta.y = Mathf.Clamp(moveDelta.y, -m_OffsetRange - m_MovedOffset.y, m_OffsetRange - m_MovedOffset.y);*/
                    /*m_InertiaOffset = m_MovedOffset + new Vector2(moveDelta.x, moveDelta.y);
                    m_InertiaTarget = CalcCameraPosFromAngles(m_RotateTheta, m_RotatePhi) + cameraTransform.right * m_InertiaOffset.x + cameraTransform.up * m_InertiaOffset.y;*/

                    //m_RotateCenter += cameraTransform.right * moveDelta.x + cameraTransform.up * moveDelta.y;
                    m_RotateCenter += cameraTransform.right * moveDelta.x + Vector3.up * moveDelta.y;
                    m_RotateCenter.x = Mathf.Clamp(m_RotateCenter.x, 0.04f, m_RoomWidth);
                    m_RotateCenter.y = Mathf.Clamp(m_RotateCenter.y, 0.04f, m_RoomHeight);
                    m_RotateCenter.z = Mathf.Clamp(m_RotateCenter.z, -m_RoomDepth, -0.04f);
                    
                    positionVec = cameraTransform.position - CalcCameraPosFromAngles(m_InitCenter, m_RotateRadius, m_RotateTheta, m_RotatePhi);
                    m_MovedOffset = new Vector2(Vector3.Dot(positionVec, cameraTransform.right), Vector3.Dot(positionVec, cameraTransform.up));
                    m_MoveInertiaTheta = m_RotateTheta;
                    m_MoveInertiaPhi = m_RotatePhi;
                }
                else //スワイプ時の動きの設定
                {
                    m_IsMoving = false;
                    m_IsInertia = true;

                    //moveDelta = -1f * InputProvider.GetDragDeltaPosition() / Time.deltaTime / 50f * Mathf.Cos(Mathf.PI / 180f * m_ZoomMaxBound) / Mathf.Cos(Mathf.PI / 180f * m_Camera.fieldOfView);
                    moveDelta = -1f * InputProvider.GetMultiDragDeltaPosition() / Time.deltaTime / 100f * Mathf.Sin(Mathf.PI / 180f * m_Camera.fieldOfView) / Mathf.Sin(Mathf.PI / 180f * m_ZoomMaxBound);
                    m_InertiaOffset = m_MovedOffset + new Vector2(moveDelta.x, moveDelta.y) * 0.6f;
                    /*m_InertiaOffset.x = Mathf.Clamp(m_InertiaOffset.x, -m_OffsetRange - 0.1f, m_OffsetRange + 0.1f);
                    m_InertiaOffset.y = Mathf.Clamp(m_InertiaOffset.y, -m_OffsetRange - 0.1f, m_OffsetRange + 0.1f);*/
                    m_InertiaTarget = m_InitCenter + cameraTransform.right * m_InertiaOffset.x + Vector3.up * m_InertiaOffset.y;
                    m_InertiaTarget.x = Mathf.Clamp(m_InertiaTarget.x, 0.04f, m_RoomWidth);
                    m_InertiaTarget.y = Mathf.Clamp(m_InertiaTarget.y, 0.04f, m_RoomHeight);
                    m_InertiaTarget.z = Mathf.Clamp(m_InertiaTarget.z, -m_RoomDepth, -0.04f);
                    m_MoveInertiaTheta = m_RotateTheta;
                    m_MoveInertiaPhi = m_RotatePhi;
                }
            }
        }
    }

    void CameraZoom()
    {
        if (!m_IsDrag)
        {
            if (InputProvider.GetIsZooming())
            {
                m_IsDrag = true;
                m_IsMove = false;
                m_DragStartTime = Time.time;
            }
        }
        else
        {
            if (InputProvider.GetIsZooming())
            {
                m_DeltaDistance = InputProvider.GetZoomDeltaDistance();
                m_Camera.fieldOfView += m_DeltaDistance * 5f;
                m_Camera.fieldOfView = Mathf.Clamp(m_Camera.fieldOfView, m_ZoomMinBound - m_ZoomBackMargin, m_ZoomMaxBound + m_ZoomFrontMargin);
            }
            else //指を離したとき -> 慣性でふわっと移動 いい感じに減衰
            {
                m_IsDrag = false;

                //どこまで行きそうか計算 -> lerp
                //ここでDeltaDistanceを計算するとWindows版でスムーズに動く、そうしない場合Android版がスムーズになる
                // m_MoveTarget = Mathf.Clamp(m_Camera.fieldOfView + InputProvider.GetZoomDeltaDistance() * 2f, m_ZoomMinBound - m_ZoomBackMargin, m_ZoomMaxBound + m_ZoomFrontMargin);
                m_MoveTarget = Mathf.Clamp(m_Camera.fieldOfView + m_DeltaDistance / Time.deltaTime * 1f, m_ZoomMinBound - m_ZoomBackMargin, m_ZoomMaxBound + m_ZoomFrontMargin);
                m_IsMove = true;
            }
        }

        if (m_IsMove)
        {
            if (Mathf.Abs(m_MoveTarget - m_Camera.fieldOfView) <= 0.1f)
            {
                if (m_MoveTarget > m_ZoomMaxBound)
                {
                    m_MoveTarget = m_ZoomMaxBound;
                }
                else if (m_MoveTarget < m_ZoomMinBound)
                {
                    m_MoveTarget = m_ZoomMinBound;
                }
                else
                {
                    m_IsMove = false;
                }
            }
            else
            {
                m_Camera.fieldOfView = Mathf.Lerp(m_Camera.fieldOfView, m_MoveTarget, InputProvider.GetZoomSpeed());
                if (Mathf.Abs(m_MoveTarget - m_Camera.fieldOfView) <= 4f)
                {
                    if (m_MoveTarget > m_ZoomMaxBound)
                    {
                        m_MoveTarget = m_ZoomMaxBound;
                    }
                    else if (m_MoveTarget < m_ZoomMinBound)
                    {
                        m_MoveTarget = m_ZoomMinBound;
                    }
                }
            }

        }
    }

    Vector3 CalcCameraPosFromAngles(Vector3 center, float radius, float theta, float phi)
    {
        float thetaRad = theta * Mathf.Deg2Rad;
        float phiRad = phi * Mathf.Deg2Rad;
        Vector3 offset = radius * new Vector3(Mathf.Sin(thetaRad) * Mathf.Cos(phiRad), Mathf.Cos(thetaRad), Mathf.Sin(thetaRad) * (-1) * Mathf.Sin(phiRad));
        return center + offset;
    }

    Vector3 CalcCenterFromCameraPosition(Vector3 position, float radius, float theta, float phi)
    {
        float thetaRad = theta * Mathf.Deg2Rad;
        float phiRad = phi * Mathf.Deg2Rad;
        Vector3 offset = radius * new Vector3(Mathf.Sin(thetaRad) * Mathf.Cos(phiRad), Mathf.Cos(thetaRad), Mathf.Sin(thetaRad) * (-1) * Mathf.Sin(phiRad));
        return position - offset;
    }

    void CalcCameraAngles()
    {
        Vector3 positionVec = m_Camera.transform.position - m_RotateCenter;
        //m_RotateRadius = Vector3.Magnitude(positionVec);
        m_RotateTheta = Vector3.Angle(Vector3.up, positionVec);
        m_RotatePhi = Vector3.Angle(Vector3.right, new Vector3(positionVec.x, 0, positionVec.z));
    }

}
