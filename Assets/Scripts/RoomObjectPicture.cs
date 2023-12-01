using UnityEngine;
using UniRx;
using System.Collections.Generic;

public class RoomObjectPicture : RoomObjectMovable
{
    [SerializeField] List<TappableBehaviour> m_TappableBehaviours;
    [SerializeField] MeshFilter m_MeshFilter;
    [SerializeField] MeshRenderer m_MeshRenderer;
    public MeshRenderer MeshRenderer => m_MeshRenderer;
    
    public override void SetTexture(Texture2D trimmedTexture, SetMaterialEvent setMaterialEvent)
    {
        base.SetTexture(trimmedTexture, setMaterialEvent);
        m_MeshRenderer.material.mainTexture = trimmedTexture;
    }

    public override void Init(RoomObjectData data, int roomIndex, PutType putType)
    {
        base.Init(data, roomIndex, putType);

        foreach(var tappable in m_TappableBehaviours)
        {
            tappable.OnTapBehaviour.Subscribe((materialNum) =>
            {
                m_OnSetMaterialSubject.OnNext(new SetMaterialEvent(this, m_MeshFilter, m_MeshRenderer, materialNum));
            }).AddTo(this);
        }

        SetTexture((Texture2D)m_MeshRenderer.material.mainTexture, new SetMaterialEvent(this, m_MeshFilter, m_MeshRenderer, 0));
    }

    public override void Init(SaveDataUnit saveDataUnit, RoomMasterData roomMasterData, RoomObjectMasterData roomObjectMasterData)
    {
        base.Init(saveDataUnit, roomMasterData, roomObjectMasterData);

        foreach (var tappable in m_TappableBehaviours)
        {
            tappable.OnTapBehaviour.Subscribe((materialNum) =>
            {
                m_OnSetMaterialSubject.OnNext(new SetMaterialEvent(this, m_MeshFilter, m_MeshRenderer, materialNum));
            }).AddTo(this);
        }

        Texture2D trimmedTexture = new Texture2D(2, 2);
        trimmedTexture.LoadImage(saveDataUnit.TextureBytes);
        SetTexture(trimmedTexture, new SetMaterialEvent(this, m_MeshFilter, m_MeshRenderer, 0));
    }
}
