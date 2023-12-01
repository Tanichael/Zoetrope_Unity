using UnityEngine;

public interface ITappable
{
    public void OnTap();
    public void OnHold();
    public void OnDoubleTap();
    public GameObject GetGameObject();
}
