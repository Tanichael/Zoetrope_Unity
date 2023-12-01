using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LitPropertyController : AbstractToonShaderPropertyController
{
    public override string GetName()
    {
        return "Lit";
    }

    public override void SetShader()
    {
        m_Shader = Shader.Find("Universal Render Pipeline/Lit");
    }
}
