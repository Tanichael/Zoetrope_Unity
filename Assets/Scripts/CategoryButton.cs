using UnityEngine;
using UnityEngine.UI;

public class CategoryButton : MonoBehaviour
{
    [SerializeField] private Button m_Button;
    [SerializeField] private Text m_Text;

    public Button Button => m_Button;
    public Text Text => m_Text;

    public UICategory Category { get; set; }

}
