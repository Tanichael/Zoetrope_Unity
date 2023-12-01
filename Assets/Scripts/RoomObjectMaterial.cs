using UnityEngine;
using UniRx;
using System;
using System.Collections.Generic;

public class RoomObjectMaterial : RoomObjectMovable
{
    [SerializeField] List<TappableMaterial> m_TappableMaterials;
    [SerializeField] MeshFilter m_MeshFilter;
    [SerializeField] MeshRenderer m_MeshRenderer;

    public override void SetTexture(Texture2D trimmedTexture, SetMaterialEvent setMaterialEvent)
    {
        base.SetTexture(trimmedTexture, setMaterialEvent);
        m_MeshRenderer.materials[setMaterialEvent.MaterialNum].mainTexture = trimmedTexture;
    }

    public override void Init(RoomObjectData data, int roomIndex, PutType putType)
    {
        base.Init(data, roomIndex, putType);

        foreach(var tappableMaterial in m_TappableMaterials)
        {
            tappableMaterial.OnTapMaterial.Subscribe((materialNum) =>
            {
                m_OnSetMaterialSubject.OnNext(new SetMaterialEvent(this, m_MeshFilter, m_MeshRenderer, materialNum));
            }).AddTo(this);
        }
    }

    public override void Init(SaveDataUnit saveDataUnit, RoomMasterData roomMasterData, RoomObjectMasterData roomObjectMasterData)
    {
        base.Init(saveDataUnit, roomMasterData, roomObjectMasterData);

        Texture2D trimmedTexture = new Texture2D(2, 2);
        trimmedTexture.LoadImage(saveDataUnit.TextureBytes);

        foreach (var tappableMaterial in m_TappableMaterials)
        {
            tappableMaterial.OnTapMaterial.Subscribe((materialNum) =>
            {
                m_OnSetMaterialSubject.OnNext(new SetMaterialEvent(this, m_MeshFilter, m_MeshRenderer, materialNum));
            }).AddTo(this);
        }
    }
}

public class SetMaterialEvent
{
    public RoomObject RoomObject { get; }
    public MeshRenderer MeshRenderer { get; }
    public MeshFilter MeshFilter { get; }
    public int MaterialNum { get; }

    public SetMaterialEvent(RoomObject roomObject, MeshFilter meshFilter, MeshRenderer renderer, int materialNum)
    {
        RoomObject = roomObject;
        MeshFilter = meshFilter;
        MeshRenderer = renderer;
        MaterialNum = materialNum;
    }
}
