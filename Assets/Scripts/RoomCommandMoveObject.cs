using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class RoomCommandMoveObject : IRoomCommand
{
    private IRoomCommander m_RoomCommander;
    private RoomPhaseMachine m_Machine;
    private MockRoomManager m_RoomManager;
    private RaycastHit m_MoveStartHit;
    private int m_MoveStartIndex;
    private RoomObject m_MoveObject;
    private RaycastHit m_StartHit;
    private int m_BefIndex;
    private PutType m_BefPutType;

    public RoomCommandMoveObject(MockRoomManager roomManager, RaycastHit hit, RoomPhaseMachine machine, IRoomCommander roomCommander)
    {
        m_RoomManager = roomManager;
        m_MoveStartHit = hit;
        m_Machine = machine;
        m_RoomCommander = roomCommander;
    }

    public bool CanUndo()
    {
        return true;
    }

    public async UniTask<bool> ExecuteAsync(CancellationToken token)
    {
        //�ړ��X�^�[�g
        m_BefIndex = m_RoomManager.SelectedObject.RoomIndex;
        m_BefPutType = m_RoomManager.SelectedObject.PutType;

        m_RoomManager.SetState(m_RoomManager.SelectedObject, false);
        m_RoomManager.SelectedObject.OnInControl(true);

        Vector3 mousePos = Input.mousePosition;
        Camera mainCamera = Camera.main;
        Transform cameraTransform = mainCamera.transform;
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 5f));

        //�u���ʒu�ɉ�����layerMask�̐ݒ�
        int layerMask = (1 << 6) + (1 << 12) + (1 << 13) + (1 << 14);
        if (m_RoomManager.SelectedObject.Data.PosType == PositionType.FLOOR)
        {
            layerMask = (1 << 6) + (1 << 14);
        }
        else if (m_RoomManager.SelectedObject.Data.PosType == PositionType.WALL)
        {
            layerMask = (1 << 12) + (1 << 13);
        }
        Ray floorRay = new Ray(cameraTransform.position, mouseWorldPos - cameraTransform.position);
        if (Physics.Raycast(floorRay, out RaycastHit startHit, Mathf.Infinity, layerMask))
        {
            m_StartHit = startHit;
            m_RoomManager.StartControl(startHit);
        }

        //�ړ��̏���
        while (true)
        {
            if(!Input.GetMouseButton(0))
            {
                //����I��
                break;
            }
            //���ۂɑ��삷�鏈��
            m_RoomManager.MoveRoomObject(m_RoomManager.SelectedObject, m_RoomManager.SelectedFloorObject, m_RoomManager.SelectedSpace != null ? m_RoomManager.SelectedSpace.EmptySpace : null);
            await UniTask.DelayFrame(1, cancellationToken: token);
        }

        //�ړ��I��
        m_MoveObject = m_RoomManager.SelectedObject;
        mousePos = Input.mousePosition;
        mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 5f));

        //�u���ʒu�ɉ�����layerMask�̐ݒ�
        if (m_RoomManager.SelectedObject.Data.PosType == PositionType.FLOOR)
        {
            layerMask = (1 << 6) + (1 << 14);
        }
        else if (m_RoomManager.SelectedObject.Data.PosType == PositionType.WALL)
        {
            layerMask = (1 << 12) + (1 << 13);
        }

        floorRay = new Ray(cameraTransform.position, mouseWorldPos - cameraTransform.position);

        //��������u���ꏊ�̑I�� -> �����Ȃ���΋߂��ɒu�����Ċ�����
        if (Physics.Raycast(floorRay, out RaycastHit endHit, Mathf.Infinity, layerMask))
        {
            m_MoveStartIndex = m_RoomManager.SelectedObject.RoomIndex;
            bool isContolSuccess = m_RoomManager.EndControl(endHit);
            if (isContolSuccess)
            {
                m_RoomManager.SelectedObject.OnInControl(false);
                m_Machine.ChangePhase(new RoomPhaseSelected(m_Machine, m_RoomManager, m_RoomCommander));
                return true;
            }
            else
            {
                Debug.LogError("Control failed");
                return false;
            }
        }

        return false;
    }

    public bool Undo()
    {
        Debug.Log("Undo Move");
        if(m_MoveObject != null)
        {
            m_RoomManager.SetState(m_MoveObject, false);
            int rotateOffset = 0;
            if(m_MoveObject.PutType == PutType.NORMAL && m_BefPutType == PutType.REVERSE)
            {
                rotateOffset = 1;
            }
            else if (m_MoveObject.PutType == PutType.REVERSE && m_BefPutType == PutType.NORMAL)
            {
                rotateOffset = 3;
            }

            m_MoveObject.SetPutType(m_BefPutType, m_BefIndex, rotateOffset);
            m_MoveObject.SetRoomIndex(m_BefIndex);
            m_RoomManager.SetState(m_MoveObject, true);
        }
        else
        {
            Debug.Log("Moved object is deleted");
        }
       
        return true;
    }
}
