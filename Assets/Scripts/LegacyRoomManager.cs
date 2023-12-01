/*using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

/// <summary>
/// legacy
/// </summary>
public class LegacyRoomManager : MonoBehaviour
{
    [SerializeField] private RoomMasterData m_RoomMasterData;
    [SerializeField] private RoomObjectMasterData m_RoomObjectMasterData;

    [SerializeField] private RoomUI m_RoomUI;

    [SerializeField] private GameObject m_FloorSelectingPlane;
    [SerializeField] private Renderer m_SelectingRenderer;
    [SerializeField] private Material m_CanPutMaterial;
    [SerializeField] private Material m_NotPutMaterial;

    private int m_RoomHeight;
    private int m_RoomWidth;
    private int m_RoomDepth;

    private RoomObject m_SelectedObject;
    //private RoomObject m_EmptySelectedObject;
    private RoomObject m_SelectedFloorObject;
    private RoomObject m_ControlObject;
    private EmptySpaceBehaviour m_SelectedSpace;

    private RoomObject ControlObject
    {
        get => m_ControlObject;
        set
        {
            if(m_ControlObject != null)
            {
                m_ControlObject.OnInControl(false);
            }
            if(value != null)
            {
                value.OnInControl(true);
                m_IsControlling = true;
            }
            else
            {
                m_IsControlling = false;
            }

            m_ControlObject = value;
        }
    }

    //private int[] m_RoomStates;
    private RoomObject[] m_RoomStates;
    private int[] m_RoomStatesSum;
    private int[] m_FloorHeights;
    private bool[] m_FloorStates;
    private int[] m_WallWidthHeights;
    private bool[] m_WallWidthStates;
    private int[] m_WallDepthHeights;
    private bool[] m_WallDepthStates;
    private int m_SpaceCount; //現状216000
    private int m_Cnt = 0;

    private bool m_IsWhileSelect;
    private bool m_IsControlling;
    private Vector2 m_SelectStartPos;
    private Vector3Int m_ControlHitBefPos;
    private Vector3Int m_ControlHitOffSet;
    private float m_SelectingTime = 0f;
    private float m_ControlTime = 0.5f;
    private RoomObject m_SuspectObject;

    private bool m_CanTouch;
    private RoomPhase m_RoomPhase;

    //きもいイベントの使い方になっている
    public event Action<bool, RoomObject> OnIsControlling;
    public event Action OnPutObject; //モノを置いたときのイベント
    public event Action OnRemoveObjectFamily; //モノを子オブジェクトごと取り除いたときのイベント
    public event Action OnRemoveAll;
    public event Action<bool, RoomObject> OnSelectedObject;
    public event Action<bool, RoomPhase, RoomObject> OnChangePhase;

    //一番重い処理はPut, RemoveのときのCaclStateSum 部屋のすべてのセルをみることになる

    private void Start()
    {
        m_CanTouch = true;
        m_RoomPhase = RoomPhase.None;

        #region INIT
        m_RoomHeight = m_RoomMasterData.RoomHeight;
        m_RoomDepth = m_RoomMasterData.RoomDepth;
        m_RoomWidth = m_RoomMasterData.RoomWidth;

        m_SpaceCount = (m_RoomHeight) * (m_RoomDepth) * (m_RoomWidth);
        //m_RoomStates = new int[m_SpaceCount];
        m_RoomStates = new RoomObject[m_SpaceCount];
        m_RoomStatesSum = new int[(m_RoomHeight + 1) * (m_RoomDepth + 1) * (m_RoomWidth + 1)];

        //床
        m_FloorHeights = new int[m_RoomDepth * m_RoomWidth]; 
        m_FloorStates = new bool[m_RoomDepth * m_RoomWidth];

        //左壁
        m_WallWidthHeights = new int[m_RoomHeight * m_RoomDepth]; 
        m_WallWidthStates = new bool[m_RoomHeight * m_RoomDepth];

        //右壁
        m_WallDepthHeights = new int[m_RoomHeight * m_RoomWidth]; 
        m_WallDepthStates = new bool[m_RoomHeight * m_RoomWidth];

        for(int i = 0; i < m_SpaceCount; i++)
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

        for(int i = 0; i < m_RoomDepth; i++)
        {
            for(int j = 0; j < m_RoomWidth; j++)
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

        #region UI_EVENT
        m_RoomUI.DetailButton
           .onClick
           .AddListener(() =>
           {
               //入力を受け付けないようにする
               if(m_RoomPhase == RoomPhase.Selected)
               {
                   if(m_CanTouch)
                   {
                       Debug.Log("detail pressed");
                       m_CanTouch = false;
                       m_SelectedFloorObject = m_SelectedObject;
                       m_RoomPhase = RoomPhase.Detail;
                       m_SelectedObject.OnPhaseChanged(m_RoomPhase);
                       OnChangePhase.Invoke(true, m_RoomPhase, m_SelectedObject);
                   }
               }
           });

        m_RoomUI.BackButton
            .onClick
            .AddListener(() =>
            {
                if(m_RoomPhase == RoomPhase.Detail)
                {
                    m_CanTouch = false;
                    
                    //selectedObjectをもとに戻す
                    if(m_SelectedFloorObject != null && m_SelectedSpace != null)
                    {
                        //m_SelectedObject = m_SelectedFloorObject;
                        Transform parentTransform = m_SelectedFloorObject.transform.parent;
                        m_SelectedFloorObject = null;
                        if (parentTransform != null)
                        {
                            RoomObject parentObject = parentTransform.gameObject.GetComponent<RoomObject>();
                            if(parentObject != null)
                            {
                                m_SelectedFloorObject = parentObject;
                            }
                        }
                    }
                    else
                    {
                        m_SelectedFloorObject = null;
                    }

                    m_RoomPhase = RoomPhase.Selected;
                    m_SelectedObject.OnPhaseChanged(m_RoomPhase);
                    OnChangePhase.Invoke(true, m_RoomPhase, m_SelectedFloorObject);
                }
                else if(m_RoomPhase == RoomPhase.EmptySpace)
                {
                    m_RoomPhase = RoomPhase.Detail;
                    //EmptySpaceの選択解除
                    m_SelectedSpace.Selected = false;
                    m_SelectedSpace = null;
                    BoxCollider collider = m_SelectedObject.GetComponent<BoxCollider>();
                    if (collider != null)
                    {
                        collider.enabled = true;
                    }
                    OnChangePhase.Invoke(false, m_RoomPhase, m_SelectedObject);
                }
            });

        m_RoomUI.DeleteButton
            .onClick
            .AddListener(() => 
            {
                RemoveFamily(m_SelectedObject);
            });

        m_RoomUI.RemoveAllButton
            .onClick
            .AddListener(() =>
            {
                if(m_RoomPhase == RoomPhase.None || m_RoomPhase == RoomPhase.Selected)
                {
                    RemoveAll();
                    OnRemoveAll.Invoke();
                }
            });

        m_RoomUI.RotateButton
            .onClick
            .AddListener(() =>
            {
                RotateObject(m_SelectedObject, m_SelectedFloorObject, m_SelectedSpace != null ? m_SelectedSpace.EmptySpace : null);
            });

        m_RoomUI.OnPut += (data) =>
        {
            PutType putType;

            if (data.PosType == PositionType.FLOOR)
            {
                putType = PutType.NORMAL;
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

            if(m_SelectedSpace != null)
            {
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
                if(m_SelectedObject.Data.IsPlane)
                {
                    //中に空間があればそこも探して入れる
                    int randSpaceId = UnityEngine.Random.Range(0, m_SelectedObject.EmptySpaceBehaviourList.Count);
                    i = -1;
                    for(int cnt = 0; cnt < m_SelectedObject.EmptySpaceBehaviourList.Count; cnt++)
                    {
                        EmptySpace randSpace = m_SelectedObject.EmptySpaceBehaviourList[randSpaceId].EmptySpace;
                        if(randSpace.Id != -1)
                        {
                            i = SelectIndex(data, ref putType, m_SelectedObject, randSpace);
                        }
                        else
                        {
                            int tempHeight = GetHeight(m_SelectedObject.RoomIndex) + m_SelectedObject.Height;
                            int areaIdx = GetIndex(tempHeight, GetDepth(m_SelectedObject.RoomIndex), GetWidth(m_SelectedObject.RoomIndex));
                            i = SelectIndex(data, ref putType, areaIdx, m_RoomMasterData.RoomHeight - tempHeight, m_SelectedObject.Depth, m_SelectedObject.Width);
                        }
                        if (i != -1) break;

                        randSpaceId++;
                        randSpaceId %= m_SelectedObject.EmptySpaceBehaviourList.Count;
                    }

                    if (i == -1)
                    {
                        i = SelectIndex(data, ref putType);
                    }
                }
                else
                {
                    i = SelectIndex(data, ref putType);
                }
            }
            else
            {
                i = SelectIndex(data, ref putType);
            }

            if(i != -1)
            {
                Put(i, data, putType);
            }
            else
            {
                Debug.Log("ここにはおけません");
            }

            m_Cnt += 1;
            m_Cnt %= 4;
        };
        #endregion

        #region ROOM_EVENT

        #region ON_IS_CONTROLLING
        OnIsControlling += (isControlling, controlObject) =>
        {
            m_FloorSelectingPlane.SetActive(isControlling);

            //新しくコントロールを開始するとき
            //StartControlに移す？
            if (isControlling == true && controlObject != null)
            {
                m_IsControlling = isControlling;
                ControlObject = controlObject;
                m_FloorSelectingPlane.SetActive(isControlling);

                SetState(controlObject, false);
                
                m_RoomUI.ChangeCanvas(RoomPhase.White);

                int index = controlObject.RoomIndex;
                int height = GetHeight(index);
                int depth = GetDepth(index);
                int width = GetWidth(index);

                int objHeight = controlObject.Data.GetObjHeight(controlObject.PutType);
                int objDepth = controlObject.Data.GetObjDepth(controlObject.PutType);
                int objWidth = controlObject.Data.GetObjWidth(controlObject.PutType);

                Vector3 planePos = new Vector3();

                if(controlObject.Data.PosType == PositionType.FLOOR)
                {
                    planePos = m_RoomMasterData.RoomUnit * new Vector3(width + (float)objWidth / 2f, height + 1f, -depth - (float)objDepth / 2f);
                    m_FloorSelectingPlane.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
                    m_FloorSelectingPlane.transform.localScale = m_RoomMasterData.RoomUnit * new Vector3(objWidth * 0.1f, 1f, objDepth * 0.1f);
                    m_FloorSelectingPlane.transform.position = planePos;
                }
                else if(controlObject.Data.PosType == PositionType.WALL) 
                {
                    if(controlObject.PutType == PutType.NORMAL)
                    {
                        planePos = m_RoomMasterData.RoomUnit * new Vector3(width + 1f, height + (float)objHeight / 2f, -depth - (float)objDepth / 2f);
                        m_FloorSelectingPlane.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, -90f));
                        m_FloorSelectingPlane.transform.localScale = m_RoomMasterData.RoomUnit * new Vector3(objHeight * 0.1f, 1f, objDepth * 0.1f);
                        m_FloorSelectingPlane.transform.position = planePos;
                    }
                    else if(controlObject.PutType == PutType.REVERSE)
                    {
                        planePos = m_RoomMasterData.RoomUnit * new Vector3(width + (float)objWidth / 2f, height + (float)objHeight / 2f, -depth - 1f);
                        m_FloorSelectingPlane.transform.rotation = Quaternion.Euler(new Vector3(-90f, 0f, 0f));
                        m_FloorSelectingPlane.transform.localScale = m_RoomMasterData.RoomUnit * new Vector3(objWidth * 0.1f, 1f, objHeight * 0.1f);
                        m_FloorSelectingPlane.transform.position = planePos;
                    }
                }

                Vector3 mousePos = Input.mousePosition;
                Camera mainCamera = Camera.main;
                Transform cameraTransform = mainCamera.transform;
                Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 5f));
                int layerMask = 1 << 6;
                Ray floorRay = new Ray(cameraTransform.position, mouseWorldPos - cameraTransform.position);
                float roomUnit = m_RoomMasterData.RoomUnit;

                if (Physics.Raycast(floorRay, out RaycastHit hit, Mathf.Infinity, layerMask))
                {
                    Vector3Int hitPos = GetHitPosition(controlObject, hit);
                    int hitHeight = hitPos.y;
                    int hitDepth = hitPos.z;
                    int hitWidth = hitPos.x;
                    m_ControlHitBefPos = new Vector3Int(hitWidth, hitHeight, hitDepth);
                    m_ControlHitOffSet = m_ControlHitBefPos - new Vector3Int(width, height, depth);
                }
            }
            else if(isControlling == false && controlObject != null) //設置するとき
            {
                Vector3 mousePos = Input.mousePosition;
                Camera mainCamera = Camera.main;
                Transform cameraTransform = mainCamera.transform;
                Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 5f));
                int layerMask = 1 << 6;
                Ray floorRay = new Ray(cameraTransform.position, mouseWorldPos - cameraTransform.position);
                float roomUnit = m_RoomMasterData.RoomUnit;

                //ここから置き場所の選定 -> おけなければ近くに置くって感じに
                if (Physics.Raycast(floorRay, out RaycastHit hit, Mathf.Infinity, layerMask))
                {
                    Vector3Int hitPos = GetHitPosition(controlObject, hit);
                    int hitHeight = hitPos.y;
                    int hitDepth = hitPos.z;
                    int hitWidth = hitPos.x;

                    //置き場所を範囲内に修正
                    int height = hitHeight - m_ControlHitOffSet.y;
                    int depth = hitDepth - m_ControlHitOffSet.z;
                    int width = hitWidth - m_ControlHitOffSet.x;

                    height = Mathf.Max(0, height);
                    height = Mathf.Min(m_RoomHeight - 1, height);
                    depth = Mathf.Max(0, depth);
                    depth = Mathf.Min(m_RoomDepth - 1, depth);
                    width = Mathf.Max(0, width);
                    width = Mathf.Min(m_RoomWidth - 1, width);

                    if (m_SelectedFloorObject != null && m_SelectedSpace != null)
                    {
                        int emptyIdx = GetIndexFromEmptySpace(m_SelectedFloorObject, m_SelectedSpace.EmptySpace);
                        int emptyMinHeight = GetHeight(emptyIdx);
                        int emptyMinDepth = GetDepth(emptyIdx);
                        int emptyMinWidth = GetWidth(emptyIdx);

                        int emptyMaxHeight = emptyMinHeight + GetEmptySpaceHeight(m_SelectedSpace.EmptySpace, m_SelectedFloorObject.PutType);
                        int emptyMaxDepth = emptyMinDepth + GetEmptySpaceDepth(m_SelectedSpace.EmptySpace, m_SelectedFloorObject.PutType);
                        int emptyMaxWidth = emptyMinWidth + GetEmptySpaceWidth(m_SelectedSpace.EmptySpace, m_SelectedFloorObject.PutType);

                        if(m_SelectedSpace.Id == -1)
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

                    int newIdx = GetIndex(height, depth, width);
                    int candIdx = GetCandidateIndex(ControlObject, newIdx, controlObject.PutType, m_SelectedFloorObject, m_SelectedSpace != null ? m_SelectedSpace.EmptySpace : null);
                   
                    if(candIdx != -1) //おける場合
                    {
                        controlObject.SetRoomIndex(candIdx);

                        //もともとおいていたはずなのでおけないことはないはず
                        SetState(ControlObject, true);
                        m_IsControlling = isControlling;

                        //ここでRoomObjectに対してイベント発火
                        ControlObject = null;
                        m_RoomUI.ChangeCanvas(RoomPhase.Selected);
                    }
                    else //どこにも置けない場合
                    {
                        Debug.Log("-1です");
                    }
                }
            }
            else
            {
                Debug.LogError("ControlObjectを渡してください");
            }
        };
        #endregion


        #region ON_SELECTED_OBJECT
        OnSelectedObject += (isSelected, selectedObject) =>
        {
            m_CanTouch = false;
            
            if(isSelected)
            {
                m_RoomPhase = RoomPhase.Selected;
                selectedObject.OnPhaseChanged(RoomPhase.Selected);

                //元のオブジェクトは選択解除
                if (m_SelectedObject != null)
                {
                    m_SelectedObject.OnPhaseChanged(RoomPhase.None);
                }
                m_SelectedObject = selectedObject;
            }
            else
            {
                if (m_SelectedObject != null)
                {
                    m_SelectedObject.OnPhaseChanged(RoomPhase.None);
                }

                if(m_SelectedFloorObject == null)
                {
                    m_RoomPhase = RoomPhase.None;
                    m_SelectedObject = selectedObject;
                }
                else
                {
                    m_RoomPhase = RoomPhase.EmptySpace;
                    m_SelectedObject = m_SelectedFloorObject;
                }
            }

            OnChangePhase.Invoke(false, m_RoomPhase, null);
            
            m_CanTouch = true;
        };
        #endregion

        #region ON_PUT_OBJECT
        
        OnPutObject += () =>
        {
            for (int idx = 0; idx < m_SpaceCount; idx++)
            {
                int x = GetWidth(idx);
                int y = GetHeight(idx);
                int z = GetDepth(idx);
                CalcStateSum(GetHeight(idx), GetDepth(idx), GetWidth(idx));
            }
        };

        #endregion

        #region ON_REMOVE_OBJECT

        OnRemoveObjectFamily += () =>
        {
            OnSelectedObject.Invoke(false, null);
            
            *//*for (int idx = 0; idx < m_SpaceCount; idx++)
            {
                int x = GetWidth(idx);
                int y = GetHeight(idx);
                int z = GetDepth(idx);
                CalcStateSum(GetHeight(idx), GetDepth(idx), GetWidth(idx));
            }*//*
        };

        #endregion

        #region ON_REMOVE_ALL
        OnRemoveAll += () =>
        {
            m_RoomUI.ChangeCanvas(RoomPhase.None);
        };
        #endregion

        #endregion
    }

    private void Update()
    {
        if (m_CanTouch)
        {
            if(m_RoomPhase == RoomPhase.None || m_RoomPhase == RoomPhase.Selected || m_RoomPhase == RoomPhase.EmptySpace)
            {
                #region START_SELECT
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (!m_IsWhileSelect && !m_IsControlling && Input.GetMouseButtonDown(0))
                {
                    //EmptySpaceに当たらないようにする
                    int layerMask = 1 << 0;
                    if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
                    {
                        RoomObject roomObject = hit.transform.gameObject.GetComponent<RoomObject>();
                        
                        if(roomObject != null && m_SelectedFloorObject == null)
                        {
                            m_IsWhileSelect = true;
                            m_SelectingTime = 0f;
                            m_SelectStartPos = Input.mousePosition;
                            m_SuspectObject = roomObject;
                        }
                        else if (roomObject != null && m_SelectedFloorObject != null && GetIsInEmptySpace(roomObject, m_SelectedFloorObject, m_SelectedSpace.EmptySpace))
                        {
                            Transform parentTransform = roomObject.transform.parent;

                            //棚の上にあるものかチェック
                            bool isOnSelectedObject = false;

                            while (parentTransform != null)
                            {
                                RoomObject parentRoomObject = parentTransform.gameObject.GetComponent<RoomObject>();

                                if (parentRoomObject == m_SelectedFloorObject)
                                {
                                    isOnSelectedObject = true;
                                    break;
                                }
                                parentTransform = parentTransform.parent;
                            }

                            if (isOnSelectedObject)
                            {
                                m_IsWhileSelect = true;
                                m_SelectingTime = 0f;
                                m_SelectStartPos = Input.mousePosition;
                                m_SuspectObject = roomObject;
                            }
                        }
                    }
                }
                #endregion

                #region WHILE_SELECT
                if (m_IsWhileSelect && !m_IsControlling)
                {
                    if(Vector2.Distance(m_SelectStartPos, Input.mousePosition) > 30f)
                    {
                        m_IsWhileSelect = false;
                    }
                    else
                    {
                        m_SelectingTime += Time.deltaTime;
                        if(m_SelectingTime > m_ControlTime)
                        {
                            //選択完了 + 操作開始
                            m_IsWhileSelect = false;
                            if (m_SelectedObject == null || m_SuspectObject != m_SelectedObject)
                            {
                                OnSelectedObject.Invoke(true, m_SuspectObject);
                            }

                            //操作可能な状態にする
                            OnIsControlling.Invoke(true, m_SelectedObject);
                        }

                        if (Input.GetMouseButtonUp(0))
                        {
                            m_IsWhileSelect = false;

                            if (m_SelectedObject == null || m_SuspectObject != m_SelectedObject)
                            {
                                OnSelectedObject.Invoke(true, m_SuspectObject);
                            }
                            else //同じオブジェクトをタップしたとき -> 選択前の状態になる
                            {
                                ControlObject = null;
                                OnSelectedObject.Invoke(false, null);
                            }
                        }
                    }
                }
                #endregion

                #region CONTROLLING
                if (m_IsControlling)
                {
                    //いったんデバッグ用解除
                    if (Input.GetMouseButtonUp(0))
                    {
                        OnIsControlling.Invoke(false, ControlObject);
                    }
                    else
                    {
                        //実際に操作する処理
                        if(m_SelectedFloorObject != null && m_SelectedSpace != null)
                        {
                            MoveRoomObject(m_SelectedObject, m_SelectedFloorObject, m_SelectedSpace.EmptySpace);
                        }
                        else 
                        {
                            MoveControlObject();
                        }
                    }

                }
                #endregion
            }

            if(m_RoomPhase == RoomPhase.Detail || m_RoomPhase == RoomPhase.EmptySpace || m_RoomPhase == RoomPhase.Selected)
            {
                #region SELECT_EMPTY_SPACE
                //EmptySpaceの選択が可能になっている
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (m_SelectedFloorObject != null && !m_IsWhileSelect && !m_IsControlling && Input.GetMouseButtonDown(0))
                {
                    //EmptySpaceレイヤーのみを選択
                    int layerMask = (1 << 7);
                    if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
                    {
                        EmptySpaceBehaviour emptySpaceBehaviour = hit.collider.gameObject.GetComponent<EmptySpaceBehaviour>();

                        if (emptySpaceBehaviour != null)
                        {
                            //m_SelectedObjectのEmptySpaceではない場合は選択できないようにする
                            //同じEmptySpaceBehaviourだったときも同様
                            if (emptySpaceBehaviour != m_SelectedSpace && GetIsEmptySpaceInRoomObject(emptySpaceBehaviour, m_SelectedFloorObject))
                            {
                                //EmptySpaceの情報に到達する
                                //おける場所を制限する
                                if (m_RoomPhase == RoomPhase.Selected)
                                {
                                    //Select中だった場合は強制的に解除
                                    OnSelectedObject.Invoke(false, null);
                                }

                                EmptySpace emptySpace = emptySpaceBehaviour.EmptySpace;
                                m_RoomPhase = RoomPhase.EmptySpace;

                                //GetComonent使ってる 要修正
                                BoxCollider collider = m_SelectedObject.GetComponent<BoxCollider>();
                                if(collider != null)
                                {
                                    collider.enabled = false;
                                }

                                OnChangePhase.Invoke(false, m_RoomPhase, null);

                                if(m_SelectedSpace != null)
                                {
                                    m_SelectedSpace.Selected = false;
                                }

                                *//*if(emptySpaceBehaviour.Id == -1)
                                {
                                    EmptySpace tempSpace = emptySpaceBehaviour.EmptySpace;
                                    float tempWidth = tempSpace.GetSize(m_SelectedFloorObject.PutType).x;
                                    float tempHeight = m_RoomMasterData.RoomUnit * (m_RoomHeight - (GetHeight(m_SelectedFloorObject.RoomIndex) + m_SelectedFloorObject.Height));
                                    float tempDepth = tempSpace.GetSize(m_SelectedFloorObject.PutType).z;
                                    tempSpace.SetSize(new Vector3(tempWidth, tempHeight, tempDepth), m_SelectedFloorObject.PutType);
                                }*//*
                                
                                m_SelectedSpace = emptySpaceBehaviour;
                                emptySpaceBehaviour.Selected = true;
                            }
                        }
                    }
                }
                #endregion
            }
        }
    }

    private Vector3Int GetHitPosition(RoomObject roomObject, RaycastHit hit)
    {
        int hitHeight = 0;
        int hitDepth = 0;
        int hitWidth = 0;

        if (roomObject.Data.PosType == PositionType.FLOOR)
        {
            hitHeight = 0;
            hitDepth = (int)(-hit.point.z / m_RoomMasterData.RoomUnit);
            hitWidth = (int)(hit.point.x / m_RoomMasterData.RoomUnit);
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

    private bool GetIsEmptySpaceInRoomObject(EmptySpaceBehaviour emptySpaceBehaviour, RoomObject roomObject)
    {
        foreach(var behav in roomObject.EmptySpaceBehaviourList)
        {
            if(emptySpaceBehaviour == behav)
            {
                return true;
            }
        }

        return false;
    }

    private int GetIndexFromEmptySpace(RoomObject floorObject, EmptySpace emptySpace)
    {
        int tempIdx = floorObject.RoomIndex;
        int floorHeight = GetHeight(tempIdx);
        int floorDepth = GetDepth(tempIdx);
        int floorWidth = GetWidth(tempIdx);

        int spaceMinHeight = floorHeight + (int)(emptySpace.GetCenter(floorObject.PutType).y / m_RoomMasterData.RoomUnit);
        int spaceMinDepth = floorDepth + floorObject.Depth / 2 + (int)((emptySpace.GetCenter(floorObject.PutType).z - emptySpace.GetSize(floorObject.PutType).z / 2f) / m_RoomMasterData.RoomUnit);
        int spaceMinWidth = floorWidth + floorObject.Width / 2 + (int)((emptySpace.GetCenter(floorObject.PutType).x - emptySpace.GetSize(floorObject.PutType).x / 2f) / m_RoomMasterData.RoomUnit);
        int idx = GetIndex(spaceMinHeight, spaceMinDepth, spaceMinWidth);

        return idx;
    }

    private bool GetIsInEmptySpace(RoomObject roomObject, RoomObject floorObject, EmptySpace emptySpace)
    {
        if(floorObject == null || emptySpace == null)
        {
            //部屋におくということなので、trueを返す
            return true;
        }

        int floorIdx = GetIndexFromEmptySpace(floorObject, emptySpace);
        int floorMinHeight = GetHeight(floorIdx);
        int floorMinDepth = GetDepth(floorIdx);
        int floorMinWidth = GetWidth(floorIdx);
        int floorMaxHeight = floorMinHeight + (int)(emptySpace.GetSize(floorObject.PutType).y / m_RoomMasterData.RoomUnit);
        int floorMaxDepth = floorMinDepth + (int)(emptySpace.GetSize(floorObject.PutType).z / m_RoomMasterData.RoomUnit);
        int floorMaxWidth = floorMinWidth + (int)(emptySpace.GetSize(floorObject.PutType).x / m_RoomMasterData.RoomUnit);

        //一番上の時は例外として扱う
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

        if(minHeight < floorMinHeight || minDepth < floorMinDepth || minWidth < floorMinWidth)
        {
            return false;
        }

        if(maxHeight > floorMaxHeight || maxDepth > floorMaxDepth || maxWidth > floorMaxWidth)
        {
            return false;
        }

        return true;
    }

    public void OnCameraMoveComplete(ViewType viewType)
    {
        OnChangePhase.Invoke(false, m_RoomPhase, null);
        if (!m_CanTouch)
        {
            m_CanTouch = true;
        }
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

            if(roomObject.PutType == PutType.NORMAL)
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
            else if(roomObject.PutType == PutType.REVERSE)
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

        //おけない時は -1 を返す
        return -1;

    }

    //RoomIndexが定まっていないオブジェクトに対し、候補を提示する関数
    private int GetCandidateIndex(RoomObject roomObject, int index, PutType putType)
    {
        if(roomObject.Data.PosType == PositionType.WALL)
        {
            return GetCandidateIndexWall(roomObject, index, putType);
        }

        int height = GetHeight(index);
        int width = GetWidth(index);
        int depth = GetDepth(index);

        //近くにおける場所を探しておく
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

            for (int tempHeight = 0; tempHeight < m_RoomHeight; tempHeight++)
            {
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

        //おけない時は -1 を返す
        return -1;
    }

    private int GetCandidateIndex(RoomObject roomObject, int index, PutType putType, RoomObject floorObject, EmptySpace emptySpace)
    {
        if(floorObject == null || emptySpace == null || roomObject.Data.PosType == PositionType.WALL)
        {
            return GetCandidateIndex(roomObject, index, putType);
        }

        int width = GetWidth(index);
        int depth = GetDepth(index);

        //近くにおける場所を探しておく
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

        if(emptySpace.Id == -1)
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

        //おけない時は -1 を返す
        return -1;
    }

    private void MoveRoomObject(RoomObject roomObject, RoomObject floorObject, EmptySpace emptySpace)
    {
        int spaceIdx;
        int spaceHeight;
        int spaceDepth;
        int spaceWidth;

        if(floorObject == null || emptySpace == null)
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
            if(emptySpace.Id == -1)
            {
                spaceIdx = GetIndex(GetHeight(floorObject.RoomIndex) + floorObject.Height, GetDepth(floorObject.RoomIndex), GetWidth(floorObject.RoomIndex));
                spaceHeight = m_RoomHeight - (GetHeight(floorObject.RoomIndex) + floorObject.Height);
                spaceDepth = floorObject.Depth;
                spaceWidth = floorObject.Width;
            }
        }

        //グリッドを表示してみる
        Camera mainCamera = Camera.main;
        Transform cameraTransform = mainCamera.transform;
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 2f));

        //床との衝突だとよくないかも？
        int layerMask = 1 << 6;
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

            //外に出た時の処理は特別
            //壁の外側 -> 壁に沿って動かすようにする
            //壁がない側の外 -> いけるが置けないという感じにする 外に置いたら捨てる？

            height = Mathf.Min(maxSpaceHeight - 1, Mathf.Max(minSpaceHeight, height));
            depth = Mathf.Min(maxSpaceDepth - 1, Mathf.Max(minSpaceDepth, depth));
            width = Mathf.Min(maxSpaceWidth - 1, Mathf.Max(minSpaceWidth, width));
            int currentIdx = GetIndex(height, depth, width);
            int candIndex = GetCandidateIndex(roomObject, currentIdx, roomObject.PutType, floorObject, emptySpace);
            int candHeight = GetHeight(candIndex);
            int candDepth = GetDepth(candIndex);
            int candWidth = GetWidth(candIndex);

            if(roomObject.Data.PosType == PositionType.FLOOR)
            {
                roomObject.transform.position = roomUnit * new Vector3(width + (float)objWidth / 2f, candHeight + 30f * spaceHeight / m_RoomHeight, -depth - (float)objDepth / 2f);
                roomObject.transform.position -= Vector3.Dot(m_ControlObject.Data.ColliderCenter, Vector3.right) * Vector3.right + Vector3.Dot(m_ControlObject.Data.ColliderCenter, Vector3.forward) * Vector3.forward;

                m_FloorSelectingPlane.transform.position = roomUnit * new Vector3(width + (float)objWidth / 2f, candHeight + 1f, -depth - (float)objDepth / 2f);
                Vector3 angles = new Vector3(0f, 0f, 0f);
                m_FloorSelectingPlane.transform.rotation = Quaternion.Euler(angles);
            }
            else if(roomObject.Data.PosType == PositionType.WALL)
            {
                if(roomObject.PutType == PutType.NORMAL)
                {
                    roomObject.transform.position = roomUnit * new Vector3(candWidth + 30f * spaceWidth / m_RoomWidth, height + (float)objHeight / 2f, -depth - (float)objDepth / 2f);
                    roomObject.transform.position -= Vector3.Dot(m_ControlObject.Data.ColliderCenter, Vector3.forward) * Vector3.up + Vector3.Dot(m_ControlObject.Data.ColliderCenter, Vector3.right) * Vector3.forward;
                    //ここ回転させる

                    m_FloorSelectingPlane.transform.position = roomUnit * new Vector3(candWidth + 1f, height + (float)objHeight / 2f, -depth - (float)objDepth / 2f);
                    Vector3 angles = new Vector3(0f, 0f, -90f);
                    m_FloorSelectingPlane.transform.rotation = Quaternion.Euler(angles);
                }
                else if(roomObject.PutType == PutType.REVERSE)
                {
                    roomObject.transform.position = roomUnit * new Vector3(width + (float)objWidth / 2f, height + (float)objHeight / 2f, -candDepth - 30f * spaceDepth / m_RoomDepth);
                    roomObject.transform.position -= Vector3.Dot(m_ControlObject.Data.ColliderCenter, Vector3.forward) * Vector3.up + Vector3.Dot(m_ControlObject.Data.ColliderCenter, Vector3.right) * Vector3.right;
                    //ここ回転させる

                    m_FloorSelectingPlane.transform.position = roomUnit * new Vector3(width + (float)objWidth / 2f, height + (float)objHeight / 2f, -candDepth - 1f);
                    Vector3 angles = new Vector3(-90f, 0f, 0f);
                    m_FloorSelectingPlane.transform.rotation = Quaternion.Euler(angles);
                }
            }

            m_ControlHitBefPos = new Vector3Int(hitWidth, hitHeight, hitDepth);

            if(roomObject.Data.PosType == PositionType.FLOOR)
            {
                if (depth == candDepth && width == candWidth)
                {
                    m_SelectingRenderer.material = m_CanPutMaterial;
                }
                else
                {
                    m_SelectingRenderer.material = m_NotPutMaterial;
                }
            }
            else if(roomObject.Data.PosType == PositionType.WALL)
            {
                if(roomObject.PutType == PutType.NORMAL)
                {
                    if (height == candHeight && depth == candDepth)
                    {
                        m_SelectingRenderer.material = m_CanPutMaterial;
                    }
                    else
                    {
                        m_SelectingRenderer.material = m_NotPutMaterial;
                    }
                }
                else if(roomObject.PutType == PutType.REVERSE)
                {
                    if (height == candHeight && width == candWidth)
                    {
                        m_SelectingRenderer.material = m_CanPutMaterial;
                    }
                    else
                    {
                        m_SelectingRenderer.material = m_NotPutMaterial;
                    }
                }
            }
        }
    }

    private void MoveControlObject()
    {
        MoveRoomObject(m_ControlObject, null, null);
    }

    private void RotateObject(RoomObject roomObject, RoomObject floorObject, EmptySpace emptySpace)
    {
        if(roomObject.Data.PosType == PositionType.WALL)
        {
            Debug.Log("壁のオブジェクトは回転しません");
            return;
        }
        if(floorObject == null || emptySpace == null)
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

            //置き場所があった場合
            //瞬間移動を抑えるための条件分岐を入れておく
            if (nextIndex != -1 && distDepth * 2 < roomObject.Depth + nextObjDepth && distWidth * 2 < roomObject.Width + nextObjWidth)
            {
                //回転
                roomObject.SetPutType(nextPutType, nextIndex, i);

                int objHeight = roomObject.Height;
                int objDepth = roomObject.Depth;
                int objWidth = roomObject.Width;

                //ここはRoomObjectに移動したほうがいい気がする
                m_SelectedObject.transform.position = m_RoomMasterData.RoomUnit * new Vector3(nextWidth + (float)objWidth / 2f, nextHeight, -nextDepth - (float)objDepth / 2f);

                Vector3 rotationAngles = m_SelectedObject.transform.localRotation.eulerAngles;
                rotationAngles.y = rotationAngles.y + 90.0f * i;
                m_SelectedObject.transform.localRotation = Quaternion.Euler(rotationAngles);
                canRotate = true;
                break;
            }
        }

        if (!canRotate)
        {
            Debug.Log("回転できません");
        }

        SetState(roomObject, true);
    }

    private void RotateObject(RoomObject roomObject)
    {
        SetState(roomObject, false);
        bool canRotate = false;

        for (int i = 1; i < Enum.GetValues(typeof(PutType)).Length * 2; i++)
        {
            Debug.Log("回転数: " + i);
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

            Debug.Log("rotate x, y, z: " + nextWidth + " " + nextHeight + " " + nextDepth);

            Vector3Int nextCenter = new Vector3Int(nextWidth, nextHeight, nextDepth) + roomObject.Data.GetCenter(nextPutType);
            Vector3Int tempCenter = new Vector3Int(tempWidth, tempHeight, tempDepth) + roomObject.Center;

            int distHeight = Mathf.Abs(nextCenter.y - tempCenter.y);
            int distDepth = Mathf.Abs(nextCenter.z - tempCenter.z);
            int distWidth = Mathf.Abs(nextCenter.x - tempCenter.x);

            //置き場所があった場合
            if (nextIndex != -1 && distDepth * 2 < roomObject.Depth + nextObjDepth && distWidth * 2 < roomObject.Width + nextObjWidth)
            {
                //回転
                roomObject.SetPutType(nextPutType, nextIndex, i);

                int objHeight = roomObject.Height;
                int objDepth = roomObject.Depth;
                int objWidth = roomObject.Width;

                //ここはRoomObjectに移動したほうがいい気がする
                m_SelectedObject.transform.position = m_RoomMasterData.RoomUnit * new Vector3(nextWidth + (float)objWidth / 2f, nextHeight, -nextDepth - (float)objDepth / 2f);

                Vector3 rotationAngles = m_SelectedObject.transform.localRotation.eulerAngles;
                rotationAngles.y = rotationAngles.y + 90.0f * i;
                m_SelectedObject.transform.localRotation = Quaternion.Euler(rotationAngles);
                canRotate = true;
                break;
            }
        }

        if(!canRotate)
        {
            Debug.Log("回転できません");
        }
        
        SetState(roomObject, true);
    }

    private int SelectIndex(RoomObjectData data, ref PutType putType, RoomObject floorObject, EmptySpace emptySpace)
    {
        if(floorObject == null || emptySpace == null)
        {
            return SelectIndex(data, ref putType);
        }
        int index = -1;

        if(data.PosType == PositionType.FLOOR)
        {
            index = SelectIndexFloor(data, putType, floorObject, emptySpace);

            if (index == -1)
            {
                putType = PutType.REVERSE;
                index = SelectIndexFloor(data, putType, floorObject, emptySpace);
            }

            return index;
        }
        else
        {
            return index;
        }
    }

    private int SelectIndex(RoomObjectData data, ref PutType putType, int areaIdx,  int areaHeight, int areaDepth, int areaWidth)
    {
        int index = -1;

        if(data.PosType == PositionType.FLOOR)
        {
            return SelectIndexFloor(data, putType, areaIdx, areaHeight, areaDepth, areaWidth);
        }
        else
        {
            return SelectIndex(data, ref putType);
        }

        return index;
    }

    //おける場所のIndexを返す関数
    //どこにも置けない場合は-1を返す
    //可能性を絞ってからGetCanPutに渡す
    //とおもったが、中空のオブジェクトなどで管理が面倒なので全探索にしてもいいかも
    private int SelectIndex(RoomObjectData data, ref PutType putType)
    {
        int index = -1;

        #region FLOOR
        if (data.PosType == PositionType.FLOOR)
        {
            index = SelectIndexFloor(data, putType);

            if (index == -1)
            {
                putType = PutType.REVERSE;
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
                //まず左壁におけるか探す
                index = SelectIndexWall(data, putType);

                //場所がなければ右壁を探す
                if(index == -1)
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

                if(index == -1)
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
                    //今のIndexを求める
                    int areaMinHeight = GetHeight(areaIdx); //おけるのは一番上の場所より上
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

                    //int tempHeight = m_FloorHeights[tempDepth * m_RoomWidth + tempWidth];
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
                for(int emptyWidth = 0; emptyWidth < emptyMaxWidth; emptyWidth++)
                {
                    //今のIndexを求める
                    int spaceMinHeight = GetHeight(spaceMinIdx);
                    int spaceMinDepth = GetDepth(spaceMinIdx);
                    int spaceMinWidth  = GetWidth(spaceMinIdx);

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

                    //int tempHeight = m_FloorHeights[tempDepth * m_RoomWidth + tempWidth];
                    int tempHeight = spaceMinHeight + emptyHeight;

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
            //ランダム性を付加する処理
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

    //TODO: 中空に対応してません
    private int SelectIndexWall(RoomObjectData data, PutType putType)
    {
        #region LEFTWALL
        if (putType == PutType.NORMAL)
        {
            for(int width = 0; width < m_RoomWidth; width++)
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

    private void Put(int index, RoomObjectData obj, PutType putType)
    {
        int objHeight = obj.GetObjHeight(putType);
        int objDepth = obj.GetObjDepth(putType);
        int objWidth = obj.GetObjWidth(putType);
        RoomObject model = obj.Model;

        //先に下が何か判定
        //またがり判定は4隅からRayを出せばいける

        int height = GetHeight(index);
        int depth = GetDepth(index);
        int width = GetWidth(index);

        RoomObject roomGameObject;

        if(obj.PosType == PositionType.FLOOR)
        {
            roomGameObject = Instantiate(model);
            roomGameObject.transform.position = m_RoomMasterData.RoomUnit * new Vector3(width + (float)objWidth / 2f, height, -depth - (float)objDepth / 2f);
            roomGameObject.transform.position -= Vector3.Dot(obj.ColliderCenter, Vector3.right) * Vector3.right + Vector3.Dot(obj.ColliderCenter, Vector3.forward) * Vector3.forward;

            //何かに乗っているとき


            if (putType == PutType.NORMAL)
            {
                roomGameObject.transform.LookAt(roomGameObject.transform.position + Vector3.forward);
            } 
            else if(putType == PutType.REVERSE)
            {
                roomGameObject.transform.LookAt(roomGameObject.transform.position + Vector3.right);
            }
            
            *//*for(int i = 0; i < objWidth; i++)
            {
                for (int j = 0; j < objDepth; j++)
                {
                    m_FloorHeights[(GetDepth(index) + j) * m_RoomWidth + GetWidth(index) + i] += objHeight;
                    if (obj.IsPlane)
                    {
                        m_FloorStates[(GetDepth(index) + j) * m_RoomWidth + GetWidth(index) + i] = true;
                    }
                    else
                    {
                        m_FloorStates[(GetDepth(index) + j) * m_RoomWidth + GetWidth(index) + i] = false;
                    }
                }
            }*//*
        }
        else if(obj.PosType == PositionType.WALL)
        {

            //左壁
            if (putType == PutType.NORMAL)
            {
                roomGameObject = Instantiate(model, m_RoomMasterData.RoomUnit * new Vector3(width, height + (float)objHeight / 2f, -depth - (float)objDepth / 2f), Quaternion.identity); ;
                roomGameObject.transform.position -= Vector3.Dot(obj.ColliderCenter, Vector3.up) * Vector3.up + Vector3.Dot(obj.ColliderCenter, Vector3.forward) * Vector3.forward;

                roomGameObject.transform.rotation = Quaternion.LookRotation(Vector3.up, Vector3.right);
                if (width > 0)
                {
                    Ray ray = new Ray(roomGameObject.transform.position + Vector3.right * 0.1f, Vector3.left);
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        Debug.Log("On " + hit.transform.gameObject.name);
                        RoomObject parentObject = hit.transform.gameObject.GetComponent<RoomObject>();
                        if (parentObject != null)
                        {
                            roomGameObject.transform.SetParent(parentObject.transform);
                        }
                    }
                }

               *//* for (int i = 0; i < objHeight; i++)
                {
                    for (int j = 0; j < objDepth; j++)
                    {
                        m_WallWidthHeights[(GetHeight(index) + i) * m_RoomDepth + GetDepth(index) + j] += objWidth;
                        if (obj.IsPlane)
                        {
                            m_WallWidthStates[(GetHeight(index) + i) * m_RoomDepth + GetDepth(index) + j] = true;
                        }
                        else
                        {
                            m_WallWidthStates[(GetHeight(index) + i) * m_RoomDepth + GetDepth(index) + j] = false;
                        }

                    }
                }*//*
            }
            else if (putType == PutType.REVERSE)
            {
                roomGameObject = Instantiate(model, m_RoomMasterData.RoomUnit * new Vector3(width + (float)objDepth / 2f, height + (float)objHeight / 2f, depth), Quaternion.identity);
                roomGameObject.transform.position -= Vector3.Dot(obj.ColliderCenter, Vector3.right) * Vector3.right + Vector3.Dot(obj.ColliderCenter, Vector3.up) * Vector3.up;

                roomGameObject.transform.rotation = Quaternion.LookRotation(Vector3.up, Vector3.back);
                if (depth > 0)
                {
                    Ray ray = new Ray(roomGameObject.transform.position + Vector3.forward * 0.1f, Vector3.back);
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        Debug.Log("On " + hit.transform.gameObject.name);
                        RoomObject parentObject = hit.transform.gameObject.GetComponent<RoomObject>();
                        if (parentObject != null)
                        {
                            roomGameObject.transform.SetParent(parentObject.transform);
                        }
                    }
                }

               *//* for (int i = 0; i < objHeight; i++)
                {
                    for (int j = 0; j < objDepth; j++)
                    {
                        m_WallDepthHeights[(GetHeight(index) + i) * m_RoomWidth + GetWidth(index) + j] += objWidth;
                        if (obj.IsPlane)
                        {
                            m_WallDepthStates[(GetHeight(index) + i) * m_RoomWidth + GetWidth(index) + j] = true;
                        }
                        else
                        {
                            m_WallDepthStates[(GetHeight(index) + i) * m_RoomWidth + GetWidth(index) + j] = false;
                        }

                    }
                }*//*
            }
            else
            {
                roomGameObject = null;
            }
           
        } 
        else
        {
            roomGameObject = null;
        }

        if(roomGameObject != null)
        {
            roomGameObject.Init(obj, index, putType);
            SetState(roomGameObject, true);

            Debug.Log(obj.Model.name + "を置きました x y z: " + GetWidth(index) + " " + GetHeight(index) + " " + GetDepth(index));
            Debug.Log("大きさ: x, y, z: " + obj.GetObjWidth(putType) + " " + obj.GetObjHeight(putType) + " " + obj.GetObjDepth(putType));

            OnPutObject.Invoke();
            //OnSelectedObject.Invoke(true, roomGameObject);
        }
    }

    private void SetState(RoomObject roomObj, bool isPut)
    {
        Queue<RoomObject> tempObjQue = new Queue<RoomObject>();
        tempObjQue.Enqueue(roomObj);

        while(tempObjQue.Count > 0)
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
                        if(stateIdx == -1)
                        {
                            Debug.Log("is out of range: " + roomObj.gameObject.name + " " + stateWidth + " " + stateHeight + " " + stateDepth);
                        }
                        m_RoomStates[stateIdx] = isPut ? tempObj : null;
                    }
                }
            }

            //中空のオブジェクトの処理
            if (isPut)
            {
                float roomUnit = m_RoomMasterData.RoomUnit;

                if (tempData.EmptySpaces.Count > 0)
                {
                    //for (int id = 0; id < tempData.EmptySpaces.Count; id++)
                    foreach(var emptySpace in tempData.EmptySpaces)
                    {
                        if (emptySpace.Id == -1) continue;
                        int emptySpaceIndex = GetEmptySpaceIndex(tempIndex, tempData, emptySpace, tempPutType);
                        int spaceHeight = GetEmptySpaceHeight(emptySpace, tempPutType);
                        int spaceDepth = GetEmptySpaceDepth(emptySpace, tempPutType);
                        int spaceWidth = GetEmptySpaceWidth(emptySpace, tempPutType);

                        int spaceMinHeight = GetHeight(emptySpaceIndex);
                        int spaceMinDepth = GetDepth(emptySpaceIndex);
                        int spaceMinWidth = GetWidth(emptySpaceIndex);

                        *//*Debug.Log("空間の最大点 x, y, z " + (spaceMinWidth + spaceWidth) + " " + (spaceMinHeight + spaceHeight) + " " + (spaceMinDepth + spaceDepth));
                        Debug.Log("空間の大きさ x, y, z " + (spaceWidth) + " " + (spaceHeight) + " " + (spaceDepth));*//*

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

            for (int i = 0; i < tempObj.transform.childCount; i++)
            {
                RoomObject childObj = tempObj.transform.GetChild(i).gameObject.GetComponent<RoomObject>();
                if(childObj != null)
                {
                    tempObjQue.Enqueue(childObj);
                }
            }
        }

        //累積和の更新
        for (int idx = 0; idx < m_SpaceCount; idx++)
        {
            int x = GetWidth(idx);
            int y = GetHeight(idx);
            int z = GetDepth(idx);
            CalcStateSum(GetHeight(idx), GetDepth(idx), GetWidth(idx));
        }

        //FamilySizeの更新
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
            if (floorObject != null)
            {
                for (int i = 0; i < floorObject.transform.childCount; i++)
                {
                    RoomObject childObject = floorObject.transform.GetChild(i).GetComponent<RoomObject>();
                    if (childObject != null)
                    {
                        childMaxHeight = Mathf.Max(childMaxHeight, childObject.GetFamilySize(childObject.PutType).y);
                    }
                }
            }

            if (isPut)
            {
                roomObj.transform.SetParent(floorObject.transform);
                //roomObj.transform.localScale = Vector3.Scale(roomObj.transform.localScale, new Vector3(1f / floorObject.transform.localScale.x, 1f / floorObject.transform.localScale.y, 1f / floorObject.transform.localScale.z));
                roomObj.SetOriginScale(roomObj.transform.localScale);

                if (floorObject.GetFamilySize(floorObject.PutType).y >= height + roomObj.GetFamilySize(roomObj.PutType).y)
                {
                    return;
                }

                int offsetHeight = roomObj.GetFamilySize(roomObj.PutType).y - childMaxHeight;

                while (floorObject != null)

                {
                    floorObject.SetFamilySize(floorObject.GetFamilySize(floorObject.PutType) + offsetHeight * Vector3Int.up, floorObject.PutType);
                    if (floorObject.transform.parent != null)
                    {
                        floorObject = floorObject.transform.parent.gameObject.GetComponent<RoomObject>();
                    }
                    else
                    {
                        floorObject = null;
                    }
                }
            }
            else
            {
                roomObj.transform.parent = null;
                roomObj.SetOriginScale(roomObj.transform.localScale);

                if (childMaxHeight >= roomObj.GetFamilySize(roomObj.PutType).y)
                {
                    return;
                }

                int offsetHeight = childMaxHeight - roomObj.GetFamilySize(roomObj.PutType).y;

                while (floorObject != null)
                {
                    floorObject.SetFamilySize(floorObject.GetFamilySize(floorObject.PutType) + offsetHeight * Vector3Int.up, floorObject.PutType);
                    if (floorObject.transform.parent != null)
                    {
                        floorObject = floorObject.transform.parent.gameObject.GetComponent<RoomObject>();
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

    private void RemoveFamily(RoomObject roomObject)
    {
        Remove(roomObject);
        OnRemoveObjectFamily.Invoke();
    }
    
    private void Remove(RoomObject roomObject)
    {
        //床面修正処理
        int index = roomObject.RoomIndex;
        int height = GetHeight(index);
        int depth = GetDepth(index);
        int width = GetWidth(index);
        int objHeight = roomObject.Data.GetObjHeight(roomObject.PutType);
        int objDepth = roomObject.Data.GetObjDepth(roomObject.PutType);
        int objWidth = roomObject.Data.GetObjWidth(roomObject.PutType);

        *//*int childCount = roomObject.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            RoomObject childObject = roomObject.transform.GetChild(i).gameObject.GetComponent<RoomObject>();
            if (childObject != null)
            {
                Remove(childObject);
            }
        }*//*

        RoomObject parentObject = null;
        if (roomObject.transform.parent != null)
        {
            parentObject = roomObject.transform.parent.gameObject.GetComponent<RoomObject>();
        }

        SetState(roomObject, false);

        #region FLOOR
        *//*if (roomObject.Data.PosType == PositionType.FLOOR)
        {
            if (roomObject.PutType == PutType.NORMAL)
            {
                Debug.Log("ひかれた");
                for (int i = 0; i < objDepth; i++)
                {
                    for (int j = 0; j < objWidth; j++)
                    {
                        int tempDepth = depth + i;
                        int tempWidth = width + j;
                        m_FloorHeights[tempDepth * m_RoomWidth + tempWidth] -= objHeight;
                        if(parentObject != null)
                        {
                            if(parentObject.Data.IsPlane)
                            {
                                m_FloorStates[tempDepth * m_RoomWidth + tempWidth] = true;
                            }
                            else
                            {
                                m_FloorStates[tempDepth * m_RoomWidth + tempWidth] = false;
                            }
                        }
                        else
                        {
                            m_FloorStates[tempDepth * m_RoomWidth + tempWidth] = true;
                        }
                    }
                }
            }
            else if (roomObject.PutType == PutType.REVERSE)
            {
                for (int i = 0; i < objDepth; i++)
                {
                    for (int j = 0; j < objWidth; j++)
                    {
                        int tempDepth = depth + i;
                        int tempWidth = width + j;
                        m_FloorHeights[tempDepth * m_RoomWidth + tempWidth] -= objHeight;
                        if (parentObject != null)
                        {
                            if (parentObject.Data.IsPlane)
                            {
                                m_FloorStates[tempDepth * m_RoomWidth + tempWidth] = true;
                            }
                            else
                            {
                                m_FloorStates[tempDepth * m_RoomWidth + tempWidth] = false;
                            }
                        }
                        else
                        {
                            m_FloorStates[tempDepth * m_RoomWidth + tempWidth] = true;
                        }
                    }
                }
            }
        }*//*
        #endregion

        #region WALL
        *//*
        if (roomObject.Data.PosType == PositionType.WALL)
        {
            if (roomObject.PutType == PutType.NORMAL)
            {
                for (int i = 0; i < objHeight; i++)
                {
                    for (int j = 0; j < objDepth; j++)
                    {
                        int tempHeight = height + i;
                        int tempDepth = depth + j;
                        m_WallWidthHeights[tempHeight * m_RoomDepth + tempDepth] -= objWidth;

                        if (parentObject != null)
                        {
                            if (parentObject.Data.IsPlane)
                            {
                                m_WallWidthStates[tempHeight * m_RoomDepth + tempDepth] = true;
                            }
                            else
                            {
                                m_WallWidthStates[tempHeight * m_RoomDepth + tempDepth] = false;
                            }
                        }
                        else
                        {
                            m_WallWidthStates[tempHeight * m_RoomDepth + tempDepth] = true;
                        }
                    }
                }
            }
            else if (roomObject.PutType == PutType.REVERSE)
            {
                for (int i = 0; i < objHeight; i++)
                {
                    for (int j = 0; j < objDepth; j++)
                    {
                        int tempHeight = height + i;
                        int tempWidth = width + j;
                        m_WallDepthHeights[tempHeight * m_RoomWidth + tempWidth] -= objWidth;

                        if (parentObject != null)
                        {
                            if (parentObject.Data.IsPlane)
                            {
                                m_WallDepthStates[tempHeight * m_RoomWidth + tempWidth] = true;
                            }
                            else
                            {
                                m_WallDepthStates[tempHeight * m_RoomWidth + tempWidth] = false;
                            }
                        }
                        else
                        {
                            m_WallDepthStates[tempHeight * m_RoomWidth + tempWidth] = true;
                        }
                    }
                }
            }
        }
        *//*
        #endregion

        Destroy(roomObject.gameObject);
        OnSelectedObject.Invoke(false, null);
        ControlObject = null;
    }

    private void RemoveAll()
    {
        //GameObject消す

        for(int idx = 0; idx < m_SpaceCount; idx++)
        {
            if(m_RoomStates[idx] != null)
            {
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

        m_SelectedObject = null;
        m_ControlObject = null;
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

        //部屋に入りきらない場合
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

        //ほかのオブジェクトと重なる場合
        if(GetIsOverlap(index, objData, putType))
        {
            return false;
        }

        //床の状態からしておけない場合
        if(objData.PosType == PositionType.FLOOR)
        {
            if(minHeight > 0)
            {
                //定数時間で計算しないとバグる
                //いったん下の状況は分からなくてもいいことにして実装
                //敷き詰められているかの確認としてOverlapsを1層分チェックする
                //和からlevelの平均値を見る + 四隅の確認で対応
                //ToDo: これは十分ではない 平均値が一致している かつ 4隅の高さは同じ、ならおけてしまうという欠点がある
                int floorIndex = GetIndex(minHeight - 1, minDepth, minWidth);
                int floorOverlaps = GetOverlaps(floorIndex, 1, maxDepth - minDepth, maxWidth - minWidth);
                if (floorOverlaps % ((maxDepth - minDepth) * (maxWidth - minWidth)) != 0)
                {
                    return false;
                }
                else
                {
                    int floorLevel = floorOverlaps % ((maxDepth - minDepth) * (maxWidth - minWidth));
                    if((int)objData.Level < floorLevel)
                    {
                        return false;
                    }

                    //4隅の確認
                    Vector2Int[] dirs = new Vector2Int[4];
                    dirs[0] = new Vector2Int(0, 0);
                    dirs[1] = new Vector2Int(1, 0);
                    dirs[2] = new Vector2Int(0, 1);
                    dirs[3] = new Vector2Int(1, 1);
                    for(int i = 0; i < 4; i++)
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
                            if(!data.IsPlane)
                            {
                                return false;
                            }
                            else if((int)objData.Level < (int)data.Level)
                            {
                                return false;
                            }
                        }
                            
                    }
                }
            }
            *//*int tempFloor = m_FloorHeights[m_RoomWidth * minDepth + minWidth];

            for (int i = minWidth; i < maxWidth; i++)
            {
                for(int j = minDepth; j < maxDepth; j++)
                {
                    if (m_FloorStates[j * m_RoomWidth + i] == false)
                    {
                        return false;
                    }

                    if(m_FloorHeights[j * m_RoomWidth + i] != tempFloor)
                    {
                        return false;
                    }
                }
            }*//*
        }
        else if(objData.PosType == PositionType.WALL)
        {
            if(putType == PutType.NORMAL)
            {
                if(minWidth > 0)
                {
                    //定数時間で計算しないとバグる
                    //いったん下の状況は分からなくてもいいことにして実装
                    //敷き詰められているかの確認としてOverlapsを1層分チェックする
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

                        //4隅の確認
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

                *//*int tempFloor = m_WallWidthHeights[m_RoomDepth * minHeight + minDepth];

                for (int i = minHeight; i < maxHeight; i++)
                {
                    for (int j = minDepth; j < maxDepth; j++)
                    {
                        if (m_WallWidthStates[i * m_RoomDepth + j] == false)
                        {
                            return false;
                        }

                        if (m_WallWidthHeights[i * m_RoomDepth + j] != tempFloor)
                        {
                            return false;
                        }
                    }
                }*//*
            }
            else if(putType == PutType.REVERSE)
            {
                if (minDepth > 0)
                {
                    //定数時間で計算しないとバグる
                    //いったん下の状況は分からなくてもいいことにして実装
                    //敷き詰められているかの確認としてOverlapsを1層分チェックする
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

                        //4隅の確認
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
               *//* int tempFloor = m_WallDepthHeights[m_RoomWidth * minHeight + minWidth];

                for (int i = minHeight; i < maxHeight; i++)
                {
                    for (int j = minWidth; j < maxWidth; j++)
                    {
                        if (m_WallDepthStates[i * m_RoomWidth + j] == false)
                        {
                            return false;
                        }

                        if (m_WallDepthHeights[i * m_RoomWidth + j] != tempFloor)
                        {
                            return false;
                        }
                    }
                }*//*
            }
        }

        return true;
    }

    /// <summary>
    /// 注意 RoomObjectのIndex, PutTypeと確認したい値はずれている可能性があるので使わない
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

        //部屋に入りきらない場合
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

        //ほかのオブジェクトと重なる場合
        if (GetIsOverlap(index, roomObject, putType))
        {
            return false;
        }

        //床の状態からしておけない場合
        if (objData.PosType == PositionType.FLOOR)
        {
            if (minHeight > 0)
            {
                //定数時間で計算しないとバグる
                //いったん下の状況は分からなくてもいいことにして実装
                //敷き詰められているかの確認としてOverlapsを1層分チェックする
                //和からlevelの平均値を見る + 四隅の確認で対応
                //ToDo: これは十分ではない 平均値が一致している かつ 4隅の高さは同じ、ならおけてしまうという欠点がある
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

                    //4隅の確認
                    Vector2Int[] dirs = new Vector2Int[4];
                    dirs[0] = new Vector2Int(0, 0);
                    dirs[1] = new Vector2Int(1, 0);
                    dirs[2] = new Vector2Int(0, 1);
                    dirs[3] = new Vector2Int(1, 1);

                    //いったんまたがる挙動を許さない実装にする
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
            *//*int tempFloor = m_FloorHeights[m_RoomWidth * minDepth + minWidth];

            for (int i = minWidth; i < maxWidth; i++)
            {
                for(int j = minDepth; j < maxDepth; j++)
                {
                    if (m_FloorStates[j * m_RoomWidth + i] == false)
                    {
                        return false;
                    }

                    if(m_FloorHeights[j * m_RoomWidth + i] != tempFloor)
                    {
                        return false;
                    }
                }
            }*//*
        }
        else if (objData.PosType == PositionType.WALL)
        {
            if (putType == PutType.NORMAL)
            {
                if (minWidth > 0)
                {
                    //定数時間で計算しないとバグる
                    //いったん下の状況は分からなくてもいいことにして実装
                    //敷き詰められているかの確認としてOverlapsを1層分チェックする
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

                        //4隅の確認
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

                *//*int tempFloor = m_WallWidthHeights[m_RoomDepth * minHeight + minDepth];

                for (int i = minHeight; i < maxHeight; i++)
                {
                    for (int j = minDepth; j < maxDepth; j++)
                    {
                        if (m_WallWidthStates[i * m_RoomDepth + j] == false)
                        {
                            return false;
                        }

                        if (m_WallWidthHeights[i * m_RoomDepth + j] != tempFloor)
                        {
                            return false;
                        }
                    }
                }*//*
            }
            else if (putType == PutType.REVERSE)
            {
                if (minDepth > 0)
                {
                    //定数時間で計算しないとバグる
                    //いったん下の状況は分からなくてもいいことにして実装
                    //敷き詰められているかの確認としてOverlapsを1層分チェックする
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

                        //4隅の確認
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
                *//* int tempFloor = m_WallDepthHeights[m_RoomWidth * minHeight + minWidth];

                 for (int i = minHeight; i < maxHeight; i++)
                 {
                     for (int j = minWidth; j < maxWidth; j++)
                     {
                         if (m_WallDepthStates[i * m_RoomWidth + j] == false)
                         {
                             return false;
                         }

                         if (m_WallDepthHeights[i * m_RoomWidth + j] != tempFloor)
                         {
                             return false;
                         }
                     }
                 }*//*
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

        if(emptySpace.Id == -1)
        {
            floorMinHeight = GetHeight(selectedFloorObject.RoomIndex) + selectedFloorObject.Height;
            floorMinDepth = GetDepth(selectedFloorObject.RoomIndex);
            floorMinWidth = GetWidth(selectedFloorObject.RoomIndex);
            floorMaxHeight = m_RoomHeight;
            floorMaxDepth = floorMinDepth + selectedFloorObject.Depth;
            floorMaxWidth = floorMinWidth + selectedFloorObject.Width;
        }

        //空間に入りきらない場合
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

        //ほかのオブジェクトと重なる場合
        if (GetIsOverlap(index, roomObject, putType))
        {
            return false;
        }

        //床の状態からしておけない場合
        if (objData.PosType == PositionType.FLOOR)
        {
            if (minHeight > 0)
            {
                //定数時間で計算しないとバグる
                //いったん下の状況は分からなくてもいいことにして実装
                //敷き詰められているかの確認としてOverlapsを1層分チェックする
                //和からlevelの平均値を見る + 四隅の確認で対応
                //ToDo: これは十分ではない 平均値が一致している かつ 4隅の高さは同じ、ならおけてしまうという欠点がある
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

                    //4隅の確認
                    Vector2Int[] dirs = new Vector2Int[4];
                    dirs[0] = new Vector2Int(0, 0);
                    dirs[1] = new Vector2Int(1, 0);
                    dirs[2] = new Vector2Int(0, 1);
                    dirs[3] = new Vector2Int(1, 1);

                    //いったんまたがる挙動を許さない実装にする
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
            *//*int tempFloor = m_FloorHeights[m_RoomWidth * minDepth + minWidth];

            for (int i = minWidth; i < maxWidth; i++)
            {
                for(int j = minDepth; j < maxDepth; j++)
                {
                    if (m_FloorStates[j * m_RoomWidth + i] == false)
                    {
                        return false;
                    }

                    if(m_FloorHeights[j * m_RoomWidth + i] != tempFloor)
                    {
                        return false;
                    }
                }
            }*//*
        }
        else if (objData.PosType == PositionType.WALL)
        {
            if (putType == PutType.NORMAL)
            {
                if (minWidth > 0)
                {
                    //定数時間で計算しないとバグる
                    //いったん下の状況は分からなくてもいいことにして実装
                    //敷き詰められているかの確認としてOverlapsを1層分チェックする
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

                        //4隅の確認
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

                *//*int tempFloor = m_WallWidthHeights[m_RoomDepth * minHeight + minDepth];

                for (int i = minHeight; i < maxHeight; i++)
                {
                    for (int j = minDepth; j < maxDepth; j++)
                    {
                        if (m_WallWidthStates[i * m_RoomDepth + j] == false)
                        {
                            return false;
                        }

                        if (m_WallWidthHeights[i * m_RoomDepth + j] != tempFloor)
                        {
                            return false;
                        }
                    }
                }*//*
            }
            else if (putType == PutType.REVERSE)
            {
                if (minDepth > 0)
                {
                    //定数時間で計算しないとバグる
                    //いったん下の状況は分からなくてもいいことにして実装
                    //敷き詰められているかの確認としてOverlapsを1層分チェックする
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

                        //4隅の確認
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
                *//* int tempFloor = m_WallDepthHeights[m_RoomWidth * minHeight + minWidth];

                 for (int i = minHeight; i < maxHeight; i++)
                 {
                     for (int j = minWidth; j < maxWidth; j++)
                     {
                         if (m_WallDepthStates[i * m_RoomWidth + j] == false)
                         {
                             return false;
                         }

                         if (m_WallDepthHeights[i * m_RoomWidth + j] != tempFloor)
                         {
                             return false;
                         }
                     }
                 }*//*
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
        if(data.EmptySpaces.Count > 0)
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
    
    private int GetIndex(int height, int depth, int width)
    {
        return m_RoomMasterData.GetIndex(height, depth, width);
    }

    private int GetHeight(int index)
    {
        return index / ((m_RoomWidth) * (m_RoomDepth));
    }

    private int GetDepth(int index)
    {
        index %= ((m_RoomWidth) * (m_RoomDepth));
        return index / ((m_RoomWidth));
    }

    private int GetWidth(int index)
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

        int offsetHeight = (int)(emptySpace.GetCenter(putType).y / roomUnit);
        int offsetDepth = (int)(emptySpace.GetCenter(putType).z / roomUnit);
        int offsetWidth = (int)(emptySpace.GetCenter(putType).x / roomUnit);

        int spaceMinHeight = height + offsetHeight;
        int spaceMinDepth = depth + objDepth / 2 + offsetDepth - (emptyDepth / 2);
        int spaceMinWidth = width + objWidth / 2 + offsetWidth - (emptyWidth / 2);

        return GetIndex(spaceMinHeight, spaceMinDepth, spaceMinWidth);
    }

    private int GetEmptySpaceHeight(EmptySpace emptySpace, PutType putType)
    {
        float roomUnit = m_RoomMasterData.RoomUnit;

        int spaceHeight = (int)(emptySpace.GetSize(putType).y / roomUnit);

        return spaceHeight;
    }

    private int GetEmptySpaceDepth(EmptySpace emptySpace, PutType putType)
    {
        float roomUnit = m_RoomMasterData.RoomUnit;
        int spaceDepth = (int)(emptySpace.GetSize(putType).z / roomUnit);

        return spaceDepth;
    }

    private int GetEmptySpaceWidth(EmptySpace emptySpace, PutType putType)
    {
        float roomUnit = m_RoomMasterData.RoomUnit;

        int spaceWidth = (int)(emptySpace.GetSize(putType).x / roomUnit);

        
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
}
*/