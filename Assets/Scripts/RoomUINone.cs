using UniRx;
using UnityEngine;
using UnityEngine.UI;
using System;

public class RoomUINone : RoomUIBase
{
    [SerializeField] Button m_EndEditButton;

    public override RoomPhase GetRoomPhase()
    {
        return RoomPhase.None;
    }

    public override void Init(RoomUIMachine machine, MockRoomManager roomManager, IInputProvider inputProvider)
    {
        base.Init(machine, roomManager, inputProvider);

        m_EndEditButton.onClick.AsObservable()
            .ThrottleFirst(TimeSpan.FromSeconds(0.2))
            .Subscribe(_ =>
            {
                Debug.Log("end edit");
                PublishUIEvent(new EndEditButtonClickEvent());
            })
            .AddTo(this);
    }

    public override void OnEnterState()
    {
        base.OnEnterState();

        m_Machine.FurniturePanel.SetActive(true);
        m_Machine.ItemCanvas.gameObject.SetActive(true);
        m_EndEditButton.gameObject.SetActive(true);
        //m_Machine.RemoveAllButton.gameObject.SetActive(true);

        RectTransform endEditRectTransform = m_EndEditButton.transform as RectTransform;
        if(endEditRectTransform != null) 
        {
            endEditRectTransform.sizeDelta = new Vector2(Screen.width / 5f, Screen.width / 2.5f / 3f);
            float dx = Screen.width / 6f;
            float dy = - Screen.width * 0.133f;
            endEditRectTransform.anchoredPosition = new Vector2(dx, dy);
        }
        
    }

    public override void OnExitState()
    {
        m_EndEditButton.gameObject.SetActive(false);
        base.OnExitState();
    }

    protected override void PublishUIEvent(RoomUIEvent roomUIEvent)
    {
        base.PublishUIEvent(roomUIEvent);
    }
}
