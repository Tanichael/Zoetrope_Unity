using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RoomObjectData
{
    [SerializeField] private RoomObject m_Model;
    [SerializeField] private RoomObjectType m_Type;
    [SerializeField] private PositionType m_PosType;
    [SerializeField] private bool m_IsPlane;
    [SerializeField] private Level m_Level;
    [SerializeField] private List<Tag> m_Tags;
    [SerializeField] private UICategory m_UICategory;

    private int m_DataIndex;
    public int DataIndex => m_DataIndex;

    private float m_Height; //y
    private float m_Depth; //z
    private float m_Width; //x
    private Vector3 m_ColliderCenter;
    private List<EmptySpace> m_EmptySpaces;

    private RoomMasterData m_RoomMasterData;
    private float m_RoomUnit = -1f;

    public RoomObjectData(RoomMasterData roomMasterData, RoomObjectDataEntity dataEntity, RoomObject model)
    {
        m_Model = model;
        m_Type = dataEntity.Type;
        m_PosType = dataEntity.PosType;
        m_IsPlane = dataEntity.IsPlane;
        m_Level = (Level)dataEntity.Level;
        m_Tags = DecodeTagsString(dataEntity.Tags);
        m_UICategory = dataEntity.UICategory;
        Init(roomMasterData, dataEntity.Id);
    }

    public RoomObjectData(RoomMasterData roomMasterData, RoomObjectDataEntity dataEntity)
    {
        //m_Model = dataEntity.Model;
        m_Type = dataEntity.Type;
        m_PosType = dataEntity.PosType;
        m_IsPlane = dataEntity.IsPlane;
        m_Level = (Level)dataEntity.Level;
        m_Tags = DecodeTagsString(dataEntity.Tags);
        m_UICategory = dataEntity.UICategory;
        Init(roomMasterData, dataEntity.Id);
    }

    public void Init(RoomMasterData roomMasterData, int dataIdx)
    {
        m_RoomMasterData = roomMasterData;
        m_DataIndex = dataIdx;
        m_RoomUnit = roomMasterData.RoomUnit;

        BoxCollider collider = m_Model.gameObject.GetComponent<BoxCollider>();
        if(collider != null)
        {
            Vector3 size = collider.size.x * collider.gameObject.transform.right + collider.size.y * collider.gameObject.transform.up + collider.size.z * collider.gameObject.transform.forward;
            size = new Vector3(Mathf.Abs(Vector3.Dot(size, Vector3.right)), Mathf.Abs(Vector3.Dot(size, Vector3.up)), Mathf.Abs(Vector3.Dot(size, Vector3.forward)));
            Vector3 center = collider.center.x * collider.gameObject.transform.right + collider.center.y * collider.gameObject.transform.up + collider.center.z * collider.gameObject.transform.forward;
            Vector3 scale = m_Model.transform.localScale;

            if (m_PosType == PositionType.FLOOR)
            {
                m_Height = size.y * scale.y;
                m_Depth = size.z * scale.z;
                m_Width = size.x * scale.x;
                m_ColliderCenter = Vector3.Scale(center, scale);
            }
            else if(m_PosType == PositionType.WALL)
            {
                m_Height = size.z * scale.z;
                m_Depth = size.x * scale.x;
                m_Width = size.y * scale.y;
                m_ColliderCenter = new Vector3(center.y * scale.y, center.z * scale.z, center.x * scale.x);
            }

            m_Height = size.y * scale.y;
            m_Depth = size.z * scale.z;
            m_Width = size.x * scale.x;
            m_ColliderCenter = Vector3.Scale(center, scale);
        }

        //EmptySpaceÇÃåüçı
        m_EmptySpaces = new List<EmptySpace>();
        RoomObject roomObject = m_Model.gameObject.GetComponent<RoomObject>();
        if(roomObject != null)
        {
            foreach(var emptySpaceBehaviour in roomObject.EmptySpaceBehaviourList)
            {
                BoxCollider spaceCollider = emptySpaceBehaviour.gameObject.GetComponent<BoxCollider>();
                if (spaceCollider == null) continue;

                Vector3 size = Vector3.Scale(emptySpaceBehaviour.gameObject.transform.localScale, spaceCollider.size);
                Vector3 center = emptySpaceBehaviour.gameObject.transform.localPosition + Vector3.Scale(emptySpaceBehaviour.gameObject.transform.localScale, spaceCollider.center) - Vector3.up * size.y / 2f;

                if(spaceCollider.gameObject.transform.parent != null)
                {
                    size = Vector3.Scale(spaceCollider.gameObject.transform.parent.localScale, size);
                    center = Vector3.Scale(spaceCollider.gameObject.transform.parent.localScale, center);
                }

                //äpìxí≤êÆèàóù
                size = size.x * spaceCollider.gameObject.transform.right + size.y * spaceCollider.gameObject.transform.up + size.z * spaceCollider.gameObject.transform.forward;
                size = new Vector3(Mathf.Abs(Vector3.Dot(size, Vector3.right)), Mathf.Abs(Vector3.Dot(size, Vector3.up)), Mathf.Abs(Vector3.Dot(size, Vector3.forward)));
                center = center.x * spaceCollider.gameObject.transform.right + center.y * spaceCollider.gameObject.transform.up + center.z * spaceCollider.gameObject.transform.forward;
                center = new Vector3(Mathf.Abs(Vector3.Dot(center, Vector3.right)), Mathf.Abs(Vector3.Dot(center, Vector3.up)), Mathf.Abs(Vector3.Dot(center, Vector3.forward)));

                //Debug.Log("name size center: " + m_Model.gameObject.name + " " + size + " " + center);

                EmptySpace emptySpace = new EmptySpace(center, size, emptySpaceBehaviour.Id);

                bool isAdd = true;
                foreach(var tempSpace in m_EmptySpaces)
                {
                    if (emptySpace.Id == tempSpace.Id)
                    {
                        isAdd = false;
                    }
                }
                if (isAdd)
                {
                    m_EmptySpaces.Add(emptySpace);
                }
            }
        }
    }

    private List<Tag> DecodeTagsString(string tagsStr)
    {
        if (tagsStr == null || tagsStr == "")
        {
            return null;
        }

        List<Tag> tags = new List<Tag>();
        string tempStr = "";
        for(int i = 0; i < tagsStr.Length; i++)
        {
            if(tagsStr[i] == ',')
            {
                foreach(Tag tag in Enum.GetValues(typeof(Tag)))
                {
                    if(tempStr == tag.ToString())
                    {
                        tags.Add(tag);
                    }
                }
                tempStr = "";
            }
            else
            {
                tempStr = tempStr + tagsStr[i];
            }
        }
        foreach (Tag tag in Enum.GetValues(typeof(Tag)))
        {
            if (tempStr == tag.ToString())
            {
                tags.Add(tag);
            }
        }
        return tags;
    }

    public int GetObjHeight(PutType putType)
    {
        return (int)Mathf.Round((m_Height) / m_RoomUnit);
    }
    public int GetObjDepth(PutType putType)
    {
        if(putType == PutType.NORMAL)
        {
            return (int)Mathf.Round((m_Depth) / m_RoomUnit);
        }
        else if(putType == PutType.REVERSE)
        {
            return (int)Mathf.Round((m_Width) / m_RoomUnit);
        }
        return (int)Mathf.Round((m_Depth) / m_RoomUnit);
    }
    public int GetObjWidth(PutType putType)
    {
        if (putType == PutType.NORMAL)
        {
            return (int)Mathf.Round((m_Width) / m_RoomUnit);
        }
        else if (putType == PutType.REVERSE)
        {
            return (int)Mathf.Round((m_Depth) / m_RoomUnit);
        }
        return (int)Mathf.Round((m_Width) / m_RoomUnit);
    }

    public Vector3Int GetCenter(PutType putType)
    {
        return new Vector3Int(GetObjWidth(putType) / 2, GetObjHeight(putType) / 2, GetObjDepth(putType) / 2);
    }

    public Vector3 ColliderCenter => m_ColliderCenter;

    public RoomObject Model => m_Model;
    public RoomObjectType Type => m_Type;
    public PositionType PosType => m_PosType;
    public bool IsPlane => m_IsPlane;
    public Level Level => m_Level;
    public List<EmptySpace> EmptySpaces => m_EmptySpaces;
    public RoomMasterData RoomMasterData => m_RoomMasterData;
    public List<Tag> Tags => m_Tags;
    public UICategory UICategory => m_UICategory;
}

