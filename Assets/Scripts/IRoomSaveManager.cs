using System.Collections.Generic;

public interface IRoomSaveManager
{
    public void Save(MockRoomManager roomManager);
    public SaveData Load();
    public List<SaveData> LoadAll();
    public void Delete();
    public SaveData GetRoomState(MockRoomManager roomManager);
    public void InitializeTemplates();
    public void SetTemplate(MockRoomManager roomManager);
}
