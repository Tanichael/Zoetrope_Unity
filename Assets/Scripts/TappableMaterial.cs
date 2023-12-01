using UnityEngine;
using System;
using UniRx;

public class TappableMaterial : TappableBehaviour
{
    [SerializeField] int m_MaterialNum;

    private Subject<int> m_OnTapMaterialSubject = new Subject<int>();
    public IObservable<int> OnTapMaterial => m_OnTapMaterialSubject;

    public override void OnHold()
    {

    }

    public override void OnTap()
    {
        m_OnTapMaterialSubject.OnNext(m_MaterialNum);
    }
}
