using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class RoomUIView : RoomUIBase
{
    [SerializeField] private Canvas m_ViewCanvas;
    [SerializeField] private Button m_RoomEditButton;
    [SerializeField] private Button m_VirtualCamButton;

    public override RoomPhase GetRoomPhase()
    {
        return RoomPhase.View;
    }

    public override void Init(RoomUIMachine machine, MockRoomManager roomManager, IInputProvider inputProvider)
    {
        base.Init(machine, roomManager, inputProvider);
        m_ViewCanvas.gameObject.SetActive(false);
        m_RoomEditButton.gameObject.SetActive(false);
        m_VirtualCamButton.gameObject.SetActive(false);

        m_RoomEditButton.onClick.AddListener(() =>
        {
            PublishUIEvent(new RoomEditButtonClickEvent());
        });

        m_VirtualCamButton.OnClickAsObservable().Subscribe(_ =>
        {
            PublishUIEvent(new VirtualCamButtonClickEvent());
        }).AddTo(this);
    }

    public override void OnEnterState()
    {
        base.OnEnterState();
        m_ViewCanvas.gameObject.SetActive(true);
        m_RoomEditButton.gameObject.SetActive(true);
        m_VirtualCamButton.gameObject.SetActive(true);

        RectTransform roomEditRectTransform = m_RoomEditButton.transform as RectTransform;
        if(roomEditRectTransform != null)
        {
            roomEditRectTransform.sizeDelta = new Vector2(Screen.width * 0.2f, Screen.height * 0.08f);
            roomEditRectTransform.anchoredPosition = (-1f) * new Vector2(Screen.width * 0.2f, Screen.height * 0.08f);
        }

        RectTransform virtualCamRectTransform = m_VirtualCamButton.transform as RectTransform;
        if (virtualCamRectTransform != null)
        {
            virtualCamRectTransform.sizeDelta = new Vector2(Screen.width * 0.2f, Screen.height * 0.08f);
            virtualCamRectTransform.anchoredPosition = (-1f) * new Vector2(Screen.width * 0.2f, Screen.height * 0.2f);
        }
    }

    public override void OnExitState()
    {
        Debug.Log("exit view");
        m_ViewCanvas.gameObject.SetActive(false);
        m_RoomEditButton.gameObject.SetActive(false);
        m_VirtualCamButton.gameObject.SetActive(false);
        base.OnExitState();
    }

    protected override void PublishUIEvent(RoomUIEvent roomUIEvent)
    {
        base.PublishUIEvent(roomUIEvent);
    }
}
