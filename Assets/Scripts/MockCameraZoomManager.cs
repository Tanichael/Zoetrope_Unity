using UnityEngine;

public class MockCameraZoomManager
{
    private float m_BefDistance;
    private float m_MinFov;
    private float m_MaxFov;
    private float m_MinMargin;
    private float m_MaxMargin;
    private float m_TargetFov;

    public MockCameraZoomManager(float minFov, float maxFov, float minMargin, float maxMargin)
    {
        m_MinFov = minFov;
        m_MaxFov = maxFov;
        m_MinMargin = minMargin;
        m_MaxMargin = maxMargin;
        m_TargetFov = maxFov;
    }

    public float Zoom(float fov)
    {
        float nextFov = fov;

        if(Input.touchCount == 2)
        {
            TouchPhase phase = TouchPhase.Stationary;
            foreach(Touch touch in Input.touches) 
            { 
                if(touch.phase == TouchPhase.Began)
                {
                    phase = TouchPhase.Began;
                    break;
                }
                if(touch.phase == TouchPhase.Ended)
                {
                    phase = TouchPhase.Ended;
                    break;
                }
            }
            if (phase == TouchPhase.Ended)
            {
                Debug.Log("ZoomEnd");
                float deltaDistance = GetDeltaDistance();
                m_TargetFov = fov + deltaDistance * 30f * 10f;
                m_TargetFov = Mathf.Clamp(m_TargetFov, m_MinFov - m_MinMargin, m_MaxFov + m_MaxMargin);
            }
            else
            {
                m_TargetFov = fov;

                if (phase == TouchPhase.Began)
                {
                    m_BefDistance = GetDeltaDistance();
                }
                else
                {
                    float deltaDistance = GetDeltaDistance();
                    nextFov = fov + deltaDistance * 30f;
                    nextFov = Mathf.Clamp(nextFov, m_MinFov - m_MinMargin, m_MaxFov + m_MaxMargin);
                }
            }
        }

        if(Mathf.Abs(m_TargetFov - fov) > 0.01f)
        {
            nextFov = Mathf.Lerp(fov, m_TargetFov, 0.2f);
            if (Mathf.Abs(m_TargetFov - fov) <= 4f)
            {
                m_TargetFov = Mathf.Min(Mathf.Max(m_MinFov, fov), m_MaxFov);
            }
        }
        else
        {
            m_TargetFov = Mathf.Min(Mathf.Max(m_MinFov, fov), m_MaxFov);
        }

        return nextFov;
    }

    private float GetDeltaDistance()
    {
        if (Input.touchCount == 2)
        {
            Touch tZero = Input.GetTouch(0);
            Touch tOne = Input.GetTouch(1);

            Vector2 tZeroPrevious = tZero.position - tZero.deltaPosition;
            Vector2 tOnePrevious = tOne.position - tOne.deltaPosition;

            /*tZeroPrevious.x /= Screen.width;
            tZeroPrevious.y /= Screen.height;
            tOnePrevious.x /= Screen.width;
            tOnePrevious.y /= Screen.height;*/

            float oldTouchDistance = Vector2.Distance(tZeroPrevious, tOnePrevious);
            float currentTouchDistance = Vector2.Distance(tZero.position, tOne.position);

            return (oldTouchDistance - currentTouchDistance) * 0.01f;
        }
        return 0f;
    }
}
