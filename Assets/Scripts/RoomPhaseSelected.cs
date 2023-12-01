using System;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

public class RoomPhaseSelected : RoomPhaseBase, IDisposable
{
    private bool m_CanTouch;
    private bool m_IsWhileSelect;
    private float m_SelectingTime;
    private Vector3 m_SelectStartPos;
    private RoomObject m_SuspectObject;
    private readonly float ms_ControlTime = 0.2f;
    private EmptySpaceBehaviour m_BefEmptySpaceBehaviour;
    private CancellationTokenSource m_Cts;

    public override RoomPhase GetRoomPhase()
    {
        return RoomPhase.Selected;
    }

    public RoomPhaseSelected(RoomPhaseMachine machine, MockRoomManager roomManager, IRoomCommander roomCommander) : base(machine, roomManager, roomCommander)
    {
        m_Cts = new CancellationTokenSource();
    }

    public override void OnEnterState()
    {
        Debug.Log("RoomPhase -> Selected");
        m_CanTouch = true;
    }

    public override void OnUpdate()
    {
        //Roomの状態を監視して、変数に応じて状態を移行する
        if(m_RoomManager.SelectedObject != null)
        {
            if(m_RoomManager.SelectedFloorObject != null && m_RoomManager.SelectedFloorObject == m_RoomManager.SelectedObject)
            {
                m_Machine.ChangePhase(new RoomPhaseEmptySpace(m_Machine, m_RoomManager, m_RoomCommander));
            }
            if(m_RoomManager.SelectedObject.IsInControl)
            {
                m_Machine.ChangePhase(new RoomPhaseControl(m_Machine, m_RoomManager, m_RoomCommander));
            }
        }

        if(m_RoomManager.SelectedObject == null)
        {
            m_Machine.ChangePhase(new RoomPhaseNone(m_Machine, m_RoomManager, m_RoomCommander));
        }

        if(m_CanTouch)
        {
            Camera currentCamera = m_Machine.CurrentCameraController.Camera;

            if (m_Machine.TouchManager.GetCanSelect())
            {
                //UI選択の場合は後の動作をスキップ
                if (EventSystem.current.IsPointerOverGameObject(-1) || EventSystem.current.IsPointerOverGameObject(0))
                {
                    return;
                }

                if (currentCamera != null)
                {
                    Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
                    int layerMask = (1 << 0) + (1 << 11);
                    TapSelectValues values = RoomSingleton.Instance.ObjectSelector.TapSelect(ray, layerMask);

                    if (values.SelectState == TapSelectState.TapStart && values.TappableObject == null)
                    {
                        //UnSelectと同じ効果
                        if (m_RoomManager.SelectedFloorObject == null)
                        {
                            m_RoomCommander.ExecuteCommandAsync(new RoomCommandSelectObject(m_RoomManager, null, m_Machine, m_RoomCommander), m_Cts.Token);
                        }
                        else
                        {
                            m_RoomCommander.ExecuteCommandAsync(new RoomCommandSelectObject(m_RoomManager, m_RoomManager.SelectedFloorObject, m_Machine, m_RoomCommander), m_Cts.Token);
                        }
                    }
                }
            }

            #region SELECT_EMPTY_SPACE
            //EmptySpaceの選択が可能になっている
            if (currentCamera != null)
            {
                Ray emptyRay = currentCamera.ScreenPointToRay(Input.mousePosition);
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
                                //EmptySpaceの情報に到達する
                                //おける場所を制限する
                                RoomObject nextObject = m_RoomManager.SelectedFloorObject;
                                m_RoomCommander.ExecuteCommandAsync(new RoomCommandSelectObject(m_RoomManager, nextObject, m_Machine, m_RoomCommander), m_Cts.Token);
                                EmptySpace emptySpace = emptySpaceBehaviour.EmptySpace;

                                BoxCollider collider = nextObject.GetComponent<BoxCollider>();
                                if (collider != null)
                                {
                                    collider.enabled = false;
                                }
                       
                                m_RoomManager.SelectedSpace = emptySpaceBehaviour;
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
       
        //以下Command未対応イベント
        if(roomUIEvent is DetailButtonClickEvent)
        {
            m_CanTouch = false;
            m_RoomManager.SelectedFloorObject = m_RoomManager.SelectedObject;
            if(m_RoomManager.SelectedObject.Data.Type != RoomObjectType.ITEM && m_RoomManager.SelectedObject.EmptySpaceBehaviourList.Count == 1)
            {
                m_RoomManager.SelectedSpace = m_RoomManager.SelectedObject.EmptySpaceBehaviourList[0];
                m_Machine.TransitPhase(new RoomPhaseEmptySpace(m_Machine, m_RoomManager, m_RoomCommander));
            }
            else
            {
                m_Machine.TransitPhase(new RoomPhaseDetail(m_Machine, m_RoomManager, m_RoomCommander));
            }
        }
        else if(roomUIEvent is EditButtonClickEvent)
        {
            if(m_RoomManager.SelectedSpace != null)
            {
                m_RoomManager.SelectedSpace.IsOutlineEnabled = false;
            }
            m_Machine.ChangePhase(new RoomPhasePicture(m_Machine, m_RoomManager, this, m_RoomCommander));
        }
        else if(roomUIEvent is UnselectButtonClickEvent)
        {
            if (m_RoomManager.SelectedFloorObject == null)
            {
                m_RoomCommander.ExecuteCommandAsync(new RoomCommandSelectObject(m_RoomManager, null, m_Machine, m_RoomCommander), m_Cts.Token);
            }
            else
            {
                m_RoomCommander.ExecuteCommandAsync(new RoomCommandSelectObject(m_RoomManager, m_RoomManager.SelectedFloorObject, m_Machine, m_RoomCommander), m_Cts.Token);
            }
        }
        else if(roomUIEvent is DeltaRotateStartEvent)
        {
            m_Machine.ChangePhase(new RoomPhaseDeltaRotate(m_Machine, m_RoomManager, m_RoomCommander));
        }
    }

    public void Dispose()
    {
        m_Cts.Cancel();
        m_Cts.Dispose();
    }
}
