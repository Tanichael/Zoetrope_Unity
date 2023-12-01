using UnityEngine;

public class MockCameraMoveManager
{
    float m_FovThreshold;
    Vector3 m_InitPos;
    Vector3 m_TargetPos;
    float m_ZoomMaxBound;
    Vector3 m_InitCenter;
    float m_InitRadius;

    public MockCameraMoveManager(float fovThreshold, Vector3 initPos, float maxFov, Vector3 initCenter)
    {
        m_FovThreshold = fovThreshold;
        m_InitPos = initPos;
        m_ZoomMaxBound = maxFov;
        m_TargetPos = initPos;
        m_InitCenter = initCenter;
        m_InitRadius = Vector3.Distance(initCenter, initPos);
    }

    public Vector3 Move(Camera camera, Vector3 befPosition, float fov, Vector3 center)
    {
        float radius = Vector3.Distance(befPosition, center);
        Vector3 newPos = befPosition;

        if(fov < m_FovThreshold)
        {
            if(Input.touchCount == 1)
            {
                if(Input.touches[0].phase == TouchPhase.Ended)
                {
                    Vector2 offset = Input.touches[0].deltaPosition;
                    offset *= (-1f) * 0.2f * Mathf.Cos(Mathf.PI / 180f * m_ZoomMaxBound) / Mathf.Cos(Mathf.PI / 180f * fov);

                    m_TargetPos = befPosition + Vector3.Scale(offset, (camera.transform.right + camera.transform.up));
                }
                else
                {
                    m_TargetPos = befPosition;

                    Vector2 offset = Input.touches[0].deltaPosition;
                    offset *= (-1f) * 0.02f * Mathf.Cos(Mathf.PI / 180f * m_ZoomMaxBound) / Mathf.Cos(Mathf.PI / 180f * fov);

                    newPos = befPosition + offset.x * camera.transform.right + offset.y * camera.transform.up;
                }
            }

            if (Vector3.Distance(m_TargetPos, befPosition) > 0.01f)
            {
                newPos = Vector3.Lerp(befPosition, m_TargetPos, 0.1f);
                Vector3 newOffset = newPos - m_InitCenter;
                float distance = Mathf.Abs(Vector3.Dot(newOffset, m_InitPos - m_InitCenter)) / m_InitRadius;
                Debug.Log("distance: " + distance);
                Debug.Log("Init: " + m_InitRadius);
                if(distance > m_InitRadius)
                {
                    newOffset *= m_InitRadius / distance;
                    m_TargetPos = m_InitCenter + newOffset;
                    newPos = m_InitCenter + newOffset;
                }

            }
        }

        return newPos;
    }

    private float CalcThetaRad(Vector3 position, Vector3 center)
    {
        float radius = Vector3.Distance(position, center);
        Vector3 offsetVec = position - center;
        Vector3 unitVec = offsetVec / radius;
        float thetaRad = Mathf.Asin(unitVec.y);

        return thetaRad;
    }

    private float CalcPhiRad(Vector3 position, Vector3 center)
    {
        float radius = Vector3.Distance(position, center);
        Vector3 offsetVec = position - center;
        Vector3 unitVec = offsetVec / radius;
        float thetaRad = Mathf.Asin(unitVec.y);
        float phiRad = Mathf.Acos(unitVec.x / Mathf.Cos(thetaRad));

        return phiRad;
    }

}
