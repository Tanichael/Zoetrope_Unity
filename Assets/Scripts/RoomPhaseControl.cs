using UnityEngine;
using System;
using System.Threading;

public class RoomPhaseControl : RoomPhaseBase, IDisposable
{
    private RaycastHit m_StartHit;
    private Vector3Int m_ControlHitBefPos;
    private Vector3Int m_ControlHitOffSet;
    private CancellationTokenSource m_Cts;

    public RoomPhaseControl(RoomPhaseMachine machine, MockRoomManager roomManager, IRoomCommander roomCommander) : base(machine, roomManager, roomCommander)
    {
        m_Cts = new CancellationTokenSource();
    }

    public override RoomPhase GetRoomPhase()
    {
        return RoomPhase.Control;
    }


    public override void OnEnterState()
    {
        Debug.Log("RoomPhase -> Control");
        m_RoomCommander.ExecuteCommandAsync(new RoomCommandMoveObject(m_RoomManager, m_StartHit, m_Machine, m_RoomCommander), m_Cts.Token);
    }

    public void Dispose()
    {
        m_Cts.Cancel();
        m_Cts.Dispose();
    }
}
