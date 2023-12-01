using UnityEngine;

public interface IMockInputProvider
{
    int TouchCount { get; }
    Touch GetTouch(int index);
    public void OnUpdate();
}

public class MobileInputProvider : IMockInputProvider
{
    public int TouchCount => Input.touchCount;
    public Touch GetTouch(int index) => Input.GetTouch(index);

    public void OnUpdate()
    {

    }
}

public class EditorInputProvider : IMockInputProvider
{
    public int TouchCount => Input.GetMouseButton(0) || Input.GetMouseButtonUp(0) ? 1 : 0;

    private Vector2 lastPosition;
    private Touch m_Touch;

    public EditorInputProvider()
    {
        m_Touch = new Touch();
    }

    public Touch GetTouch(int index)
    {
        return m_Touch;
    }

    public void OnUpdate()
    {
        m_Touch.phase = TouchPhase.Stationary;

        if (Input.GetMouseButtonDown(0))
        {
            m_Touch.phase = TouchPhase.Began;
            lastPosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            Vector2 currentPosition = Input.mousePosition;
            Vector2 deltaPosition = currentPosition - lastPosition;
            if (deltaPosition.magnitude >= 1f)
            {
                deltaPosition = new Vector2(deltaPosition.x / Screen.width, deltaPosition.y / Screen.height);
                m_Touch.phase = TouchPhase.Moved;
                m_Touch.deltaPosition = deltaPosition;
                lastPosition = currentPosition;
            }
            else
            {
                m_Touch.phase = TouchPhase.Stationary;
                m_Touch.deltaPosition = Vector2.zero;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Vector2 currentPosition = Input.mousePosition;
            Vector2 deltaPosition = currentPosition - lastPosition;
            if (deltaPosition.magnitude >= 1f)
            {
                deltaPosition = new Vector2(deltaPosition.x / Screen.width, deltaPosition.y / Screen.height);
                m_Touch.deltaPosition = deltaPosition;
                lastPosition = currentPosition;
            }
            else
            {
                m_Touch.deltaPosition = Vector2.zero;
            }
            m_Touch.phase = TouchPhase.Ended;
        }
    }
}
