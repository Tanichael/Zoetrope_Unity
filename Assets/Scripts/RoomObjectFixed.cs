using UnityEngine;

public class RoomObjectFixed : RoomObject, ITappable
{
    public void OnHold()
    {
        OnTap();
    }

    public void OnTap()
    {
        m_OnTapRoomObject.OnNext(this);
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
