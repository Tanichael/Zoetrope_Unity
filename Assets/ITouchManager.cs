using UnityEngine;

public interface ITouchManager
{
    public bool GetCanSelect();
}

public class MobileTouchManager : ITouchManager
{
    public bool GetCanSelect()
    {
        if(Input.touchCount == 2)
        {
            return false;
        }
        return true;
    }
}

public class WindowsTouchManager : ITouchManager
{
    public bool GetCanSelect()
    {
        return true;
    }
}
