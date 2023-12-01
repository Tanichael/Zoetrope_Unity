using UnityEngine;

public static class MockRoomUtilities
{
    

}
public static class Vector3Extensions
{
    public static Vector3 RotateAngleXZPlane(this Vector3 vec, float angle)
    {
        float angleRad = angle * Mathf.Deg2Rad;
        return new Vector3(vec.x * Mathf.Cos(angleRad) - vec.z * Mathf.Sin(angleRad), vec.y, vec.x * Mathf.Sin(angleRad) + vec.z * Mathf.Cos(angleRad));
    }
}