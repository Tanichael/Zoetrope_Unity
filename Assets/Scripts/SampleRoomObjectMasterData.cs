using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/SampleRoomObjectMasterData", fileName = "SampleRoomObjectMasterData")]
[ExcelAsset]
public class SampleRoomObjectMasterData : ScriptableObject
{
	public List<RoomObjectDataEntity> RoomObjectEntities; // Replace 'EntityType' to an actual type that is serializable.

	private List<RoomObjectData> m_RoomObjects;
	public List<RoomObjectData> RoomObjects => m_RoomObjects;
}
