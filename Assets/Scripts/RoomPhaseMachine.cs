using UnityEngine;
using System;
using UniRx;

public class RoomPhaseMachine : MonoBehaviour
{
    [SerializeField] RoomPhase m_DisplayPhase = RoomPhase.View;

    private ITouchManager m_TouchManager;
    public ITouchManager TouchManager => m_TouchManager;
    private IRoomCommander m_RoomCommander;
    private MockRoomManager m_RoomManager;
    private RoomPhaseBase m_CurrentPhase;
    private RoomPhaseBase m_NextPhase;
    private bool m_IsInLogEvent;
    private CameraController m_CurrentCameraController;

    public RoomPhaseBase CurrentPhase => m_CurrentPhase;
    private Subject<RoomPhaseBase> m_OnChangePhase = new Subject<RoomPhaseBase>();
    private Subject<RoomPhaseBase> m_OnTransitPhase = new Subject<RoomPhaseBase>();
    private Subject<RoomEvent> m_OnRoomEvent = new Subject<RoomEvent>();

    public IObservable<RoomPhaseBase> OnChangePhse => m_OnChangePhase;
    public IObservable<RoomPhaseBase> OnTransitPhase => m_OnTransitPhase;
    public IObservable<RoomEvent> OnRoomEvent => m_OnRoomEvent;

    public CameraController CurrentCameraController { get; set; }

    public bool ChangePhase(RoomPhaseBase nextPhase)
    {
        bool isNull = nextPhase == null;
        if (!isNull)
        {
            if (m_OnChangePhase != null) m_OnChangePhase.OnNext(nextPhase);
        }
        m_NextPhase = nextPhase;
        return isNull;
    }

    public bool FireRoomEvent(RoomEvent roomEvent)
    {
        bool isNull = roomEvent == null;

        if(roomEvent is RoomEventLogEnd)
        {
            m_IsInLogEvent = false;
        }
        else
        {
            m_IsInLogEvent = true;
        }
        m_OnRoomEvent.OnNext(roomEvent);
        return isNull;
    }

    public bool TransitPhase(RoomPhaseBase nextPhase)
    {
        bool isNull = nextPhase == null;
        m_NextPhase = new RoomPhaseTransition(this, m_RoomManager, m_RoomCommander);
        if(!isNull) m_OnTransitPhase.OnNext(nextPhase);
        return isNull;
    }

    public void Init(MockRoomManager roomManager, ITouchManager touchManager, IRoomCommander roomCommander)
    {
        m_RoomManager = roomManager;
        m_TouchManager = touchManager;
        m_RoomCommander = roomCommander;
    }

    private void Start()
    {
        ChangePhase(new RoomPhaseView(this, m_RoomManager, m_RoomCommander));
        m_IsInLogEvent = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_IsInLogEvent) return;

        if (m_NextPhase != null)
        {
            if(m_CurrentPhase != null)
            {
                m_CurrentPhase.OnExitState();
            }
            m_CurrentPhase = m_NextPhase;
            m_CurrentPhase.OnEnterState();
            m_NextPhase = null;
        }

        if(m_CurrentPhase != null)
        {
            m_CurrentPhase.OnUpdate();
            m_DisplayPhase = m_CurrentPhase.GetRoomPhase();
        }
    }

    private void OnDestroy()
    {
        m_CurrentPhase = null;
    }
}
public enum RoomPhase
{
    None = 0,
    Selected = 1,
    Detail = 2,
    EmptySpace = 3,
    White = 4,
    Control = 5,
    Transition = 6,
    Picture = 7,
    Trim = 8,
    View = 9,
    DeltaRotate = 10,
    VirtualCam = 11,
}