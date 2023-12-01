using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ZoetropeSceneController : MonoBehaviour
{
    [SerializeField, Range(1f, 361f)] private float m_RotateSpeed = 3.9f;
    private float m_BefSpeed;

    private Zoetrope m_Zoetrope;
    private AsyncOperationHandle m_RotaterHandle;
    private ZoetropeRotater m_Rotater;

    // Start is called before the first frame update
    void Start()
    {
        InitAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }

    async UniTask InitAsync(CancellationToken token)
    {
        IZoetropeMaker maker = new ZoetropeMaker();
        m_Zoetrope = await maker.Make(token);
        m_RotaterHandle = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/ZoetropeRotater.prefab");
        await m_RotaterHandle.Task;
        GameObject rotaterObject = Instantiate((GameObject)m_RotaterHandle.Result);
        m_Rotater = rotaterObject.GetComponent<ZoetropeRotater>();
        if(m_Rotater == null)
        {
            Debug.LogError("Rotater is null");
        }
        m_Rotater.Init(m_Zoetrope);
    }

    private void Update()
    {
        if(m_BefSpeed != m_RotateSpeed)
        {
            if(m_Rotater != null)
            {
                m_Rotater.SetSpeed(m_RotateSpeed);
                m_BefSpeed = m_RotateSpeed;
            }
        }   
    }
}