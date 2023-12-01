using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;

public class ZoetropeLoader : IZoetropeLoader
{
    protected AsyncOperationHandle m_MaskHandle;
    protected AsyncOperationHandle m_AvatarHandle;
    protected GameObject m_MaskPrefab;
    protected GameObject m_AvatarPrefab;

    public GameObject GetMaskPrefab()
    {

        return m_MaskPrefab;
    }

    public GameObject GetAvatarPrefab()
    {
        return m_AvatarPrefab;
    }

    public async UniTask LoadPrefabsAsync(CancellationToken token)
    {
        m_MaskHandle = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/ZoetropeMask.prefab");
        m_AvatarHandle = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Robot Kyle.prefab");

        await m_MaskHandle.ToUniTask(cancellationToken: token);
        await m_AvatarHandle.ToUniTask(cancellationToken: token);
        m_MaskPrefab = (GameObject)m_MaskHandle.Result;
        m_AvatarPrefab = (GameObject)m_AvatarHandle.Result;
    }
}
