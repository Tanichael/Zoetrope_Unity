//RoomSelectSceneから実際のRoomSceneにデータを受け渡すために用いる
public class RoomDataHolder : IRoomDataHolder
{
    private SaveData m_SelectedSaveData;
    public void SetData(SaveData saveData)
    {
        m_SelectedSaveData = saveData;
    }

    public SaveData GetData()
    {
        return m_SelectedSaveData;
    }
}
