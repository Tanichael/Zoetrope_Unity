using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveDataUnit
{
    public int RoomIndex;
    public int DataIndex;
    public int PutTypeNum;
    public bool IsPictureSet;
    public string ItemText;
    public int FamilyHeight;
    public int FamilyDepth;
    public int FamilyWidth;
    public byte[] TextureBytes;
    public float DeltaAngleHorizontal;
    public float DeltaAngleVertical;
    public float DeltaAngleX;

    public SaveDataUnit(RoomObject roomObject)
    {
        int roomIndex = roomObject.RoomIndex;
        int dataIndex = roomObject.Data.DataIndex;
        int putTypeNum = (int)roomObject.PutType;
        bool isPictureSet = roomObject.IsPictureSet;
        string itemText = roomObject.ItemText;
        int familyHeight = roomObject.GetFamilySize(PutType.NORMAL).y;
        int familyDepth = roomObject.GetFamilySize(PutType.NORMAL).z;
        int familyWidth = roomObject.GetFamilySize(PutType.NORMAL).x;
        byte[] textureBytes = roomObject.TrimmedTexture.EncodeToPNG();
        float deltaAngleHorizontal = roomObject.GetDeltaAngleHorizontal();
        float deltaAngleVertical = roomObject.GetDeltaAngleVertical();
        float deltaAngleX = roomObject.transform.localRotation.eulerAngles.x;

        RoomIndex = roomIndex;
        DataIndex = dataIndex;
        PutTypeNum = putTypeNum;
        IsPictureSet = isPictureSet;
        ItemText = itemText;
        FamilyHeight = familyHeight;
        FamilyDepth = familyDepth;
        FamilyWidth = familyWidth;
        TextureBytes = textureBytes;
        DeltaAngleHorizontal = deltaAngleHorizontal;
        DeltaAngleVertical = deltaAngleVertical;
        DeltaAngleX = deltaAngleX;
    }

    /*public SaveDataUnit(int roomIndex, int dataIndex, int putTypeNum, bool isPictureSet, string itemText, int familyHeight, int familyDepth, int familyWidth)
    {
        RoomIndex = roomIndex;
        DataIndex = dataIndex;
        PutTypeNum = putTypeNum;
        IsPictureSet = isPictureSet;
        ItemText = itemText;
        FamilyHeight = familyHeight;
        FamilyDepth = familyDepth;
        FamilyWidth = familyWidth;
    }*/
}

[Serializable]
public class SaveData
{
    public string Name;
    public List<SaveDataUnit> Data;
}
