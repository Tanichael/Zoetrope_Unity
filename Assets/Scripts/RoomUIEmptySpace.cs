public class RoomUIEmptySpace : RoomUIBase
{
    public override RoomPhase GetRoomPhase()
    {
        return RoomPhase.EmptySpace;
    }

    public override void OnEnterState()
    {
        base.OnEnterState();
        m_Machine.FurniturePanel.SetActive(true);
        m_Machine.BackButton.gameObject.SetActive(true);
        m_Machine.SelectedCanvas.gameObject.SetActive(true);
        m_Machine.ItemCanvas.gameObject.SetActive(true);
    }
}
