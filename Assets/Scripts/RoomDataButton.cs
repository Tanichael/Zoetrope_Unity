using UnityEngine;
using UnityEngine.UI;

public class RoomDataButton : MonoBehaviour
{
    [SerializeField] Button m_Button;
    [SerializeField] RawImage m_RawImage;
    [SerializeField] Text m_Text;

    public Button Button => m_Button;
    public Text Text => m_Text;
    public RawImage RawImage => m_RawImage;
    public SaveData SaveData { get; set; }

}
