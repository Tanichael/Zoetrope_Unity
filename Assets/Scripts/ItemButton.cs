using UnityEngine;
using UnityEngine.UI;

public class ItemButton : MonoBehaviour
{
    [SerializeField] private Button m_Button;
    [SerializeField] private Text m_Text;
    [SerializeField] private RawImage m_IconImage;

    public Button Button => m_Button;
    public Text Text => m_Text;
    public RawImage IconImage => m_IconImage;

    public RoomObjectData Data { get; set; }

    private void Start()
    {
        m_Button.onClick.AddListener(() =>
        {
            DataManager.Instance.Data = Data;
        });
    }
}
