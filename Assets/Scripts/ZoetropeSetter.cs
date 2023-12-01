using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ZoetropeSetter : AbstractZoetropeSetter, IDisposable
{
    private AsyncOperationHandle<GameObject> m_MaskHandle;
    private AsyncOperationHandle<GameObject> m_AvatarHandle;

    private int m_Count;
    private GameObject m_MaskPrefab;
    private GameObject m_AvatarPrefab;
    private float m_Radius;

    public ZoetropeSetter(GameObject maskPrefab, GameObject avatarPrefab)
    {
        m_Count = 12;
        m_MaskPrefab = maskPrefab;
        m_AvatarPrefab = avatarPrefab;
        m_Radius = 1.08f / (2f * Mathf.Tan(Mathf.Deg2Rad * 180f / m_Count));
    }

    protected override void CreateZoetropeObject(Zoetrope zoetrope)
    {
        GameObject zoetropeParent = new GameObject();
        zoetropeParent.name = "ZoetropeParent";

        zoetrope.ZoetRopeObject = zoetropeParent;
    }

    protected override void CreateMasks(Zoetrope zoetrope)
    {
        float maskHeight = m_MaskPrefab.transform.localScale.y;

        for (int i = 0; i < m_Count; i++)
        {
            GameObject mask = GameObject.Instantiate(m_MaskPrefab, zoetrope.ZoetRopeObject.transform);

            float rad = i * 360f * Mathf.Deg2Rad / m_Count;
            mask.transform.position = m_Radius * new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) + maskHeight * 0.5f * Vector3.up;
            mask.transform.LookAt(new Vector3(0f, maskHeight * 0.5f, 0f));
            zoetrope.Masks.Add(mask);
        }
    }

    protected override void CreateAvatars(Zoetrope zoetrope)
    {
        for (int i = 0; i < m_Count; i++)
        {
            //各モーションを参照するようにする
            GameObject avatar = GameObject.Instantiate(m_AvatarPrefab, zoetrope.ZoetRopeObject.transform);
            float rad = i * 360f * Mathf.Deg2Rad / m_Count;

            avatar.transform.position = m_Radius * 0.8f * new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad));

            float avatarDirRad = ((180f - 60f * i) * Mathf.Deg2Rad);
            Vector3 avatarDir = avatar.transform.position + new Vector3(Mathf.Cos(avatarDirRad), 0f, Mathf.Sin(avatarDirRad));
            avatar.transform.LookAt(avatarDir);

            zoetrope.Avatars.Add(avatar);
        }
    }

    public void Dispose()
    {
        if(m_MaskHandle.IsValid())
        {
            Addressables.Release(m_MaskHandle);
        }

        if(m_AvatarHandle.IsValid())
        {
            Addressables.Release(m_AvatarHandle);
        }
    }
}
