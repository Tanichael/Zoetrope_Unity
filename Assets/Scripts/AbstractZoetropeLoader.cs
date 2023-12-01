using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public abstract class AbstractZoetropeLoader : IZoetropeLoader
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

        await m_MaskHandle.Task;
        await m_AvatarHandle.Task;
        m_MaskPrefab = (GameObject)m_MaskHandle.Result;
        m_AvatarPrefab = (GameObject)m_AvatarHandle.Result;
    }
}

public interface IZoetropeLoader
{
    GameObject GetMaskPrefab();
    GameObject GetAvatarPrefab();
    UniTask LoadPrefabsAsync(CancellationToken token);
}
