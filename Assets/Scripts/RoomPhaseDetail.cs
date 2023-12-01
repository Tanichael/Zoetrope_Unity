using UnityEngine;

public class RoomPhaseDetail : RoomPhaseBase
{
    private bool m_CanTouch;

    public override RoomPhase GetRoomPhase()
    {
        return RoomPhase.Detail;
    }
    public RoomPhaseDetail(RoomPhaseMachine machine, MockRoomManager roomManager, IRoomCommander roomCommander) : base(machine, roomManager, roomCommander)
    {

    }

    public override void OnEnterState()
    {
        Debug.Log("RoomPhase -> Detail");
        m_CanTouch = true;
    }

    public override void OnUpdate()
    {
        if(m_CanTouch)
        {
            #region SELECT_EMPTY_SPACE
            //EmptySpaceの選択が可能になっている

            Camera currentCamera = m_Machine.CurrentCameraController.Camera;
            if(currentCamera != null)
            {
                Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
                if (m_RoomManager.SelectedFloorObject != null && Input.GetMouseButtonDown(0))
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
                            if (emptySpaceBehaviour != m_RoomManager.SelectedSpace && m_RoomManager.GetIsEmptySpaceInRoomObject(emptySpaceBehaviour, m_RoomManager.SelectedFloorObject))
                            {
                                //EmptySpaceの情報に到達する
                                //おける場所を制限する

                                EmptySpace emptySpace = emptySpaceBehaviour.EmptySpace;

                                //GetComonent使ってる 要修正
                                BoxCollider collider = m_RoomManager.SelectedObject.GetComponent<BoxCollider>();
                                if (collider != null)
                                {
                                    collider.enabled = false;
                                }

                                EmptySpaceBehaviour befEmptySpaceBehaviour = m_RoomManager.SelectedSpace;
                                m_RoomManager.SelectedSpace = emptySpaceBehaviour;

                                m_Machine.ChangePhase(new RoomPhaseEmptySpace(m_Machine, m_RoomManager, m_RoomCommander));
                            }
                        }
                    }
                }
            }
            #endregion
        }
    }

    public override void OnExitState()
    {

    }

    public override void HandleUIEvent(RoomUIEvent roomUIEvent)
    {
        base.HandleUIEvent(roomUIEvent);
        if(roomUIEvent is BackButtonClickEvent)
        {
            m_CanTouch = false;

            //selectedObjectをもとに戻す
            if (m_RoomManager.SelectedFloorObject != null && m_RoomManager.SelectedSpace != null)
            {
                //m_RoomManager.SelectedObject = m_RoomManager.SelectedFloorObject;
                Transform parentTransform = m_RoomManager.SelectedFloorObject.transform.parent;
                m_RoomManager.SelectedFloorObject = null;
                if (parentTransform != null)
                {
                    RoomObject parentObject = parentTransform.gameObject.GetComponent<RoomObject>();
                    if (parentObject != null)
                    {
                        m_RoomManager.SelectedFloorObject = parentObject;
                    }
                }
            }
            else
            {
                m_RoomManager.SelectedFloorObject = null;
            }

            m_Machine.TransitPhase(new RoomPhaseSelected(m_Machine, m_RoomManager, m_RoomCommander));
        }
        else if(roomUIEvent is EditButtonClickEvent)
        {
            m_Machine.ChangePhase(new RoomPhasePicture(m_Machine, m_RoomManager, this, m_RoomCommander));
        }
        else if (roomUIEvent is DeltaRotateStartEvent)
        {
            m_Machine.ChangePhase(new RoomPhaseDeltaRotate(m_Machine, m_RoomManager, m_RoomCommander));
        }
    }
}
