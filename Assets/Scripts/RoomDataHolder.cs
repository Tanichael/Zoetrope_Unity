//RoomSelectScene������ۂ�RoomScene�Ƀf�[�^���󂯓n�����߂ɗp����
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
