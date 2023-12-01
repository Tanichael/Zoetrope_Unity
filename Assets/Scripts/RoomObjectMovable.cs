using UnityEngine;

public class RoomObjectMovable : RoomObject, ITappable
{
    public void OnTap()
    {
        m_OnTapRoomObject.OnNext(this);
    }

    public void OnHold()
    {
        m_OnHoldRoomObject.OnNext(this);
    }

    public void OnDoubleTap()
    {
        m_OnDoubleTapRoomObject.OnNext(this);
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }
}
