using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

public interface IZoetropeMaker
{
    // Start is called before the first frame update
    UniTask<Zoetrope> Make(CancellationToken token);
}

public abstract class AbstractZoetropeMaker : IZoetropeMaker
{
    protected IZoetropeLoader m_Loader;
    protected IZoetropeSetter m_Setter;

    public async UniTask<Zoetrope> Make(CancellationToken token)
    {
        SetLoader();
        await m_Loader.LoadPrefabsAsync(token);
        GameObject maskPrefab = m_Loader.GetMaskPrefab();
        GameObject avatarPrefab = m_Loader.GetAvatarPrefab();
        SetSetter(maskPrefab, avatarPrefab);
        return m_Setter.Set();
    }

    public virtual void SetLoader()
    {
        m_Loader = new ZoetropeLoader();
    }

    public virtual void SetSetter(GameObject maskPrefab, GameObject avatarPrefab)
    {
        m_Setter = new ZoetropeSetter(maskPrefab, avatarPrefab);
    }
}

public class ZoetropeMaker : AbstractZoetropeMaker
{

}
