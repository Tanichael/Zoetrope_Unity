using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FOVSetter : MonoBehaviour
{
    Camera cam;
    // Start is called before the first frame update
    void Start()
    {
    }

    public void SetFoV(float val)
    {
        if (cam == null)
            cam = Camera.main;
        cam.fieldOfView = val;
    }
}
