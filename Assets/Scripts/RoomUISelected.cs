using UnityEngine;
using UnityEngine.UI;

public class RoomUISelected : RoomUIBase
{
    [SerializeField] private Button m_UnselectButton;
    [SerializeField] private Button m_DeltaRotateButton;

    private RoomObject m_TempObject;

    public override RoomPhase GetRoomPhase()
    {
        return RoomPhase.Selected;
    }

    public override void Init(RoomUIMachine machine, MockRoomManager roomManager, IInputProvider inputProvider)
    {
        base.Init(machine, roomManager, inputProvider);
        m_UnselectButton.onClick.AddListener(() =>
        {
            PublishUIEvent(new UnselectButtonClickEvent());
        });

        RectTransform unselectTransform = m_UnselectButton.transform as RectTransform;
        if(unselectTransform != null)
        {
            unselectTransform.sizeDelta = new Vector2(Screen.width / 2.5f, Screen.width / 2.5f / 3f);
            float dx = 0f;
            float dy = unselectTransform.sizeDelta.y * 1f + Screen.height * 0.5f;
            unselectTransform.anchoredPosition = new Vector2(dx, dy);
        }

        RectTransform deltaRotateRectTransform = m_DeltaRotateButton.gameObject.transform as RectTransform;
        if (deltaRotateRectTransform != null)
        {
            deltaRotateRectTransform.sizeDelta = new Vector2(Screen.width / 5f, Screen.width / 2.5f / 3f);
            float dx = -Screen.width / 4f;
            float dy = deltaRotateRectTransform.sizeDelta.y + Screen.height * 0.37f;
            deltaRotateRectTransform.anchoredPosition = new Vector2(dx, dy);
        }
    }

    public override void OnEnterState()
    {
        base.OnEnterState();
        m_DeltaRotateButton.onClick.AddListener(() =>
        {
            PublishUIEvent(new DeltaRotateStartEvent());
        });

        m_TempObject = m_RoomManager.SelectedObject;

        if (m_RoomManager.SelectedObject is RoomObjectFixed)
        {
            m_Machine.FurniturePanel.SetActive(true);
            m_Machine.DetailButton.gameObject.SetActive(true);
            m_Machine.SelectedCanvas.gameObject.SetActive(true);
            m_Machine.ItemCanvas.gameObject.SetActive(true);
            m_DeltaRotateButton.gameObject.SetActive(false);
            m_Machine.DeleteButton.gameObject.SetActive(false);
        }
        else
        {
            m_Machine.FurniturePanel.SetActive(true);
            m_Machine.DetailButton.gameObject.SetActive(true);
            m_Machine.DeleteButton.gameObject.SetActive(true);
            m_Machine.SelectedCanvas.gameObject.SetActive(true);
            m_Machine.ItemCanvas.gameObject.SetActive(true);
            m_DeltaRotateButton.gameObject.SetActive(true);
            m_Machine.DeleteButton.gameObject.SetActive(true);
        }

        if (m_RoomManager.SelectedObject != null)
        {
            if (m_RoomManager.SelectedObject.Data.Type == RoomObjectType.ITEM)
            {
                m_Machine.EditButton.gameObject.SetActive(true);
            }
        }


    }

    private void Update()
    {
        if (!m_Machine.EditButton.gameObject.activeSelf && m_RoomManager.SelectedObject != null && m_RoomManager.SelectedObject.Data.Type == RoomObjectType.ITEM)
        {
            m_Machine.EditButton.gameObject.SetActive(true);
        }

        if(m_TempObject != m_RoomManager.SelectedObject)
        {
            m_TempObject = m_RoomManager.SelectedObject;

            if (m_RoomManager.SelectedObject is RoomObjectFixed)
            {
                m_Machine.FurniturePanel.SetActive(true);
                //m_Machine.RotateButton.gameObject.SetActive(true);
                m_Machine.DetailButton.gameObject.SetActive(true);
                m_Machine.SelectedCanvas.gameObject.SetActive(true);
                m_Machine.ItemCanvas.gameObject.SetActive(true);
                m_DeltaRotateButton.gameObject.SetActive(false);
                m_Machine.DeleteButton.gameObject.SetActive(false);
            }
            else if(m_RoomManager.SelectedObject != null)
            {
                m_Machine.FurniturePanel.SetActive(true);
                //m_Machine.RotateButton.gameObject.SetActive(true);
                m_Machine.DetailButton.gameObject.SetActive(true);
                m_Machine.DeleteButton.gameObject.SetActive(true);
                m_Machine.SelectedCanvas.gameObject.SetActive(true);
                m_Machine.ItemCanvas.gameObject.SetActive(true);
                m_DeltaRotateButton.gameObject.SetActive(true);
                m_Machine.DeleteButton.gameObject.SetActive(true);
            }
        }

    }

    public override void OnExitState()
    {
        base.OnExitState();
        m_DeltaRotateButton.gameObject.SetActive(false);
        m_Machine.DeleteButton.gameObject.SetActive(false);
        m_DeltaRotateButton.onClick.RemoveAllListeners();
    }

    protected override void PublishUIEvent(RoomUIEvent roomUIEvent)
    {
        base.PublishUIEvent(roomUIEvent);
    }

}
