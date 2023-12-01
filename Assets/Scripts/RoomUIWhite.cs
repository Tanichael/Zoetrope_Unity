using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomUIWhite : RoomUIBase
{
    public override RoomPhase GetRoomPhase()
    {
        return RoomPhase.White;
    }

    public override void OnEnterState()
    {
        base.OnEnterState();
        m_Machine.FurniturePanel.SetActive(true);
    }
}
