using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Cysharp.Threading.Tasks;
using UniRx;
using UniRx.Triggers;
using UnityEngine.EventSystems;

public class RoomUIMachine : MonoBehaviour
{
    private readonly string ms_IconDirPath = "Textures/RoomObjectIcons/";

    public IInputProvider InputProvider;
    [SerializeField] private List<RoomUIBase> m_RoomUIList;
    [SerializeField] private Text m_DebugText;
    
    private MockRoomManager m_RoomManager;
    private RoomUIBase m_CurrentUI;
    private RoomUIBase m_NextUI;

    [SerializeField] private RoomObjectMasterData m_MasterData;
    [SerializeField] private RoomMasterData m_RoomMasterData;
    [SerializeField] private Canvas m_SelectedCanvas;
    [SerializeField] private Canvas m_ItemCanvas;
    [SerializeField] private Button m_DetailButton;
    [SerializeField] private Button m_BackButton;
    [SerializeField] private Button m_RotateButton;
    [SerializeField] private Button m_DeleteButton;
    [SerializeField] private Button m_RemoveAllButton;
    [SerializeField] private GameObject m_FurniturePanel;
    [SerializeField] private GameObject m_ItemPanel;
    [SerializeField] private Button m_CompleteTrimButton;
    [SerializeField] private Button m_EditButton;
    [SerializeField] private Button m_UndoButton;
    [SerializeField] private Canvas m_LogCanvas;
    [SerializeField] private GameObject m_LogButton;
    [SerializeField] private Text m_LogText;
    [SerializeField] private Button m_LogYesButton;
    [SerializeField] private Button m_LogNoButton;

    [SerializeField] private ScrollRect m_ScrollTabs;
    [SerializeField] private ScrollRect m_ScrollView;
    [SerializeField] private ItemButton m_ItemButtonPrefab;
    [SerializeField] private CategoryButton m_CategoryButtonPrefab;

    public Canvas SelectedCanvas => m_SelectedCanvas;
    public Canvas ItemCanvas => m_ItemCanvas;
    public Button DetailButton => m_DetailButton;
    public Button BackButton => m_BackButton;
    public Button RotateButton => m_RotateButton;
    public Button DeleteButton => m_DeleteButton;
    public Button RemoveAllButton => m_RemoveAllButton;
    public GameObject FurniturePanel => m_FurniturePanel;
    public GameObject ItemPanel => m_ItemPanel;
    public Button CompleteTrimButton => m_CompleteTrimButton;
    public Button EditButton => m_EditButton;
    public Button UndoButton => m_UndoButton;

    private readonly float ms_UndoCoolTime = 0.2f;
    private float m_UndoElapsedTime = 0f;
    private bool m_IsDragging = false;

    private Subject<RoomUIEvent> m_OnUIEvent = new Subject<RoomUIEvent>();
    public IObservable<RoomUIEvent> OnUIEvent => m_OnUIEvent;

    private bool m_InitLists = false;

    public void Init(MockRoomManager roomManager)
    {
        m_RoomManager = roomManager;
    }

    public void ChangeUI(RoomPhase phase)
    {
        foreach (var roomUI in m_RoomUIList)
        {
            if (roomUI.GetRoomPhase() == phase)
            {
                ChangeUI(roomUI);
            }
        }
    }

    public bool ChangeUI(RoomUIBase nextUI)
    {
        bool isNull = nextUI == null;
        m_NextUI = nextUI;
        return isNull;
    }

    public void TransitUI(RoomPhase phase)
    {
        foreach (var roomUI in m_RoomUIList)
        {
            if (roomUI.GetRoomPhase() == phase)
            {
                TransitUI(roomUI);
            }
        }
    }

    public bool TransitUI(RoomUIBase nextUI)
    {
        bool isNull = nextUI == null;
        m_NextUI = nextUI;
        return isNull;
    }

