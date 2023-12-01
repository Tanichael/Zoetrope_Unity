using UnityEngine;
using System;
using UniRx;

public class TappableBehaviour : MonoBehaviour, ITappable
{
    private Subject<int> m_OnTapBehaviourSubject = new Subject<int>();
    public IObservable<int> OnTapBehaviour => m_OnTapBehaviourSubject;

    public virtual void OnHold()
    {

    }

    public virtual void OnTap()
    {
        m_OnTapBehaviourSubject.OnNext(0);
    }

    public virtual void OnDoubleTap()
    {

    }

    public virtual GameObject GetGameObject()
    {
        return gameObject;
    }
}
