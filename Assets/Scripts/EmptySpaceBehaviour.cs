using UnityEngine;
using System;

public class EmptySpaceBehaviour : MonoBehaviour
{
    [SerializeField] private int m_Id;
    [SerializeField] private Outline m_FloorOutline;
    [SerializeField] private float m_Speed = 3f;

    private bool m_Selected = false;
    private float m_Time;
    private GameObject m_EmptyFloor;
    private BoxCollider m_EmptyFloorCollider;
    private RoomObject m_RoomObject;
    private BoxCollider m_BoxCollider;
    private bool m_IsOutlineEnabled;
    public bool IsOutlineEnabled
    {
        get => m_IsOutlineEnabled;
        set
        {
            m_IsOutlineEnabled = value;
        }
    }

    public int Id
    {
        get => m_Id;
        set
        {
            m_Id = value;
        }
    }

    public EmptySpace EmptySpace { get; set; }

    public bool Selected
    {
        get => m_Selected;
        set
        {
            m_Selected = value;
        }
    }

    public Outline FloorOutline
    {
        get => m_FloorOutline;

        set
        {
            m_FloorOutline = value;
        }
    }

    private void Start()
    {
        m_Time = 0f;
        m_Speed = 3f;
    }

    public void Init(RoomObject roomObject, EmptySpace emptySpace)
    {
        m_RoomObject = roomObject;
        EmptySpace = emptySpace;
        m_BoxCollider = GetComponent<BoxCollider>();
        m_IsOutlineEnabled = true;
        m_FloorOutline.OutlineMode = Outline.Mode.OutlineVisible;
        m_FloorOutline.OutlineColor = new Vector4(233f, 218f, 61f, 255f) / 255f;

        if (m_EmptyFloor == null)
        {
            m_EmptyFloor = new GameObject("EmptyFloor" + Id.ToString());
            m_EmptyFloor.transform.SetParent(transform.parent);
            m_EmptyFloor.layer = LayerMask.NameToLayer("EmptyFloor");
            m_EmptyFloor.transform.localScale = transform.localScale;
            m_EmptyFloor.transform.position = gameObject.transform.position;
            m_EmptyFloor.transform.localRotation = transform.localRotation;
            m_EmptyFloorCollider = m_EmptyFloor.AddComponent<BoxCollider>();
            if(m_EmptyFloorCollider != null)
            {
                m_EmptyFloorCollider.size = new Vector3(m_BoxCollider.size.x, 0.01f, m_BoxCollider.size.z);
            }
        }
    }

    private void Update()
    {
        //Debug.Log("isenabled " + IsOutlineEnabled);
        if(m_FloorOutline != null)
        {
            m_Time += Time.deltaTime * m_Speed;
            if (m_Time >= Mathf.PI * 2f) m_Time = 0f;
            if(m_Selected && m_IsOutlineEnabled)
            {
                m_FloorOutline.OutlineWidth = 1f * Mathf.Cos(m_Time) + 2f;
            }
            else
            {
                m_FloorOutline.OutlineWidth = 0f;
            }
        }
    }

    public void SetFloorCollider(bool isOn)
    {
        if(m_EmptyFloorCollider != null)
        {
            m_EmptyFloorCollider.enabled = isOn;
        }
    }
}
