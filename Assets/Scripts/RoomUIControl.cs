using UnityEngine;

public class RoomUIControl : RoomUIBase
{
    public override RoomPhase GetRoomPhase()
    {
        return RoomPhase.Control;
    }
}