public enum RoomObjectType
{
    FURNITURE = 0,
    ITEM = 1
}

public enum PositionType
{
    FLOOR = 0,
    WALL = 1
}

public enum UICategory
{
    Poster = 0,
    Chair = 1,
    Shelf = 2,
    Prop = 3, 
    Electronics = 4,
    Photo = 5,
    WallFurniture = 6,
    Art = 7,
    Plant = 8,
    Acryl = 9,
    Window = 10,
    Carpet = 11,
    Partitions = 12,
    Book = 13,
    Bed = 14, 
    Music = 15,
    Lamp = 16,
    Desk = 17,
}

public enum Level
{
    ONE = 1,
    TWO = 2
}

[Serializable]
public class EmptySpace
{
    private int m_Id;
    private Vector3 m_Center;
    private Vector3 m_Size;

    public EmptySpace(Vector3 center, Vector3 size, int id)
    {
        m_Center = center;
        m_Size = size;
        m_Id = id;
    }

    public Vector3 GetCenter(PutType putType)
    {
        if(putType == PutType.NORMAL)
        {
            return m_Center;
        }
        else if(putType == PutType.REVERSE)
        {
            return new Vector3(m_Center.z, m_Center.y, m_Center.x);
        }
        return m_Center;
    }

    public Vector3 GetSize(PutType putType)
    {
        if (putType == PutType.NORMAL)
        {
            return m_Size;
        }
        else if (putType == PutType.REVERSE)
        {
            return new Vector3(m_Size.z, m_Size.y, m_Size.x);
        }
        return m_Size;
    }

    public int Id => m_Id;
}

public enum Tag
{
    Book = 0,
    AcrylStand = 1,
}