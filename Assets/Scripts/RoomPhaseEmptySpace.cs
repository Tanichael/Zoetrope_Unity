using UnityEngine;

public class RoomPhaseEmptySpace : RoomPhaseBase
{
    private bool m_IsWhileSelect;
    private float m_SelectingTime;
    private Vector3 m_SelectStartPos;
    private RoomObject m_SuspectObject;
    private readonly float ms_ControlTime = 0.2f;

    public override RoomPhase GetRoomPhase()
    {
        return RoomPhase.EmptySpace;
    }

    public RoomPhaseEmptySpace(RoomPhaseMachine machine, MockRoomManager roomManager, IRoomCommander roomCommander) : base(machine, roomManager, roomCommander)
    {

    }

    public override void OnEnterState()
    {
        Debug.Log("RoomPhase -> EmptySpace");
        m_IsWhileSelect = false;
        m_RoomManager.SelectedObject = m_RoomManager.SelectedFloorObject;
    }

    public override void OnUpdate()
    {
        if(m_Machine.TouchManager.GetCanSelect())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            int layerMask = (1 << 0) + (1 << 11);

            RoomSingleton.Instance.ObjectSelector.TapSelect(ray, layerMask);
        }

        if(m_RoomManager.SelectedObject != null && m_RoomManager.SelectedFloorObject != null)
        {
            if(m_RoomManager.SelectedObject != m_RoomManager.SelectedFloorObject)
            {
                if(m_RoomManager.SelectedObject.IsInControl)
                {
                    m_Machine.ChangePhase(new RoomPhaseControl(m_Machine, m_RoomManager, m_RoomCommander));
                }
                else if (m_RoomManager.SelectedObject.IsSelected)
                {
                    m_Machine.ChangePhase(new RoomPhaseSelected(m_Machine, m_RoomManager, m_RoomCommander));
                }
            }
        }
        
        #region SELECT_EMPTY_SPACE
        Ray emptyRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (m_RoomManager.SelectedFloorObject != null && !m_IsWhileSelect && Input.GetMouseButtonDown(0) && m_Machine.TouchManager.GetCanSelect())
        {
            //EmptySpaceレイヤーのみを選択
            int layerMask = (1 << 7);
            if (Physics.Raycast(emptyRay, out RaycastHit hit, Mathf.Infinity, layerMask))
            {
                EmptySpaceBehaviour emptySpaceBehaviour = hit.collider.gameObject.GetComponent<EmptySpaceBehaviour>();

                if (emptySpaceBehaviour != null)
                {
                    //m_SelectedObjectのEmptySpaceではない場合は選択できないようにする
                    //同じEmptySpaceBehaviourだったときも同様
                    if (emptySpaceBehaviour != m_RoomManager.SelectedSpace && m_RoomManager.GetIsEmptySpaceInRoomObject(emptySpaceBehaviour, m_RoomManager.SelectedFloorObject))
                    {
                        EmptySpace emptySpace = emptySpaceBehaviour.EmptySpace;

                        BoxCollider collider = m_RoomManager.SelectedObject.GetComponent<BoxCollider>();
                        if (collider != null)
                        {
                            collider.enabled = false;
                        }

                        m_RoomManager.SelectedSpace = emptySpaceBehaviour;
                    }
                }
            }
        }
        #endregion
    }

    public override void HandleUIEvent(RoomUIEvent roomUIEvent)
    {
        base.HandleUIEvent(roomUIEvent);
        if(roomUIEvent is BackButtonClickEvent)
        {
            //EmptySpaceの選択解除
            m_RoomManager.SelectedSpace = null;

            //parentObjectから来たとは限らないので注意
            RoomObject parentObject = null;

            Transform parentTransform = m_RoomManager.SelectedObject.transform.parent;
            if(parentTransform != null)
            {
                parentObject = parentTransform.gameObject.GetComponent<RoomObject>();
            }

            if(parentObject != null)
            {
                foreach(var emptySpaceBehaviour in parentObject.EmptySpaceBehaviourList)
                {
                    if(m_RoomManager.GetIsInEmptySpace(m_RoomManager.SelectedObject, parentObject, emptySpaceBehaviour.EmptySpace))
                    {
                        m_RoomManager.SelectedSpace = emptySpaceBehaviour;
                    }
                }
            }

            BoxCollider collider = m_RoomManager.SelectedObject.GetComponent<BoxCollider>();
            if (collider != null)
            {
                collider.enabled = true;
            }

            if(m_RoomManager.SelectedObject.EmptySpaceBehaviourList.Count == 1)
            {
                m_RoomManager.SelectedFloorObject = parentObject;
                m_Machine.TransitPhase(new RoomPhaseSelected(m_Machine, m_RoomManager, m_RoomCommander));
            }
            else
            {
                m_Machine.ChangePhase(new RoomPhaseDetail(m_Machine, m_RoomManager, m_RoomCommander));
            }
        }
    }
}
