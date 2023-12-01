public class RoomSingleton : Singleton<RoomSingleton>
{
    private IRoomSaveManager m_RoomSaveManager;
    private IRoomDataHolder m_RoomDataHolder;
    private ITapSelector m_ObjectSelector;

    public IRoomSaveManager RoomSaveManager
    {
        get 
        {
            if(m_RoomSaveManager == null)
            {
                //デフォルトのものを設定しておく
                m_RoomSaveManager = new RoomSaveManager();
            }
            return m_RoomSaveManager;
        }
        set
        {
            m_RoomSaveManager = value;
        }
    }

    public IRoomDataHolder RoomDataHolder
    {
        get
        {
            if(m_RoomDataHolder == null)
            {
                m_RoomDataHolder = new RoomDataHolder();
            }
            return m_RoomDataHolder;
        }
        set
        {
            m_RoomDataHolder = value;
        }
    }

    public ITapSelector ObjectSelector
    {
        get
        {
            if(m_ObjectSelector == null)
            {
                m_ObjectSelector = new MockRoomSelector();
            }
            return m_ObjectSelector;
        }
        set
        {
            m_ObjectSelector = value;
        }
    }
}
