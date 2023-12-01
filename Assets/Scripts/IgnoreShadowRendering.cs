using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Camera))]
public class IgnoreShadowRendering : MonoBehaviour
{
    private UniversalAdditionalCameraData additionalCameraData;

    void Start()
    {
        additionalCameraData = GetComponent<Camera>().GetUniversalAdditionalCameraData();
        additionalCameraData.renderPostProcessing = false;
        additionalCameraData.renderShadows = false;
    }
}
