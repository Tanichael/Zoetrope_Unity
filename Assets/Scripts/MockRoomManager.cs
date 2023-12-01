using System;
using UnityEngine;
using System.Collections.Generic;
using UniRx;

public class MockRoomManager : MonoBehaviour
{
    [SerializeField] private RoomMasterData m_RoomMasterData;
    [SerializeField] private RoomObjectMasterData m_RoomObjectMasterData;
    [SerializeField] private GameObject m_FloorSelectingPlane;
    [SerializeField] private Renderer m_SelectingRenderer;
    [SerializeField] private Material m_CanPutMaterial;
    [SerializeField] private Material m_NotPutMaterial;

    private Subject<RoomObject> m_OnTapRoomObjectSubject = new Subject<RoomObject>();
    private Subject<RoomObject> m_OnHoldRoomObjectSubject = new Subject<RoomObject>();
    private Subject<RoomObject> m_OnDoubleTapRoomObjectSubject = new Subject<RoomObject>();

    public IObservable<RoomObject> OnTapRoomObject => m_OnTapRoomObjectSubject;
    public IObservable<RoomObject> OnHoldRoomObject => m_OnHoldRoomObjectSubject;
    public IObservable<RoomObject> OnDoubleTapRoomObject => m_OnDoubleTapRoomObjectSubject;
    
    public void SetSelectedObject(RoomObject roomObject)
    {
        if(m_SelectedFloorObject != null)
        {
            if(m_SelectedFloorObject == roomObject)
            {
                m_SelectedObject?.OnSelected(false);
                m_SelectedObject = roomObject;
                m_SelectedObject?.OnSelected(true);
            }
            if (GetIsInEmptySpace(roomObject, SelectedFloorObject, SelectedSpace != null ? SelectedSpace.EmptySpace : null))
            {
                Transform parentTransform = roomObject.transform.parent;

                bool isOnFloorObject = false;

                while (parentTransform != null)
                {
                    RoomObject parentRoomObject = parentTransform.gameObject.GetComponent<RoomObject>();

                    if (parentRoomObject == SelectedFloorObject)
                    {
                        isOnFloorObject = true;
                        break;
                    }
                    parentTransform = parentTransform.parent;
                }

                if(!isOnFloorObject)
                {
#if UNITY_EDITOR
                    Debug.Log("Not on floor object");
#endif
                    return;
                }

                if (m_SelectedObject != null)
                {
                    m_SelectedObject.OnSelected(false);

                    if (m_SelectedObject == roomObject)
                    {
                        m_SelectedObject = m_SelectedFloorObject;
                        m_SelectedObject?.OnSelected(true);
                    }
                    else
                    {
                        roomObject?.OnSelected(true);
                        m_SelectedObject = roomObject;
                    }
                }
                else
                {
                    roomObject?.OnSelected(true);
                    m_SelectedObject = roomObject;
                }
            }
            else
            {
#if UNITY_EDITOR
                Debug.Log("Not in selected space");
#endif
            }
        }
        else
        {
            if (m_SelectedObject != null)
            {
                m_SelectedObject.OnSelected(false);

                if (m_SelectedObject == roomObject)
                {
                    m_SelectedObject = null;
                }
                else 
                {
                    roomObject?.OnSelected(true);
                    m_SelectedObject = roomObject;
                }
            }
            else
            {
                roomObject?.OnSelected(true);
                m_SelectedObject = roomObject;
            }
        }
    }

    public void SetControlObject(RoomObject roomObject)
    {
        if (m_SelectedFloorObject != null)
        {
            if (GetIsInEmptySpace(roomObject, SelectedFloorObject, SelectedSpace != null ? SelectedSpace.EmptySpace : null))
            {
                Transform parentTransform = roomObject.transform.parent;

                //?I?????????????????`?F?b?N
                bool isOnFloorObject = false;

                while (parentTransform != null)
                {
                    RoomObject parentRoomObject = parentTransform.gameObject.GetComponent<RoomObject>();

                    if (parentRoomObject == SelectedFloorObject)
                    {
                        isOnFloorObject = true;
                        break;
                    }
                    parentTransform = parentTransform.parent;
                }

                if (!isOnFloorObject)
                {
#if UNITY_EDITOR
                    Debug.Log("Not on floor object");
#endif
                    return;
                }

                if (m_SelectedObject != null)
                {
                    if (m_SelectedObject == roomObject)
                    {
                        roomObject?.OnInControl(true);
                    }
                    else
                    {
                        m_SelectedObject.OnSelected(false);
                        roomObject?.OnSelected(true);
                        roomObject?.OnInControl(true);
                        m_SelectedObject = roomObject;
                    }
                }
                else
                {
                    roomObject?.OnSelected(true);
                    roomObject?.OnInControl(true);
                    m_SelectedObject = roomObject;
                }
            }
            else
            {
#if UNITY_EDITOR
                Debug.Log("Not in selected space");
#endif
            }
        }
        else
        {
            if (m_SelectedObject != null)
            {
                if (m_SelectedObject == roomObject)
                {
                    roomObject?.OnInControl(true);
                }
                else
                {
                    m_SelectedObject.OnSelected(false);
                    roomObject?.OnSelected(true);
                    roomObject?.OnInControl(true);
                    m_SelectedObject = roomObject;
                }
            }
            else
            {
                roomObject?.OnSelected(true);
                roomObject?.OnInControl(true);
                m_SelectedObject = roomObject;
            }
        }
    }

    private void SetSelectedPictureObject(SetMaterialEvent setMaterialEvent)
    {
        SelectedPictureObject = setMaterialEvent;
    }

    public RoomObject SelectedObject
    {
        get
        {
            return m_SelectedObject;
        }
        set
        {
            SetSelectedObject(value);
        }
    }

    public RoomObject SelectedFloorObject
    { 
        get
        {
            return m_SelectedFloorObject;
        }
        set
        {
           /* if(m_SelectedFloorObject != null)
            {
                m_SelectedFloorObject.OnDetail(false);
            }*/
            m_SelectedFloorObject = value;
            /*if(m_SelectedFloorObject != null)
            {
                m_SelectedFloorObject.OnDetail(true);
            }*/
        }
    }
    public EmptySpaceBehaviour SelectedSpace 
    { 
        get
        {
            return m_SelectedSpace;
        }
        set
        {
            if (m_SelectedSpace != null)
            {
                m_SelectedSpace.Selected = false;
            }
            m_SelectedSpace = value;
            if (m_SelectedSpace != null)
            {
                m_SelectedSpace.Selected = true;
            }
        }
    }

    public SetMaterialEvent SelectedPictureObject { get; set; }

    private int m_RoomHeight;
    private int m_RoomWidth;
    private int m_RoomDepth;

    private RoomObject m_SelectedObject;
    private RoomObject m_SelectedFloorObject;
    private EmptySpaceBehaviour m_SelectedSpace;

    //private int[] m_RoomStates;
    private RoomObject[] m_RoomStates;
    public RoomObject[] RoomStates => m_RoomStates;
    private int[] m_RoomStatesSum;
    private int[] m_FloorHeights;
    private bool[] m_FloorStates;
    private int[] m_WallWidthHeights;
    private bool[] m_WallWidthStates;
    private int[] m_WallDepthHeights;
    private bool[] m_WallDepthStates;
    private int m_SpaceCount; //????216000
    private int m_Cnt = 0;
    private AvatarController m_AvatarController;
    public AvatarController AvatarController => m_AvatarController;

    private Vector3Int m_ControlHitBefPos;
    private Vector3Int m_ControlHitOffSet;

    public void Init(SaveData saveData, RoomObjectData fixedTableData, RoomObjectData fixedChairData, float offset, AvatarController avatarPrefab)
    {
        #region INIT
        m_RoomHeight = m_RoomMasterData.RoomHeight;
        m_RoomDepth = m_RoomMasterData.RoomDepth;
        m_RoomWidth = m_RoomMasterData.RoomWidth;

        m_SpaceCount = (m_RoomHeight) * (m_RoomDepth) * (m_RoomWidth);
        //m_RoomStates = new int[m_SpaceCount];
        m_RoomStates = new RoomObject[m_SpaceCount];
        m_RoomStatesSum = new int[(m_RoomHeight + 1) * (m_RoomDepth + 1) * (m_RoomWidth + 1)];

        //??
        m_FloorHeights = new int[m_RoomDepth * m_RoomWidth];
        m_FloorStates = new bool[m_RoomDepth * m_RoomWidth];

        //????
        m_WallWidthHeights = new int[m_RoomHeight * m_RoomDepth];
        m_WallWidthStates = new bool[m_RoomHeight * m_RoomDepth];

        //?E??
        m_WallDepthHeights = new int[m_RoomHeight * m_RoomWidth];
        m_WallDepthStates = new bool[m_RoomHeight * m_RoomWidth];

        for (int i = 0; i < m_SpaceCount; i++)
        {
            //m_RoomStates[i] = 0;
            m_RoomStates[i] = null;
        }

        for (int i = 0; i < m_RoomHeight + 1; i++)
        {
            for (int j = 0; j < m_RoomDepth + 1; j++)
            {
                for (int k = 0; k < m_RoomWidth + 1; k++)
                {
                    m_RoomStatesSum[GetSumIndex(i, j, k)] = 0;
                }
            }
        }

        for (int i = 0; i < m_RoomDepth; i++)
        {
            for (int j = 0; j < m_RoomWidth; j++)
            {
                m_FloorHeights[i * m_RoomWidth + j] = 0;
                m_FloorStates[i * m_RoomWidth + j] = true;
            }
        }

        for (int i = 0; i < m_RoomHeight; i++)
        {
            for (int j = 0; j < m_RoomDepth; j++)
            {
                m_WallWidthHeights[i * m_RoomDepth + j] = 0;
                m_WallWidthStates[i * m_RoomDepth + j] = true;
            }
        }

        for (int i = 0; i < m_RoomHeight; i++)
        {
            for (int j = 0; j < m_RoomWidth; j++)
            {
                m_WallDepthHeights[i * m_RoomWidth + j] = 0;
                m_WallDepthStates[i * m_RoomWidth + j] = true;
            }
        }

        m_FloorSelectingPlane.SetActive(false);
        #endregion

        int chairIndex = 0;
        RoomObject tableObject = null;
        RoomObject chairObject = null;

        if (fixedTableData != null && fixedChairData != null && avatarPrefab != null)
        {
            int tablePosWidth = (int)(offset / m_RoomMasterData.RoomUnit) - fixedTableData.GetObjWidth(PutType.REVERSE) / 2;
            int tableIndex = GetIndex(0, 0, tablePosWidth);
            tableObject = InstantiateRoomObject(tableIndex, fixedTableData, PutType.REVERSE);

            chairIndex = GetIndex(0, fixedTableData.GetObjDepth(tableObject.PutType), tablePosWidth + fixedTableData.GetObjWidth(tableObject.PutType) / 2 - fixedChairData.GetObjWidth(PutType.REVERSE) / 2);

            chairObject = InstantiateRoomObject(chairIndex, fixedChairData, PutType.NORMAL);
            RotateObject(chairObject);
            RotateObject(chairObject);
            /*tableObject.gameObject.SetActive(false);
            chairObject.gameObject.SetActive(false);*/

            m_AvatarController = Instantiate(avatarPrefab);
            m_AvatarController.transform.position = chairObject.transform.position + new Vector3(0f, 0.18f, 0.05f);
            m_AvatarController.transform.Rotate(0f, 180f, 0f);
            var avatarObj = m_AvatarController.GetComponent<RoomObjectAvator>();

            avatarObj.OnTapRoomObject.Subscribe((roomObject) =>
            {
                m_OnTapRoomObjectSubject.OnNext(roomObject);
            }).AddTo(this);

            avatarObj.OnHoldRoomObject.Subscribe((roomObject) =>
            {
                m_OnHoldRoomObjectSubject.OnNext(roomObject);
            }).AddTo(this);
        }

        if (saveData != null)
        {
            foreach (var data in saveData.Data)
            {
                InstantiateRoomObject(data);
            }
        }
    }

    public RoomObject Put(RoomObjectData data)
    {
        RoomObject roomObject = null;
        PutType putType;

        if (data.PosType == PositionType.FLOOR)
        {
            if (UnityEngine.Random.Range(0, 101) < 50) putType = PutType.NORMAL;
            else putType = PutType.REVERSE;
        }
        else if (data.PosType == PositionType.WALL)
        {
            if (UnityEngine.Random.Range(0, 101) < 50) putType = PutType.NORMAL;
            else putType = PutType.REVERSE;
        }
        else
        {
            putType = PutType.NORMAL;
        }

        int i;

        if (m_SelectedSpace != null)
        {
            putType = m_SelectedFloorObject.PutType;
            if (m_SelectedSpace.Id != -1)
            {
                i = SelectIndex(data, ref putType, m_SelectedFloorObject, m_SelectedSpace != null ? m_SelectedSpace.EmptySpace : null);
            }
            else
            {
                int tempHeight = GetHeight(m_SelectedFloorObject.RoomIndex) + m_SelectedFloorObject.Height;
                int areaIdx = GetIndex(tempHeight, GetDepth(m_SelectedFloorObject.RoomIndex), GetWidth(m_SelectedFloorObject.RoomIndex));
                i = SelectIndex(data, ref putType, areaIdx, m_RoomMasterData.RoomHeight - tempHeight, m_SelectedFloorObject.Depth, m_SelectedFloorObject.Width);
            }
        }
        else if (m_SelectedObject != null)
        {
            RoomObject parentObject = m_SelectedObject;
            i = -1;

            while(parentObject != null)
            {
                if (parentObject.Data.IsPlane)
                {
                    putType = m_SelectedObject.PutType;

                    int randSpaceId = UnityEngine.Random.Range(0, parentObject.EmptySpaceBehaviourList.Count);
                    i = -1;
                    for (int cnt = 0; cnt < parentObject.EmptySpaceBehaviourList.Count; cnt++)
                    {
                        EmptySpace randSpace = parentObject.EmptySpaceBehaviourList[randSpaceId].EmptySpace;
                        if (randSpace.Id != -1)
                        {
                            i = SelectIndex(data, ref putType, parentObject, randSpace);
                        }
                        else
                        {
                            int tempHeight = GetHeight(parentObject.RoomIndex) + parentObject.Height;
                            int areaIdx = GetIndex(tempHeight, GetDepth(parentObject.RoomIndex), GetWidth(parentObject.RoomIndex));
                            i = SelectIndex(data, ref putType, areaIdx, m_RoomMasterData.RoomHeight - tempHeight, parentObject.Depth, parentObject.Width);
                        }
                        if (i != -1) break;

                        randSpaceId++;
                        randSpaceId %= parentObject.EmptySpaceBehaviourList.Count;
                    }
                }

                if (i == -1)
                {
                    parentObject = parentObject.ParentObject;
                }
                else
                {
                    break;
                }
            }

            if(i == -1)
            {
                i = SelectIndex(data, ref putType);
            }
        }
        else
        {
            i = SelectIndex(data, ref putType);
        }

        if (i != -1)
        {
            roomObject = InstantiateRoomObject(i, data, putType);
        }
        else
        {
            Debug.Log("‚¨‚¯‚Ü‚¹‚ñ");
        }

        m_Cnt += 1;
        m_Cnt %= 4;

        return roomObject;
    }

