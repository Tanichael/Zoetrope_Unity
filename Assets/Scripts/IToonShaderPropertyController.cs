using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IToonShaderPropertyController
{
    Shader Shader { get; }

    string GetName();
    void SetMaterials(List<Material> materials);
    void SetShader();
    GameObject GetGameObject();
    void SetUIActive(bool isActive);
    GameObject GetUI();
}

public abstract class AbstractToonShaderPropertyController : MonoBehaviour, IToonShaderPropertyController
{
    [SerializeField] protected GameObject m_ShaderCustomUI;

    protected List<Material> m_MaterialList;
    protected Shader m_Shader;
    //protected GameObject m_ShaderCustomUI;

    public Shader Shader => m_Shader;
    public abstract string GetName();

    public virtual void SetMaterials(List<Material> materials)
    {
        m_MaterialList = materials;
    }

    public abstract void SetShader();
    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public virtual void SetUIActive(bool isActive)
    {
        /*if(m_ShaderCustomUI == null && m_ShaderCustomUIPrefab != null)
        {
            m_ShaderCustomUI = Instantiate(m_ShaderCustomUIPrefab, transform);
        }*/
        if(m_ShaderCustomUI != null)
        {
            m_ShaderCustomUI.SetActive(isActive);
        }
    }

    public virtual GameObject GetUI()
    {
        return m_ShaderCustomUI;
    }
}
