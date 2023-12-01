using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelInfo : MonoBehaviour
{
    [SerializeField] List<EmptySpaceBehaviour> m_EmptySpaces;

    public List<EmptySpaceBehaviour> EmptySpaces => m_EmptySpaces;
}
