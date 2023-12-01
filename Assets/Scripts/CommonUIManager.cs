using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UniRx;
using System.Threading;
using Cysharp.Threading.Tasks;

public class CommonUIManager : MonoBehaviour
{
    public static CommonUIManager Instance;

    [SerializeField] private Camera m_CommonUICamera;
    [SerializeField] private List<Button> m_PageButtons;
    
    /*[SerializeField] private Button m_VisitButton;
    [SerializeField] private Button m_FindButton;
    [SerializeField] private Button m_StudioButton;
    [SerializeField] private Button m_NotifButton;
    [SerializeField] private Button m_UserButton;*/
    [SerializeField] private GameObject m_LoadingCover;

    private int m_CurrentSceneIndex = 0;
    private CancellationTokenSource m_Cts;
    private Color m_SelectedColor = new Color(0.286f, 0.286f, 0.286f);
    private Color m_UnselectedColor = new Color(1, 1, 1);
        
    // Start is called before the first frame update
    void Start()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this);
        }

        Instance = this;
        m_Cts = new CancellationTokenSource();

        ChangeSceneAsync(1, m_Cts.Token).Forget();

        m_PageButtons[0].image.color = m_SelectedColor;

        /*m_TestButton.onClick.AsObservable().Subscribe(_ =>
        {
            SceneManager.LoadScene("MockRoomSelectScene", LoadSceneMode.Additive);
        }).AddTo(this);*/

        //各ボタンでシーン切り替え
        //シーン切り替え時はカメラをOnにする必要がある

        for(int i = 0; i < m_PageButtons.Count; i++)
        {
            int index = i;
            Button tempButton = m_PageButtons[i];
            
            tempButton.onClick.AsObservable().Subscribe(_ =>
            {
                int nextSceneIndex = 0;
                /*if (index == 0)
                {
                    nextSceneIndex = 1;
                }
                else
                {
                    nextSceneIndex = index + 2;
                }*/

                nextSceneIndex = index + 1;
                if (m_CurrentSceneIndex == nextSceneIndex) return;
                ChangeSceneAsync(nextSceneIndex, m_Cts.Token).Forget();
                foreach(var button in m_PageButtons)
                {
                    button.image.color = m_UnselectedColor;
                }
                tempButton.image.color = m_SelectedColor;
            }).AddTo(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public async UniTask ChangeSceneAsync(int nextSceneIndex, CancellationToken token)
    {
        if (m_CurrentSceneIndex == nextSceneIndex) return;

        m_CommonUICamera.gameObject.SetActive(true);
        m_LoadingCover.SetActive(true);
        if(m_CurrentSceneIndex != 0)
        {
            await SceneManager.UnloadSceneAsync(m_CurrentSceneIndex);
        }
        await SceneManager.LoadSceneAsync(nextSceneIndex, LoadSceneMode.Additive);
        m_LoadingCover.SetActive(false);
        m_CommonUICamera.gameObject.SetActive(false);
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(nextSceneIndex));
        m_CurrentSceneIndex = nextSceneIndex;
    }

    public async UniTask LoadSceneAsync(string befScene, string aftScene)
    {
#if UNITY_EDITOR
        Debug.Log("Scene Load Start");
#endif
        //シーン遷移
        m_CommonUICamera.gameObject.SetActive(true);
        m_LoadingCover.SetActive(true);
        await SceneManager.UnloadSceneAsync(befScene);
        await SceneManager.LoadSceneAsync(aftScene, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(aftScene));
        m_CurrentSceneIndex = SceneManager.GetSceneByName(aftScene).buildIndex;
        m_LoadingCover.SetActive(false);
        m_CommonUICamera.gameObject.SetActive(false);
#if UNITY_EDITOR
        Debug.Log("Scene Loaded");
#endif
    }
}
