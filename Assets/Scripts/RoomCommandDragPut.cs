using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class RoomCommandDragPut : IRoomCommand
{
    private IRoomCommander m_RoomCommander;
    private RoomPhaseMachine m_Machine;
    private MockRoomManager m_RoomManager;
    private RoomUIEvent m_UIEvent;

    private RaycastHit m_StartHit;
    private RoomObject m_PutObject;

    public RoomCommandDragPut(MockRoomManager roomManager, RoomUIEvent uiEvent, RoomPhaseMachine machine, IRoomCommander roomCommander)
    {
        m_RoomManager = roomManager;
        m_UIEvent = uiEvent;
        m_Machine = machine;
        m_RoomCommander = roomCommander;
    }

    public bool CanUndo()
    {
        return true;
    }

    public UniTask<bool> ExecuteAsync(CancellationToken token)
    {
        //座標指定 -> instantiate -> control
        if(m_UIEvent is DragPutItemEvent)
        {
            Debug.Log("drag put event received");
            DragPutItemEvent dragPutItemEvent = m_UIEvent as DragPutItemEvent;
            RoomObjectData data = dragPutItemEvent.Data;

            //どこにも置けない場合キャンセルが必要

            Camera mainCamera = Camera.main;
            Transform cameraTransform = mainCamera.transform;
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 5f));

            int layerMask = (1 << 6) + (1 << 12) + (1 << 13) + (1 << 14);
            if (data.PosType == PositionType.FLOOR)
            {
                layerMask = (1 << 6) + (1 << 14);
            }
            else if (data.PosType == PositionType.WALL)
            {
                layerMask = (1 << 12) + (1 << 13);
            }
            Ray floorRay = new Ray(cameraTransform.position, mouseWorldPos - cameraTransform.position);
            if (Physics.Raycast(floorRay, out RaycastHit startHit, Mathf.Infinity, layerMask))
            {
                int putIndex = 0;
                m_StartHit = startHit;
                PutType putType = PutType.NORMAL;

                Vector3Int putPosition = m_RoomManager.GetHitPosition(data, putType, startHit);
                putPosition -= new Vector3Int(data.GetObjWidth(putType), data.GetObjHeight(putType), data.GetObjDepth(putType)) / 2;

                putPosition = m_RoomManager.GetClampedPos(putPosition);
                putIndex = m_RoomManager.GetIndex(putPosition.y, putPosition.z, putPosition.x);

                //indexに応じてputTypeを決定

                /*Vector3Int hitPos = m_RoomManager.GetHitPosition(data, putType, m_StartHit);
                putIndex = m_RoomManager.GetIndex(hitPos.y, hitPos.z, hitPos.x);*/

                RoomObject putObject = m_RoomManager.InstantiateRoomObject(putIndex, data, putType);
                if (putObject == null)
                {
                    //どこにも置けなかった場合
                    Debug.Log("You cannot put this object");
                    return UniTask.FromResult(false);
                }
                else
                {
                    //置くことができた場合
                    m_RoomManager.SelectedObject = putObject;
                    m_RoomManager.SetState(m_RoomManager.SelectedObject, false);
                    m_RoomManager.SelectedObject.OnInControl(true);
                    m_PutObject = putObject;

                    m_Machine.ChangePhase(new RoomPhaseControl(m_Machine, m_RoomManager, m_RoomCommander));

                   /* m_RoomManager.StartControl(startHit);

                    while (true)
                    {
                        if (!Input.GetMouseButton(0))
                        {
                            //操作終了
                            break;
                        }
                        //実際に操作する処理
                        m_RoomManager.MoveRoomObject(m_RoomManager.SelectedObject, m_RoomManager.SelectedFloorObject, m_RoomManager.SelectedSpace != null ? m_RoomManager.SelectedSpace.EmptySpace : null);
                        await UniTask.DelayFrame(1, cancellationToken: token);
                    }*/


                    //フェーズをControlに移行
                    return UniTask.FromResult(true);
                }

            }

        }

        return UniTask.FromResult(false);
    }

    public bool Undo()
    {
        return true;
    }
}
