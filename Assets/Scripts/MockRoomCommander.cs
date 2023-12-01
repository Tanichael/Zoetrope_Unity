using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;

public class MockRoomCommander : IRoomCommander, IDisposable
{
    private readonly CompositeDisposable ms_Disposables = new CompositeDisposable();

    private RoomPhaseMachine m_Machine;
    private MockRoomManager m_RoomManager;
    private List<IRoomCommand> m_ExecutedCommands;
    private IRoomCommand m_LastCommand;
    private readonly int ms_CommandCashCount = 20;
    private int m_TopIndex = 0;
    private CancellationTokenSource m_Cts;

    public MockRoomCommander(MockRoomManager roomManager, RoomPhaseMachine phaseMachine)
    {
        m_Machine = phaseMachine;
        m_RoomManager = roomManager;

        m_RoomManager.OnTapRoomObject.Subscribe(async (roomObject) =>
        {
            try
            {
                bool result = await ExecuteCommandAsync(new RoomCommandSelectObject(m_RoomManager, roomObject, m_Machine, this), m_Cts.Token);
            }
            catch(OperationCanceledException ex)
            {
#if UNITY_EDITOR
                UnityEngine.Debug.Log("Operation Cancelled: " + ex.ToString());
#endif
            }
        }).AddTo(ms_Disposables);

        m_RoomManager.OnHoldRoomObject.Subscribe((roomObject) =>
        {
            try
            {
                m_RoomManager.SetControlObject(roomObject);
            }
            catch (OperationCanceledException ex)
            {
#if UNITY_EDITOR
                UnityEngine.Debug.Log("Operation Cancelled: " + ex.ToString());
#endif
            }
        }).AddTo(ms_Disposables);

        m_ExecutedCommands = new List<IRoomCommand>();
        for(int i = 0; i < ms_CommandCashCount; i++)
        {
            m_ExecutedCommands.Add(null);
        }
        m_Cts = new CancellationTokenSource();
    } 

    public bool ExecuteUIEvent(RoomUIEvent uiEvent, RoomPhaseBase roomPhase)
    {
        if(uiEvent is UndoButtonClickEvent)
        {
            Undo();
            return true;
        }

        IRoomCommand roomCommand = uiEvent.GetCommand(roomPhase, m_RoomManager, m_Machine, this);
        if(roomCommand == null)
        {
            Debug.Log("command is not set to this UI Event");
            for (int i = 0; i < ms_CommandCashCount; i++)
            {
                m_ExecutedCommands[i] = null;
            }
            return false;
        }
        else
        {
            UniTask<bool> task = ExecuteCommandAsync(roomCommand, m_Cts.Token);
            return true;
        }
    }

    public async UniTask<bool> ExecuteCommandAsync(IRoomCommand command, CancellationToken token)
    {
        bool isSuccess = await command.ExecuteAsync(token);
        m_LastCommand = command;
        if(command.CanUndo())
        {
            m_ExecutedCommands[m_TopIndex] = command;
            m_TopIndex = (m_TopIndex + 1) % ms_CommandCashCount;
        }
        return isSuccess;
    }

    public bool Undo()
    {
        bool isSuccess = false;
        int undoIndex = (m_TopIndex - 1 + ms_CommandCashCount) % ms_CommandCashCount;
        IRoomCommand lastCommand = m_ExecutedCommands[undoIndex];
        if(lastCommand != null)
        {
            lastCommand.Undo();
            isSuccess = true;
            m_TopIndex = undoIndex;
        }
        else
        {
            Debug.Log("前のコマンドが存在しません");
            isSuccess = false;
        }
        return isSuccess;
    }

    public void Dispose()
    {
        ms_Disposables.Dispose();
        m_Cts.Cancel();
        m_Cts.Dispose();
    }
}
