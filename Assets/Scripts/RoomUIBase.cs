using UnityEngine;
using System;
using UniRx;

public abstract class RoomUIBase : MonoBehaviour
{
    protected IInputProvider m_InputProvider;
    protected RoomUIMachine m_Machine;
    protected MockRoomManager m_RoomManager;

    private Subject<RoomUIEvent> m_OnUIEvent = new Subject<RoomUIEvent>();
    public IObservable<RoomUIEvent> OnUIEvent => m_OnUIEvent;
    public virtual void Init(RoomUIMachine machine, MockRoomManager roomManager, IInputProvider inputProvider)
    {
        m_Machine = machine;
        m_RoomManager = roomManager;
        m_InputProvider = inputProvider;
    }

    public virtual void OnEnterState()
    {
        gameObject.SetActive(true);
        m_Machine.FurniturePanel.gameObject.SetActive(false);
        m_Machine.SelectedCanvas.gameObject.SetActive(false);
        m_Machine.ItemCanvas.gameObject.SetActive(false);
        m_Machine.DetailButton.gameObject.SetActive(false);
        m_Machine.BackButton.gameObject.SetActive(false);
        m_Machine.RotateButton.gameObject.SetActive(false);
        m_Machine.DeleteButton.gameObject.SetActive(false);
        m_Machine.RemoveAllButton.gameObject.SetActive(false);
        m_Machine.EditButton.gameObject.SetActive(false);
        m_Machine.FurniturePanel.SetActive(false);
        m_Machine.ItemPanel.SetActive(false);
    }

    public virtual void OnExitState() 
    {
        gameObject.SetActive(false);
    }

    public abstract RoomPhase GetRoomPhase();

    protected virtual void PublishUIEvent(RoomUIEvent roomUIEvent)
    {
        m_OnUIEvent?.OnNext(roomUIEvent);
    }
}