    private void Start()
    {
        //await UniTask.WaitUntil(() => InputProvider != null, cancellationToken: this.GetCancellationTokenOnDestroy());
        m_DebugText.gameObject.SetActive(false);

        foreach (var roomUI in m_RoomUIList)
        {
            roomUI.Init(this, m_RoomManager, InputProvider);
            roomUI.gameObject.SetActive(false);

            roomUI.OnUIEvent.Subscribe((roomUIEvent) =>
            {
                m_OnUIEvent.OnNext(roomUIEvent);
            }).AddTo(this);
        }

        //ChangeUI(RoomPhase.None);

        //初期化
        #region RECTTRANSFORM
        RectTransform tabsRectTransform = m_ScrollTabs.gameObject.transform as RectTransform;
        if (tabsRectTransform != null)
        {
            tabsRectTransform.sizeDelta = new Vector2(Screen.width, Screen.height * 0.1f);
            float dx = 0f;
            float dy = tabsRectTransform.sizeDelta.y * 0.5f + Screen.height * 0.27f;
            tabsRectTransform.anchoredPosition = new Vector2(dx, dy);
        }

        RectTransform scrollRectTransform = m_ScrollView.gameObject.transform as RectTransform;
        if (scrollRectTransform != null)
        {
            scrollRectTransform.sizeDelta = new Vector2(Screen.width, Screen.height * 0.17f);
            float dx = 0f;
            float dy = scrollRectTransform.sizeDelta.y * 0.5f + Screen.height * 0.1f;
            scrollRectTransform.anchoredPosition = new Vector2(dx, dy);
        }

        RectTransform editRectTransform = m_EditButton.gameObject.transform as RectTransform;
        if (editRectTransform != null)
        {
            editRectTransform.sizeDelta = new Vector2(Screen.width / 5f, Screen.width / 2.5f / 3f);
            float dx = Screen.width / 3f;
            float dy = -Screen.width * 0.133f;
            editRectTransform.anchoredPosition = new Vector2(dx, dy);
        }

        RectTransform rotateRectTransform = m_RotateButton.gameObject.transform as RectTransform;
        if (rotateRectTransform != null)
        {
            rotateRectTransform.sizeDelta = new Vector2(Screen.width / 5f, Screen.width / 2.5f / 3f);
            float dx = -Screen.width / 4f;
            float dy = rotateRectTransform.sizeDelta.y * 3f;
            rotateRectTransform.anchoredPosition = new Vector2(dx, dy);
        }

        RectTransform detailRectTransform = m_DetailButton.gameObject.transform as RectTransform;
        if (detailRectTransform != null)
        {
            detailRectTransform.sizeDelta = new Vector2(Screen.width / 5f, Screen.width / 2.5f / 3f);
            float dx = 0f;
            float dy = detailRectTransform.sizeDelta.y + Screen.height * 0.37f;
            detailRectTransform.anchoredPosition = new Vector2(dx, dy);
        }

        RectTransform backRectTransform = m_BackButton.gameObject.transform as RectTransform;
        if (backRectTransform != null)
        {
            backRectTransform.sizeDelta = new Vector2(Screen.width / 5f, Screen.width / 2.5f / 3f);
            float dx = Screen.width / 4f;
            float dy = backRectTransform.sizeDelta.y + Screen.height * 0.5f;
            backRectTransform.anchoredPosition = new Vector2(dx, dy);
        }

        RectTransform deleteRectTransform = m_DeleteButton.gameObject.transform as RectTransform;
        if (deleteRectTransform != null)
        {
            deleteRectTransform.sizeDelta = new Vector2(Screen.width / 5f, Screen.width / 2.5f / 3f);
            float dx = Screen.width / 4f;
            float dy = deleteRectTransform.sizeDelta.y + Screen.height * 0.37f;
            deleteRectTransform.anchoredPosition = new Vector2(dx, dy);
        }

        RectTransform removeAllRectTransform = m_RemoveAllButton.gameObject.transform as RectTransform;
        if (removeAllRectTransform != null)
        {
            removeAllRectTransform.sizeDelta = new Vector2(Screen.width / 6f, Screen.width / 8f);
            float dx = Screen.width / 3f;
            float dy = removeAllRectTransform.sizeDelta.y * 1f;
            removeAllRectTransform.anchoredPosition = new Vector2(dx, dy);
        }
        
        RectTransform undoRectTransform = m_UndoButton.gameObject.transform as RectTransform;
        if (undoRectTransform != null)
        {
            undoRectTransform.sizeDelta = new Vector2(Screen.width / 6f, Screen.width / 8f);
            float dx = -Screen.width * 0.4f;
            float dy = -undoRectTransform.sizeDelta.y * 1f;
            undoRectTransform.anchoredPosition = new Vector2(dx, dy);
        }

        RectTransform completeTrimRectTransform = m_CompleteTrimButton.gameObject.transform as RectTransform;
        if (completeTrimRectTransform != null)
        {
            completeTrimRectTransform.sizeDelta = new Vector2(Screen.width / 2f, Screen.width / 2.5f / 3f);
            float dx = 0f;
            float dy = Screen.height * 0.15f;
            completeTrimRectTransform.anchoredPosition = new Vector2(dx, dy);
        }

        RectTransform logRectTransform = m_LogButton.gameObject.transform as RectTransform;
        if (logRectTransform != null)
        {
            logRectTransform.sizeDelta = new Vector2(Screen.width * 4f / 5f, Screen.height / 3f);
            float dx = 0f;
            float dy = 0f;
            logRectTransform.anchoredPosition = new Vector2(dx, dy);
        }
        m_LogCanvas.gameObject.SetActive(false);
        m_LogButton.gameObject.SetActive(false);

        RectTransform logYesRectTransform = m_LogYesButton.gameObject.transform as RectTransform;
        if (logYesRectTransform != null)
        {
            logYesRectTransform.sizeDelta = new Vector2(Screen.width / 4f, Screen.width / 10f);
            float dx = - Screen.width / 6f;
            float dy = - Screen.width / 6f;
            logYesRectTransform.anchoredPosition = new Vector2(dx, dy);
        }
        RectTransform logNoRectTransform = m_LogNoButton.gameObject.transform as RectTransform;
        if (logNoRectTransform != null)
        {
            logNoRectTransform.sizeDelta = new Vector2(Screen.width / 4f, Screen.width / 10f);
            float dx = Screen.width / 6f;
            float dy = -Screen.width / 6f;
            logNoRectTransform.anchoredPosition = new Vector2(dx, dy);
        }

        #endregion

        #region EVENT
        DetailButton.onClick.AddListener(() => 
        {
            m_OnUIEvent.OnNext(new DetailButtonClickEvent());
        });

        BackButton.onClick.AddListener(() =>
        {
            m_OnUIEvent.OnNext(new BackButtonClickEvent());

        });

        RotateButton.onClick.AddListener(() =>
        {
            m_OnUIEvent.OnNext(new RotateButtonClickEvent());
        });

        DeleteButton.onClick.AddListener(() =>
        {
            m_OnUIEvent.OnNext(new DeleteButtonClickEvent());
        });

        RemoveAllButton.onClick.AddListener(() =>
        {
            m_OnUIEvent.OnNext(new RemoveAllButtonClickEvent());
        });

        EditButton.onClick.AddListener(() =>
        {
            m_OnUIEvent.OnNext(new EditButtonClickEvent());
        });

        m_UndoButton.onClick.AddListener(() =>
        {
            if(ms_UndoCoolTime < m_UndoElapsedTime)
            {
                m_UndoElapsedTime = 0f;
                m_OnUIEvent.OnNext(new UndoButtonClickEvent());
            }
        });

        #endregion

        m_ScrollTabs.content.sizeDelta = new Vector2((Enum.GetValues(typeof(UICategory))).Length * Screen.width / 4.5f, m_ScrollTabs.content.sizeDelta.y);
        for(int i = 0; i < Enum.GetValues(typeof(UICategory)).Length; i++)
        {
            UICategory category = (UICategory)i;
            CategoryButton categoryButton = Instantiate(m_CategoryButtonPrefab, m_ScrollTabs.content);
            categoryButton.Category = category;
            categoryButton.Text.text = category.ToString();
            categoryButton.Button.onClick.AddListener(() =>
            {
                //OnPut.Invoke(itemButton.Data);
                SetItemCategoryList(categoryButton.Category);
            });

            RectTransform rectTransform = categoryButton.transform as RectTransform;
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(Screen.width / 4.5f, Screen.width / 5f * 3f / 4f);

                int col = i;

                float dx = rectTransform.sizeDelta.x * (0.5f + i);
                float dy = -rectTransform.sizeDelta.y / 1.8f;

                /* float dx = (i - ((float)m_MasterData.RoomObjects.Count / 2f) + 0.5f) * rectTransform.sizeDelta.x;
                float dy = -rectTransform.sizeDelta.y;*/
                rectTransform.anchoredPosition = new Vector2(dx, dy);
            }
            categoryButton.gameObject.SetActive(true);
        }

