using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomPhaseTransition : RoomPhaseBase
{
    public RoomPhaseTransition(RoomPhaseMachine machine, MockRoomManager roomManager, IRoomCommander roomCommander) : base(machine, roomManager, roomCommander)
    {
    }

    public override RoomPhase GetRoomPhase()
    {
        return RoomPhase.Transition;
    }

    public override void OnEnterState()
    {
        base.OnEnterState();
        Debug.Log("RoomPhase -> Transition");
    }
}
