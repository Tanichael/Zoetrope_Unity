using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class RoomSelectManager : MonoBehaviour
{
    [SerializeField] private ScrollRect m_ScrollRect;
    [SerializeField] private RoomDataButton m_RoomDataButtonPrefab;
    [SerializeField] private GameObject m_LoadingCover;

    // Start is called before the first frame update
    async void Start()
    {
        m_LoadingCover.SetActive(false);

        RoomSingleton.Instance.RoomSaveManager.InitializeTemplates();

        //?f?[?^?????[?h?????{?^???????f
        List<SaveData> saveDataList = RoomSingleton.Instance.RoomSaveManager.LoadAll();
        Debug.Log("save data count " + saveDataList.Count);

        for (int i = 0; i < saveDataList.Count + 1; i++)
        {
            RoomDataButton roomDataButton = Instantiate(m_RoomDataButtonPrefab, m_ScrollRect.content);

            SaveData saveData = null;
            if (i != saveDataList.Count)
            {
                saveData = saveDataList[i];
                roomDataButton.Text.text = saveData.Name;

                if (saveData.Name.Contains("template"))
                {
                    //画像を検索してセット
                    roomDataButton.Text.gameObject.SetActive(false);
                    Texture2D tex = null;
                    tex = Resources.Load("Textures/TemplateIcons/" + saveData.Name) as Texture2D;
                    roomDataButton.RawImage.texture = tex;
                }
            }
            else
            {
                roomDataButton.Text.text = "Imamura";

                roomDataButton.Text.gameObject.SetActive(false);
                Texture2D tex = null;
                tex = Resources.Load("Textures/SampleIcon") as Texture2D;
                roomDataButton.RawImage.texture = tex;
            }

            RectTransform buttonRectTransform = roomDataButton.transform as RectTransform;
            if (buttonRectTransform != null)
            {
                buttonRectTransform.sizeDelta = new Vector2(Screen.width * 0.4f, Screen.width * 0.35f);

                float width = buttonRectTransform.sizeDelta.x;
                float height = buttonRectTransform.sizeDelta.y;

                float posX = (width / 2f + Screen.width * 0.05f) * Mathf.Pow(-1, (i + 1) % 2);
                float posY = -200f - (height + Screen.width * 0.05f) * (i / 2);
                buttonRectTransform.anchoredPosition = new Vector2(posX, posY);
            }

            int index = i;

            //クリック時の動作定義
            roomDataButton.Button.onClick.AddListener(() =>
            {
                Debug.Log("Index " + index);
                Debug.Log("count " + saveDataList.Count);
                if(index != saveDataList.Count)
                {
                    //データ受け渡し
                    RoomSingleton.Instance.RoomDataHolder.SetData(saveData);

                    m_LoadingCover.SetActive(true);
                    //シーン遷移
                    /*await SceneManager.UnloadSceneAsync("MockRoomSelectScene");
                    await SceneManager.LoadSceneAsync("MockScene", LoadSceneMode.Additive);*/
                    CommonUIManager.Instance.LoadSceneAsync("MockRoomSelectScene", "MockScene").Forget();
                }
                else
                {
                    CommonUIManager.Instance.LoadSceneAsync("MockRoomSelectScene", "SampleScene").Forget();
                }
            });

            roomDataButton.gameObject.SetActive(true);
        }

        if(saveDataList.Count == 0)
        {
            m_LoadingCover.SetActive(true);

            //シーン遷移
            /*await SceneManager.UnloadSceneAsync("MockRoomSelectScene");
            //APIをつかって名称取得？
            await SceneManager.LoadSceneAsync("MockScene", LoadSceneMode.Additive);*/
            CommonUIManager.Instance.LoadSceneAsync("MockRoomSelectScene", "MockScene").Forget();
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