        SetItemCategoryList(UICategory.Poster);

        m_InitLists = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_InitLists) return;

        if (m_NextUI != null)
        {
            if (m_CurrentUI != null)
            {
                m_CurrentUI.OnExitState();
            }
            m_CurrentUI = m_NextUI;
            m_CurrentUI.OnEnterState();
            m_NextUI = null;
        }

        if(Input.GetMouseButtonDown(2) || Input.GetKeyDown(KeyCode.K))
        {
            //m_DebugText.gameObject.SetActive(!m_DebugText.gameObject.activeSelf);
        }

        m_UndoElapsedTime += Time.deltaTime;
    }

    public void HandleRoomEvent(RoomEvent roomEvent)
    {
        if(roomEvent is RoomEventLogTwoAnswer)
        {
            m_LogCanvas.gameObject.SetActive(true);
            m_FurniturePanel.SetActive(false);
            m_ItemPanel.SetActive(false);

            //ひとまずダウンキャストで対応
            RoomEventLogTwoAnswer logTwoAnswer = roomEvent as RoomEventLogTwoAnswer;
            m_LogText.text = logTwoAnswer.LogText;
            SetLogPanelAsync(0f, 1f).Forget();

            m_LogYesButton.onClick.AddListener(() =>
            {
                m_LogCanvas.gameObject.SetActive(false);
                m_FurniturePanel.SetActive(true);
                m_ItemPanel.SetActive(true);
                m_OnUIEvent.OnNext(logTwoAnswer.YesEvent);
                m_LogButton.gameObject.SetActive(false);
                m_LogYesButton.onClick.RemoveAllListeners();
            });
            m_LogNoButton.onClick.AddListener(() =>
            {
                m_LogCanvas.gameObject.SetActive(false);
                m_FurniturePanel.SetActive(true);
                m_ItemPanel.SetActive(true);
                m_OnUIEvent.OnNext(logTwoAnswer.NoEvent);
                m_LogButton.gameObject.SetActive(false);
                m_LogNoButton.onClick.RemoveAllListeners();
            });
        }
    }

    private async UniTask SetLogPanelAsync(float startRatio, float endRatio)
    {
        RectTransform logTransform = m_LogButton.transform as RectTransform;
        float duration = 0.1f;
        float elapsedTime = 0f;
        m_LogButton.gameObject.SetActive(true);
        if (logTransform != null)
        {
            Vector2 originSize = logTransform.sizeDelta;
            logTransform.sizeDelta = originSize * startRatio;

            while(elapsedTime < duration)
            {
                Debug.Log("logTransform.sizeDelta " + logTransform.sizeDelta);
                logTransform.sizeDelta = originSize * startRatio + originSize * elapsedTime / duration * (endRatio - startRatio);
                elapsedTime += Time.deltaTime;
                await UniTask.DelayFrame(1);
            }
            logTransform.sizeDelta = originSize;
        }
        Debug.Log("set log panel");
    }

    private void SetItemCategoryList(UICategory category)
    {
        //いったんリセット
        for(int i = 1; i < m_ScrollView.content.transform.childCount; i++)
        {
            Destroy(m_ScrollView.content.transform.GetChild(i).gameObject);
        }

        RectTransform contentRectTransform = m_ScrollView.content.gameObject.transform as RectTransform;
        contentRectTransform.anchoredPosition = new Vector2(contentRectTransform.anchoredPosition.x, 0f);
        m_ScrollView.velocity = new Vector2(0f, 0f);

        int categoryIdx = 0;
        for (int i = 0; i < m_MasterData.RoomObjects.Count; i++)
        {
            //ItemButton itemButton = Instantiate(m_ItemButtonPrefab, m_ItemCanvas.transform);
            RoomObjectData data = m_MasterData.RoomObjects[i];
            if (data.UICategory == category)
            {
                if(GetIsOverSize(m_MasterData.RoomObjects[i]))
                {
                    continue;
                }

                if(data.Model is RoomObjectMovable == false)
                {
                    continue;
                }
                
                ItemButton itemButton = Instantiate(m_ItemButtonPrefab, m_ScrollView.content);
                itemButton.Data = m_MasterData.RoomObjects[i];
                itemButton.Text.text = m_MasterData.RoomObjects[i].Model.name;
                Texture2D iconTexture = Resources.Load(ms_IconDirPath + m_MasterData.RoomObjects[i].Model.name) as Texture2D;
                if(iconTexture != null)
                {
                    itemButton.IconImage.texture = iconTexture;
                }
                else
                {
                    Debug.Log("Could not read an icon picture");
                }

                //ランダム配置の廃止
                /*itemButton.Button.onClick.AddListener(() =>
                {
                    //OnPut.Invoke(itemButton.Data);
                    m_OnUIEvent.OnNext(new PutItemEvent(itemButton.Data));
                    m_DebugText.text = itemButton.Data.Model.gameObject.name;
                });*/

                IDisposable longPressSubscription = null;

                var eventTrigger = itemButton.gameObject.AddComponent<ObservableEventTrigger>();

                eventTrigger.OnPointerDownAsObservable()
                    .Subscribe(_ =>
                    {
                        longPressSubscription = Observable.Timer(TimeSpan.FromSeconds(0.3))
                            .TakeUntil(eventTrigger.OnBeginDragAsObservable())
                            .Subscribe(_ =>
                            {
                                m_OnUIEvent.OnNext(new DragPutItemEvent(itemButton.Data));
                            }).AddTo(itemButton.gameObject);
                    }).AddTo(gameObject);
                eventTrigger.OnPointerUpAsObservable()
                    .Merge(eventTrigger.OnBeginDragAsObservable())
                    .Subscribe(_ =>
                    {
                        longPressSubscription?.Dispose();
                    }).AddTo(itemButton.gameObject);

                /*eventTrigger
                    .OnPointerDownAsObservable().Select(_ => true)
                    .Merge(eventTrigger.OnPointerUpAsObservable().Select(_ => false))
                    .Throttle(TimeSpan.FromSeconds(0.3))
                    .Where(b => b)
                    .AsUnitObservable()
                    .Subscribe(unit =>
                    {
                        m_OnUIEvent.OnNext(new DragPutItemEvent(itemButton.Data));
                    }).AddTo(itemButton.gameObject);*/

                RectTransform rectTransform = itemButton.transform as RectTransform;
                if (rectTransform != null)
                {
                    rectTransform.sizeDelta = new Vector2(Screen.width / 4.5f, Screen.width / 4.5f);

                    int col = categoryIdx % 4;
                    int row = categoryIdx / 4;

                    float dx = (col - 2f + 0.5f) * rectTransform.sizeDelta.x;
                    float dy = -rectTransform.sizeDelta.y * (row + 0.6f);

                    /* float dx = (i - ((float)m_MasterData.RoomObjects.Count / 2f) + 0.5f) * rectTransform.sizeDelta.x;
                    float dy = -rectTransform.sizeDelta.y;*/
                    rectTransform.anchoredPosition = new Vector2(dx, dy);
                }
                itemButton.gameObject.SetActive(true);
                categoryIdx++;
            }
            //m_ScrollView.content.sizeDelta = new Vector2(m_ScrollView.content.sizeDelta.x, Screen.width / 5f * 3f / 4f * (categoryIdx + 12) / 4);
            m_ScrollView.content.sizeDelta = new Vector2(m_ScrollView.content.sizeDelta.x, Screen.width / 4.5f * (categoryIdx + 8) / 4);
        }
    }

    private bool GetIsOverSize(RoomObjectData data)
    {
        if(data.GetObjWidth(PutType.NORMAL) > m_RoomMasterData.RoomWidth || data.GetObjWidth(PutType.REVERSE) > m_RoomMasterData.RoomWidth)
        {
            return true;
        }

        if (data.GetObjDepth(PutType.NORMAL) > m_RoomMasterData.RoomDepth || data.GetObjDepth(PutType.REVERSE) > m_RoomMasterData.RoomDepth)
        {
            return true;
        }

        if (data.GetObjHeight(PutType.NORMAL) > m_RoomMasterData.RoomHeight || data.GetObjHeight(PutType.REVERSE) > m_RoomMasterData.RoomHeight)
        {
            return true;
        }

        return false;
    }
}