    public void StartControl(RaycastHit hit)
    {
        int index = SelectedObject.RoomIndex;
        int height = GetHeight(index);
        int depth = GetDepth(index);
        int width = GetWidth(index);

        int objHeight = m_SelectedObject.Data.GetObjHeight(m_SelectedObject.PutType);
        int objDepth = m_SelectedObject.Data.GetObjDepth(m_SelectedObject.PutType);
        int objWidth = m_SelectedObject.Data.GetObjWidth(m_SelectedObject.PutType);

        Vector3 planePos = new Vector3();

        if (m_SelectedObject.Data.PosType == PositionType.FLOOR)
        {
            planePos = m_RoomMasterData.RoomUnit * new Vector3(width + (float)objWidth / 2f, height + 1f, -depth - (float)objDepth / 2f);
            m_FloorSelectingPlane.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
            m_FloorSelectingPlane.transform.localScale = m_RoomMasterData.RoomUnit * new Vector3(objWidth * 0.1f, 1f, objDepth * 0.1f);
            m_FloorSelectingPlane.transform.position = planePos;
        }
        else if (m_SelectedObject.Data.PosType == PositionType.WALL)
        {
            if (m_SelectedObject.PutType == PutType.NORMAL)
            {
                planePos = m_RoomMasterData.RoomUnit * new Vector3(width + 1f, height + (float)objHeight / 2f, -depth - (float)objDepth / 2f);
                m_FloorSelectingPlane.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, -90f));
                m_FloorSelectingPlane.transform.localScale = m_RoomMasterData.RoomUnit * new Vector3(objHeight * 0.1f, 1f, objDepth * 0.1f);
                m_FloorSelectingPlane.transform.position = planePos;
            }
            else if (m_SelectedObject.PutType == PutType.REVERSE)
            {
                planePos = m_RoomMasterData.RoomUnit * new Vector3(width + (float)objWidth / 2f, height + (float)objHeight / 2f, -depth - 1f);
                m_FloorSelectingPlane.transform.rotation = Quaternion.Euler(new Vector3(-90f, 0f, 0f));
                m_FloorSelectingPlane.transform.localScale = m_RoomMasterData.RoomUnit * new Vector3(objWidth * 0.1f, 1f, objHeight * 0.1f);
                m_FloorSelectingPlane.transform.position = planePos;
            }
        }

        Vector3Int hitPos = GetHitPosition(SelectedObject, hit);
        int hitHeight = hitPos.y;
        int hitDepth = hitPos.z;
        int hitWidth = hitPos.x;

        hitHeight = Mathf.Clamp(hitHeight, 0, m_RoomHeight);
        hitDepth = Mathf.Clamp(hitDepth, 0, m_RoomDepth);
        hitWidth = Mathf.Clamp(hitWidth, 0, m_RoomWidth);
        m_ControlHitBefPos = new Vector3Int(hitWidth, hitHeight, hitDepth);
        m_ControlHitOffSet = m_ControlHitBefPos - new Vector3Int(width, height, depth);
    }

    public bool EndControl(RaycastHit hit)
    {
        m_FloorSelectingPlane.SetActive(false);

        Vector3Int hitPos = GetHitPosition(SelectedObject, hit);
        int hitHeight = hitPos.y;
        int hitDepth = hitPos.z;
        int hitWidth = hitPos.x;

        //?u?????????????????C??
        int height = hitHeight - m_ControlHitOffSet.y;
        int depth = hitDepth - m_ControlHitOffSet.z;
        int width = hitWidth - m_ControlHitOffSet.x;
        height = Mathf.Max(0, height);
        height = Mathf.Min(m_RoomHeight - 1, height);
        depth = Mathf.Max(0, depth);
        depth = Mathf.Min(m_RoomDepth - 1, depth);
        width = Mathf.Max(0, width);
        width = Mathf.Min(m_RoomWidth - 1, width);

        if (SelectedFloorObject != null && SelectedSpace != null)
        {
            int emptyIdx = GetIndexFromEmptySpace(SelectedFloorObject, SelectedSpace.EmptySpace);
            int emptyMinHeight = GetHeight(emptyIdx);
            int emptyMinDepth = GetDepth(emptyIdx);
            int emptyMinWidth = GetWidth(emptyIdx);

            int emptyMaxHeight = emptyMinHeight + GetEmptySpaceHeight(m_SelectedSpace.EmptySpace, m_SelectedFloorObject.PutType);
            int emptyMaxDepth = emptyMinDepth + GetEmptySpaceDepth(m_SelectedSpace.EmptySpace, m_SelectedFloorObject.PutType);
            int emptyMaxWidth = emptyMinWidth + GetEmptySpaceWidth(m_SelectedSpace.EmptySpace, m_SelectedFloorObject.PutType);

            if (m_SelectedSpace.Id == -1)
            {
                emptyIdx = GetIndex(GetHeight(m_SelectedFloorObject.RoomIndex) + m_SelectedFloorObject.Height, GetDepth(m_SelectedFloorObject.RoomIndex), GetWidth(m_SelectedFloorObject.RoomIndex));
                emptyMinHeight = GetHeight(emptyIdx);
                emptyMinDepth = GetDepth(emptyIdx);
                emptyMinWidth = GetWidth(emptyIdx);

                emptyMaxHeight = m_RoomHeight;
                emptyMaxDepth = emptyMinDepth + m_SelectedFloorObject.Depth;
                emptyMaxWidth = emptyMinWidth + m_SelectedFloorObject.Width;
            }

            height = Mathf.Min(emptyMaxHeight - 1, Mathf.Max(emptyMinHeight, height));
            depth = Mathf.Min(emptyMaxDepth - 1, Mathf.Max(emptyMinDepth, depth));
            width = Mathf.Min(emptyMaxWidth - 1, Mathf.Max(emptyMinWidth, width));
        }

        Debug.Log("x, y, z: " + width + " " + height + " " + depth);
        int newIdx = GetIndex(height, depth, width);
        int candIdx = GetCandidateIndex(m_SelectedObject, newIdx, m_SelectedObject.PutType, m_SelectedFloorObject, m_SelectedSpace != null ? m_SelectedSpace.EmptySpace : null);
        if (candIdx != -1) //??????????
        {
            SelectedObject.SetRoomIndex(candIdx);
            SetState(SelectedObject, true);

            return true;
        }
        else if(candIdx == -1)
        {
            PutType nextPutType = (PutType)(((int)m_SelectedObject.PutType + 1) % Enum.GetValues(typeof(PutType)).Length);
            if(m_SelectedObject.PutType == PutType.NORMAL)
            {
                m_SelectedObject.SetPutType(PutType.REVERSE, 0, 1);
            }
            else if(m_SelectedObject.PutType == PutType.REVERSE)
            {
                m_SelectedObject.SetPutType(PutType.NORMAL, 0, 3);
            }

            candIdx = GetCandidateIndex(m_SelectedObject, newIdx, nextPutType, m_SelectedFloorObject, m_SelectedSpace != null ? m_SelectedSpace.EmptySpace : null);
            if(candIdx != -1)
            {
                SelectedObject.SetRoomIndex(candIdx);

                SetState(SelectedObject, true);

                return true;
            }
        }
        //?????????u?????????? (???????????N??????????????)
        return false;
    }

    public Vector3Int GetHitPosition(RoomObjectData data, PutType putType, RaycastHit hit)
    {
        if (hit.collider.gameObject.layer == 14)
        {
            return GetHitPositionEmptyFloor(data, putType, hit);
        }

        int hitHeight = 0;
        int hitDepth = 0;
        int hitWidth = 0;

        if (data.PosType == PositionType.FLOOR)
        {
            hitHeight = 0;
            hitDepth = (int)(-hit.point.z / m_RoomMasterData.RoomUnit);
            hitWidth = (int)(hit.point.x / m_RoomMasterData.RoomUnit);

            /*if (hit.collider.gameObject.layer == LayerMask.NameToLayer("RoomRightWall"))
            {
                hitHeight = 0;
                hitDepth = (int)((-hit.point.z - hit.point.y) / m_RoomMasterData.RoomUnit);
                hitWidth = (int)(hit.point.x / m_RoomMasterData.RoomUnit);
            }
            else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("RoomLeftWall"))
            {
                hitHeight = 0;
                hitDepth = (int)(-hit.point.z / m_RoomMasterData.RoomUnit);
                hitWidth = (int)((hit.point.x - hit.point.y) / m_RoomMasterData.RoomUnit);
            }*/
        }
        if (data.PosType == PositionType.WALL)
        {
            if (putType == PutType.NORMAL)
            {
                hitHeight = (int)((hit.point.y) / m_RoomMasterData.RoomUnit);
                hitDepth = (int)((-hit.point.z) / m_RoomMasterData.RoomUnit);
                hitWidth = 0;
            }
            else if (putType == PutType.REVERSE)
            {
                hitHeight = (int)((hit.point.y) / m_RoomMasterData.RoomUnit);
                hitDepth = 0;
                hitWidth = (int)(hit.point.x / m_RoomMasterData.RoomUnit);
            }
        }

        return new Vector3Int(hitWidth, hitHeight, hitDepth);
    }

    public Vector3Int GetHitPosition(RoomObject roomObject, RaycastHit hit)
    {
        if(hit.collider.gameObject.layer == 14)
        {
            return GetHitPositionEmptyFloor(roomObject, hit);
        }

        int hitHeight = 0;
        int hitDepth = 0;
        int hitWidth = 0;

        if (roomObject.Data.PosType == PositionType.FLOOR)
        {
            hitHeight = 0;
            hitDepth = (int)(-hit.point.z / m_RoomMasterData.RoomUnit);
            hitWidth = (int)(hit.point.x / m_RoomMasterData.RoomUnit);

            /*if (hit.collider.gameObject.layer == LayerMask.NameToLayer("RoomRightWall"))
            {
                hitHeight = 0;
                hitDepth = (int)((-hit.point.z - hit.point.y) / m_RoomMasterData.RoomUnit);
                hitWidth = (int)(hit.point.x / m_RoomMasterData.RoomUnit);
            }
            else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("RoomLeftWall"))
            {
                hitHeight = 0;
                hitDepth = (int)(-hit.point.z / m_RoomMasterData.RoomUnit);
                hitWidth = (int)((hit.point.x - hit.point.y) / m_RoomMasterData.RoomUnit);
            }*/
        }
        if (roomObject.Data.PosType == PositionType.WALL)
        {
            if (roomObject.PutType == PutType.NORMAL)
            {
                hitHeight = (int)((hit.point.y) / m_RoomMasterData.RoomUnit);
                hitDepth = (int)((-hit.point.z) / m_RoomMasterData.RoomUnit);
                hitWidth = 0;
            }
            else if (roomObject.PutType == PutType.REVERSE)
            {
                hitHeight = (int)((hit.point.y) / m_RoomMasterData.RoomUnit);
                hitDepth = 0;
                hitWidth = (int)(hit.point.x / m_RoomMasterData.RoomUnit);
            }
        }

        return new Vector3Int(hitWidth, hitHeight, hitDepth);
    }

    private Vector3Int GetHitPositionEmptyFloor(RoomObjectData data, PutType putType, RaycastHit hit)
    {
        int hitHeight = 0;
        int hitDepth = 0;
        int hitWidth = 0;

        hitHeight = (int)(hit.point.y / m_RoomMasterData.RoomUnit);
        hitDepth = (int)(-hit.point.z / m_RoomMasterData.RoomUnit);
        hitWidth = (int)(hit.point.x / m_RoomMasterData.RoomUnit);

        return new Vector3Int(hitWidth, hitHeight, hitDepth);
    }

    private Vector3Int GetHitPositionEmptyFloor(RoomObject roomObject, RaycastHit hit)
    {
        int hitHeight = 0;
        int hitDepth = 0;
        int hitWidth = 0;

        hitHeight = (int)(hit.point.y / m_RoomMasterData.RoomUnit);
        hitDepth = (int)(-hit.point.z / m_RoomMasterData.RoomUnit);
        hitWidth = (int)(hit.point.x / m_RoomMasterData.RoomUnit);

        return new Vector3Int(hitWidth, hitHeight, hitDepth);
    }

    public bool GetIsEmptySpaceInRoomObject(EmptySpaceBehaviour emptySpaceBehaviour, RoomObject roomObject)
    {
        foreach (var behav in roomObject.EmptySpaceBehaviourList)
        {
            if (emptySpaceBehaviour == behav)
            {
                return true;
            }
        }

        return false;
    }

    private int GetIndexFromEmptySpace(RoomObject floorObject, EmptySpace emptySpace)
    {
        return GetEmptySpaceIndex(floorObject.RoomIndex, floorObject.Data, emptySpace, floorObject.PutType);
    }

    public bool GetIsInEmptySpace(RoomObject roomObject, RoomObject floorObject, EmptySpace emptySpace)
    {
        if (floorObject == null || emptySpace == null)
        {
            //???????????????????????????Atrue??????
            return true;
        }
        if(roomObject == null)
        {
            //?I???????????????????????????Atrue??????
            return true;
        }

        int floorIdx = GetIndexFromEmptySpace(floorObject, emptySpace);
        int floorMinHeight = GetHeight(floorIdx);
        int floorMinDepth = GetDepth(floorIdx);
        int floorMinWidth = GetWidth(floorIdx);
        int floorMaxHeight = floorMinHeight + GetEmptySpaceHeight(emptySpace, floorObject.PutType);
        int floorMaxDepth = floorMinDepth + GetEmptySpaceDepth(emptySpace, floorObject.PutType);
        int floorMaxWidth = floorMinWidth + GetEmptySpaceWidth(emptySpace, floorObject.PutType);

        //???????????????O??????????
        if (emptySpace.Id == -1)
        {
            floorMinHeight = GetHeight(floorObject.RoomIndex) + floorObject.Height;
            floorMinDepth = GetDepth(floorObject.RoomIndex);
            floorMinWidth = GetWidth(floorObject.RoomIndex);
            floorMaxHeight = m_RoomHeight;
            floorMaxDepth = floorMinDepth + floorObject.Depth;
            floorMaxWidth = floorMinWidth + floorObject.Width;
        }

        int roomIdx = roomObject.RoomIndex;
        int minHeight = GetHeight(roomIdx);
        int minDepth = GetDepth(roomIdx);
        int minWidth = GetWidth(roomIdx);
        int maxHeight = minHeight + roomObject.Height;
        int maxDepth = minDepth + roomObject.Depth;
        int maxWidth = minWidth + roomObject.Width;

        if (minHeight < floorMinHeight || minDepth < floorMinDepth || minWidth < floorMinWidth)
        {
            return false;
        }

        if (maxHeight > floorMaxHeight || maxDepth > floorMaxDepth || maxWidth > floorMaxWidth)
        {
            return false;
        }

        return true;
    }

    private int GetCandidateIndexWall(RoomObject roomObject, int index, PutType putType)
    {
        int height = GetHeight(index);
        int width = GetWidth(index);
        int depth = GetDepth(index);

        Queue<Vector2Int> candidataQueue = new Queue<Vector2Int>();
        int[,] visited = new int[0, 0];

        if (roomObject.PutType == PutType.NORMAL)
        {
            visited = new int[m_RoomHeight, m_RoomDepth];

            for (int i = 0; i < m_RoomHeight; i++)
            {
                for (int j = 0; j < m_RoomDepth; j++)
                {
                    visited[i, j] = 0;
                }
            }
        }
        else if (roomObject.PutType == PutType.REVERSE)
        {
            visited = new int[m_RoomHeight, m_RoomWidth];

            for (int i = 0; i < m_RoomHeight; i++)
            {
                for (int j = 0; j < m_RoomWidth; j++)
                {
                    visited[i, j] = 0;
                }
            }
        }

        Vector2Int[] deltaOffset = new Vector2Int[] { new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(-1, 0) };
        if (roomObject.PutType == PutType.NORMAL)
        {
            candidataQueue.Enqueue(new Vector2Int(height, depth));
            visited[height, depth] = 1;
        }
        else if (roomObject.PutType == PutType.REVERSE)
        {
            candidataQueue.Enqueue(new Vector2Int(height, width));
            visited[height, width] = 1;
        }

        while (candidataQueue.Count > 0)
        {
            Vector2Int tempOffset = candidataQueue.Peek();
            candidataQueue.Dequeue();

            int tempHeight = tempOffset.x;

            if (roomObject.PutType == PutType.NORMAL)
            {
                int tempDepth = tempOffset.y;

                for (int tempWidth = 0; tempWidth < m_RoomWidth; tempWidth++)
                {
                    int tempIdx = GetIndex(tempHeight, tempDepth, tempWidth);
                    if (GetCanPut(tempIdx, roomObject, putType))
                    {
                        return tempIdx;
                    }
                }

                foreach (var delta in deltaOffset)
                {
                    int nextHeight = tempHeight + delta.x;
                    int nextDepth = tempDepth + delta.y;

                    if (nextHeight >= 0 && nextHeight < m_RoomHeight && nextDepth >= 0 && nextDepth < m_RoomDepth)
                    {
                        if (visited[nextHeight, nextDepth] == 0)
                        {
                            candidataQueue.Enqueue(new Vector2Int(nextHeight, nextDepth));
                            visited[nextHeight, nextDepth] = 1;
                        }
                    }
                }
            }
            else if (roomObject.PutType == PutType.REVERSE)
            {
                int tempWidth = tempOffset.y;

                for (int tempDepth = 0; tempDepth < m_RoomDepth; tempDepth++)
                {
                    int tempIdx = GetIndex(tempHeight, tempDepth, tempWidth);
                    if (GetCanPut(tempIdx, roomObject, putType))
                    {
                        return tempIdx;
                    }
                }

                foreach (var delta in deltaOffset)
                {
                    int nextHeight = tempHeight + delta.x;
                    int nextWidth = tempWidth + delta.y;

                    if (nextHeight >= 0 && nextHeight < m_RoomHeight && nextWidth >= 0 && nextWidth < m_RoomWidth)
                    {
                        if (visited[nextHeight, nextWidth] == 0)
                        {
                            candidataQueue.Enqueue(new Vector2Int(nextHeight, nextWidth));
                            visited[nextHeight, nextWidth] = 1;
                        }
                    }
                }
            }
        }

        //???????????? -1 ??????
        return -1;

    }

    private int GetCandidateIndexWallOneAxis(RoomObject roomObject, int index, PutType putType)
    {
        int height = GetHeight(index);
        int width = GetWidth(index);
        int depth = GetDepth(index);

        if (roomObject.PutType == PutType.NORMAL)
        {
            for (int tempWidth = 0; tempWidth < m_RoomWidth; tempWidth++)
            {
                int tempIdx = GetIndex(height, depth, tempWidth);
                if (GetCanPut(tempIdx, roomObject, putType))
                {
                    return tempIdx;
                }
            }
        }
        else if (roomObject.PutType == PutType.REVERSE)
        {
            for (int tempDepth = 0; tempDepth < m_RoomDepth; tempDepth++)
            {
                int tempIdx = GetIndex(height, tempDepth, width);
                if (GetCanPut(tempIdx, roomObject, putType))
                {
                    return tempIdx;
                }
            }
        }

        //???????????? -1 ??????
        return -1;
    }

    //RoomIndex?????????????????I?u?W?F?N?g???????A??????????????????
    private int GetCandidateIndex(RoomObject roomObject, int index, PutType putType)
    {
        if (roomObject.Data.PosType == PositionType.WALL)
        {
            return GetCandidateIndexWall(roomObject, index, putType);
        }

        int height = GetHeight(index);
        int width = GetWidth(index);
        int depth = GetDepth(index);

        //???????????????????T????????
        Queue<Vector2Int> candidataQueue = new Queue<Vector2Int>();
        int[,] visited;

        visited = new int[m_RoomWidth, m_RoomDepth];

        #region INIT_VISITED
        for (int i = 0; i < m_RoomWidth; i++)
        {
            for (int j = 0; j < m_RoomDepth; j++)
            {
                visited[i, j] = 0;
            }
        }
        #endregion

        Vector2Int[] deltaOffset = new Vector2Int[] { new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(-1, 0) };
        candidataQueue.Enqueue(new Vector2Int(width, depth));
        visited[width, depth] = 1;

        while (candidataQueue.Count > 0)
        {
            Vector2Int tempOffset = candidataQueue.Peek();
            candidataQueue.Dequeue();

            int tempDepth = tempOffset.y;
            int tempWidth = tempOffset.x;

            for (int offsetHeight = 0; offsetHeight < m_RoomHeight; offsetHeight++)
            {
                int tempHeight = (height + offsetHeight) % m_RoomHeight;
                int tempIdx = GetIndex(tempHeight, tempDepth, tempWidth);
                if (GetCanPut(tempIdx, roomObject, putType))
                {
                    return tempIdx;
                }
            }

            foreach (var delta in deltaOffset)
            {
                int nextDepth = tempDepth + delta.y;
                int nextWidth = tempWidth + delta.x;

                if (nextDepth >= 0 && nextDepth < m_RoomDepth && nextWidth >= 0 && nextWidth < m_RoomWidth)
                {
                    if (visited[nextWidth, nextDepth] == 0)
                    {
                        candidataQueue.Enqueue(new Vector2Int(nextWidth, nextDepth));
                        visited[nextWidth, nextDepth] = 1;
                    }
                }
            }
        }

        //???????????? -1 ??????
        return -1;
    }

    private int GetCandidateIndexOneAxis(RoomObject roomObject, int index, PutType putType)
    {
        if (roomObject.Data.PosType == PositionType.WALL)
        {
            return GetCandidateIndexWallOneAxis(roomObject, index, putType);
        }

        int height = GetHeight(index);
        int width = GetWidth(index);
        int depth = GetDepth(index);

        for (int offsetHeight = 0; offsetHeight < m_RoomHeight; offsetHeight++)
        {
            int tempHeight = (height + offsetHeight) % m_RoomHeight;
            int tempIdx = GetIndex(tempHeight, depth, width);
            if (GetCanPut(tempIdx, roomObject, putType))
            {
                return tempIdx;
            }
        }
        //???????????? -1 ??????
        return -1;
    }

    private int GetCandidateIndexOneAxis(RoomObject roomObject, int index, PutType putType, RoomObject floorObject, EmptySpace emptySpace)
    {
        if (floorObject == null || emptySpace == null || roomObject.Data.PosType == PositionType.WALL)
        {
            return GetCandidateIndexOneAxis(roomObject, index, putType);
        }
        int width = GetWidth(index);
        int depth = GetDepth(index);

        int emptyIdx = GetIndexFromEmptySpace(floorObject, emptySpace);
        int emptyHeight = GetHeight(emptyIdx);
        int emptyDepth = GetDepth(emptyIdx);
        int emptyWidth = GetWidth(emptyIdx);
        int emptyMaxDepth = emptyDepth + GetEmptySpaceDepth(emptySpace, floorObject.PutType);
        int emptyMaxWidth = emptyWidth + GetEmptySpaceWidth(emptySpace, floorObject.PutType);
        int emptySpaceHeight = GetEmptySpaceHeight(emptySpace, floorObject.PutType);

        if (emptySpace.Id == -1)
        {
            emptyHeight = GetHeight(floorObject.RoomIndex) + floorObject.Height;
            emptyDepth = GetDepth(floorObject.RoomIndex);
            emptyWidth = GetWidth(floorObject.RoomIndex);
            emptyMaxDepth = emptyDepth + floorObject.Depth;
            emptyMaxWidth = emptyWidth + floorObject.Width;
            emptySpaceHeight = m_RoomHeight - (GetHeight(floorObject.RoomIndex) + floorObject.Height);
        }

        for (int offsetHeight = 0; offsetHeight < emptySpaceHeight; offsetHeight++)
        {
            int tempHeight = emptyHeight + offsetHeight;

            int tempIdx = GetIndex(tempHeight, depth, width);
            if (GetCanPut(tempIdx, roomObject, putType, floorObject, emptySpace))
            {
                return tempIdx;
            }
        }

        return -1;
    }

    private int GetCandidateIndex(RoomObject roomObject, int index, PutType putType, RoomObject floorObject, EmptySpace emptySpace)
    {
        if (floorObject == null || emptySpace == null || roomObject.Data.PosType == PositionType.WALL)
        {
            return GetCandidateIndex(roomObject, index, putType);
        }
        int width = GetWidth(index);
        int depth = GetDepth(index);

        //???????????????????T????????
        Queue<Vector2Int> candidataQueue = new Queue<Vector2Int>();
        int[,] visited = new int[GetEmptySpaceWidth(emptySpace, floorObject.PutType), GetEmptySpaceDepth(emptySpace, floorObject.PutType)];

        for (int i = 0; i < GetEmptySpaceWidth(emptySpace, floorObject.PutType); i++)
        {
            for (int j = 0; j < GetEmptySpaceDepth(emptySpace, floorObject.PutType); j++)
            {
                visited[i, j] = 0;
            }
        }

        Vector2Int[] deltaOffset = new Vector2Int[] { new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(-1, 0) };
        candidataQueue.Enqueue(new Vector2Int(width, depth));

        int emptyIdx = GetIndexFromEmptySpace(floorObject, emptySpace);
        int emptyHeight = GetHeight(emptyIdx);
        int emptyDepth = GetDepth(emptyIdx);
        int emptyWidth = GetWidth(emptyIdx);
        int emptyMaxDepth = emptyDepth + GetEmptySpaceDepth(emptySpace, floorObject.PutType);
        int emptyMaxWidth = emptyWidth + GetEmptySpaceWidth(emptySpace, floorObject.PutType);
        int emptySpaceHeight = GetEmptySpaceHeight(emptySpace, floorObject.PutType);

        if (emptySpace.Id == -1)
        {
            emptyHeight = GetHeight(floorObject.RoomIndex) + floorObject.Height;
            emptyDepth = GetDepth(floorObject.RoomIndex);
            emptyWidth = GetWidth(floorObject.RoomIndex);
            emptyMaxDepth = emptyDepth + floorObject.Depth;
            emptyMaxWidth = emptyWidth + floorObject.Width;
            emptySpaceHeight = m_RoomHeight - (GetHeight(floorObject.RoomIndex) + floorObject.Height);
            visited = new int[floorObject.Width, floorObject.Depth];

            for (int i = 0; i < floorObject.Width; i++)
            {
                for (int j = 0; j < floorObject.Depth; j++)
                {
                    visited[i, j] = 0;
                }
            }
        }

        visited[width - emptyWidth, depth - emptyDepth] = 1;

        while (candidataQueue.Count > 0)
        {
            Vector2Int tempOffset = candidataQueue.Peek();
            candidataQueue.Dequeue();

            int tempDepth = tempOffset.y;
            int tempWidth = tempOffset.x;

            for (int tempHeight = 0; tempHeight < emptySpaceHeight; tempHeight++)
            {
                tempHeight += emptyHeight;

                int tempIdx = GetIndex(tempHeight, tempDepth, tempWidth);
                if (GetCanPut(tempIdx, roomObject, putType, floorObject, emptySpace))
                {
                    return tempIdx;
                }
            }

            foreach (var delta in deltaOffset)
            {
                int nextDepth = tempDepth + delta.y;
                int nextWidth = tempWidth + delta.x;

                if (nextDepth >= emptyDepth && nextDepth < emptyMaxDepth && nextWidth >= emptyWidth && nextWidth < emptyMaxWidth)
                {
                    if (visited[nextWidth - emptyWidth, nextDepth - emptyDepth] == 0)
                    {
                        candidataQueue.Enqueue(new Vector2Int(nextWidth, nextDepth));
                        visited[nextWidth - emptyWidth, nextDepth - emptyDepth] = 1;
                    }
                }
            }
        }

        //???????????? -1 ??????
        return -1;
    }

    public void MoveRoomObject(RoomObject roomObject, RoomObject floorObject, EmptySpace emptySpace)
    {
        int spaceIdx;
        int spaceHeight;
        int spaceDepth;
        int spaceWidth;

        if (floorObject == null || emptySpace == null)
        {
            spaceIdx = 0;
            spaceHeight = m_RoomHeight;
            spaceDepth = m_RoomDepth;
            spaceWidth = m_RoomWidth;
        }
        else
        {
            spaceIdx = GetIndexFromEmptySpace(floorObject, emptySpace);
            spaceHeight = GetEmptySpaceHeight(emptySpace, floorObject.PutType);
            spaceDepth = GetEmptySpaceDepth(emptySpace, floorObject.PutType);
            spaceWidth = GetEmptySpaceWidth(emptySpace, floorObject.PutType);
            if (emptySpace.Id == -1)
            {
                spaceIdx = GetIndex(GetHeight(floorObject.RoomIndex) + floorObject.Height, GetDepth(floorObject.RoomIndex), GetWidth(floorObject.RoomIndex));
                spaceHeight = m_RoomHeight - (GetHeight(floorObject.RoomIndex) + floorObject.Height);
                spaceDepth = floorObject.Depth;
                spaceWidth = floorObject.Width;
            }
        }

        //?O???b?h???\??????????
        Camera mainCamera = Camera.main;
        Transform cameraTransform = mainCamera.transform;
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 2f));

        //???????????????????????????H
        //??, ?E??, ????, EmptyFloor ???C???[??????????????
        int layerMask = (1 << 6) + (1 << 12) + (1 << 13) + (1 << 14);
        if (m_SelectedObject.Data.PosType == PositionType.FLOOR)
        {
            layerMask = (1 << 6) + (1 << 14);
        }
        else if (m_SelectedObject.Data.PosType == PositionType.WALL)
        {
            layerMask = (1 << 12) + (1 << 13);
        }
        Ray floorRay = new Ray(cameraTransform.position, mouseWorldPos - cameraTransform.position);
        float roomUnit = m_RoomMasterData.RoomUnit;

        if (Physics.Raycast(floorRay, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            m_FloorSelectingPlane.SetActive(true);

            Vector3Int hitPos = GetHitPosition(roomObject, hit);

            int hitHeight = hitPos.y;
            int hitDepth = hitPos.z;
            int hitWidth = hitPos.x;

            int height = hitHeight - m_ControlHitOffSet.y;
            int depth = hitDepth - m_ControlHitOffSet.z;
            int width = hitWidth - m_ControlHitOffSet.x;

            //??????????????????????
            if (roomObject.Data.PosType == PositionType.WALL)
            {
                if(roomObject.PutType == PutType.NORMAL && hit.collider.gameObject.layer == 12)
                {
                    roomObject.SetPutType(PutType.REVERSE, roomObject.RoomIndex, 1);
                    m_FloorSelectingPlane.transform.localScale = m_RoomMasterData.RoomUnit * new Vector3(roomObject.Data.GetObjWidth(roomObject.PutType) * 0.1f, 1f, roomObject.Data.GetObjHeight(roomObject.PutType) * 0.1f);
                }
                else if(roomObject.PutType == PutType.REVERSE && hit.collider.gameObject.layer == 13)
                {
                    roomObject.SetPutType(PutType.NORMAL, roomObject.RoomIndex, 3);
                    m_FloorSelectingPlane.transform.localScale = m_RoomMasterData.RoomUnit * new Vector3(roomObject.Data.GetObjHeight(roomObject.PutType) * 0.1f, 1f, roomObject.Data.GetObjDepth(roomObject.PutType) * 0.1f);
                }
            }

            if (roomObject.Data.PosType == PositionType.FLOOR)
            {
                if (roomObject.PutType == PutType.NORMAL && width > depth)
                {
                    //roomObject.SetPutType(PutType.REVERSE, roomObject.RoomIndex, 1);
                    roomObject.SetPutType(PutType.REVERSE, 0, 1);
                    m_FloorSelectingPlane.transform.localScale = m_RoomMasterData.RoomUnit * new Vector3(roomObject.Data.GetObjWidth(roomObject.PutType) * 0.1f, 1f, roomObject.Data.GetObjDepth(roomObject.PutType) * 0.1f);
                }
                else if (roomObject.PutType == PutType.REVERSE && width <= depth)
                {
                    //roomObject.SetPutType(PutType.NORMAL, roomObject.RoomIndex, 3);
                    roomObject.SetPutType(PutType.NORMAL, 0, 3);
                    m_FloorSelectingPlane.transform.localScale = m_RoomMasterData.RoomUnit * new Vector3(roomObject.Data.GetObjWidth(roomObject.PutType) * 0.1f, 1f, roomObject.Data.GetObjDepth(roomObject.PutType) * 0.1f);
                }
            }

            int objHeight = roomObject.Data.GetObjHeight(roomObject.PutType);
            int objDepth = roomObject.Data.GetObjDepth(roomObject.PutType);
            int objWidth = roomObject.Data.GetObjWidth(roomObject.PutType);

            int maxHeight = height + objHeight;
            int maxDepth = depth + objDepth;
            int maxWidth = width + objWidth;

            int minSpaceHeight = GetHeight(spaceIdx);

            int minSpaceDepth = GetDepth(spaceIdx);
            int minSpaceWidth = GetWidth(spaceIdx);
            int maxSpaceHeight = minSpaceHeight + spaceHeight;
            int maxSpaceDepth = minSpaceDepth + spaceDepth;
            int maxSpaceWidth = minSpaceWidth + spaceWidth;

            //?O???o????????????????
            //?????O?? -> ??????????????????????????
            //?????????????O -> ?????????u?????????????????????? ?O???u?????????????H

            height = Mathf.Min(maxSpaceHeight - 1, Mathf.Max(minSpaceHeight, height));
            depth = Mathf.Min(maxSpaceDepth - 1, Mathf.Max(minSpaceDepth, depth));
            width = Mathf.Min(maxSpaceWidth - 1, Mathf.Max(minSpaceWidth, width));
            int currentIdx = GetIndex(height, depth, width);
            int candIndex = GetCandidateIndexOneAxis(roomObject, currentIdx, roomObject.PutType, floorObject, emptySpace);
            bool canPut = true;
            if(candIndex == -1)
            {
                canPut = false;
            }
            else
            {
                canPut = true;
                height = GetHeight(candIndex);
                depth = GetDepth(candIndex);
                width = GetWidth(candIndex);
            }
            roomObject.ControlPosition = new RoomObjectControlPosition(canPut, roomUnit * height, -roomUnit * depth, roomUnit * width);

            if (roomObject.Data.PosType == PositionType.FLOOR)
            {
                roomObject.ControlPosition += new RoomObjectControlPosition(roomUnit * 30f * spaceHeight / m_RoomHeight, 0f, 0f);
                m_FloorSelectingPlane.transform.position = roomUnit * new Vector3(width + (float)objWidth / 2f, height + 1f, -depth - (float)objDepth / 2f);
                Vector3 angles = new Vector3(0f, 0f, 0f);
                m_FloorSelectingPlane.transform.rotation = Quaternion.Euler(angles);
            }
            else if (roomObject.Data.PosType == PositionType.WALL)
            {
                if (roomObject.PutType == PutType.NORMAL)
                {
                    roomObject.ControlPosition += new RoomObjectControlPosition(0f, 0f, roomUnit * 10f * spaceWidth / m_RoomWidth);
                    m_FloorSelectingPlane.transform.position = roomUnit * new Vector3(width + 1f, height + (float)objHeight / 2f, -depth - (float)objDepth / 2f);
                    Vector3 angles = new Vector3(0f, 0f, -90f);
                    m_FloorSelectingPlane.transform.rotation = Quaternion.Euler(angles);
                }
                else if (roomObject.PutType == PutType.REVERSE)
                {
                    roomObject.ControlPosition += new RoomObjectControlPosition(0f, -roomUnit * 10f * spaceDepth / m_RoomDepth, 0f);
                    m_FloorSelectingPlane.transform.position = roomUnit * new Vector3(width + +(float)objWidth / 2f, height + (float)objHeight / 2f, -depth - 1f);
                    Vector3 angles = new Vector3(-90f, 0f, 0f);
                    m_FloorSelectingPlane.transform.rotation = Quaternion.Euler(angles);
                }
            }

            m_ControlHitBefPos = new Vector3Int(hitWidth, hitHeight, hitDepth);
            
            if (canPut)
            {
                m_SelectingRenderer.material = m_CanPutMaterial;
            }
            else
            {
                m_SelectingRenderer.material = m_NotPutMaterial;
            }
           
        }
    }

    public void MoveControlObject()
    {
        MoveRoomObject(m_SelectedObject, null, null);
    }

    public void RotateObject(RoomObject roomObject, RoomObject floorObject, EmptySpace emptySpace)
    {
        if (roomObject.Data.PosType == PositionType.WALL)
        {
            Debug.Log("?????I?u?W?F?N?g?????]????????");
            return;
        }
        if (floorObject == null || emptySpace == null)
        {
            RotateObject(roomObject);
            return;
        }

        SetState(roomObject, false);
        bool canRotate = false;

        for (int i = 1; i < Enum.GetValues(typeof(PutType)).Length * 2; i++)
        {
            PutType nextPutType;
            nextPutType = (PutType)(((int)roomObject.PutType + i) % Enum.GetValues(typeof(PutType)).Length);

            int nextObjHeight = roomObject.Data.GetObjHeight(nextPutType);
            int nextObjDepth = roomObject.Data.GetObjDepth(nextPutType);
            int nextObjWidth = roomObject.Data.GetObjWidth(nextPutType);

            int nextHeight = GetHeight(roomObject.RoomIndex) + (roomObject.Height - nextObjHeight) / 2;
            int nextDepth = GetDepth(roomObject.RoomIndex) + (roomObject.Depth - nextObjDepth) / 2;
            int nextWidth = GetWidth(roomObject.RoomIndex) + (roomObject.Width - nextObjWidth) / 2;

            int tempHeight = GetHeight(roomObject.RoomIndex);
            int tempDepth = GetDepth(roomObject.RoomIndex);
            int tempWidth = GetWidth(roomObject.RoomIndex);

            nextHeight = Mathf.Min(m_RoomHeight - 1, Mathf.Max(0, nextHeight));
            nextDepth = Mathf.Min(m_RoomDepth - 1, Mathf.Max(0, nextDepth));
            nextWidth = Mathf.Min(m_RoomWidth - 1, Mathf.Max(0, nextWidth));

            int nextIndex = GetIndex(nextHeight, nextDepth, nextWidth);
            nextIndex = GetCandidateIndex(roomObject, nextIndex, nextPutType, floorObject, emptySpace);
            nextHeight = GetHeight(nextIndex);
            nextDepth = GetDepth(nextIndex);
            nextWidth = GetWidth(nextIndex);

            Vector3Int nextCenter = new Vector3Int(nextWidth, nextHeight, nextDepth) + roomObject.Data.GetCenter(nextPutType);
            Vector3Int tempCenter = new Vector3Int(tempWidth, tempHeight, tempDepth) + roomObject.Center;

            int distHeight = Mathf.Abs(nextCenter.y - tempCenter.y);
            int distDepth = Mathf.Abs(nextCenter.z - tempCenter.z);
            int distWidth = Mathf.Abs(nextCenter.x - tempCenter.x);

            //?u??????????????????
            //?u?????????}??????????????????????????????
            if (nextIndex != -1 && distDepth * 2 < roomObject.Depth + nextObjDepth && distWidth * 2 < roomObject.Width + nextObjWidth)
            {
                //???]
                roomObject.SetPutType(nextPutType, nextIndex, i);
                canRotate = true;
                break;
            }
        }

        if (!canRotate)
        {
            Debug.Log("???]??????????");
        }

        SetState(roomObject, true);
    }

    private void RotateObject(RoomObject roomObject)
    {
        SetState(roomObject, false);
        bool canRotate = false;

        for (int i = 1; i < Enum.GetValues(typeof(PutType)).Length * 2; i++)
        {
            Debug.Log("???]??: " + i);
            PutType nextPutType;
            nextPutType = (PutType)(((int)roomObject.PutType + i) % Enum.GetValues(typeof(PutType)).Length);

            int nextObjHeight = roomObject.Data.GetObjHeight(nextPutType);
            int nextObjDepth = roomObject.Data.GetObjDepth(nextPutType);
            int nextObjWidth = roomObject.Data.GetObjWidth(nextPutType);

            int nextHeight = GetHeight(roomObject.RoomIndex) + (roomObject.Height - nextObjHeight) / 2;
            int nextDepth = GetDepth(roomObject.RoomIndex) + (roomObject.Depth - nextObjDepth) / 2;
            int nextWidth = GetWidth(roomObject.RoomIndex) + (roomObject.Width - nextObjWidth) / 2;

            int tempHeight = GetHeight(roomObject.RoomIndex);
            int tempDepth = GetDepth(roomObject.RoomIndex);
            int tempWidth = GetWidth(roomObject.RoomIndex);

            nextHeight = Mathf.Min(m_RoomHeight - 1, Mathf.Max(0, nextHeight));
            nextDepth = Mathf.Min(m_RoomDepth - 1, Mathf.Max(0, nextDepth));
            nextWidth = Mathf.Min(m_RoomWidth - 1, Mathf.Max(0, nextWidth));

            int nextIndex = GetIndex(nextHeight, nextDepth, nextWidth);
            nextIndex = GetCandidateIndex(roomObject, nextIndex, nextPutType);
            nextHeight = GetHeight(nextIndex);
            nextDepth = GetDepth(nextIndex);
            nextWidth = GetWidth(nextIndex);


            Vector3Int nextCenter = new Vector3Int(nextWidth, nextHeight, nextDepth) + roomObject.Data.GetCenter(nextPutType);
            Vector3Int tempCenter = new Vector3Int(tempWidth, tempHeight, tempDepth) + roomObject.Center;

            int distHeight = Mathf.Abs(nextCenter.y - tempCenter.y);
            int distDepth = Mathf.Abs(nextCenter.z - tempCenter.z);
            int distWidth = Mathf.Abs(nextCenter.x - tempCenter.x);

            //?u??????????????????
            if (nextIndex != -1 && distDepth * 2 < roomObject.Depth + nextObjDepth && distWidth * 2 < roomObject.Width + nextObjWidth)
            {
                //???]
                roomObject.SetPutType(nextPutType, nextIndex, i);
                canRotate = true;
                break;
            }
        }

        if (!canRotate)
        {
            Debug.Log("???]??????????");
        }

        SetState(roomObject, true);
    }

    //???W?b?N???????????????????????]
    private void RotateMinObject(RoomObject roomObject)
    {

    }

    private int SelectIndex(RoomObjectData data, ref PutType putType, RoomObject floorObject, EmptySpace emptySpace)
    {
        if (floorObject == null || emptySpace == null)
        {
            return SelectIndex(data, ref putType);
        }
        int index = -1;

        if (data.PosType == PositionType.FLOOR)
        {
            for (int i = 0; i < Enum.GetValues(typeof(PutType)).Length; i++)
            {
                PutType tempPutType = (PutType)((int)putType + i);

                index = SelectIndexFloor(data, tempPutType, floorObject, emptySpace);

                if (index != -1)
                {
                    break;
                }
            }

            return index;
            /*index = SelectIndexFloor(data, putType, floorObject, emptySpace);

            if (index == -1)
            {
                putType = PutType.REVERSE;
                index = SelectIndexFloor(data, putType, floorObject, emptySpace);
            }

            return index;*/
        }
        else
        {
            return index;
        }
    }

    private int SelectIndex(RoomObjectData data, ref PutType putType, int areaIdx, int areaHeight, int areaDepth, int areaWidth)
    {
        if (data.PosType == PositionType.FLOOR)
        {
            return SelectIndexFloor(data, putType, areaIdx, areaHeight, areaDepth, areaWidth);
        }
        else
        {
            //return SelectIndex(data, ref putType);
            return -1;
        }
    }

    //????????????Index??????????
    //?????????u????????????-1??????
    private int SelectIndex(RoomObjectData data, ref PutType putType)
    {
        int index = -1;

        #region FLOOR
        if (data.PosType == PositionType.FLOOR)
        {
            index = SelectIndexFloor(data, putType);

            if (index == -1)
            {
                putType = (PutType)(((int)putType + 1) / Enum.GetValues(typeof(PutType)).Length);
                index = SelectIndexFloor(data, putType);
            }

            return index;
        }
        #endregion

        #region WALL
        if (data.PosType == PositionType.WALL)
        {
            #region LEFT
            if (putType == PutType.NORMAL)
            {
                //???????????????????T??
                index = SelectIndexWall(data, putType);

                //???????????????E?????T??
                if (index == -1)
                {
                    putType = PutType.REVERSE;
                    index = SelectIndexWall(data, putType);
                }
                return index;
            }
            #endregion

            #region RIGHT
            if (putType == PutType.REVERSE)
            {
                index = SelectIndexWall(data, putType);

                if (index == -1)
                {
                    putType = PutType.NORMAL;
                    index = SelectIndexWall(data, putType);
                }

                return index;
            }
            #endregion
        }
        #endregion

        return -1;
    }

    private int SelectIndexFloor(RoomObjectData data, PutType putType, int areaIdx, int areaHeight, int areaDepth, int areaWidth)
    {
        for (int emptyHeight = 0; emptyHeight < areaHeight; emptyHeight++)
        {
            int randIdx = UnityEngine.Random.Range(0, areaDepth * areaWidth);
            int randDepth = randIdx / areaWidth;
            int randWidth = randIdx % areaWidth;

            for (int emptyDepth = 0; emptyDepth < areaDepth; emptyDepth++)
            {
                for (int emptyWidth = 0; emptyWidth < areaWidth; emptyWidth++)
                {
                    //????Index????????
                    int areaMinHeight = GetHeight(areaIdx); //????????????????????????????
                    int areaMinDepth = GetDepth(areaIdx);
                    int areaMinWidth = GetWidth(areaIdx);

                    int tempDepth;
                    int tempWidth;

                    if (m_Cnt == 1)
                    {
                        tempDepth = areaMinDepth + (randDepth - emptyDepth + areaDepth) % areaDepth;
                        tempWidth = areaMinWidth + (randWidth + emptyWidth) % areaWidth;
                    }
                    else if (m_Cnt == 2)
                    {
                        tempDepth = areaMinDepth + (randDepth + emptyDepth) % areaDepth;
                        tempWidth = areaMinWidth + (randWidth - emptyWidth + areaWidth) % areaWidth;
                    }
                    else if (m_Cnt == 3)
                    {
                        tempDepth = areaMinDepth + (randDepth - emptyDepth + areaDepth) % areaDepth;
                        tempWidth = areaMinWidth + (randWidth - emptyWidth + areaWidth) % areaWidth;
                    }
                    else
                    {
                        tempDepth = areaMinDepth + (randDepth + emptyDepth) % areaDepth;
                        tempWidth = areaMinWidth + (randWidth + emptyWidth) % areaWidth;
                    }

                    int tempHeight = areaMinHeight + emptyHeight;

                    int i = GetIndex(tempHeight, tempDepth, tempWidth);

                    if (GetCanPut(i, data, putType))
                    {
                        return i;
                    }
                }
            }
        }

        return -1;
    }

    private int SelectIndexFloor(RoomObjectData data, PutType putType, RoomObject floorObject, EmptySpace emptySpace)
    {
        int spaceMinIdx = GetIndexFromEmptySpace(floorObject, emptySpace);
        int emptyMaxHeight = (int)(emptySpace.GetSize(floorObject.PutType).y / m_RoomMasterData.RoomUnit);
        int emptyMaxDepth = (int)(emptySpace.GetSize(floorObject.PutType).z / m_RoomMasterData.RoomUnit);
        int emptyMaxWidth = (int)(emptySpace.GetSize(floorObject.PutType).x / m_RoomMasterData.RoomUnit);

        for (int emptyHeight = 0; emptyHeight < emptyMaxHeight; emptyHeight++)
        {
            int randIdx = UnityEngine.Random.Range(0, emptyMaxDepth * emptyMaxWidth);
            int randDepth = randIdx / emptyMaxWidth;
            int randWidth = randIdx % emptyMaxWidth;

            for (int emptyDepth = 0; emptyDepth < emptyMaxDepth; emptyDepth++)
            {
                for (int emptyWidth = 0; emptyWidth < emptyMaxWidth; emptyWidth++)
                {
                    //????Index????????
                    int spaceMinHeight = GetHeight(spaceMinIdx);
                    int spaceMinDepth = GetDepth(spaceMinIdx);
                    int spaceMinWidth = GetWidth(spaceMinIdx);

                    int tempDepth;
                    int tempWidth;

                    if (m_Cnt == 1)
                    {
                        tempDepth = spaceMinDepth + (randDepth - emptyDepth + emptyMaxDepth) % emptyMaxDepth;
                        tempWidth = spaceMinWidth + (randWidth + emptyWidth) % emptyMaxWidth;
                    }
                    else if (m_Cnt == 2)
                    {
                        tempDepth = spaceMinDepth + (randDepth + emptyDepth) % emptyMaxDepth;
                        tempWidth = spaceMinWidth + (randWidth - emptyWidth + emptyMaxWidth) % emptyMaxWidth;
                    }
                    else if (m_Cnt == 3)
                    {
                        tempDepth = spaceMinDepth + (randDepth - emptyDepth + emptyMaxDepth) % emptyMaxDepth;
                        tempWidth = spaceMinWidth + (randWidth - emptyWidth + emptyMaxWidth) % emptyMaxWidth;
                    }
                    else
                    {
                        tempDepth = spaceMinDepth + (randDepth + emptyDepth) % emptyMaxDepth;
                        tempWidth = spaceMinWidth + (randWidth + emptyWidth) % emptyMaxWidth;
                    }

                    int tempHeight = spaceMinHeight + emptyHeight;

                    int tempCenterWidth = tempWidth + data.GetObjWidth(putType) / 2;
                    int tempCenterDepth = tempDepth + data.GetObjDepth(putType) / 2;

                    if(putType == PutType.NORMAL)
                    {
                        if (tempCenterWidth > tempCenterDepth) continue;
                    }
                    if(putType == PutType.REVERSE)
                    {
                        if (tempCenterWidth <= tempCenterDepth) continue;
                    }

                    int i = GetIndex(tempHeight, tempDepth, tempWidth);

                    if (GetCanPut(i, data, putType))
                    {
                        return i;
                    }
                }
            }
        }

        return -1;
    }

    private int SelectIndexFloor(RoomObjectData data, PutType putType)
    {
        for (int height = 0; height < m_RoomHeight; height++)
        {
            int randIdx = UnityEngine.Random.Range(0, m_RoomDepth * m_RoomWidth);
            int randDepth = randIdx / m_RoomWidth;
            int randWidth = randIdx % m_RoomWidth;

            for (int depth = 0; depth < m_RoomDepth; depth++)
            {
                for (int width = 0; width < m_RoomWidth; width++)
                {
                    int tempDepth;
                    int tempWidth;

                    if (m_Cnt == 1)
                    {
                        tempDepth = (randDepth - depth + m_RoomDepth) % m_RoomDepth;
                        tempWidth = (randWidth + width) % m_RoomWidth;
                    }
                    else if (m_Cnt == 2)
                    {
                        tempDepth = (randDepth + depth) % m_RoomDepth;
                        tempWidth = (randWidth - width + m_RoomWidth) % m_RoomWidth;
                    }
                    else if (m_Cnt == 3)
                    {
                        tempDepth = (randDepth - depth + m_RoomDepth) % m_RoomDepth;
                        tempWidth = (randWidth - width + m_RoomWidth) % m_RoomWidth;
                    }
                    else
                    {
                        tempDepth = (randDepth + depth) % m_RoomDepth;
                        tempWidth = (randWidth + width) % m_RoomWidth;
                    }

                    //int tempHeight = m_FloorHeights[tempDepth * m_RoomWidth + tempWidth];
                    int tempHeight = height;

                    int tempCenterWidth = tempWidth + data.GetObjWidth(putType) / 2;
                    int tempCenterDepth = tempDepth + data.GetObjDepth(putType) / 2;

                    if (putType == PutType.NORMAL)
                    {
                        if (tempCenterWidth > tempCenterDepth) continue;
                    }
                    if (putType == PutType.REVERSE)
                    {
                        if (tempCenterWidth <= tempCenterDepth) continue;
                    }

                    int i = GetIndex(tempHeight, tempDepth, tempWidth);

                    if (GetCanPut(i, data, putType))
                    {
                        return i;
                    }
                }
            }
        }

        return -1;
    }

    private int SelectIndexWall(RoomObjectData data, PutType putType, bool isFront = false)
    {
        #region LEFTWALL
        if (putType == PutType.NORMAL)
        {
            for (int width = 0; width < m_RoomWidth; width++)
            {
                for (int height = 0; height < m_RoomHeight; height++)
                {
                    for (int depth = 0; depth < m_RoomDepth; depth++)
                    {
                        int randIdx = UnityEngine.Random.Range(0, m_RoomHeight * m_RoomDepth);
                        int randHeight = randIdx / m_RoomDepth;
                        int randDepth = randIdx % m_RoomDepth;

                        int tempHeight;
                        int tempDepth;

                        if (m_Cnt == 1)
                        {
                            tempDepth = (randDepth - depth + m_RoomDepth) % m_RoomDepth;
                            tempHeight = (randHeight + height) % m_RoomHeight;
                        }
                        else if (m_Cnt == 2)
                        {
                            tempDepth = (randDepth + depth) % m_RoomDepth;
                            tempHeight = (randHeight - height + m_RoomHeight) % m_RoomHeight;
                        }
                        else if (m_Cnt == 3)
                        {
                            tempDepth = (randDepth - depth + m_RoomDepth) % m_RoomDepth;
                            tempHeight = (randHeight - height + m_RoomHeight) % m_RoomHeight;
                        }
                        else
                        {
                            tempDepth = (randDepth + depth) % m_RoomDepth;
                            tempHeight = (randHeight + height) % m_RoomHeight;
                        }

                        //int tempWidth = m_WallWidthHeights[tempHeight * m_RoomDepth + tempDepth];
                        //if (tempWidth == -1) continue;
                        int tempWidth = width;
                        if (!isFront) tempWidth = m_RoomWidth - 1 - width;
                        int i = GetIndex(tempHeight, tempDepth, tempWidth);

                        if (GetCanPut(i, data, putType))
                        {
                            return i;
                        }
                    }
                }
            }

            return -1;
        }
        #endregion

        #region RIGHTWALL
        else if (putType == PutType.REVERSE)
        {
            for (int depth = 0; depth < m_RoomDepth; depth++)
            {
                for (int height = 0; height < m_RoomHeight; height++)
                {
                    for (int width = 0; width < m_RoomWidth; width++)
                    {
                        int randIdx = UnityEngine.Random.Range(0, m_RoomHeight * m_RoomWidth);
                        int randHeight = randIdx / m_RoomWidth;
                        int randWidth = randIdx % m_RoomWidth;

                        int tempHeight;
                        int tempWidth;

                        if (m_Cnt == 1)
                        {
                            tempWidth = (randWidth - width + m_RoomWidth) % m_RoomWidth;
                            tempHeight = (randHeight + height) % m_RoomHeight;
                        }
                        else if (m_Cnt == 2)
                        {
                            tempWidth = (randWidth + width) % m_RoomWidth;
                            tempHeight = (randHeight - height + m_RoomHeight) % m_RoomHeight;
                        }
                        else if (m_Cnt == 3)
                        {
                            tempWidth = (randWidth - width + m_RoomWidth) % m_RoomWidth;
                            tempHeight = (randHeight - height + m_RoomHeight) % m_RoomHeight;
                        }
                        else
                        {
                            tempWidth = (randWidth + width) % m_RoomWidth;
                            tempHeight = (randHeight + height) % m_RoomHeight;
                        }

                        //int tempDepth = m_WallDepthHeights[height * m_RoomWidth + width];
                        //if (tempDepth == -1) continue;
                        int tempDepth = depth;
                        if (!isFront) tempDepth = m_RoomDepth - 1 - depth;
                        int i = GetIndex(tempHeight, tempDepth, tempWidth);

                        if (GetCanPut(i, data, putType))
                        {
                            return i;
                        }
                    }
                }
            }
            return -1;
        }
        #endregion

        return -1;
    }

    public RoomObject InstantiateRoomObject(SaveDataUnit saveDataUnit)
    {
        int index = saveDataUnit.RoomIndex;
        RoomObjectData obj = m_RoomObjectMasterData.RoomObjects[saveDataUnit.DataIndex];
        PutType putType = (PutType)saveDataUnit.PutTypeNum;

        RoomObject model = obj.Model;
        RoomObject roomGameObject = null;
        roomGameObject = Instantiate(model);

        if (roomGameObject != null)
        {
            roomGameObject.Init(saveDataUnit, m_RoomMasterData, m_RoomObjectMasterData);
            roomGameObject.OnTapRoomObject.Subscribe((roomObject) =>
            {
                m_OnTapRoomObjectSubject.OnNext(roomObject);
            }).AddTo(this);

            roomGameObject.OnHoldRoomObject.Subscribe((roomObject) =>
            {
                m_OnHoldRoomObjectSubject.OnNext(roomObject);
            }).AddTo(this);

            roomGameObject.OnDoubleTapRoomObject.Subscribe((roomObject) =>
            {
                m_OnDoubleTapRoomObjectSubject.OnNext(roomObject);
            }).AddTo(this);

            roomGameObject.OnSetMaterial.Subscribe((setMaterialEvent) => 
            {
                SetSelectedPictureObject(setMaterialEvent);
            }).AddTo(this);

            SetState(roomGameObject, true);

            Debug.Log(obj.Model.name + "???u???????? x y z: " + GetWidth(index) + " " + GetHeight(index) + " " + GetDepth(index));
            Debug.Log("??????: x, y, z: " + obj.GetObjWidth(putType) + " " + obj.GetObjHeight(putType) + " " + obj.GetObjDepth(putType));

            for (int idx = 0; idx < m_SpaceCount; idx++)
            {
                CalcStateSum(GetHeight(idx), GetDepth(idx), GetWidth(idx));
            }
        }

        return roomGameObject;
    }

    public RoomObject InstantiateRoomObject(int index, RoomObjectData obj, PutType putType)
    {
        RoomObject model = obj.Model;
        RoomObject roomGameObject = null;
        roomGameObject = Instantiate(model);

        if (roomGameObject != null)
        {
            roomGameObject.Init(obj, index, putType);

            roomGameObject.OnTapRoomObject.Subscribe((roomObject) =>
            {
                m_OnTapRoomObjectSubject.OnNext(roomObject);
            }).AddTo(this);

            roomGameObject.OnHoldRoomObject.Subscribe((roomObject) =>
            {
                m_OnHoldRoomObjectSubject.OnNext(roomObject);
            }).AddTo(this);

            roomGameObject.OnDoubleTapRoomObject.Subscribe((roomObject) =>
            {
                m_OnDoubleTapRoomObjectSubject.OnNext(roomObject);
            }).AddTo(this);

            roomGameObject.OnSetMaterial.Subscribe((setMaterialEvent) =>
            {
                SetSelectedPictureObject(setMaterialEvent);
            }).AddTo(this);

            SetState(roomGameObject, true);

            for (int idx = 0; idx < m_SpaceCount; idx++)
            {
                CalcStateSum(GetHeight(idx), GetDepth(idx), GetWidth(idx));
            }
        }

        return roomGameObject;
    }

    public void SetState(RoomObject roomObj, bool isPut)
    {
        Queue<RoomObject> tempObjQue = new Queue<RoomObject>();
        tempObjQue.Enqueue(roomObj);

        while (tempObjQue.Count > 0)
        {
            RoomObject tempObj = tempObjQue.Peek();
            tempObjQue.Dequeue();

            RoomObjectData tempData = tempObj.Data;
            int tempIndex = tempObj.RoomIndex;
            PutType tempPutType = tempObj.PutType;
            int tempHeight = GetHeight(tempIndex);
            int tempDepth = GetDepth(tempIndex);
            int tempWidth = GetWidth(tempIndex);

            int currentObjHeight = tempObj.Height;
            int currentObjDepth = tempObj.Depth;
            int currentObjWidth = tempObj.Width;


            for (int i = 0; i < currentObjHeight; i++)
            {
                for (int j = 0; j < currentObjDepth; j++)
                {
                    for (int k = 0; k < currentObjWidth; k++)
                    {
                        int stateHeight = tempHeight + i;
                        int stateDepth = tempDepth + j;
                        int stateWidth = tempWidth + k;
                        int stateIdx = GetIndex(stateHeight, stateDepth, stateWidth);
                        if (stateIdx == -1)
                        {
                            Debug.Log("is out of range: " + roomObj.gameObject.name + " " + stateWidth + " " + stateHeight + " " + stateDepth);
                        }
                        m_RoomStates[stateIdx] = isPut ? tempObj : null;
                    }
                }
            }

            if (isPut)
            {
                float roomUnit = m_RoomMasterData.RoomUnit;

                if (tempData.EmptySpaces.Count > 0)
                {
                    //for (int id = 0; id < tempData.EmptySpaces.Count; id++)
                    foreach (var emptySpace in tempData.EmptySpaces)
                    {
                        if (emptySpace.Id == -1) continue;
                        int emptySpaceIndex = GetEmptySpaceIndex(tempIndex, tempData, emptySpace, tempPutType);
                        int spaceHeight = GetEmptySpaceHeight(emptySpace, tempPutType);
                        int spaceDepth = GetEmptySpaceDepth(emptySpace, tempPutType);
                        int spaceWidth = GetEmptySpaceWidth(emptySpace, tempPutType);

                        int spaceMinHeight = GetHeight(emptySpaceIndex);
                        int spaceMinDepth = GetDepth(emptySpaceIndex);
                        int spaceMinWidth = GetWidth(emptySpaceIndex);

                        //Debug.Log("space id x, y, z, height" + emptySpace.Id + " " + spaceMinWidth + " " + spaceMinHeight + " " + spaceMinDepth + " " + spaceHeight);

                        for (int atHeight = 0; atHeight < spaceHeight; atHeight++)
                        {
                            for (int atDepth = 0; atDepth < spaceDepth; atDepth++)
                            {
                                for (int atWidth = 0; atWidth < spaceWidth; atWidth++)
                                {
                                    int stateHeight = spaceMinHeight + atHeight;
                                    int stateDepth = spaceMinDepth + atDepth;
                                    int stateWidth = spaceMinWidth + atWidth;
                                    int stateIdx = GetIndex(stateHeight, stateDepth, stateWidth);
                                    m_RoomStates[stateIdx] = null;
                                }
                            }
                        }
                    }
                }
            }

            for(int i = 0; i < tempObj.ChildRoomObjectList.Count; i++)
            {
                RoomObject childObj = tempObj.ChildRoomObjectList[i];
                if (childObj != null)
                {
                    tempObjQue.Enqueue(childObj);
                }
            }
        }

        //?????a???X?V
        for (int idx = 0; idx < m_SpaceCount; idx++)
        {
            CalcStateSum(GetHeight(idx), GetDepth(idx), GetWidth(idx));
        }

        //FamilySize???X?V
        #region FAMILY_SIZE

        int index = roomObj.RoomIndex;
        int height = GetHeight(index);
        int depth = GetDepth(index);
        int width = GetWidth(index);

        int objHeight = roomObj.Height;
        int objDepth = roomObj.Depth;
        int objWidth = roomObj.Width;

        if (height > 0 && roomObj.Data.PosType == PositionType.FLOOR)
        {
            RoomObject floorObject = m_RoomStates[GetIndex(height - 1, depth + objDepth / 2, width + objWidth / 2)];

            int childMaxHeight = 0;
            RoomObject childMaxObject = null;
            if (floorObject != null)
            {
                foreach(var childObject in floorObject.ChildRoomObjectList)
                {
                    if (childObject != null)
                    {
                        if(childMaxHeight < childObject.GetFamilySize(childObject.PutType).y)
                        {
                            childMaxObject = childObject;
                        }
                        childMaxHeight = Mathf.Max(childMaxHeight, childObject.GetFamilySize(childObject.PutType).y);

                    }
                }
            }
            int childSecondMaxHeight = 0;
            if(floorObject != null)
            {
                foreach(var childObject in floorObject.ChildRoomObjectList)
                {
                    if(childObject != null && childObject != childMaxObject)
                    {
                        //??????????????????????????
                        childSecondMaxHeight = Mathf.Max(childSecondMaxHeight, childObject.GetFamilySize(childObject.PutType).y);
                    }
                }
            }

            if (isPut)
            {
                roomObj.SetParent(floorObject);

                if (floorObject.GetFamilySize(floorObject.PutType).y + GetHeight(floorObject.RoomIndex) >= height + roomObj.GetFamilySize(roomObj.PutType).y)
                {
                    return;
                }

                int offsetHeight = roomObj.GetFamilySize(roomObj.PutType).y - childMaxHeight;
                while (floorObject != null)

                {
                    floorObject.SetFamilySize(floorObject.GetFamilySize(floorObject.PutType) + offsetHeight * Vector3Int.up, floorObject.PutType);
                    if (floorObject.ParentObject != null)
                    {
                        floorObject = floorObject.ParentObject;
                    }
                    else
                    {
                        floorObject = null;
                    }
                }
            }
            else
            {
                roomObj.DetachFromParent();

                if (GetHeight(floorObject.RoomIndex) + floorObject.GetFamilySize(floorObject.PutType).y > height + roomObj.GetFamilySize(roomObj.PutType).y)
                {
                    return;
                }

                int offsetHeight = childSecondMaxHeight - roomObj.GetFamilySize(roomObj.PutType).y;

                while (floorObject != null)
                {
                    floorObject.SetFamilySize(floorObject.GetFamilySize(floorObject.PutType) + offsetHeight * Vector3Int.up, floorObject.PutType);
                    if (floorObject.ParentObject != null)
                    {
                        floorObject = floorObject.ParentObject;
                    }
                    else
                    {
                        floorObject = null;
                    }
                }
            }
        }

        #endregion
    }

    public void RemoveFamily(RoomObject roomObject)
    {
        Remove(roomObject);
    }

    private void Remove(RoomObject roomObject)
    {
        SetState(roomObject, false);
        Destroy(roomObject.gameObject);
    }

    public void RemoveAll()
    {
        //GameObject????

        for (int idx = 0; idx < m_SpaceCount; idx++)
        {
            if (m_RoomStates[idx] != null)
            {
                if (m_RoomStates[idx] is RoomObjectFixed) continue;

                if (m_RoomStates[idx].gameObject != null)
                {
                    Destroy(m_RoomStates[idx].gameObject);
                }
            }
            m_RoomStates[idx] = null;
        }

        for (int idx = 0; idx < m_SpaceCount; idx++)
        {
            int x = GetWidth(idx);
            int y = GetHeight(idx);
            int z = GetDepth(idx);
            CalcStateSum(GetHeight(idx), GetDepth(idx), GetWidth(idx));
        }
    }

    private bool GetCanPut(int index, RoomObjectData objData, PutType putType)
    {
        int minHeight = GetHeight(index);
        int minDepth = GetDepth(index);
        int minWidth = GetWidth(index);
        int maxHeight;
        int maxDepth;
        int maxWidth;

        maxHeight = GetHeight(index) + objData.GetObjHeight(putType);
        maxDepth = GetDepth(index) + objData.GetObjDepth(putType);
        maxWidth = GetWidth(index) + objData.GetObjWidth(putType);

        //??????????????????????
        if (minHeight < 0 || maxHeight > m_RoomHeight)
        {
            return false;
        }

        if (minDepth < 0 || maxDepth > m_RoomDepth)
        {
            return false;
        }

        if (minWidth < 0 || maxWidth > m_RoomWidth)
        {
            return false;
        }

        //???????I?u?W?F?N?g???d????????
        if (GetIsOverlap(index, objData, putType))
        {
            return false;
        }

        //????????????????????????????
        if (objData.PosType == PositionType.FLOOR)
        {
            if (minHeight > 0)
            {
                int floorIndex = GetIndex(minHeight - 1, minDepth, minWidth);
                int floorOverlaps = GetOverlaps(floorIndex, 1, maxDepth - minDepth, maxWidth - minWidth);
                if (floorOverlaps % ((maxDepth - minDepth) * (maxWidth - minWidth)) != 0)
                {
                    return false;
                }
                else
                {
                    int floorLevel = floorOverlaps % ((maxDepth - minDepth) * (maxWidth - minWidth));
                    if ((int)objData.Level < floorLevel)
                    {
                        return false;
                    }

                    //4?????m?F
                    Vector2Int[] dirs = new Vector2Int[4];
                    dirs[0] = new Vector2Int(0, 0);
                    dirs[1] = new Vector2Int(1, 0);
                    dirs[2] = new Vector2Int(0, 1);
                    dirs[3] = new Vector2Int(1, 1);
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2Int dir = dirs[i];
                        int floorDepth = minDepth + dir.x * (maxDepth - 1 - minDepth);
                        int floorWidth = minWidth + dir.y * (maxWidth - 1 - minWidth);
                        RoomObject roomObject = m_RoomStates[GetIndex(minHeight - 1, floorDepth, floorWidth)];
                        if (roomObject == null)
                        {
                            return false;

                        }
                        else
                        {
                            RoomObjectData data = roomObject.Data;
                            if (!data.IsPlane)
                            {
                                return false;
                            }
                            else if ((int)objData.Level < (int)data.Level)
                            {
                                return false;
                            }
                        }

                    }
                }
            }
        }
        else if (objData.PosType == PositionType.WALL)
        {
            if (putType == PutType.NORMAL)
            {
                if (minWidth > 0)
                {
                    int floorIndex = GetIndex(minHeight, minDepth, minWidth - 1);
                    int floorOverlaps = GetOverlaps(floorIndex, maxHeight - minHeight, maxDepth - minDepth, 1);
                    if (floorOverlaps % ((maxHeight - minHeight) * (maxDepth - minDepth)) == 0)
                    {
                        return false;
                    }
                    else
                    {

                        int floorLevel = floorOverlaps % ((maxHeight - minHeight) * (maxDepth - minDepth));
                        if ((int)objData.Level < floorLevel)
                        {
                            return false;
                        }

                        Vector2Int[] dirs = new Vector2Int[4];
                        dirs[0] = new Vector2Int(0, 0);
                        dirs[1] = new Vector2Int(1, 0);
                        dirs[2] = new Vector2Int(0, 1);
                        dirs[3] = new Vector2Int(1, 1);
                        for (int i = 0; i < 4; i++)
                        {
                            Vector2Int dir = dirs[i];
                            int floorHeight = minHeight + dir.x * (maxHeight - 1 - minHeight);
                            int floorDepth = minDepth + dir.y * (maxDepth - 1 - minDepth);
                            RoomObject roomObject = m_RoomStates[GetIndex(floorHeight, floorDepth, minWidth - 1)];
                            if (roomObject == null)
                            {
                                return false;
                            }
                            else
                            {
                                RoomObjectData data = roomObject.Data;
                                if (!data.IsPlane)
                                {
                                    return false;
                                }
                                if ((int)objData.Level < (int)data.Level)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            else if (putType == PutType.REVERSE)
            {
                if (minDepth > 0)
                {
                    int floorIndex = GetIndex(minHeight, minDepth, minWidth);
                    int floorOverlaps = GetOverlaps(floorIndex, maxHeight - minHeight, 1, maxWidth - minWidth);
                    if (floorOverlaps % ((maxHeight - minHeight) * (maxWidth - minWidth)) == 0)
                    {
                        return false;
                    }
                    else
                    {
                        int floorLevel = floorOverlaps % ((maxHeight - minHeight) * (maxWidth - minWidth));
                        if ((int)objData.Level > floorLevel)
                        {
                            return false;
                        }

                        Vector2Int[] dirs = new Vector2Int[4];
                        dirs[0] = new Vector2Int(0, 0);
                        dirs[1] = new Vector2Int(1, 0);
                        dirs[2] = new Vector2Int(0, 1);
                        dirs[3] = new Vector2Int(1, 1);
                        for (int i = 0; i < 4; i++)
                        {
                            Vector2Int dir = dirs[i];
                            int floorHeight = minHeight + dir.x * (maxHeight - 1 - minHeight);
                            int floorWidth = minWidth + dir.y * (maxWidth - 1 - minWidth);
                            RoomObject roomObject = m_RoomStates[GetIndex(floorHeight, minDepth - 1, floorWidth)];
                            if (roomObject == null)
                            {
                                return false;
                            }
                            else
                            {
                                RoomObjectData data = roomObject.Data;
                                if (!data.IsPlane)
                                {
                                    return false;
                                }
                                if ((int)objData.Level < (int)data.Level)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
        }

        return true;
    }

    /// <summary>
    /// ???? RoomObject??Index, PutType???m?F???????l???????????????\?????????????g??????
    /// </summary>
    /// <param name="index"></param>
    /// <param name="roomObject"></param>
    /// <param name="putType"></param>
    /// <returns></returns>
    private bool GetCanPut(int index, RoomObject roomObject, PutType putType)
    {
        RoomObjectData objData = roomObject.Data;
        int minHeight = GetHeight(index);
        int minDepth = GetDepth(index);
        int minWidth = GetWidth(index);
        int maxHeight;
        int maxDepth;
        int maxWidth;

        maxHeight = GetHeight(index) + roomObject.GetFamilySize(putType).y;
        maxDepth = GetDepth(index) + roomObject.GetFamilySize(putType).z;
        maxWidth = GetWidth(index) + roomObject.GetFamilySize(putType).x;


        //??????????????????????
        if (minHeight < 0 || maxHeight > m_RoomHeight)
        {
            return false;
        }

        if (minDepth < 0 || maxDepth > m_RoomDepth)
        {
            return false;
        }

        if (minWidth < 0 || maxWidth > m_RoomWidth)
        {
            return false;
        }

        //???????I?u?W?F?N?g???d????????
        if (GetIsOverlap(index, roomObject, putType))
        {
            return false;
        }

        //????????????????????????????
        if (objData.PosType == PositionType.FLOOR)
        {
            if (minHeight > 0)
            {
                //???????????v?Z?????????o?O??
                //??????????????????????????????????????????????????
                //?~???l?????????????????m?F??????Overlaps??1?w???`?F?b?N????
                //?a????level???????l?????? + ?l?????m?F??????
                //ToDo: ???????\?????????? ?????l?????v???????? ???? 4???????????????A?????????????????????????_??????
                int floorIndex = GetIndex(minHeight - 1, minDepth, minWidth);
                int floorOverlaps = GetOverlaps(floorIndex, 1, maxDepth - minDepth, maxWidth - minWidth);
                if (floorOverlaps % ((maxDepth - minDepth) * (maxWidth - minWidth)) != 0)
                {
                    return false;
                }
                else
                {
                    int floorLevel = floorOverlaps % ((maxDepth - minDepth) * (maxWidth - minWidth));
                    if ((int)objData.Level < floorLevel)
                    {
                        return false;
                    }

                    //4?????m?F
                    Vector2Int[] dirs = new Vector2Int[4];
                    dirs[0] = new Vector2Int(0, 0);
                    dirs[1] = new Vector2Int(1, 0);
                    dirs[2] = new Vector2Int(0, 1);
                    dirs[3] = new Vector2Int(1, 1);

                    //????????????????????????????????????????
                    int originFloorDepth = minDepth + dirs[0].x * (maxDepth - 1 - minDepth);
                    int originFloorWidth = minWidth + dirs[0].y * (maxWidth - 1 - minWidth);
                    RoomObject originFloorObject = m_RoomStates[GetIndex(minHeight - 1, originFloorDepth, originFloorWidth)];

                    for (int i = 0; i < 4; i++)
                    {
                        Vector2Int dir = dirs[i];
                        int floorDepth = minDepth + dir.x * (maxDepth - 1 - minDepth);
                        int floorWidth = minWidth + dir.y * (maxWidth - 1 - minWidth);
                        RoomObject floorObject = m_RoomStates[GetIndex(minHeight - 1, floorDepth, floorWidth)];
                        if (floorObject == null || floorObject != originFloorObject)
                        {
                            return false;

                        }
                        else
                        {
                            RoomObjectData data = floorObject.Data;
                            if (!data.IsPlane)
                            {
                                return false;
                            }
                            else if ((int)objData.Level < (int)data.Level)
                            {
                                return false;
                            }
                        }

                    }
                }
            }
        }
        else if (objData.PosType == PositionType.WALL)
        {
            if (putType == PutType.NORMAL)
            {
                if (minWidth > 0)
                {
                    //???????????v?Z?????????o?O??
                    //??????????????????????????????????????????????????
                    //?~???l?????????????????m?F??????Overlaps??1?w???`?F?b?N????
                    int floorIndex = GetIndex(minHeight, minDepth, minWidth - 1);
                    int floorOverlaps = GetOverlaps(floorIndex, maxHeight - minHeight, maxDepth - minDepth, 1);
                    if (floorOverlaps % ((maxHeight - minHeight) * (maxDepth - minDepth)) == 0)
                    {
                        return false;
                    }
                    else
                    {

                        int floorLevel = floorOverlaps % ((maxHeight - minHeight) * (maxDepth - minDepth));
                        if ((int)objData.Level < floorLevel)
                        {
                            return false;
                        }

                        //4?????m?F
                        Vector2Int[] dirs = new Vector2Int[4];
                        dirs[0] = new Vector2Int(0, 0);
                        dirs[1] = new Vector2Int(1, 0);
                        dirs[2] = new Vector2Int(0, 1);
                        dirs[3] = new Vector2Int(1, 1);
                        for (int i = 0; i < 4; i++)
                        {
                            Vector2Int dir = dirs[i];
                            int floorHeight = minHeight + dir.x * (maxHeight - 1 - minHeight);
                            int floorDepth = minDepth + dir.y * (maxDepth - 1 - minDepth);
                            RoomObject floorObject = m_RoomStates[GetIndex(floorHeight, floorDepth, minWidth - 1)];
                            if (floorObject == null)
                            {
                                return false;
                            }
                            else
                            {
                                RoomObjectData data = floorObject.Data;
                                if (!data.IsPlane)
                                {
                                    return false;
                                }
                                if ((int)objData.Level < (int)data.Level)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            else if (putType == PutType.REVERSE)
            {
                if (minDepth > 0)
                {
                    //???????????v?Z?????????o?O??
                    //??????????????????????????????????????????????????
                    //?~???l?????????????????m?F??????Overlaps??1?w???`?F?b?N????
                    int floorIndex = GetIndex(minHeight, minDepth, minWidth);
                    int floorOverlaps = GetOverlaps(floorIndex, maxHeight - minHeight, 1, maxWidth - minWidth);
                    if (floorOverlaps % ((maxHeight - minHeight) * (maxWidth - minWidth)) == 0)
                    {
                        return false;
                    }
                    else
                    {
                        int floorLevel = floorOverlaps % ((maxHeight - minHeight) * (maxWidth - minWidth));
                        if ((int)objData.Level > floorLevel)
                        {
                            return false;
                        }

                        //4?????m?F
                        Vector2Int[] dirs = new Vector2Int[4];
                        dirs[0] = new Vector2Int(0, 0);
                        dirs[1] = new Vector2Int(1, 0);
                        dirs[2] = new Vector2Int(0, 1);
                        dirs[3] = new Vector2Int(1, 1);
                        for (int i = 0; i < 4; i++)
                        {
                            Vector2Int dir = dirs[i];
                            int floorHeight = minHeight + dir.x * (maxHeight - 1 - minHeight);
                            int floorWidth = minWidth + dir.y * (maxWidth - 1 - minWidth);
                            RoomObject floorObject = m_RoomStates[GetIndex(floorHeight, minDepth - 1, floorWidth)];
                            if (floorObject == null)
                            {
                                return false;
                            }
                            else
                            {
                                RoomObjectData data = floorObject.Data;
                                if (!data.IsPlane)
                                {
                                    return false;
                                }
                                if ((int)objData.Level < (int)data.Level)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
        }

        return true;
    }

    private bool GetCanPut(int index, RoomObject roomObject, PutType putType, RoomObject selectedFloorObject, EmptySpace emptySpace)
    {
        RoomObjectData objData = roomObject.Data;
        int minHeight = GetHeight(index);
        int minDepth = GetDepth(index);
        int minWidth = GetWidth(index);
        int maxHeight;
        int maxDepth;
        int maxWidth;

        maxHeight = GetHeight(index) + roomObject.GetFamilySize(putType).y;
        maxDepth = GetDepth(index) + roomObject.GetFamilySize(putType).z;
        maxWidth = GetWidth(index) + roomObject.GetFamilySize(putType).x;

        int floorIdx = GetIndexFromEmptySpace(selectedFloorObject, emptySpace);
        int floorMinHeight = GetHeight(floorIdx);
        int floorMinDepth = GetDepth(floorIdx);
        int floorMinWidth = GetWidth(floorIdx);

        int floorMaxHeight = floorMinHeight + GetEmptySpaceHeight(emptySpace, selectedFloorObject.PutType);
        int floorMaxDepth = floorMinDepth + GetEmptySpaceDepth(emptySpace, selectedFloorObject.PutType);
        int floorMaxWidth = floorMinWidth + GetEmptySpaceWidth(emptySpace, selectedFloorObject.PutType);

        if (emptySpace.Id == -1)
        {
            floorMinHeight = GetHeight(selectedFloorObject.RoomIndex) + selectedFloorObject.Height;
            floorMinDepth = GetDepth(selectedFloorObject.RoomIndex);
            floorMinWidth = GetWidth(selectedFloorObject.RoomIndex);
            floorMaxHeight = m_RoomHeight;
            floorMaxDepth = floorMinDepth + selectedFloorObject.Depth;
            floorMaxWidth = floorMinWidth + selectedFloorObject.Width;
        }

        //??????????????????????
        if (minHeight < floorMinHeight || maxHeight > floorMaxHeight)
        {
            return false;
        }

        if (minDepth < floorMinDepth || maxDepth > floorMaxDepth)
        {
            return false;
        }

        if (minWidth < floorMinWidth || maxWidth > floorMaxWidth)
        {
            return false;
        }

        //???????I?u?W?F?N?g???d????????
        if (GetIsOverlap(index, roomObject, putType))
        {
            return false;
        }

        //????????????????????????????
        if (objData.PosType == PositionType.FLOOR)
        {
            if (minHeight > 0)
            {
                //???????????v?Z?????????o?O??
                //??????????????????????????????????????????????????
                //?~???l?????????????????m?F??????Overlaps??1?w???`?F?b?N????
                //?a????level???????l?????? + ?l?????m?F??????
                //ToDo: ???????\?????????? ?????l?????v???????? ???? 4???????????????A?????????????????????????_??????
                int floorIndex = GetIndex(minHeight - 1, minDepth, minWidth);
                int floorOverlaps = GetOverlaps(floorIndex, 1, maxDepth - minDepth, maxWidth - minWidth);
                if (floorOverlaps % ((maxDepth - minDepth) * (maxWidth - minWidth)) != 0)
                {
                    return false;
                }
                else
                {
                    int floorLevel = floorOverlaps % ((maxDepth - minDepth) * (maxWidth - minWidth));
                    if ((int)objData.Level < floorLevel)
                    {
                        return false;
                    }

                    //4?????m?F
                    Vector2Int[] dirs = new Vector2Int[4];
                    dirs[0] = new Vector2Int(0, 0);
                    dirs[1] = new Vector2Int(1, 0);
                    dirs[2] = new Vector2Int(0, 1);
                    dirs[3] = new Vector2Int(1, 1);

                    //????????????????????????????????????????
                    int originFloorDepth = minDepth + dirs[0].x * (maxDepth - 1 - minDepth);
                    int originFloorWidth = minWidth + dirs[0].y * (maxWidth - 1 - minWidth);
                    RoomObject originFloorObject = m_RoomStates[GetIndex(minHeight - 1, originFloorDepth, originFloorWidth)];

                    for (int i = 0; i < 4; i++)
                    {
                        Vector2Int dir = dirs[i];
                        int floorDepth = minDepth + dir.x * (maxDepth - 1 - minDepth);
                        int floorWidth = minWidth + dir.y * (maxWidth - 1 - minWidth);
                        RoomObject floorObject = m_RoomStates[GetIndex(minHeight - 1, floorDepth, floorWidth)];
                        if (floorObject == null || floorObject != originFloorObject)
                        {
                            return false;

                        }
                        else
                        {
                            RoomObjectData data = floorObject.Data;
                            if (!data.IsPlane)
                            {
                                return false;
                            }
                            else if ((int)objData.Level < (int)data.Level)
                            {
                                return false;
                            }
                        }

                    }
                }
            }
        }
        else if (objData.PosType == PositionType.WALL)
        {
            if (putType == PutType.NORMAL)
            {
                if (minWidth > 0)
                {
                    //???????????v?Z?????????o?O??
                    //??????????????????????????????????????????????????
                    //?~???l?????????????????m?F??????Overlaps??1?w???`?F?b?N????
                    int floorIndex = GetIndex(minHeight, minDepth, minWidth - 1);
                    int floorOverlaps = GetOverlaps(floorIndex, maxHeight - minHeight, maxDepth - minDepth, 1);
                    if (floorOverlaps % ((maxHeight - minHeight) * (maxDepth - minDepth)) == 0)
                    {
                        return false;
                    }
                    else
                    {

                        int floorLevel = floorOverlaps % ((maxHeight - minHeight) * (maxDepth - minDepth));
                        if ((int)objData.Level < floorLevel)
                        {
                            return false;
                        }

                        //4?????m?F
                        Vector2Int[] dirs = new Vector2Int[4];
                        dirs[0] = new Vector2Int(0, 0);
                        dirs[1] = new Vector2Int(1, 0);
                        dirs[2] = new Vector2Int(0, 1);
                        dirs[3] = new Vector2Int(1, 1);
                        for (int i = 0; i < 4; i++)
                        {
                            Vector2Int dir = dirs[i];
                            int floorHeight = minHeight + dir.x * (maxHeight - 1 - minHeight);
                            int floorDepth = minDepth + dir.y * (maxDepth - 1 - minDepth);
                            RoomObject floorObject = m_RoomStates[GetIndex(floorHeight, floorDepth, minWidth - 1)];
                            if (floorObject == null)
                            {
                                return false;
                            }
                            else
                            {
                                RoomObjectData data = floorObject.Data;
                                if (!data.IsPlane)
                                {
                                    return false;
                                }
                                if ((int)objData.Level < (int)data.Level)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            else if (putType == PutType.REVERSE)
            {
                if (minDepth > 0)
                {
                    //???????????v?Z?????????o?O??
                    //??????????????????????????????????????????????????
                    //?~???l?????????????????m?F??????Overlaps??1?w???`?F?b?N????
                    int floorIndex = GetIndex(minHeight, minDepth, minWidth);
                    int floorOverlaps = GetOverlaps(floorIndex, maxHeight - minHeight, 1, maxWidth - minWidth);
                    if (floorOverlaps % ((maxHeight - minHeight) * (maxWidth - minWidth)) == 0)
                    {
                        return false;
                    }
                    else
                    {
                        int floorLevel = floorOverlaps % ((maxHeight - minHeight) * (maxWidth - minWidth));
                        if ((int)objData.Level > floorLevel)
                        {
                            return false;
                        }

                        //4?????m?F
                        Vector2Int[] dirs = new Vector2Int[4];
                        dirs[0] = new Vector2Int(0, 0);
                        dirs[1] = new Vector2Int(1, 0);
                        dirs[2] = new Vector2Int(0, 1);
                        dirs[3] = new Vector2Int(1, 1);
                        for (int i = 0; i < 4; i++)
                        {
                            Vector2Int dir = dirs[i];
                            int floorHeight = minHeight + dir.x * (maxHeight - 1 - minHeight);
                            int floorWidth = minWidth + dir.y * (maxWidth - 1 - minWidth);
                            RoomObject floorObject = m_RoomStates[GetIndex(floorHeight, minDepth - 1, floorWidth)];
                            if (floorObject == null)
                            {
                                return false;
                            }
                            else
                            {
                                RoomObjectData data = floorObject.Data;
                                if (!data.IsPlane)
                                {
                                    return false;
                                }
                                if ((int)objData.Level < (int)data.Level)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
        }

        return true;
    }

    private bool GetIsOverlap(int index, RoomObjectData data, PutType putType)
    {
        int allOverlaps = GetOverlaps(index, data.GetObjHeight(putType), data.GetObjDepth(putType), data.GetObjWidth(putType));
        int emptyOverlapsSum = 0;
        if (data.EmptySpaces.Count > 0)
        {
            foreach (EmptySpace emptySpace in data.EmptySpaces)
            {
                if (emptySpace.Id == -1) continue;
                int emptyMinIndex = GetEmptySpaceIndex(index, data, emptySpace, putType);
                int emptyOverlaps = GetOverlaps(emptyMinIndex, GetEmptySpaceHeight(emptySpace, putType), GetEmptySpaceDepth(emptySpace, putType), GetEmptySpaceWidth(emptySpace, putType));
                emptyOverlapsSum += emptyOverlaps;
            }
        }
        //Debug.Log("Overlap: " + allOverlaps);
        return allOverlaps - emptyOverlapsSum > 0 ? true : false;
    }

    private bool GetIsOverlap(int index, RoomObject roomObj, PutType putType)
    {
        RoomObjectData data = roomObj.Data;
        int allOverlaps = GetOverlaps(index, roomObj.GetFamilySize(putType).y, roomObj.GetFamilySize(putType).z, roomObj.GetFamilySize(putType).x);
        int emptyOverlapsSum = 0;
        if (data.EmptySpaces.Count > 0)
        {
            foreach (EmptySpace emptySpace in data.EmptySpaces)
            {
                if (emptySpace.Id == -1) continue;
                int emptyMinIndex = GetEmptySpaceIndex(index, data, emptySpace, putType);
                int emptyOverlaps = GetOverlaps(emptyMinIndex, GetEmptySpaceHeight(emptySpace, putType), GetEmptySpaceDepth(emptySpace, putType), GetEmptySpaceWidth(emptySpace, putType));
                emptyOverlapsSum += emptyOverlaps;
            }
        }
        //Debug.Log("Overlap: " + allOverlaps);
        return allOverlaps - emptyOverlapsSum > 0 ? true : false;
    }

    private int GetOverlaps(int maxHeight, int maxDepth, int maxWidth, int minHeight, int minDepth, int minWidth)
    {
        int contain;
        int all = m_RoomStatesSum[GetSumIndex(maxHeight, maxDepth, maxWidth)];
        int a = m_RoomStatesSum[GetSumIndex(maxHeight, maxDepth, minWidth)];
        int b = m_RoomStatesSum[GetSumIndex(maxHeight, minDepth, maxWidth)];
        int c = m_RoomStatesSum[GetSumIndex(minHeight, maxDepth, maxWidth)];
        int d = m_RoomStatesSum[GetSumIndex(minHeight, minDepth, maxWidth)];
        int e = m_RoomStatesSum[GetSumIndex(minHeight, maxDepth, minWidth)];
        int f = m_RoomStatesSum[GetSumIndex(maxHeight, minDepth, minWidth)];
        int g = m_RoomStatesSum[GetSumIndex(minHeight, minDepth, minWidth)];
        contain = all - a - b - c + d + e + f - g;

        return contain;
    }

    private int GetOverlaps(int maxIndex, int minIndex)
    {
        int maxHeight = GetHeight(maxIndex);
        int minHeight = GetHeight(minIndex);
        int maxDepth = GetDepth(maxIndex);
        int minDepth = GetDepth(minIndex);
        int maxWidth = GetWidth(maxIndex);
        int minWidth = GetWidth(minIndex);

        int contain;
        int all = m_RoomStatesSum[GetSumIndex(maxHeight, maxDepth, maxWidth)];
        int a = m_RoomStatesSum[GetSumIndex(maxHeight, maxDepth, minWidth)];
        int b = m_RoomStatesSum[GetSumIndex(maxHeight, minDepth, maxWidth)];
        int c = m_RoomStatesSum[GetSumIndex(minHeight, maxDepth, maxWidth)];
        int d = m_RoomStatesSum[GetSumIndex(minHeight, minDepth, maxWidth)];
        int e = m_RoomStatesSum[GetSumIndex(minHeight, maxDepth, minWidth)];
        int f = m_RoomStatesSum[GetSumIndex(maxHeight, minDepth, minWidth)];
        int g = m_RoomStatesSum[GetSumIndex(minHeight, minDepth, minWidth)];
        contain = all - a - b - c + d + e + f - g;

        return contain;
    }

    private int GetOverlaps(int minIndex, int height, int depth, int width)
    {
        int minHeight = GetHeight(minIndex);
        int minDepth = GetDepth(minIndex);
        int minWidth = GetWidth(minIndex);

        int maxHeight = minHeight + height;
        int maxDepth = minDepth + depth;
        int maxWidth = minWidth + width;

        int contain;
        int all = m_RoomStatesSum[GetSumIndex(maxHeight, maxDepth, maxWidth)];
        int a = m_RoomStatesSum[GetSumIndex(maxHeight, maxDepth, minWidth)];
        int b = m_RoomStatesSum[GetSumIndex(maxHeight, minDepth, maxWidth)];
        int c = m_RoomStatesSum[GetSumIndex(minHeight, maxDepth, maxWidth)];
        int d = m_RoomStatesSum[GetSumIndex(minHeight, minDepth, maxWidth)];
        int e = m_RoomStatesSum[GetSumIndex(minHeight, maxDepth, minWidth)];
        int f = m_RoomStatesSum[GetSumIndex(maxHeight, minDepth, minWidth)];
        int g = m_RoomStatesSum[GetSumIndex(minHeight, minDepth, minWidth)];
        contain = all - a - b - c + d + e + f - g;

        return contain;
    }
    private void CalcStateSum(int height, int depth, int width)
    {
        RoomObject roomObject = m_RoomStates[GetIndex(height, depth, width)];
        m_RoomStatesSum[GetSumIndex(height + 1, depth + 1, width + 1)] = (roomObject != null ? (int)roomObject.Data.Level : 0) + m_RoomStatesSum[GetSumIndex(height + 1, depth + 1, width)] + m_RoomStatesSum[GetSumIndex(height + 1, depth, width + 1)] + m_RoomStatesSum[GetSumIndex(height, depth + 1, width + 1)] - m_RoomStatesSum[GetSumIndex(height + 1, depth, width)] - m_RoomStatesSum[GetSumIndex(height, depth + 1, width)] - m_RoomStatesSum[GetSumIndex(height, depth, width + 1)] + m_RoomStatesSum[GetSumIndex(height, depth, width)];
    }

    public int GetIndex(int height, int depth, int width)
    {
        return m_RoomMasterData.GetIndex(height, depth, width);
    }

    public int GetHeight(int index)
    {
        return index / ((m_RoomWidth) * (m_RoomDepth));
    }

    public int GetDepth(int index)
    {
        index %= ((m_RoomWidth) * (m_RoomDepth));
        return index / ((m_RoomWidth));
    }

    public int GetWidth(int index)
    {
        index %= ((m_RoomWidth) * (m_RoomDepth));
        return index % (m_RoomWidth);
    }

    private int GetEmptySpaceIndex(int index, RoomObjectData data, EmptySpace emptySpace, PutType putType)
    {
        int height = GetHeight(index);
        int depth = GetDepth(index);
        int width = GetWidth(index);

        int objDepth = data.GetObjDepth(putType);
        int objWidth = data.GetObjWidth(putType);
        int emptyDepth = GetEmptySpaceDepth(emptySpace, putType);
        int emptyWidth = GetEmptySpaceWidth(emptySpace, putType);

        float roomUnit = m_RoomMasterData.RoomUnit;

        int offsetHeight = (int)Mathf.Round(emptySpace.GetCenter(putType).y / roomUnit);
        int offsetDepth = (int)Mathf.Round(emptySpace.GetCenter(putType).z / roomUnit);
        int offsetWidth = (int)Mathf.Round(emptySpace.GetCenter(putType).x / roomUnit);
        
        int spaceMinHeight = height + offsetHeight;
        int spaceMinDepth = depth + objDepth / 2 + offsetDepth - (emptyDepth / 2);
        int spaceMinWidth = width + objWidth / 2 + offsetWidth - (emptyWidth / 2);

        return GetIndex(spaceMinHeight, spaceMinDepth, spaceMinWidth);
    }

    private int GetEmptySpaceHeight(EmptySpace emptySpace, PutType putType)
    {
        float roomUnit = m_RoomMasterData.RoomUnit;
        int spaceHeight = (int)Mathf.Round(emptySpace.GetSize(putType).y / roomUnit);

        return spaceHeight;
    }

    private int GetEmptySpaceDepth(EmptySpace emptySpace, PutType putType)
    {
        float roomUnit = m_RoomMasterData.RoomUnit;
        int spaceDepth = (int)Mathf.Round(emptySpace.GetSize(putType).z / roomUnit);

        return spaceDepth;
    }

    private int GetEmptySpaceWidth(EmptySpace emptySpace, PutType putType)
    {
        float roomUnit = m_RoomMasterData.RoomUnit;

        int spaceWidth = (int)Mathf.Round(emptySpace.GetSize(putType).x / roomUnit);


        return spaceWidth;
    }

    private int GetSumIndex(int height, int depth, int width)
    {
        int index = width + (m_RoomWidth + 1) * depth + (m_RoomWidth + 1) * (m_RoomDepth + 1) * height;
        return index;
    }

    private int GetSumHeight(int index)
    {
        return index / ((m_RoomWidth + 1) * (m_RoomDepth + 1));
    }

    private int GetSumDepth(int index)
    {
        index %= ((m_RoomWidth + 1) * (m_RoomDepth + 1));
        return index / ((m_RoomWidth));
    }

    private int GetSumWidth(int index)
    {
        index %= ((m_RoomWidth + 1) * (m_RoomDepth + 1));
        return index % (m_RoomWidth + 1);
    }

    public Vector3Int GetClampedPos(Vector3Int pos)
    {
        pos.x = (int)Mathf.Clamp(pos.x, 0, m_RoomWidth);
        pos.y = (int)Mathf.Clamp(pos.y, 0, m_RoomHeight);
        pos.z = (int)Mathf.Clamp(pos.z, 0, m_RoomDepth);
        return pos;
    }
}

public enum PutType
{
    NORMAL = 0,
    REVERSE = 1
}