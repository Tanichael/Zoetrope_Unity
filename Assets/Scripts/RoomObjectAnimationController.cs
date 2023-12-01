using UnityEngine;
using System;
using System.Collections.Generic;
using UniRx;

public class RoomObjectAnimationController
{
    private RoomObjectAnimationBase m_CurrentPhase;
    private RoomObjectAnimationBase m_NextPhase;

    private Queue<RoomObjectAnimationBase> m_AnimationQueue;

    public RoomObjectAnimationController(Transform transform)
    {
        Vector3 originScale = transform.localScale;
        m_AnimationQueue = new Queue<RoomObjectAnimationBase>();
    }

    public void OnUpdate()
    {
        if(m_AnimationQueue.Count > 0)
        {
            RoomObjectAnimationBase animation = m_AnimationQueue.Peek();
            bool isTransition = false;
            if(m_CurrentPhase != null)
            {
                if(m_CurrentPhase.IsLoop)
                {
                    m_CurrentPhase.OnExitState();
                    isTransition = true;
                }
                else if(!m_CurrentPhase.GetIsPlaying())
                {
                    m_CurrentPhase.OnExitState();
                    isTransition = true;
                }
            }
            else
            {
                isTransition = true;
            }

            if(isTransition)
            {
                m_CurrentPhase = animation;
                m_CurrentPhase.OnEnterState();
                m_AnimationQueue.Dequeue();
            }
        }

        if(m_CurrentPhase != null)
        {
            m_CurrentPhase.OnUpdate();
        }
    }

    public bool ChangePhase(params RoomObjectAnimationBase[] nextPhases)
    {
        bool isNull = nextPhases == null;
        for(int i = 0; i < nextPhases.Length; i++)
        {
            RoomObjectAnimationBase nextPhase = nextPhases[i];
            m_AnimationQueue.Enqueue(nextPhase);
        }
        return isNull;
    }

    public void OnDestroy()
    {
        if(m_CurrentPhase != null)
        {
            m_CurrentPhase.ForceStop();
            m_CurrentPhase.OnExitState();
            m_CurrentPhase = null;
        }
        if(m_NextPhase != null)
        {
            m_NextPhase = null;
        }
    }
}

public enum RoomObjectPhase
{
    None = 0,
    Put = 1,
    Selected = 2,
    Control = 3,
}