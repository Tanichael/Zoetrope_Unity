using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoomObjectDataEntity
{
    public int Id;
    public string Key;
    public RoomObjectType Type;
    public PositionType PosType;
    public bool IsPlane;
    public int Level;
    public string Tags;
    public UICategory UICategory;
}
