using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRoomObjectRotater
{
    public float GetDeltaAngleHorizontal();

    public float GetDeltaAngleVertical();

    public void Rotate(Transform transform, float baseAngle, float angleHorizontal, float angleVertical);
}

public class HorizontalFloorRoomObjectRotater : IRoomObjectRotater
{
    private float m_DeltaAngleHorizontal;
    private float m_DeltaAngleVertical;

    public HorizontalFloorRoomObjectRotater()
    {
        m_DeltaAngleHorizontal = 0f;
        m_DeltaAngleVertical = 0f;
    }

    public float GetDeltaAngleHorizontal()
    {
        return m_DeltaAngleHorizontal;
    }

    public float GetDeltaAngleVertical()
    {
        return m_DeltaAngleVertical;
    }

    public void Rotate(Transform transform, float baseAngle, float angleHorizontal, float angleVertical)
    {
        Vector3 eulerAngles = transform.rotation.eulerAngles;
        float tempAngle = 0f;
        m_DeltaAngleHorizontal = angleHorizontal;
        angleHorizontal *= -1;
        tempAngle = eulerAngles.y;
        float delta = (baseAngle + angleHorizontal - tempAngle + 360f) % 360f;
        transform.Rotate(0f, delta, 0f);
    }
}

public class HorizontalWallRoomObjectRotater : IRoomObjectRotater
{
    private float m_DeltaAngleHorizontal;
    private float m_DeltaAngleVertical;

    public HorizontalWallRoomObjectRotater()
    {
        m_DeltaAngleHorizontal = 0f;
        m_DeltaAngleVertical = 0f;
    }

    public float GetDeltaAngleHorizontal()
    {
        return m_DeltaAngleHorizontal;
    }

    public float GetDeltaAngleVertical()
    {
        return m_DeltaAngleVertical;
    }
    public void Rotate(Transform transform, float baseAngle, float angleHorizontal, float angleVertical)
    {
        Vector3 eulerAngles = transform.rotation.eulerAngles;
        float tempAngle = 0f;
        m_DeltaAngleHorizontal = angleHorizontal;
        tempAngle = eulerAngles.x;
        float delta = (baseAngle + angleHorizontal - tempAngle + 360f) % 360f;
        transform.Rotate(delta, 0f, 0f);
    }
}

public class BothFloorRoomObjectRotater : IRoomObjectRotater
{
    private float m_DeltaAngleHorizontal;
    private float m_DeltaAngleVertical;

    public BothFloorRoomObjectRotater()
    {
        m_DeltaAngleHorizontal = 0f;
        m_DeltaAngleVertical = 0f;
    }

    public float GetDeltaAngleHorizontal()
    {
        return m_DeltaAngleHorizontal;
    }

    public float GetDeltaAngleVertical()
    {
        return m_DeltaAngleVertical;
    }
    public void Rotate(Transform transform, float baseAngle, float angleHorizontal, float angleVertical)
    {
        //vertical‚É‚Â‚¢‚Ä‚ÍPutType‚É‚æ‚Á‚Ä•Ï‚í‚é
        //NORMAL‚È‚çzŽ²‰ñ“], REVERSE‚È‚çxŽ²‰ñ“]
        Vector3 eulerAngles = transform.rotation.eulerAngles;
        float tempAngleHorizontal = 0f;
        float tempAngleVertical = 0f;
        m_DeltaAngleHorizontal = angleHorizontal;
        m_DeltaAngleVertical = angleVertical;
        angleHorizontal *= -1;
        tempAngleHorizontal = eulerAngles.y;
        tempAngleVertical = eulerAngles.z;
        float deltaHorizontal = (baseAngle + angleHorizontal - tempAngleHorizontal + 360f) % 360f;
        float deltaVertical = (angleVertical - tempAngleVertical + 360f) % 360f;
        transform.Rotate(0f, deltaHorizontal, deltaVertical);
    }
}