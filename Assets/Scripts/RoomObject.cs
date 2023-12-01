using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public abstract class RoomObject : MonoBehaviour
{
    [SerializeField] protected List<EmptySpaceBehaviour> m_EmptySpaceBehaviourList;

    protected readonly int m_SelectedLayerNum = 11;
    protected readonly float ms_MaxDeltaAngle = 45f;

    protected RoomMasterData m_RoomMasterData;
    protected RoomObjectData m_Data;
    protected IRoomObjectRotater m_Rotater;
    public bool IsBothRotate { get; protected set; }

    protected int m_RoomIndex;
    protected PutType m_PutType;
    protected int m_FamilyHeight; //NORMAL時の高さ
    protected int m_FamilyDepth; //NORMAL時の奥行
    protected int m_FamilyWidth; //NORMAL時の幅
    protected bool m_IsPictureSet;
    protected string m_ItemText;
    protected Vector3 m_OriginScale;
    protected Texture2D m_TrimmedTexture;
    protected float m_BaseAngle = 0f;
    protected PutType m_BefPutType;
    protected int m_BefRotateOffset;

    public PutType BefPutType => m_BefPutType;
    public int BefRotateOffset => m_BefRotateOffset;

    protected Subject<RoomObject> m_OnTapRoomObject = new Subject<RoomObject>();
    protected Subject<RoomObject> m_OnHoldRoomObject = new Subject<RoomObject>();
    protected Subject<RoomObject> m_OnDoubleTapRoomObject = new Subject<RoomObject>();
    protected Subject<SetMaterialEvent> m_OnSetMaterialSubject = new Subject<SetMaterialEvent>();

    public IObservable<RoomObject> OnTapRoomObject => m_OnTapRoomObject;
    public IObservable<RoomObject> OnHoldRoomObject => m_OnHoldRoomObject;
    public IObservable<RoomObject> OnDoubleTapRoomObject => m_OnDoubleTapRoomObject;
    public IObservable<SetMaterialEvent> OnSetMaterial => m_OnSetMaterialSubject;


    public bool IsSelected { get; set; }
    public bool IsInControl { get; set; }

    public Vector3 OriginScale => m_OriginScale;

    protected List<RoomObject> m_ChildRoomObjectList;
    public List<RoomObject> ChildRoomObjectList => m_ChildRoomObjectList;

    protected EmptySpaceBehaviour m_ParentEmptySpaceBehaviour;
    public EmptySpaceBehaviour ParentEmptySpaceBehaviour => m_ParentEmptySpaceBehaviour;

    protected RoomObject m_ParentObject;
    public RoomObject ParentObject => m_ParentObject;

    protected RoomObjectControlPosition m_ControlPosition = new RoomObjectControlPosition(0, 0, 0);
    public RoomObjectControlPosition ControlPosition
    {
        get => m_ControlPosition;
        set
        {
            m_ControlPosition = value;
        }
    }

    protected Transform m_BefParent;

    protected RoomObjectAnimationController m_AnimationController;
    public CursorManager CursorManager => m_RoomMasterData.CursorManager != null ? m_RoomMasterData.CursorManager : null;

    public List<EmptySpaceBehaviour> EmptySpaceBehaviourList => m_EmptySpaceBehaviourList;
    public Action<RoomObject, EmptySpace> OnEmptySpaceSelected;
    
    public float RoomUnit => m_RoomMasterData.RoomUnit;

    public int Height => m_Data.GetObjHeight(m_PutType);
    public int Depth => m_Data.GetObjDepth(m_PutType);
    public int Width => m_Data.GetObjWidth(m_PutType);

    public Vector3Int Center
    {
        get
        {
            return m_Data.GetCenter(m_PutType);
        }
    }

    public bool IsPictureSet
    {
        get => m_IsPictureSet;
        set
        {
            m_IsPictureSet = value;
        }
    }

    public string ItemText
    {
        get => m_ItemText;
        set
        {
            m_ItemText = value;
        }
    }

    public Vector3Int GetFamilySize(PutType putType)
    {
        if (putType == PutType.NORMAL)
        {
            return new Vector3Int(m_FamilyWidth, m_FamilyHeight, m_FamilyDepth);
        }
        else if (putType == PutType.REVERSE)
        {
            return new Vector3Int(m_FamilyDepth, m_FamilyHeight, m_FamilyWidth);
        }
        return new Vector3Int(m_FamilyWidth, m_FamilyHeight, m_FamilyDepth);
    }

    public void SetFamilySize(Vector3Int familySize, PutType putType)
    {
        if(putType == PutType.NORMAL)
        {
            m_FamilyHeight = familySize.y;
            m_FamilyDepth = familySize.z;
            m_FamilyWidth = familySize.x;
        }
        else if(putType == PutType.REVERSE)
        {
            m_FamilyHeight = familySize.y;
            m_FamilyDepth = familySize.x;
            m_FamilyWidth = familySize.z;
        }
    }

    public RoomObjectData Data => m_Data;
    public int RoomIndex
    { 
        get => m_RoomIndex;
    }
    public PutType PutType => m_PutType;

    public Texture2D TrimmedTexture => m_TrimmedTexture;

    public virtual void SetTexture(Texture2D trimmedTexture, SetMaterialEvent setMaterialEvent)
    {
        m_TrimmedTexture = trimmedTexture;
    }

    public virtual void Init(SaveDataUnit saveDataUnit, RoomMasterData roomMasterData, RoomObjectMasterData roomObjectMasterData)
    {
        m_IsPictureSet = saveDataUnit.IsPictureSet;
        m_ItemText = saveDataUnit.ItemText;
        m_RoomMasterData = roomMasterData;
        m_Data = roomObjectMasterData.RoomObjects[saveDataUnit.DataIndex];
        m_RoomIndex = saveDataUnit.RoomIndex;
        m_PutType = (PutType)saveDataUnit.PutTypeNum;
        m_BefPutType = m_PutType;
        if(m_Data.PosType == PositionType.FLOOR)
        {
            if(m_Data.Type == RoomObjectType.ITEM || m_Data.UICategory == UICategory.Prop)
            {
                m_Rotater = new BothFloorRoomObjectRotater();
                IsBothRotate = true;
            }
            else if(m_Data.Type == RoomObjectType.FURNITURE)
            {
                m_Rotater = new HorizontalFloorRoomObjectRotater();
                IsBothRotate = false;
            }
        }
        else if(m_Data.PosType == PositionType.WALL)
        {
            m_Rotater = new HorizontalWallRoomObjectRotater();
            IsBothRotate = false;
        }
        else
        {
            m_Rotater = new HorizontalFloorRoomObjectRotater();
            IsBothRotate = false;
        }

        float deltaAngleHorizontal = saveDataUnit.DeltaAngleHorizontal;
        float deltaAngleVertical = saveDataUnit.DeltaAngleVertical;

        m_FamilyHeight = saveDataUnit.FamilyHeight;
        m_FamilyDepth = saveDataUnit.FamilyDepth;
        m_FamilyWidth = saveDataUnit.FamilyWidth;

        foreach (var emptySpaceBehaviour in m_EmptySpaceBehaviourList)
        {
            foreach (var emptySpace in m_Data.EmptySpaces)
            {
                if (emptySpace.Id == emptySpaceBehaviour.Id)
                {
                    emptySpaceBehaviour.Init(this, emptySpace);
                    break;
                }
            }
        }
        m_OriginScale = transform.localScale;
        m_AnimationController = new RoomObjectAnimationController(transform);
        m_AnimationController.ChangePhase(new RoomObjectAnimationNone(this, m_AnimationController));

        m_ChildRoomObjectList = new List<RoomObject>();
        m_ParentObject = null;

        SetRoomObjectPosition(m_RoomIndex, m_Data, m_PutType);
        if (m_Data.PosType == PositionType.FLOOR)
        {
            if (m_PutType == PutType.NORMAL)
            {
                transform.LookAt(transform.position + Vector3.forward);
            }
            else if (m_PutType == PutType.REVERSE)
            {
                transform.LookAt(transform.position + Vector3.right);
            }
            m_BaseAngle = transform.rotation.eulerAngles.y;
        }
        else if (m_Data.PosType == PositionType.WALL)
        {
            if (m_PutType == PutType.NORMAL)
            {
                transform.LookAt(transform.position + Vector3.forward);
            }
            else if (m_PutType == PutType.REVERSE)
            {
                transform.LookAt(transform.position + Vector3.right);
            }
            m_BaseAngle = transform.rotation.eulerAngles.x;
        }

        transform.Rotate(saveDataUnit.DeltaAngleX, 0f, 0f);
        //m_DeltaAngleをReactivePropertyにして見張るという方法を検討
        DeltaRotate(deltaAngleHorizontal, deltaAngleVertical);
    }

    public virtual void Init(RoomObjectData data, int roomIndex, PutType putType)
    {
        m_IsPictureSet = false;
        m_ItemText = "";
        m_RoomMasterData = data.RoomMasterData;
        m_Data = data;
        m_RoomIndex = roomIndex;
        m_PutType = putType;

        if (m_Data.PosType == PositionType.FLOOR)
        {
            if (m_Data.Type == RoomObjectType.ITEM || m_Data.UICategory == UICategory.Prop)
            {
                m_Rotater = new BothFloorRoomObjectRotater();
                IsBothRotate = true;
            }
            else if (m_Data.Type == RoomObjectType.FURNITURE)
            {
                m_Rotater = new HorizontalFloorRoomObjectRotater();
                IsBothRotate = false;
            }
        }
        else if (m_Data.PosType == PositionType.WALL)
        {
            m_Rotater = new HorizontalWallRoomObjectRotater();
            IsBothRotate = false;
        }
        else
        {
            m_Rotater = new HorizontalFloorRoomObjectRotater();
            IsBothRotate = false;
        }

        m_FamilyHeight = Data.GetObjHeight(PutType.NORMAL);
        m_FamilyDepth = Data.GetObjDepth(PutType.NORMAL);
        m_FamilyWidth = Data.GetObjWidth(PutType.NORMAL);

        foreach (var emptySpaceBehaviour in m_EmptySpaceBehaviourList)
        {
            foreach (var emptySpace in data.EmptySpaces)
            {
                if (emptySpace.Id == emptySpaceBehaviour.Id)
                {
                    emptySpaceBehaviour.Init(this, emptySpace);
                    break;
                }
            }
        }
        m_OriginScale = transform.localScale;
        m_AnimationController = new RoomObjectAnimationController(transform);
        m_AnimationController.ChangePhase(new RoomObjectAnimationPut(this, m_AnimationController));

        m_ChildRoomObjectList = new List<RoomObject>();
        m_ParentObject = null;

        SetRoomObjectPosition(m_RoomIndex, m_Data, m_PutType);

        //以下はいずれSetPutTypeに移譲
        if (data.PosType == PositionType.FLOOR)
        {
            if (putType == PutType.NORMAL)
            {
                transform.LookAt(transform.position + Vector3.forward);
            }
            else if (putType == PutType.REVERSE)
            {
                transform.LookAt(transform.position + Vector3.right);
            }
            m_BaseAngle = transform.rotation.eulerAngles.y;
        }
        else if (data.PosType == PositionType.WALL)
        {
            if (putType == PutType.NORMAL)
            {
                transform.LookAt(transform.position + Vector3.forward);
            }
            else if (putType == PutType.REVERSE)
            {
                transform.LookAt(transform.position + Vector3.right);
            }
            m_BaseAngle = transform.rotation.eulerAngles.x;
        }
    }

    public void SetParent(RoomObject parentObject)
    {
        m_ParentObject = parentObject;
        m_OriginScale = Vector3.Scale(m_OriginScale, parentObject.transform.localScale);
        transform.SetParent(parentObject.transform);
        parentObject.ChildRoomObjectList.Add(this);
    }

    public void DetachFromParent()
    {
        if(m_ParentObject.ChildRoomObjectList.Contains(this))
        {
            m_ParentObject.ChildRoomObjectList.Remove(this);
        }
        m_OriginScale = new Vector3(m_OriginScale.x / m_ParentObject.transform.localScale.x, m_OriginScale.y / m_ParentObject.transform.localScale.y, m_OriginScale.z / m_ParentObject.transform.localScale.z);
        transform.parent = null;
        m_ParentObject = null;
    }

    protected void Update()
    {
        if(m_RoomIndex == -1)
        {
            Debug.Log("out of range: " + gameObject.name);
        }

        if(transform.parent != m_BefParent)
        {
            
            m_BefParent = transform.parent;
        }

        if (m_AnimationController != null)
        {
            m_AnimationController.OnUpdate();
        }
    }

    public void SetRoomIndex(int roomIndex)
    {
        Vector3Int newPos = m_RoomMasterData.GetPosition(roomIndex);
        Vector3Int oldPos = m_RoomMasterData.GetPosition(m_RoomIndex);

        m_RoomIndex = roomIndex;
        SetRoomObjectPosition(m_RoomIndex, m_Data, m_PutType);

        foreach (var childRoomObject in m_ChildRoomObjectList)
        {
            if (childRoomObject != null)
            {
                int childIndex = childRoomObject.RoomIndex;
                Vector3Int childOldPos = m_RoomMasterData.GetPosition(childIndex);
                Vector3Int childNewPos = childOldPos + newPos - oldPos;

                childRoomObject.SetRoomIndex(m_RoomMasterData.GetIndex(childNewPos));
            }
        }
    }

    public void SetPutType(PutType putType, int roomIndex, int rotateOffset, int parentId = 0)
    {
        m_BefPutType = m_PutType;
        m_BefRotateOffset = rotateOffset;

        //Hierarchy上で親子関係になっているので見かけとしては一番の親を回転させるだけでよい
        if (parentId == 0)
        {
            for(int i = 0; i < rotateOffset; i++)
            {
                transform.Rotate(0f, 90f, 0f, Space.World);
                if(m_Data.PosType == PositionType.FLOOR)
                {
                    m_BaseAngle = (m_BaseAngle + 90f) % 360f;
                }
                SetRoomObjectPosition(roomIndex, m_Data, putType);
            }
        }

        foreach (var childObj in m_ChildRoomObjectList)
        {
            if (childObj != null)
            {
                PutType nextPutType = (PutType)(((int)childObj.PutType + rotateOffset) % Enum.GetValues(typeof(PutType)).Length);
                int offsetWidth = (int)Mathf.Round((m_RoomMasterData.GetWidth(childObj.RoomIndex) + ((float)childObj.Width / 2f)) - (m_RoomMasterData.GetWidth(m_RoomIndex) + ((float)this.Width / 2f)));
                int offsetDepth = (int)Mathf.Round((m_RoomMasterData.GetDepth(childObj.RoomIndex) + (float)childObj.Depth / 2f) - (m_RoomMasterData.GetDepth(m_RoomIndex) + (float)this.Depth / 2f));
                int offsetHeight = m_RoomMasterData.GetHeight(childObj.RoomIndex) - m_RoomMasterData.GetHeight(m_RoomIndex);
                Vector3Int posOffset = new Vector3Int(offsetWidth, offsetDepth, offsetHeight);

                for (int rotateCnt = 0; rotateCnt < rotateOffset; rotateCnt++)
                {
                    int temp = posOffset.y;
                    posOffset.y = posOffset.x;
                    posOffset.x = -temp;
                }

                int floorWidth = m_RoomMasterData.GetWidth(roomIndex);
                int floorDepth = m_RoomMasterData.GetDepth(roomIndex);
                int floorHeight = m_RoomMasterData.GetHeight(roomIndex);

                //次の座標を求める
                int nextWidth = (int)Mathf.Round(floorWidth + (float)Data.GetObjWidth(putType) / 2f + posOffset.x - (float)childObj.Data.GetObjWidth(nextPutType) / 2f);
                int nextDepth = (int)Mathf.Round(floorDepth + (float)Data.GetObjDepth(putType) / 2f + posOffset.y - (float)childObj.Data.GetObjDepth(nextPutType) / 2f);
                int nextHeight = floorHeight + posOffset.z;
                int nextIdx = m_RoomMasterData.GetIndex(nextHeight, nextDepth, nextWidth);
                childObj.SetPutType(nextPutType, nextIdx, rotateOffset, ++parentId);
            }
        }
        m_PutType = putType;
        m_RoomIndex = roomIndex;
    }

    public void OnSelected(bool isSelected)
    {
        IsSelected = isSelected;
        if(isSelected)
        {
            Debug.Log("Selected " + gameObject.name);
            ChangeFamilyLayer(gameObject, m_SelectedLayerNum);
            foreach(var emptySpaceBehaviour in m_EmptySpaceBehaviourList)
            {
                emptySpaceBehaviour.SetFloorCollider(false);
            }
            m_AnimationController.ChangePhase(new RoomObjectAnimationSelected(this, m_AnimationController));
        }
        else
        {
            Debug.Log("Unselected " + gameObject.name);
            ChangeFamilyLayer(gameObject, 0);
            foreach (var emptySpaceBehaviour in m_EmptySpaceBehaviourList)
            {
                emptySpaceBehaviour.SetFloorCollider(true);
            }
            m_AnimationController.ChangePhase(new RoomObjectAnimationNone(this, m_AnimationController));
        }
    }

    protected void ChangeFamilyLayer(GameObject parentObject, int layerNum)
    {
        if (parentObject.gameObject.layer != LayerMask.NameToLayer("EmptySpace") && parentObject.gameObject.layer != LayerMask.NameToLayer("EmptyFloor"))
        {
            parentObject.gameObject.layer = layerNum;
        }

        for(int i = 0; i < parentObject.transform.childCount; i++)
        {
            ChangeFamilyLayer(parentObject.transform.GetChild(i).gameObject, layerNum);
        }
    }

    public void OnDetail(bool isDetailed)
    {
        /*if(isDetailed)
        {
            m_AnimationController.ChangePhase(new RoomObjectAnimationNone(this, m_AnimationController));
        }
        else
        {
            m_AnimationController.ChangePhase(new RoomObjectAnimationSelected(this, m_AnimationController));
        }*/
    }

    public void OnInControl(bool isInControl)
    {
        IsInControl = isInControl;
        if (isInControl)
        {
            m_AnimationController.ChangePhase(new RoomObjectAnimationControl(this, m_AnimationController));
        }
        else //モノを置いたときの挙動
        {
            SetRoomObjectPosition(m_RoomIndex, m_Data, m_PutType);
            m_AnimationController.ChangePhase(new RoomObjectAnimationPut(this, m_AnimationController), new RoomObjectAnimationSelected(this, m_AnimationController));
        }
    }

    public Vector3 SetRoomObjectPosition(int roomIndex, RoomObjectData data, PutType putType)
    {
        int height = m_RoomMasterData.GetHeight(roomIndex);
        int depth = m_RoomMasterData.GetDepth(roomIndex);
        int width = m_RoomMasterData.GetWidth(roomIndex);

        if(data.PosType == PositionType.FLOOR)
        {
            transform.position = m_RoomMasterData.RoomUnit * new Vector3(width + (float)data.GetObjWidth(putType)/ 2f, height, -depth - (float)data.GetObjDepth(putType) / 2f);
            transform.position -= Vector3.Scale(data.ColliderCenter.RotateAngleXZPlane(transform.localRotation.eulerAngles.y), new Vector3(1f, 0f, 1f));
            //Debug.Log("center rotated " + data.ColliderCenter.RotateAngleXZPlane(transform.localRotation.eulerAngles.y));
        }
        else if(data.PosType == PositionType.WALL)
        {
            if(putType == PutType.NORMAL)
            {
                transform.position = m_RoomMasterData.RoomUnit * new Vector3(width, height + (float)data.GetObjHeight(putType) / 2f, -depth - (float)data.GetObjDepth(putType) / 2f);
                transform.position -= Vector3.Dot(data.ColliderCenter, Vector3.up) * Vector3.up + Vector3.Dot(data.ColliderCenter, Vector3.forward) * Vector3.forward;
            }
            else if(putType == PutType.REVERSE)
            {
                transform.position = m_RoomMasterData.RoomUnit * new Vector3(width + (float)data.GetObjWidth(putType) / 2f, height + (float)data.GetObjHeight(putType) / 2f, depth);
                transform.position -= Vector3.Dot(data.ColliderCenter, Vector3.forward) * Vector3.right + Vector3.Dot(data.ColliderCenter, Vector3.up) * Vector3.up;
            }
        }

        //縦のDeltaAngleを確認し、かぶらないように上にずらす
        Vector3 eulerAngles = transform.localRotation.eulerAngles;
        transform.position += Vector3.up * Mathf.Abs(0.5f * Mathf.Sin(eulerAngles.z * Mathf.Deg2Rad) * m_Data.GetObjWidth(PutType.NORMAL) * m_RoomMasterData.RoomUnit);
        
        return transform.position;
    }

    public float GetDeltaAngleHorizontal()
    {
        return m_Rotater.GetDeltaAngleHorizontal();
    }

    public float GetDeltaAngleVertical()
    {
        return m_Rotater.GetDeltaAngleVertical();
    }

    public virtual void DeltaRotate(float deltaAngleHorizontal, float deltaAngleVertical)
    {
        deltaAngleHorizontal = Mathf.Clamp(deltaAngleHorizontal, -ms_MaxDeltaAngle, ms_MaxDeltaAngle);
        deltaAngleVertical = Mathf.Clamp(deltaAngleVertical, -ms_MaxDeltaAngle, ms_MaxDeltaAngle);

        m_Rotater.Rotate(transform, m_BaseAngle, deltaAngleHorizontal, deltaAngleVertical);

        transform.position = SetRoomObjectPosition(m_RoomIndex, m_Data, m_PutType);
    }

    //新しいRoomManagerでは使われていない
    public void OnPhaseChanged(RoomPhase phase)
    {
        if (phase == RoomPhase.None)
        {

        }
        else if (phase == RoomPhase.Selected)
        {

        }
        else if (phase == RoomPhase.Detail)
        {

        }
    }

    protected void OnDestroy()
    {
        if(m_AnimationController != null)
        {
            m_AnimationController.OnDestroy();
            m_AnimationController = null;
        }
    }
}

