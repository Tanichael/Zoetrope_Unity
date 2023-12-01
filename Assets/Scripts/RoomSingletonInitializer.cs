using UnityEngine;

public class RoomSingletonInitializer : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        IRoomSaveManager roomSaveManager = new RoomSaveManager();
        IRoomDataHolder roomDataHolder = new RoomDataHolder();
        ITapSelector objectSelector = new MockRoomSelector();

        RoomSingleton.Instance.RoomSaveManager = roomSaveManager;
        RoomSingleton.Instance.RoomDataHolder = roomDataHolder;
        RoomSingleton.Instance.ObjectSelector = objectSelector;
    }
}