using UnityEditor;

/// <summary>
/// �}�X�^�[�f�[�^�쐬�p�G�f�B�^�g��
/// �g����; Assets/MasterData�t�H���_���̂��ׂẴt�@�C����I������Reimport����
/// </summary>
#if UNITY_EDITOR
public class SampleRoomObjectMasterDataEditor : AssetPostprocessor
{
    private static RoomMasterData ms_RoomMasterData;
    private static RoomObjectMasterData ms_RoomObjectMasterData;
    private static SampleRoomObjectMasterData ms_SampleRoomObjectMasterData;

    static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromPath)
    {
        foreach (var asset in importedAssets)
        {
            RoomObjectMasterData roomObjectMasterData = AssetDatabase.LoadAssetAtPath<RoomObjectMasterData>(asset);
            if (roomObjectMasterData != null)
            {
                ms_RoomObjectMasterData = roomObjectMasterData;
                ms_RoomObjectMasterData.RoomObjects.Clear();
            }

            RoomMasterData roomMasterData = AssetDatabase.LoadAssetAtPath<RoomMasterData>(asset);
            if (roomMasterData != null)
            {
                ms_RoomMasterData = roomMasterData;
            }

        }

        foreach (var asset in importedAssets)
        {
            SampleRoomObjectMasterData sampleRoomObjectMasterData = AssetDatabase.LoadAssetAtPath<SampleRoomObjectMasterData>(asset);
            if (sampleRoomObjectMasterData != null)
            {
                ms_SampleRoomObjectMasterData = sampleRoomObjectMasterData;
                foreach (var entity in sampleRoomObjectMasterData.RoomObjectEntities)
                {
                    string path = "Assets/Prefab/MasterData/" + entity.Key + ".prefab";
                    RoomObject model = AssetDatabase.LoadAssetAtPath<RoomObject>(path);
                    if (model != null)
                    {
                        //entity.Model = model;
                        RoomObjectData roomObjectData = new RoomObjectData(ms_RoomMasterData, entity, model);
                        ms_RoomObjectMasterData.RoomObjects.Add(roomObjectData);
                    }
                    else
                    {
                        UnityEngine.Debug.Log("Key : " + entity.Key);
                    }
                }
            }

        }

        AssetDatabase.Refresh();
    }
}
#endif