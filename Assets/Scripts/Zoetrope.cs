using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zoetrope
{
    private GameObject m_ZoetropeObject;
    private List<GameObject> m_Masks;
    private List<GameObject> m_Avatars;

    public GameObject ZoetRopeObject { get; set; }
    public List<GameObject> Masks => m_Masks;
    public List<GameObject> Avatars => m_Avatars;

    public Zoetrope()
    {
        m_Masks = new List<GameObject>();
        m_Avatars = new List<GameObject>();
    }
}
