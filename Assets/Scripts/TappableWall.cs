using UnityEngine;
using System.Collections.Generic;
using UniRx;

public class TappableWall : MonoBehaviour, ITappable
{
    [SerializeField] MeshRenderer m_MeshRenderer;
    [SerializeField] List<Color> m_Colors;

    private ReactiveProperty<int> m_ColorIdx = new ReactiveProperty<int>();

    void Start()
    {
        m_ColorIdx.Value = 0;
        m_ColorIdx.Subscribe(value =>
        {
            value %= m_Colors.Count;
            m_MeshRenderer.material.color = m_Colors[value];
        }).AddTo(this);
    }

    void OnDestroy()
    {
        m_MeshRenderer.material.color = m_Colors[0];
    }

    public GameObject GetGameObject()
    {
        return null;
    }

    public void OnHold()
    {
        Debug.Log("OnHold");
        m_ColorIdx.Value++;
        m_ColorIdx.Value %= m_Colors.Count;
    }

    public void OnTap()
    {
        Debug.Log("OnTap");
        m_ColorIdx.Value++;
        m_ColorIdx.Value %= m_Colors.Count;
    }

    public void OnDoubleTap()
    {
        
    }
}