public class RoomObjectControlPosition
{
    private bool m_CanPut;
    private float m_HeightPos;
    private float m_DepthPos;
    private float m_WidthPos;
    private float m_FloatDistance;

    public bool CanPut => m_CanPut;
    public float HeightPos => m_HeightPos;
    public float DepthPos => m_DepthPos;
    public float WidthPos => m_WidthPos;
    public float FloatDistance => m_FloatDistance;

    public RoomObjectControlPosition(float heightPos, float depthPos, float widthPos)
    {
        m_HeightPos = heightPos;
        m_DepthPos = depthPos;
        m_WidthPos = widthPos;
    }

    public RoomObjectControlPosition(bool canPut, float heightPos, float depthPos, float widthPos)
    {
        m_CanPut = canPut;
        m_HeightPos = heightPos;
        m_DepthPos = depthPos;
        m_WidthPos = widthPos;
    }

    public static RoomObjectControlPosition operator+ (RoomObjectControlPosition x, RoomObjectControlPosition y)
    {
        return new RoomObjectControlPosition(x.CanPut, x.HeightPos + y.HeightPos, x.DepthPos + y.DepthPos, x.WidthPos + y.WidthPos);
    }

    public static RoomObjectControlPosition operator -(RoomObjectControlPosition x, RoomObjectControlPosition y)
    {
        return new RoomObjectControlPosition(x.CanPut, x.HeightPos - y.HeightPos, x.DepthPos - y.DepthPos, x.WidthPos - y.WidthPos);
    }
}


