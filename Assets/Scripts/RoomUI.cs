using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class RoomUI : MonoBehaviour
{
    [SerializeField] private RoomObjectMasterData m_MasterData;
    [SerializeField] private Canvas m_SelectedCanvas;
    [SerializeField] private Canvas m_ItemCanvas;
    [SerializeField] private Button m_DetailButton;
    [SerializeField] private Button m_BackButton;
    [SerializeField] private Button m_RotateButton;
    [SerializeField] private Button m_DeleteButton;
    [SerializeField] private ItemButton m_ItemButtonPrefab;
    [SerializeField] private Button m_RemoveAllButton;
    [SerializeField] private ScrollRect m_ScrollView;
    [SerializeField] private GameObject m_FurniturePanel;
    [SerializeField] private GameObject m_ItemPanel;
    [SerializeField] private Button m_EditButton;

    public Button DetailButton => m_DetailButton;
    public Button BackButton => m_BackButton;
    public Button RotateButton => m_RotateButton;
    public Button DeleteButton => m_DeleteButton;

    public Button RemoveAllButton => m_RemoveAllButton;
    public Button EditButton => m_EditButton;

    private void Start()
    {
        ChangeCanvas(RoomPhase.None).Forget();

        #region RECTTRANSFORM
        RectTransform scrollRectTransform = m_ScrollView.gameObject.transform as RectTransform;
        if(scrollRectTransform != null)
        {
            scrollRectTransform.sizeDelta = new Vector2(Screen.width, Screen.width / 2f);
            float dx = 0f;
            float dy = -scrollRectTransform.sizeDelta.y / 1.9f;
            scrollRectTransform.anchoredPosition = new Vector2(dx, dy);
        }

        RectTransform editRectTransform = m_RotateButton.gameObject.transform as RectTransform;
        if(editRectTransform != null)
        {
            editRectTransform.sizeDelta = new Vector2(Screen.width / 5f, Screen.width / 2.5f / 3f);
            float dx = -Screen.width / 4f;
            float dy = editRectTransform.sizeDelta.y * 3f;
            editRectTransform.anchoredPosition = new Vector2(dx, dy);
        }

        RectTransform detailRectTransform = m_DetailButton.gameObject.transform as RectTransform;
        if (detailRectTransform != null)
        {
            detailRectTransform.sizeDelta = new Vector2(Screen.width / 5f, Screen.width / 2.5f / 3f);
            float dx = 0f;
            float dy = detailRectTransform.sizeDelta.y * 3f;
            detailRectTransform.anchoredPosition = new Vector2(dx, dy);
        }

        RectTransform backRectTransform = m_BackButton.gameObject.transform as RectTransform;
        if (backRectTransform != null)
        {
            backRectTransform.sizeDelta = new Vector2(Screen.width / 5f, Screen.width / 2.5f / 3f);
            float dx = Screen.width / 4f;
            float dy = backRectTransform.sizeDelta.y * 3f;
            backRectTransform.anchoredPosition = new Vector2(dx, dy);
        }

        RectTransform deleteRectTransform = m_DeleteButton.gameObject.transform as RectTransform;
        if (deleteRectTransform != null)
        {
            deleteRectTransform.sizeDelta = new Vector2(Screen.width / 5f, Screen.width / 2.5f / 3f);
            float dx = Screen.width / 4f;
            float dy = deleteRectTransform.sizeDelta.y * 3f;
            deleteRectTransform.anchoredPosition = new Vector2(dx, dy);
        }

        RectTransform removeAllRectTransform = m_RemoveAllButton.gameObject.transform as RectTransform;
        if(removeAllRectTransform != null)
        {
            removeAllRectTransform.sizeDelta = new Vector2(Screen.width / 2f, Screen.width / 2.5f / 3f);
            float dx = 0f;
            float dy = removeAllRectTransform.sizeDelta.y * 1f;
            removeAllRectTransform.anchoredPosition = new Vector2(dx, dy);
        }

        #endregion

       /* m_ScrollView.content.sizeDelta = new Vector2(m_ScrollView.content.sizeDelta.x, Screen.width / 5f * 3f / 4f * (m_MasterData.RoomObjects.Count + 12) / 4);

        for (int i = 0; i < m_MasterData.RoomObjects.Count; i++)
        {
            //ItemButton itemButton = Instantiate(m_ItemButtonPrefab, m_ItemCanvas.transform);
            ItemButton itemButton = Instantiate(m_ItemButtonPrefab, m_ScrollView.content);
            itemButton.Data = m_MasterData.RoomObjects[i];
            itemButton.Text.text = m_MasterData.RoomObjects[i].Model.name;
            itemButton.Button.onClick.AddListener(() =>
            {
                OnPut.Invoke(itemButton.Data);
            });

            RectTransform rectTransform = itemButton.transform as RectTransform;
            if(rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(Screen.width / 4.5f, Screen.width / 5f * 3f / 4f);

                int col = i % 4;
                int row = i / 4;

                float dx = (col - 2f + 0.5f) * rectTransform.sizeDelta.x;
                float dy = -rectTransform.sizeDelta.y * (row + 1);

                *//* float dx = (i - ((float)m_MasterData.RoomObjects.Count / 2f) + 0.5f) * rectTransform.sizeDelta.x;
                float dy = -rectTransform.sizeDelta.y;*//*
                rectTransform.anchoredPosition = new Vector2(dx, dy);
            }
            itemButton.gameObject.SetActive(true);
        }*/
    }

    public async UniTask ChangeCanvas(RoomPhaseBase phase)
    {
        await ChangeCanvas(phase.GetRoomPhase());
    }

    public async UniTask ChangeCanvas(RoomPhase phase)
    {
        //Debug.Log("ChangeCanvas: next UI -> " + phase);
        if(phase == RoomPhase.None)
        {
            m_FurniturePanel.SetActive(true);
            m_ItemPanel.SetActive(false);

            m_SelectedCanvas.gameObject.SetActive(false);
            m_ItemCanvas.gameObject.SetActive(true);
            m_RemoveAllButton.gameObject.SetActive(true);
        }
        if(phase == RoomPhase.Selected)
        {
            m_FurniturePanel.SetActive(true);
            m_ItemPanel.SetActive(false);

            m_RotateButton.gameObject.SetActive(true);
            m_BackButton.gameObject.SetActive(false);
            m_DetailButton.gameObject.SetActive(true);
            m_DeleteButton.gameObject.SetActive(true);
            m_SelectedCanvas.gameObject.SetActive(true);
            m_ItemCanvas.gameObject.SetActive(true);
            m_RemoveAllButton.gameObject.SetActive(false);
        }
        if(phase == RoomPhase.Detail)
        {
            m_FurniturePanel.SetActive(true);
            m_ItemPanel.SetActive(false);

            m_RotateButton.gameObject.SetActive(false);
            m_BackButton.gameObject.SetActive(true);
            m_DetailButton.gameObject.SetActive(false);
            m_DeleteButton.gameObject.SetActive(false);
            m_SelectedCanvas.gameObject.SetActive(true);
            m_ItemCanvas.gameObject.SetActive(false);
            m_RemoveAllButton.gameObject.SetActive(false);
        }
        if(phase == RoomPhase.EmptySpace)
        {
            m_FurniturePanel.SetActive(true);
            m_ItemPanel.SetActive(false);

            m_RotateButton.gameObject.SetActive(false);
            m_BackButton.gameObject.SetActive(true);
            m_DetailButton.gameObject.SetActive(false);
            m_DeleteButton.gameObject.SetActive(false);
            m_SelectedCanvas.gameObject.SetActive(true);
            m_ItemCanvas.gameObject.SetActive(true);
            m_RemoveAllButton.gameObject.SetActive(false);
        }
        if(phase == RoomPhase.White)
        {
            m_FurniturePanel.SetActive(true);
            m_ItemPanel.SetActive(false);

            m_SelectedCanvas.gameObject.SetActive(false);
            m_ItemCanvas.gameObject.SetActive(false);
        }
        if(phase == RoomPhase.Control)
        {
            m_FurniturePanel.SetActive(true);
            m_ItemPanel.SetActive(false);

            m_SelectedCanvas.gameObject.SetActive(false);
            m_ItemCanvas.gameObject.SetActive(false);
        }
        if(phase == RoomPhase.Transition)
        {
            m_FurniturePanel.SetActive(true);
            m_ItemPanel.SetActive(false);

            m_SelectedCanvas.gameObject.SetActive(false);
            m_ItemCanvas.gameObject.SetActive(false);
        }
        
        if(phase == RoomPhase.Picture)
        {
            m_FurniturePanel.SetActive(false);
            m_ItemPanel.SetActive(true);
        }

        if(phase == RoomPhase.Trim)
        {
            m_FurniturePanel.SetActive(false);
            m_ItemPanel.SetActive(true);
        }
    }
}

public enum UIPhase
{
    None = 0,
    Selected = 1,
    Furniture = 2,
    White = 4,
}

