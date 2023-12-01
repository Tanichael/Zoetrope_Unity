using UnityEngine;

public interface IInputProvider
{
    public bool GetIsDragging();
    public bool GetIsDragEnd();
    public Vector2 GetDragPosition();
    public Vector2 GetDragDeltaPosition();
    public Vector2 GetDragSpeed();
    public Vector2 GetBefDragPosition();
    public bool GetIsMultiDragging();
    public bool GetIsMultiDragEnd();
    public Vector2 GetMultiDragPosition();
    public Vector2 GetMultiDragDeltaPosition();
    public bool GetIsZooming();
    public float GetZoomDeltaDistance();
    public float GetZoomSpeed();

    public Vector2 GetDragDeltaWithID(int id);
}
