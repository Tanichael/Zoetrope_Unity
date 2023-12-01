using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARCameraController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleAR()
    {
        var cam = FindObjectOfType<UnityEngine.XR.ARFoundation.ARSessionOrigin>(true).gameObject;
        cam.SetActive(!cam.activeInHierarchy);
    }
}
