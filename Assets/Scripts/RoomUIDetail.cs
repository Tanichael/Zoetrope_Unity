using UnityEngine;
using UnityEngine.UI;

public class RoomUIDetail : RoomUIBase
{
    [SerializeField] Button m_DeltaRotateButton;

    public override RoomPhase GetRoomPhase()
    {
        return RoomPhase.Detail;
    }

    public override void OnEnterState()
    {
        base.OnEnterState();
        m_Machine.FurniturePanel.SetActive(true);
        m_Machine.BackButton.gameObject.SetActive(true);
        m_Machine.SelectedCanvas.gameObject.SetActive(true);

        if (m_RoomManager.SelectedObject.Data.Type == RoomObjectType.ITEM)
        {
            m_Machine.EditButton.gameObject.SetActive(true);
        }

        if (m_RoomManager.SelectedObject != null)
        {
            if (m_RoomManager.SelectedObject is RoomObjectFixed)
            {
                m_DeltaRotateButton.gameObject.SetActive(false);
            }
            else
            {
                m_DeltaRotateButton.gameObject.SetActive(true);
            }
        }
        m_DeltaRotateButton.onClick.AddListener(() =>
        {
            PublishUIEvent(new DeltaRotateStartEvent());
        });
    }

    public override void OnExitState()
    {
        base.OnExitState();
        m_DeltaRotateButton.gameObject.SetActive(false);
        m_DeltaRotateButton.onClick.RemoveAllListeners();
    }

    private void Update()
    {
        if(!m_Machine.EditButton.gameObject.activeSelf && m_RoomManager.SelectedObject.Data.Type == RoomObjectType.ITEM)
        {
            m_Machine.EditButton.gameObject.SetActive(true);
        }
    }

    protected override void PublishUIEvent(RoomUIEvent roomUIEvent)
    {
        base.PublishUIEvent(roomUIEvent);
    }
}
