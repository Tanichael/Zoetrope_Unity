using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public class GameManager : MonoBehaviour
{
    [SerializeField] private WindowsInputProvider m_WindowsInputProviderPrefab;
    [SerializeField] private AndroidInputProvider m_AndroidInputProviderPrefab;
    [SerializeField] private RoomObjectMasterData m_RoomObjectMasterData;
    [SerializeField] private RoomMasterData m_RoomMasterData;
    [SerializeField] private MockRoomManager m_MockRoomManager;
    [SerializeField] private AvatarController m_AvatarControllerPrefab;
    //[SerializeField] private SampleCameraController m_CameraController;

    [SerializeField] private GameObject m_Ceiling;
    [SerializeField] private GameObject m_BackWallX;
    [SerializeField] private GameObject m_BackWallZ;
    [SerializeField] private Camera m_SampleCamera;
    [SerializeField] private Camera m_InsideCamera;
    [SerializeField] private Camera m_StudioNonARCamera;
    [SerializeField] private SampleCameraController m_SampleCameraControllerPrefab;
    [SerializeField] private InsideCameraController m_InsideCameraControllerPrefab;
    [SerializeField] private StudioNonARCameraController m_StudioNonARCameraControllerPrefab;
    //[SerializeField] private MockCameraController m_MockCameraControllerPrefab;

    [SerializeField] private RoomUIMachine m_RoomUIMachinePrefab;
    [SerializeField] private RoomPhaseMachine m_RoomPhaseMachinePrefab;
    [SerializeField] private Text m_DebugMessage;
    [SerializeField] private Button m_DeleteSaveDataButton;
    [SerializeField] private Button m_ChangeCameraButton;
    [SerializeField] private Button m_SetTemplateButton;
    [SerializeField] private AvatarController m_AvatarControllerPreafab;

    private CameraController m_CameraController;
    private List<CameraController> m_CameraList = new List<CameraController>();
    private RoomUIMachine m_RoomUIMachine;
    private RoomPhaseMachine m_RoomPhaseMachine;

    private float m_SaveTimer;
    private float m_DebugTimer;
    private bool m_IsAlart = false;

    private Dictionary<RoomPhase, CameraController> m_CameraControllerDict = new Dictionary<RoomPhase, CameraController>();

    private CancellationTokenSource m_Cts;

    private void Start()
    {
        m_Cts = new CancellationTokenSource();
        m_DebugMessage.gameObject.SetActive(false);
        m_DeleteSaveDataButton.gameObject.SetActive(false);
        m_SetTemplateButton.gameObject.SetActive(false);
        m_ChangeCameraButton.gameObject.SetActive(false);
        Application.logMessageReceived += HandleLog;

        m_RoomUIMachine = Instantiate(m_RoomUIMachinePrefab);
        m_RoomUIMachine.Init(m_MockRoomManager);
        m_RoomPhaseMachine = Instantiate(m_RoomPhaseMachinePrefab);
        IRoomCommander roomCommander = new MockRoomCommander(m_MockRoomManager, m_RoomPhaseMachine);

#if UNITY_EDITOR
        WindowsTouchManager windowsTouchManager = new WindowsTouchManager();
        m_RoomPhaseMachine.Init(m_MockRoomManager, windowsTouchManager, roomCommander);
        WindowsInputProvider windowsInputProvider = Instantiate(m_WindowsInputProviderPrefab);
        SampleCameraController sampleCameraController = Instantiate(m_SampleCameraControllerPrefab);
        InsideCameraController insideCameraController = Instantiate(m_InsideCameraControllerPrefab);
        StudioNonARCameraController studioNonARCameraController = Instantiate(m_StudioNonARCameraControllerPrefab);
        sampleCameraController.Camera = m_SampleCamera;
        sampleCameraController.RoomMasterData = m_RoomMasterData;
        sampleCameraController.InputProvider = windowsInputProvider;
        insideCameraController.Camera = m_InsideCamera;
        insideCameraController.RoomMasterData = m_RoomMasterData;
        insideCameraController.InputProvider = windowsInputProvider;
        studioNonARCameraController.Camera = m_StudioNonARCamera;
        studioNonARCameraController.RoomMasterData = m_RoomMasterData;
        studioNonARCameraController.InputProvider = windowsInputProvider;

        foreach(RoomPhase phase in Enum.GetValues(typeof(RoomPhase)))
        {
            CameraController tempController = null;
            switch(phase)
            {
                case RoomPhase.None:
                    //tempController = sampleCameraController;
                    tempController = studioNonARCameraController;
                    break;
                case RoomPhase.Detail:
                case RoomPhase.EmptySpace:
                case RoomPhase.View:
                case RoomPhase.VirtualCam:
                    tempController = studioNonARCameraController;
                    break;
                default:
                    break;
            }
            if(tempController != null)
            {
                m_CameraControllerDict.Add(phase, tempController);
            }
        }

        //m_CameraController.InputProvider = windowsInputProvider;
        m_RoomUIMachine.InputProvider = windowsInputProvider;
#else
        Screen.orientation = ScreenOrientation.Portrait;
        MobileTouchManager mobileTouchManager = new MobileTouchManager();
        //WindowsTouchManager mobileTouchManager = new WindowsTouchManager();
        m_RoomPhaseMachine.Init(m_MockRoomManager, mobileTouchManager, roomCommander);
        AndroidInputProvider androidInputProvider = Instantiate(m_AndroidInputProviderPrefab);
/*        MockCameraController mockCameraController = Instantiate(m_MockCameraControllerPrefab);
        m_CameraController = mockCameraController;*/
        SampleCameraController sampleCameraController = Instantiate(m_SampleCameraControllerPrefab);
        InsideCameraController insideCameraController = Instantiate(m_InsideCameraControllerPrefab);
        StudioNonARCameraController studioNonARCameraController = Instantiate(m_StudioNonARCameraControllerPrefab);
        sampleCameraController.Camera = m_SampleCamera;
        sampleCameraController.RoomMasterData = m_RoomMasterData;
        sampleCameraController.InputProvider = androidInputProvider;
        insideCameraController.Camera = m_InsideCamera;
        insideCameraController.RoomMasterData = m_RoomMasterData;
        insideCameraController.InputProvider = androidInputProvider;
        studioNonARCameraController.Camera = m_StudioNonARCamera;
        studioNonARCameraController.RoomMasterData = m_RoomMasterData;
        studioNonARCameraController.InputProvider = androidInputProvider;
        //m_CameraController.InputProvider = androidInputProvider;
        m_RoomUIMachine.InputProvider = androidInputProvider;
#endif
        m_CameraList.Add(sampleCameraController);
        m_CameraList.Add(insideCameraController);
        m_CameraList.Add(studioNonARCameraController);
        
        m_CameraController = studioNonARCameraController;
        sampleCameraController.Camera.gameObject.SetActive(false);
        insideCameraController.Camera.gameObject.SetActive(false);
        sampleCameraController.gameObject.SetActive(false);
        insideCameraController.gameObject.SetActive(false);
        studioNonARCameraController.Camera.gameObject.SetActive(true);
        studioNonARCameraController.gameObject.SetActive(true);

        m_RoomPhaseMachine.CurrentCameraController = studioNonARCameraController;

        m_Ceiling.gameObject.SetActive(true);
        m_BackWallX.gameObject.SetActive(true);
        m_BackWallZ.gameObject.SetActive(true);

        m_RoomMasterData.InitSharedUI();

        m_RoomPhaseMachine.OnChangePhse.Subscribe(async (nextPhase) =>
        {
            if (nextPhase != null)
            {
                Debug.Log("nextphase: " + nextPhase.GetRoomPhase());
                ChangeCamera(nextPhase.GetRoomPhase());

                if ((m_RoomPhaseMachine.CurrentPhase?.GetRoomPhase() == RoomPhase.View && nextPhase.GetRoomPhase() != RoomPhase.Picture) || (m_RoomPhaseMachine.CurrentPhase?.GetRoomPhase() == RoomPhase.Picture && nextPhase.GetRoomPhase() == RoomPhase.Selected))
                {
                    if(nextPhase.GetRoomPhase() != RoomPhase.VirtualCam)
                    {
                        m_Cts.Cancel();
                        m_Cts = new CancellationTokenSource();
                        try
                        {
                            //await SetEditRectAsync(m_CameraController.Camera, true, m_Cts.Token);
                            await SetEditCameraConditionAsync(m_CameraController.Camera, true, m_Cts.Token);
                            //StartCoroutine(SetEditRectCoroutine(m_CameraController.Camera, true));
                        }
                        catch (Exception e)
                        {
                            Debug.Log(e.Message);
                        }
                    }
                }

                if (nextPhase.GetRoomPhase() == RoomPhase.View || nextPhase.GetRoomPhase() == RoomPhase.Picture)
                {
                    m_Cts.Cancel();
                    m_Cts = new CancellationTokenSource();
                    try
                    {
                        //await SetEditRectAsync(m_CameraController.Camera, false, m_Cts.Token);
                        await SetEditCameraConditionAsync(m_CameraController.Camera, false, m_Cts.Token);
                        //StartCoroutine(SetEditRectCoroutine(m_CameraController.Camera, false));
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message);
                    }
                }

                m_RoomUIMachine.ChangeUI(nextPhase.GetRoomPhase());
            }
        }).AddTo(this);

        m_RoomPhaseMachine.OnTransitPhase.Subscribe(async (nextPhase) =>
        {
            if (nextPhase != null)
            {
                m_RoomUIMachine.ChangeUI(RoomPhase.White);
                await m_CameraController.ChangeViewType(nextPhase, m_MockRoomManager.SelectedFloorObject);
                m_RoomUIMachine.ChangeUI(nextPhase.GetRoomPhase());
                m_CameraController.SetRoomPhase(nextPhase);

                foreach (var cameraContoller in m_CameraList)
                {
                    cameraContoller.SetRoomPhase(nextPhase);
                }

                m_RoomPhaseMachine.ChangePhase(nextPhase);
            }
        }).AddTo(this);

        m_RoomPhaseMachine.OnRoomEvent.Subscribe((roomEvent) =>
        {
            m_CameraController.HandleRoomEvent(roomEvent);
            m_RoomUIMachine.HandleRoomEvent(roomEvent);
        }).AddTo(this);

        m_RoomUIMachine.OnUIEvent.Subscribe((roomUIEvent) =>
        {
            m_RoomPhaseMachine.CurrentPhase.HandleUIEvent(roomUIEvent);
        }).AddTo(this);

        m_MockRoomManager.OnDoubleTapRoomObject.Subscribe(async (roomObject) =>
        {
            RoomPhase currentPhase = m_RoomPhaseMachine.CurrentPhase.GetRoomPhase();
            m_RoomUIMachine.ChangeUI(RoomPhase.White);
            await m_CameraController.HandleUIEventAsync(new RoomObjectDoubleTapEvent(roomObject));
            m_RoomUIMachine.ChangeUI(currentPhase);
        }).AddTo(this);

        m_SaveTimer = 0f;
        m_DebugTimer = 0f;

        RoomObjectData fixedTable = null;
        RoomObjectData fixedChair = null;

        for (int i = 0; i < m_RoomObjectMasterData.RoomObjects.Count; i++)
        {
            var data = m_RoomObjectMasterData.RoomObjects[i];
            data.Init(m_RoomMasterData, i);

            if (data.Model.gameObject.name == "chair")
            {
                fixedChair = data;
            }
            if (data.Model.gameObject.name == "desk6_mod")
            {
                fixedTable = data;
            }
        }

        SaveData saveData = RoomSingleton.Instance.RoomDataHolder.GetData();
        float offset = 0.96f;

        //saveData = null;

        m_MockRoomManager.Init(saveData, fixedTable, fixedChair, offset, m_AvatarControllerPreafab);
        m_CameraController.Init(fixedTable, fixedChair, offset);
        m_DeleteSaveDataButton.onClick.AddListener(() =>
        {
            RoomSingleton.Instance.RoomSaveManager.Delete();
        });

        m_SetTemplateButton.onClick.AddListener(() =>
        {
            RoomSingleton.Instance.RoomSaveManager.SetTemplate(m_MockRoomManager);
        });

        m_ChangeCameraButton.OnClickAsObservable().Subscribe(_ =>
        {
           /* if (m_CameraController == sampleCameraController)
            {
                sampleCameraController.Camera.gameObject.SetActive(false);
                insideCameraController.Camera.gameObject.SetActive(true);
                m_Ceiling.gameObject.SetActive(true);
                m_BackWallX.gameObject.SetActive(true);
                m_BackWallZ.gameObject.SetActive(true);
                sampleCameraController.gameObject.SetActive(false);
                insideCameraController.gameObject.SetActive(true);
                m_CameraController = insideCameraController;
            }
            else if (m_CameraController == insideCameraController)
            {
                sampleCameraController.Camera.gameObject.SetActive(true);
                insideCameraController.Camera.gameObject.SetActive(false);
                m_Ceiling.gameObject.SetActive(false);
                m_BackWallX.gameObject.SetActive(false);
                m_BackWallZ.gameObject.SetActive(false);
                sampleCameraController.gameObject.SetActive(true);
                insideCameraController.gameObject.SetActive(false);
                m_CameraController = sampleCameraController;
            }*/
        }).AddTo(this);
    }

    private void Update()
    {
        m_SaveTimer += Time.deltaTime;

        if (!m_IsAlart && m_SaveTimer >= 10f)
        {
            m_SaveTimer = 0f;
            RoomSingleton.Instance.RoomSaveManager.Save(m_MockRoomManager);
        }
        
        if (Input.GetMouseButton(2) && m_DebugTimer <= 3f)
        {
            m_DebugTimer += Time.deltaTime;
            if (m_DebugTimer >= 3f)
            {
                //m_DebugMessage.gameObject.SetActive(!m_DebugMessage.gameObject.activeSelf);
                m_DeleteSaveDataButton.gameObject.SetActive(!m_DeleteSaveDataButton.gameObject.activeSelf);
                m_SetTemplateButton.gameObject.SetActive(!m_SetTemplateButton.gameObject.activeSelf);
                m_ChangeCameraButton.gameObject.SetActive(!m_ChangeCameraButton.gameObject.activeSelf);
            }
        }
        if(Input.GetMouseButtonDown(2))
        {
            m_DebugTimer = 0f;
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            m_CameraController.HandleUIEventAsync(new RoomObjectDoubleTapEvent(m_MockRoomManager.SelectedObject)).Forget();
        }

       /* if (EventSystem.current.IsPointerOverGameObject(-1) || EventSystem.current.IsPointerOverGameObject(0))
        {
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int layerMask = (1 << 6) + (1 << 12) + (1 << 13);
        TapSelectValues values = RoomSingleton.Instance.ObjectSelector.TapSelect(ray, layerMask);*/
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Error)
        {
            Debug.Log("An error occurred: " + logString);
            m_IsAlart = true;
            m_DebugMessage.text += logString;
        }
    }

    private async UniTask SetEditRectAsync(Camera camera, bool isEdit, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        float target = 0.37f;
        if(!isEdit)
        {
            target = 0f; 
        }
        while(true)
        {
            token.ThrowIfCancellationRequested();

            if (Mathf.Abs(camera.rect.y - target) < 0.01f)
            {
                break;
            }
            float tempY = Mathf.Lerp(camera.rect.y, target, 0.2f);
            camera.rect = new Rect(0f, tempY, 1f, 1f - tempY);
            await UniTask.DelayFrame(1);
        }
    }

    private async UniTask SetEditCameraConditionAsync(Camera camera, bool isEdit, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        float target = 0.37f;
        float aftFOV;
        float befFOV = camera.fieldOfView;
        Debug.Log("beffov: " + befFOV);
        aftFOV = Mathf.Atan(Mathf.Tan(befFOV / 2 * Mathf.Deg2Rad) / target) * Mathf.Rad2Deg * 2;
        float distance = camera.nearClipPlane;
        Vector3 targetPos = camera.transform.position - Vector3.up * distance * (Mathf.Tan(aftFOV * Mathf.Deg2Rad / 2f) - Mathf.Tan(befFOV * Mathf.Deg2Rad / 2f));

        if (!isEdit)
        {
            aftFOV = Mathf.Atan(Mathf.Tan(befFOV / 2 * Mathf.Deg2Rad) * target) * Mathf.Rad2Deg * 2;
            targetPos = camera.transform.position - Vector3.up * distance * (Mathf.Tan(befFOV * Mathf.Deg2Rad / 2f) - Mathf.Tan(aftFOV * Mathf.Deg2Rad / 2f));
        }
        while (true)
        {
            token.ThrowIfCancellationRequested();

            if(Mathf.Abs(camera.fieldOfView - aftFOV) < 0.01f && Vector3.Distance(camera.transform.position, targetPos) < 0.01f)
            {
                break;
            }

            camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, aftFOV, 0.2f);
            camera.transform.position = Vector3.Lerp(camera.transform.position, targetPos, 0.2f);

            if (Mathf.Abs(camera.rect.y - target) < 0.01f)
            {
                break;
            }
            /*float tempY = Mathf.Lerp(camera.rect.y, target, 0.2f);
            camera.rect = new Rect(0f, tempY, 1f, 1f - tempY);*/
            await UniTask.DelayFrame(1);
        }
    }

    private IEnumerator SetEditRectCoroutine(Camera camera, bool isEdit)
    {
        float target = 0.37f;
        if (!isEdit)
        {
            target = 0f;
        }
        while (true)
        {
            if (Mathf.Abs(camera.rect.y - target) < 0.01f)
            {
                break;
            }
            float tempY = Mathf.Lerp(camera.rect.y, target, 0.2f);
            camera.rect = new Rect(0f, tempY, 1f, 1f - tempY);
            //await UniTask.DelayFrame(1);
            yield return null;
        }
    }

    private void ChangeCamera(RoomPhase nextPhase)
    {
        if (m_CameraControllerDict.ContainsKey(nextPhase))
        {
            m_CameraController = m_CameraControllerDict[nextPhase];
        }

        foreach (var controller in m_CameraList)
        {
            controller.SetRoomPhase(nextPhase);

            if (controller == m_CameraController)
            {
                controller.Camera.gameObject.SetActive(true);
                controller.gameObject.SetActive(true);
                m_RoomPhaseMachine.CurrentCameraController = controller;
            }
            else
            {
                controller.Camera.gameObject.SetActive(false);
                controller.gameObject.SetActive(false);
            }
        }
    }
}
