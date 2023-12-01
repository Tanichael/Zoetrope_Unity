
public abstract class RoomEvent
{

}

public class RoomEventLog : RoomEvent
{
    private string m_LogText;

    public RoomEventLog(string logText)
    {
        m_LogText = logText;
    }
}

public class RoomEventLogTwoAnswer : RoomEventLog
{
    private string m_LogText;
    private RoomUIEvent m_YesEvent;
    private RoomUIEvent m_NoEvent;

    public string LogText => m_LogText;
    public RoomUIEvent YesEvent => m_YesEvent;
    public RoomUIEvent NoEvent => m_NoEvent;

    public RoomEventLogTwoAnswer(string logText, RoomUIEvent yesEvent, RoomUIEvent noEvent) : base(logText)
    {
        m_LogText = logText;
        m_YesEvent = yesEvent;
        m_NoEvent = noEvent;
    }
}

public class RoomEventLogEnd : RoomEventLog
{
    public RoomEventLogEnd(string logText) : base(logText)
    {

    }
}