using UnityEngine;

public class MockCameraRotateManager
{
    private float m_Theta;
    private float m_Phi;
    private float m_FovThreshold;

    private Vector2 m_BefPos;
    private Vector3 m_TargetPos;

    public MockCameraRotateManager(float fovThreshold, Vector3 initPos)
    {
        m_FovThreshold = fovThreshold;
        m_TargetPos = initPos;
    }

    public Vector3 Rotate(Vector3 befPosition, float fov, Vector3 center)
    {
        float radius = Vector3.Distance(befPosition, center);
        float thetaRad = CalcThetaRad(befPosition, center);
        float phiRad = CalcPhiRad(befPosition, center);
        Vector3 nextPos = befPosition;

        if (fov >= m_FovThreshold)
        {
            if (Input.touchCount == 1)
            {
                if(Input.touches[0].phase == TouchPhase.Ended)
                {
                    Vector2 offset = Input.touches[0].deltaPosition;
                    phiRad += offset.x * 5f / (float)Screen.width;
                    thetaRad -= offset.y * 25f / (float)Screen.height;

                    phiRad = Mathf.Clamp(phiRad, 3f * Mathf.Deg2Rad, 88f * Mathf.Deg2Rad);
                    thetaRad = Mathf.Clamp(thetaRad, 7f * Mathf.Deg2Rad, 88f * Mathf.Deg2Rad);

                    m_TargetPos = center + radius * new Vector3(Mathf.Cos(thetaRad) * Mathf.Cos(phiRad), Mathf.Sin(thetaRad), -Mathf.Cos(thetaRad) * Mathf.Sin(phiRad));
                }
                else
                {
                    m_TargetPos = befPosition;

                    Vector2 offset = Input.touches[0].deltaPosition;
                    phiRad += offset.x / (float)Screen.width * 1f;
                    thetaRad -= offset.y / (float)Screen.height * 5f;

                    phiRad = Mathf.Clamp(phiRad, 5f * Mathf.Deg2Rad, 85f * Mathf.Deg2Rad);
                    thetaRad = Mathf.Clamp(thetaRad, 10f * Mathf.Deg2Rad, 85f * Mathf.Deg2Rad);

                    nextPos = center + radius * new Vector3(Mathf.Cos(thetaRad) * Mathf.Cos(phiRad), Mathf.Sin(thetaRad), -Mathf.Cos(thetaRad) * Mathf.Sin(phiRad));
                }
            }

            if(Vector3.Distance(m_TargetPos, befPosition) > 0.01f)
            {
                nextPos = Vector3.Lerp(befPosition, m_TargetPos, 0.1f);
                float nextPhi = CalcPhiRad(nextPos, center);
                float nextTheta = CalcThetaRad(nextPos, center);

                float promisePhi = Mathf.Clamp(nextPhi, 5f * Mathf.Deg2Rad, 85f * Mathf.Deg2Rad);
                float promiseTheta = Mathf.Clamp(nextTheta, 10f * Mathf.Deg2Rad, 85f * Mathf.Deg2Rad);

                nextPos = center + radius * new Vector3(Mathf.Cos(promiseTheta) * Mathf.Cos(promisePhi), Mathf.Sin(promiseTheta), -Mathf.Cos(promiseTheta) * Mathf.Sin(promisePhi));

                if (promisePhi != nextPhi || promiseTheta != nextTheta)
                {
                    m_TargetPos = center + radius * new Vector3(Mathf.Cos(promiseTheta) * Mathf.Cos(promisePhi), Mathf.Sin(promiseTheta), -Mathf.Cos(promiseTheta) * Mathf.Sin(promisePhi));
                }
            }
        }
        else
        {

        }

        return nextPos;
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
