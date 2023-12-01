using UnityEngine;
using Cysharp.Threading.Tasks;

public abstract class CameraController : MonoBehaviour
{
    public IInputProvider InputProvider;
    
    protected RoomMasterData m_RoomMasterData;

    public RoomMasterData RoomMasterData
    {
        get => m_RoomMasterData;
        set
        {
            m_RoomMasterData = value;
        }
    }

    protected Camera m_Camera;

    public Camera Camera
    {
        get => m_Camera;
        set
        {
            m_Camera = value;
        }
    }

    protected GameObject m_CameraParent;
    public GameObject CameraParent
    {
        get => m_CameraParent;
        set
        {
            m_CameraParent = value;
        }
    }
    
    protected RoomPhase m_RoomPhase;
    protected ViewType m_ViewType;

    public virtual void Init(RoomObjectData table, RoomObjectData chair, float offset)
    {

    }

    public virtual Vector3 SetProfilePosition(RoomObjectData table, RoomObjectData chair, float offset)
    {
        return Vector3.zero;
    }

    public virtual void SetRoomPhase(RoomPhaseBase phase)
    {
        m_RoomPhase = phase.GetRoomPhase();
    }

    public virtual void SetRoomPhase(RoomPhase phase)
    {
        m_RoomPhase = phase;
    }

    public virtual async UniTask ChangeViewType(RoomPhaseBase roomPhase, RoomObject roomObject)
    {
        Debug.Log("Camera Move Start");
        RoomPhase phase = roomPhase.GetRoomPhase();
        switch (phase)
        {
            case RoomPhase.None:
            case RoomPhase.Selected:
            case RoomPhase.EmptySpace:
                m_ViewType = ViewType.EDIT;
                break;

            case RoomPhase.Detail:
                m_ViewType = ViewType.FURNITURE;
                break;

            case RoomPhase.View:
                m_ViewType = ViewType.VIEW;
                break;

            default:
                m_ViewType = ViewType.EDIT;
                break;
        }

        await ChangeViewType(m_ViewType, roomObject);
        Debug.Log("Camera Move End");
    }

    public virtual void HandleRoomEvent(RoomEvent roomEvent)
    {

    }

    public virtual async UniTask ChangeViewType(ViewType viewType, RoomObject roomObject)
    {
        await UniTask.DelayFrame(1, cancellationToken: this.GetCancellationTokenOnDestroy());
    }

    public virtual async UniTask MoveToTargetAsync(ViewType viewType, RoomObject roomObject)
    {
        await UniTask.DelayFrame(1, cancellationToken: this.GetCancellationTokenOnDestroy());
    }

    public virtual UniTask<bool> HandleUIEventAsync(RoomUIEvent roomUIEvent)
    {
        return UniTask.FromResult(false);
    }
}

public enum ViewType
{
    EDIT = 0,
    FURNITURE = 1,
    ITEM = 2,
    VIEW = 3,
}