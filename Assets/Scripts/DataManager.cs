public class DataManager
{
    private static DataManager m_Instance;
    public static DataManager Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = new DataManager();
            }
            return m_Instance;
        }
    }

    private DataManager(){ }

    public RoomObjectData Data { get; set; }
}
