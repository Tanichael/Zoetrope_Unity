using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoetropeRotater : MonoBehaviour
{
    private bool m_IsRotating = false;
    private Zoetrope m_Zoetrope;
    private float m_Speed;

    public void Init(Zoetrope zoetrope)
    {
        m_Zoetrope = zoetrope;
        m_Speed = 3.9f;

        foreach (var mask in m_Zoetrope.Masks)
        {
            mask.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if(m_Zoetrope == null)
        {
            return;
        }
        
        if(Input.GetKeyDown(KeyCode.A))
        {
            foreach(var mask in m_Zoetrope.Masks)
            {
                mask.gameObject.SetActive(!mask.gameObject.activeSelf);
            }
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            m_IsRotating = !m_IsRotating;
        }

        if(m_IsRotating)
        {
            Vector3 eulerAngles = m_Zoetrope.ZoetRopeObject.transform.rotation.eulerAngles;
            eulerAngles += new Vector3(0f, m_Speed, 0f);
            m_Zoetrope.ZoetRopeObject.transform.rotation = Quaternion.Euler(eulerAngles);
        }
    }

    public void SetSpeed(float speed)
    {
        m_Speed = speed;
    }
}
