using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/RoomObjectMasterData", fileName = "RoomObjectMasterData")]
public class RoomObjectMasterData : ScriptableObject
{
    [SerializeField] private List<RoomObjectData> m_RoomObjects;

    public List<RoomObjectData> RoomObjects => m_RoomObjects;
}
