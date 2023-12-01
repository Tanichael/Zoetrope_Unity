/*using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

/// <summary>
/// ref: https://zenn.dev/daichi_gamedev/articles/526869bb674495
/// </summary>
public class TestCameraController
{
    [SerializeField] RoomMasterData m_RoomMasterData;
    [SerializeField] private Camera m_Camera;
    [SerializeField] private float m_ZoomSpeed = 1f; //ズームのスピード
    [SerializeField] private float m_ZoomMinBound = 20f;
    [SerializeField] private float m_ZoomMaxBound = 85f;
    [SerializeField] private float m_FieldOfViewThreshold = 75f;
    [SerializeField] private Vector3 m_CameraEndPoint = new Vector3(0f, 0f, 0f);

    [SerializeField] private ViewType m_ViewType;

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
    private float m_ZoomBackMargin = 10f;
    private Vector3 m_CameraStartPoint;
    private Vector3 m_CameraOffset;
    private float m_DragStartTime;
    private Vector3 m_DragStartPos;
    private bool m_IsDrag;
    private bool m_IsMove;
    private float m_MoveTarget;
    private float m_DeltaDistance;

    //カメラ回転用変数
    private float m_RotateRadius;
    private float m_RotateTheta; //0~360タイプの角度
    private float m_RotatePhi; //0~360タイプの角度
    private float m_RotateMargin = 5f;
    private float m_RotateMinTheta = 10f;
    private float m_RotateMaxTheta = 85f;
    private float m_RotateMinPhi = 5f;
    private float m_RotateMaxPhi = 85f;
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

    #region ROOM_VARIABLES
    private RoomPhase m_RoomPhase;
    #endregion

    void Start()
    {
        m_CameraStartPoint = m_Camera.transform.position;
        m_CameraOffset = m_CameraEndPoint - m_CameraStartPoint;
        m_IsDrag = false;
        m_IsRotate = false;
        m_Camera.fieldOfView = m_ZoomMaxBound;

        m_RotateCenter = new Vector3(m_RoomWidth / 2, m_RoomHeight / 2, -m_RoomDepth / 2); //いったんこのあたりにする
        m_Camera.transform.LookAt(m_RotateCenter);
        m_RotateRadius = Vector3.Distance(m_RotateCenter, m_CameraStartPoint); //半径算出
        m_InitRadius = m_RotateRadius;

        Vector3 positionVec = m_CameraStartPoint - m_RotateCenter;
        m_RotateTheta = Vector3.Angle(Vector3.up, positionVec);
        m_RotatePhi = Vector3.Angle(Vector3.right, new Vector3(positionVec.x, 0, positionVec.z));

        m_InitPos = positionVec;
        m_InitTheta = m_RotateTheta;
        m_InitPhi = m_RotatePhi;

        m_FocusTheta = 70f;
        m_FocusPhi = 70f;
        m_InitCenter = m_RotateCenter;
    }

    // Update is called once per frame
    void Update()
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

        //CalcCameraAngles();
        if (m_ViewType == ViewType.EDIT)
        {
            //オブジェクト操作中はカメラ操作を制限
            if (!m_IsViewChanging)
            {
                //UIの位置はタップしても動かないようにする
                if (Input.mousePosition.y < Screen.height - (Screen.width / 4f + Screen.width / 3.8f + Screen.width / 10f))
                {
                    CameraZoom();
                    CameraRotate();
                    CameraMove();
                }
            }
        }

        if (m_ViewType == ViewType.FURNITURE)
        {
            //オブジェクト操作中はカメラ操作を制限
            if (!m_IsViewChanging)
            {
                //UIの位置はタップしても動かないようにする
                if (Input.mousePosition.y < Screen.height - (Screen.width / 4f + Screen.width / 3.8f + Screen.width / 10f))
                {
                    CameraZoom();
                    CameraRotate();
                }
            }
        }

        if (m_ViewType == ViewType.VIEW)
        {
            if (!m_IsViewChanging)
            {
                //UIの位置はタップしても動かないようにする
                if (Input.mousePosition.y < Screen.height - (Screen.width / 4f + Screen.width / 3.8f + Screen.width / 10f))
                {
                    CameraZoom();
                    CameraRotate();
                    CameraMove();
                }
            }
        }
    }

    public void SetRoomPhase(RoomPhaseBase phase)
    {
        m_RoomPhase = phase.GetRoomPhase();
    }

    public void SetRoomPhase(RoomPhase phase)
    {
        m_RoomPhase = phase;
    }

    public async UniTask ChangeViewType(RoomPhaseBase roomPhase, RoomObject roomObject)
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

    public async UniTask ChangeViewType(ViewType viewType, RoomObject roomObject)
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

    private async UniTask MoveToTargetAsync(RoomObject roomObject)
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
            targetFOV = Mathf.Atan(0.3f * roomObject.GetFamilySize(roomObject.PutType).y * m_RoomMasterData.RoomUnit / m_RoomHeight * Mathf.Tan(m_ZoomMaxBound * Mathf.PI / 180f)) * 180f / Mathf.PI;
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
            targetFOV = m_ZoomMaxBound;
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
        CalcCameraAngles();
        //OnMoveComplete.Invoke(m_ViewType);
        m_IsViewChanging = false;
    }

    void CameraRotate()
    {
        if (m_Camera.fieldOfView >= m_FieldOfViewThreshold || m_ViewType == ViewType.FURNITURE || m_ViewType == ViewType.VIEW) //Aのとき
        {
            //一本指の回転の実装
            if (!m_IsRotate)
            {
                if (InputProvider.GetIsDragging())
                {
                    m_IsRotate = true;
                    m_IsMoveRotateX = false;
                    m_IsMoveRotateY = false;
                    CalcCameraAngles();
                    m_Camera.transform.position = CalcCameraPosFromAngles(m_RotateCenter, m_RotateRadius, m_RotateTheta, m_RotatePhi);
                    m_RotateBefPos = InputProvider.GetDragPosition();
                }
            }
            else
            {
                if (InputProvider.GetIsDragging())
                {
                    m_RotateDiff = InputProvider.GetDragDeltaPosition();
                    m_RotateBefPos = InputProvider.GetDragPosition();

                    float phi = Mathf.Clamp(m_RotatePhi + m_RotateDiff.x * m_RotateSpeed, m_RotateMinPhi, m_RotateMaxPhi);
                    float theta = Mathf.Clamp(m_RotateTheta + m_RotateDiff.y * m_RotateSpeed, m_RotateMinTheta, m_RotateMaxTheta);

                    m_Camera.transform.position = CalcCameraPosFromAngles(m_RotateCenter, m_RotateRadius, theta, phi);
                    m_Camera.transform.LookAt(m_RotateCenter);
                    CalcCameraAngles();
                }
                else if (InputProvider.GetIsDragEnd())
                {
                    m_IsRotate = false;

                    m_IsMoveRotateX = true;
                    m_IsMoveRotateY = true;

                    m_RotateDiff = InputProvider.GetDragDeltaPosition();

                    m_RotateTargetTheta = Mathf.Clamp(m_RotateTheta + 7f * m_RotateDiff.y * m_RotateSpeed, m_RotateMinTheta - 3f, m_RotateMaxTheta + 5f);
                    m_RotateTargetPhi = Mathf.Clamp(m_RotatePhi + 7f * m_RotateDiff.x * m_RotateSpeed, m_RotateMinPhi - 3f, m_RotateMaxPhi + 5f);
                }
            }

            if (m_IsMoveRotateX || m_IsMoveRotateY)
            {
                if (m_RotateTargetPhi > m_RotateMaxPhi && m_RotatePhi > m_RotateMaxPhi)
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
                }

                m_Camera.transform.position = Vector3.Lerp(m_Camera.transform.position, CalcCameraPosFromAngles(m_RotateCenter, m_RotateRadius, m_RotateTargetTheta, m_RotateTargetPhi), 0.1f);

                m_Camera.transform.LookAt(m_RotateCenter);
                CalcCameraAngles();

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
        *//*
        else if(m_Camera.fieldOfView < m_FieldOfViewThreshold)
        {
            if (!m_IsRotate)
            {
                if (InputProvider.GetIsMultiDragging())
                {
                    m_IsRotate = true;
                    m_IsMoveRotateX = false;
                    m_IsMoveRotateY = false;
                    CalcCameraAngles();
                    //m_Camera.transform.position = CalcCameraPosFromAngles(m_RotateCenter, m_RotateRadius, m_RotateTheta, m_RotatePhi);
                    m_RotateBefPos = InputProvider.GetMultiDragPosition();
                }
            }
            else
            {
                if (InputProvider.GetIsMultiDragging())
                {
                    m_RotateDiff = InputProvider.GetMultiDragDeltaPosition();
                    m_RotateBefPos = InputProvider.GetMultiDragPosition();

                    float phi = Mathf.Clamp(m_RotatePhi + m_RotateDiff.x * m_RotateSpeed, m_RotateMinPhi, m_RotateMaxPhi);
                    float theta = Mathf.Clamp(m_RotateTheta + m_RotateDiff.y * m_RotateSpeed, m_RotateMinTheta, m_RotateMaxTheta);

                    m_Camera.transform.position = CalcCameraPosFromAngles(m_RotateCenter, m_RotateRadius, theta, phi);
                    m_Camera.transform.LookAt(m_RotateCenter);
                    CalcCameraAngles();
                }
                else if (InputProvider.GetIsMultiDragEnd())
                {
                    m_IsRotate = false;

                    m_IsMoveRotateX = true;
                    m_IsMoveRotateY = true;

                    m_RotateDiff = InputProvider.GetMultiDragDeltaPosition();

                    m_RotateTargetTheta = Mathf.Clamp(m_RotateTheta + 7f * m_RotateDiff.y * m_RotateSpeed, m_RotateMinTheta - 3f, m_RotateMaxTheta + 5f);
                    m_RotateTargetPhi = Mathf.Clamp(m_RotatePhi + 7f * m_RotateDiff.x * m_RotateSpeed, m_RotateMinPhi - 3f, m_RotateMaxPhi + 5f);
                }
            }

            if (m_IsMoveRotateX || m_IsMoveRotateY)
            {
                if (m_RotateTargetPhi > m_RotateMaxPhi && m_RotatePhi > m_RotateMaxPhi)
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
                }

                m_Camera.transform.position = Vector3.Lerp(m_Camera.transform.position, CalcCameraPosFromAngles(m_RotateCenter, m_RotateRadius, m_RotateTargetTheta, m_RotateTargetPhi), 0.1f);

                m_Camera.transform.LookAt(m_RotateCenter);
                CalcCameraAngles();

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
        *//*
    }

    void CameraMove()
    {
        Transform cameraTransform = m_Camera.transform;
        Vector3 positionVec;
        m_OffsetRange = Mathf.Max(0f, (m_FieldOfViewThreshold - m_Camera.fieldOfView) / 20f);
        positionVec = cameraTransform.position - CalcCameraPosFromAngles(m_RotateCenter, m_RotateRadius, m_RotateTheta, m_RotatePhi);
        m_MovedOffset = new Vector2(Vector3.Dot(positionVec, cameraTransform.right), Vector3.Dot(positionVec, cameraTransform.up));
        float adjustRange = m_OffsetRange + 0.1f;
        //縮小時の調整
        if (!m_IsMoving && !m_IsInertia && m_MovedOffset.x < -adjustRange || m_MovedOffset.x > adjustRange || m_MovedOffset.y < -adjustRange || m_MovedOffset.y > adjustRange)
        {
            m_IsInertia = true;
            m_InertiaOffset.x = Mathf.Max(-m_OffsetRange, m_MovedOffset.x);
            m_InertiaOffset.x = Mathf.Min(m_OffsetRange, m_InertiaOffset.x);
            m_InertiaOffset.y = Mathf.Max(-m_OffsetRange, m_MovedOffset.y);
            m_InertiaOffset.x = Mathf.Min(m_OffsetRange, m_InertiaOffset.y);
            m_InertiaTarget = CalcCameraPosFromAngles(m_RotateCenter, m_RotateRadius, m_RotateTheta, m_RotatePhi) + cameraTransform.right * m_InertiaOffset.x + cameraTransform.up * m_InertiaOffset.y;
        }
        if (m_IsInertia) //慣性の動き
        {
            float wallRange = m_OffsetRange - 0.1f;
            if (m_InertiaOffset.x > m_OffsetRange && m_MovedOffset.x > wallRange)
            {
                m_InertiaOffset.x = m_OffsetRange;
                m_InertiaTarget = CalcCameraPosFromAngles(m_RotateCenter, m_RotateRadius, m_RotateTheta, m_RotatePhi) + cameraTransform.right * m_InertiaOffset.x + cameraTransform.up * m_InertiaOffset.y;
            }
            else if (m_InertiaOffset.x < -m_OffsetRange && m_MovedOffset.x < -wallRange)
            {
                m_InertiaOffset.x = -m_OffsetRange;
                m_InertiaTarget = CalcCameraPosFromAngles(m_RotateCenter, m_RotateRadius, m_RotateTheta, m_RotatePhi) + cameraTransform.right * m_InertiaOffset.x + cameraTransform.up * m_InertiaOffset.y;
            }
            if (m_InertiaOffset.y > m_OffsetRange && m_MovedOffset.y > wallRange)
            {
                m_InertiaOffset.y = m_OffsetRange;
                m_InertiaTarget = CalcCameraPosFromAngles(m_RotateCenter, m_RotateRadius, m_RotateTheta, m_RotatePhi) + cameraTransform.right * m_InertiaOffset.x + cameraTransform.up * m_InertiaOffset.y;
            }
            else if (m_InertiaOffset.y < -m_OffsetRange && m_MovedOffset.y < -wallRange)
            {
                m_InertiaOffset.y = -m_OffsetRange;
                m_InertiaTarget = CalcCameraPosFromAngles(m_RotateCenter, m_RotateRadius, m_RotateTheta, m_RotatePhi) + cameraTransform.right * m_InertiaOffset.x + cameraTransform.up * m_InertiaOffset.y;
            }
            if (Vector3.Distance(m_Camera.transform.position, m_InertiaTarget) <= 0.1f)
            {
                m_IsInertia = false;
            }
            else
            {
                m_Camera.transform.position = Vector3.Lerp(m_Camera.transform.position, m_InertiaTarget, 0.1f);
            }
            positionVec = cameraTransform.position - CalcCameraPosFromAngles(m_RotateCenter, m_RotateRadius, m_RotateTheta, m_RotatePhi);
            m_MovedOffset = new Vector2(Vector3.Dot(positionVec, cameraTransform.right), Vector3.Dot(positionVec, cameraTransform.up));
        }
        if (m_Camera.fieldOfView < m_FieldOfViewThreshold) //Bのとき
        {
            Vector2 moveDelta = new Vector2();
            if (!m_IsMoving)
            {
                if (InputProvider.GetIsDragging())
                {
                    m_IsMoving = true;
                    m_IsInertia = false;
                    m_MoveStartPos = InputProvider.GetDragPosition();
                }
            }
            else
            {
                if (InputProvider.GetIsDragging()) //ドラッグ時の動き
                {
                    m_IsMoving = true;
                    //指と実際の視点の動きがずれる 吸いつくように動くにはどうすればいいか？
                    moveDelta = -1f * InputProvider.GetDragDeltaPosition() / 50f * Mathf.Cos(Mathf.PI / 180f * m_ZoomMaxBound) / Mathf.Cos(Mathf.PI / 180f * m_Camera.fieldOfView);
                    m_MoveStartPos = InputProvider.GetDragPosition();
                    *//*Vector3 positionVec = cameraTransform.position - CalcCameraPosFromAngles(m_RotateTheta, m_RotatePhi);
                    m_MovedOffset = new Vector2(Vector3.Dot(positionVec, cameraTransform.right), Vector3.Dot(positionVec, cameraTransform.up));*//*
                    moveDelta.x = Mathf.Clamp(moveDelta.x, -m_OffsetRange - m_MovedOffset.x, m_OffsetRange - m_MovedOffset.x);
                    moveDelta.y = Mathf.Clamp(moveDelta.y, -m_OffsetRange - m_MovedOffset.y, m_OffsetRange - m_MovedOffset.y);
                    *//*m_InertiaOffset = m_MovedOffset + new Vector2(moveDelta.x, moveDelta.y);
                    m_InertiaTarget = CalcCameraPosFromAngles(m_RotateTheta, m_RotatePhi) + cameraTransform.right * m_InertiaOffset.x + cameraTransform.up * m_InertiaOffset.y;*//*
                    cameraTransform.position = cameraTransform.position + cameraTransform.right * moveDelta.x + cameraTransform.up * moveDelta.y;
                    positionVec = cameraTransform.position - CalcCameraPosFromAngles(m_RotateCenter, m_RotateRadius, m_RotateTheta, m_RotatePhi);
                    m_MovedOffset = new Vector2(Vector3.Dot(positionVec, cameraTransform.right), Vector3.Dot(positionVec, cameraTransform.up));
                }
                else //スワイプ時の動きの設定
                {
                    m_IsMoving = false;
                    m_IsInertia = true;
                    moveDelta = -1f * InputProvider.GetDragDeltaPosition() / Time.deltaTime / 50f * Mathf.Cos(Mathf.PI / 180f * m_ZoomMaxBound) / Mathf.Cos(Mathf.PI / 180f * m_Camera.fieldOfView);
                    m_InertiaOffset = m_MovedOffset + new Vector2(moveDelta.x, moveDelta.y) * 0.5f;
                    m_InertiaOffset.x = Mathf.Clamp(m_InertiaOffset.x, -m_OffsetRange - 0.1f, m_OffsetRange + 0.1f);
                    m_InertiaOffset.y = Mathf.Clamp(m_InertiaOffset.y, -m_OffsetRange - 0.1f, m_OffsetRange + 0.1f);
                    m_InertiaTarget = CalcCameraPosFromAngles(m_RotateCenter, m_RotateRadius, m_RotateTheta, m_RotatePhi) + cameraTransform.right * m_InertiaOffset.x + cameraTransform.up * m_InertiaOffset.y;
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
                m_Camera.fieldOfView += m_DeltaDistance * 30f;
                m_Camera.fieldOfView = Mathf.Clamp(m_Camera.fieldOfView, m_ZoomMinBound - m_ZoomBackMargin, m_ZoomMaxBound + m_ZoomFrontMargin);
            }
            else //指を離したとき -> 慣性でふわっと移動 いい感じに減衰
            {
                m_IsDrag = false;

                //どこまで行きそうか計算 -> lerp
                //ここでDeltaDistanceを計算するとWindows版でスムーズに動く、そうしない場合Android版がスムーズになる
                // m_MoveTarget = Mathf.Clamp(m_Camera.fieldOfView + InputProvider.GetZoomDeltaDistance() * 2f, m_ZoomMinBound - m_ZoomBackMargin, m_ZoomMaxBound + m_ZoomFrontMargin);
                m_MoveTarget = Mathf.Clamp(m_Camera.fieldOfView + m_DeltaDistance / Time.deltaTime * 10f, m_ZoomMinBound - m_ZoomBackMargin, m_ZoomMaxBound + m_ZoomFrontMargin);
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
        float thetaRad = theta * Mathf.PI / 180f;
        float phiRad = phi * Mathf.PI / 180f;
        Vector3 offset = radius * new Vector3(Mathf.Sin(thetaRad) * Mathf.Cos(phiRad), Mathf.Cos(thetaRad), Mathf.Sin(thetaRad) * (-1) * Mathf.Sin(phiRad));
        return center + offset;
    }

    void CalcCameraAngles()
    {
        Vector3 positionVec = m_Camera.transform.position - m_RotateCenter;
        m_RotateRadius = Vector3.Magnitude(positionVec);
        m_RotateTheta = Vector3.Angle(Vector3.up, positionVec);
        m_RotatePhi = Vector3.Angle(Vector3.right, new Vector3(positionVec.x, 0, positionVec.z));
    }

}
*/