using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomUIPicture : RoomUIBase
{
    [SerializeField] private Canvas m_PictureCanvas;
    [SerializeField] private PickerController m_PickerController;
    [SerializeField] private RawImage m_BackgroundImage;
    [SerializeField] private TMP_InputField m_InputField;
    [SerializeField] private Camera m_SelectedCamera;
    [SerializeField] private RoomMasterData m_RoomMasterData;
    [SerializeField] private RawImage m_RawImage;
    [SerializeField] private Button m_EditCompleteButton;

    private BoxCollider m_RoomObjectCollider;
    private float m_Phi;
    private float m_Theta;
    private Vector3 m_BefInputPos;
    private Vector3 m_RotateCenter;
    private float m_RotateRadius = 1f;
    private GameObject m_SuspectObject;

    public override RoomPhase GetRoomPhase()
    {
        return RoomPhase.Picture;
    }

    public override void Init(RoomUIMachine machine, MockRoomManager roomManager, IInputProvider inputProvider)
    {
        base.Init(machine, roomManager, inputProvider);
        m_SelectedCamera.gameObject.SetActive(false);
        m_EditCompleteButton.onClick.AddListener(() =>
        {
            PublishUIEvent(new EditCompleteButtonClickEvent());
        });

        m_PickerController.OnPickComplete += () =>
        {
            //m_Machine.OnPickComplete.Invoke();
            PublishUIEvent(new PickCompleteEvent());
        };
    }

    public override void OnEnterState()
    {
        base.OnEnterState();
        m_SelectedCamera.gameObject.SetActive(true);

        RoomObject selected = m_RoomManager.SelectedObject;
        float height = selected.Height * m_RoomMasterData.RoomUnit;
        m_RotateCenter = selected.transform.position + Vector3.up * height / 2f;
        //m_SelectedCamera.gameObject.transform.position = m_RotateCenter - selected.transform.forward * m_RotateRadius;
        m_SelectedCamera.gameObject.transform.position = m_RotateCenter + selected.transform.right * m_RotateRadius;
        m_SelectedCamera.gameObject.transform.LookAt(m_RotateCenter);
        m_SelectedCamera.fieldOfView = 15f * m_RoomManager.SelectedObject.Height / 5f;

        if (m_RoomManager.SelectedObject.Data.PosType == PositionType.WALL)
        {
            m_RotateCenter = selected.transform.position;
            m_SelectedCamera.gameObject.transform.position = m_RotateCenter + selected.transform.right * m_RotateRadius;
            m_SelectedCamera.gameObject.transform.LookAt(m_RotateCenter);
            m_SelectedCamera.fieldOfView = 15f * m_RoomManager.SelectedObject.Height / 5f;
        }
        if (m_RoomManager.SelectedObject.Data.Tags.Contains(Tag.Book))
        {
            /*m_RotateCenter = selected.transform.position;
            m_SelectedCamera.gameObject.transform.position = m_RotateCenter + selected.transform.up * m_RotateRadius;
            m_SelectedCamera.gameObject.transform.LookAt(m_RotateCenter);*/
            m_SelectedCamera.fieldOfView = 15f * m_RoomManager.SelectedObject.Data.GetObjDepth(PutType.NORMAL) / 2f;
        }

        m_Phi = 0f;
        m_Theta = Mathf.PI / 2f;

        //FOVí≤êÆÇ‡

        m_RoomManager.SelectedObject.OnDetail(true);
        m_RoomObjectCollider = m_RoomManager.SelectedObject.GetComponent<BoxCollider>();
        if(m_RoomObjectCollider != null)
        {
            m_RoomObjectCollider.enabled = false;
        }

        RectTransform inputRectTransform = m_InputField.transform as RectTransform;
        if(inputRectTransform != null)
        {
            inputRectTransform.sizeDelta = new Vector2(Screen.width * 0.8f, Screen.height / 3f);
            inputRectTransform.anchoredPosition = new Vector2(0f, Screen.height * (-0.1f));
        }

        RectTransform rawRectTransform = m_RawImage.transform as RectTransform;
        if(rawRectTransform != null)
        {
            rawRectTransform.anchoredPosition = new Vector2(0f, Screen.height * 0.25f);
        }

        RectTransform editCompleteRectTransform = m_EditCompleteButton.transform as RectTransform;
        if(editCompleteRectTransform != null)
        {
            editCompleteRectTransform.anchoredPosition = new Vector2(0f, Screen.height * -0.35f);
            editCompleteRectTransform.sizeDelta = new Vector2(Screen.width * 0.3f, Screen.height * 0.05f);
        }

        m_PictureCanvas.gameObject.SetActive(true);
        m_Machine.ItemPanel.SetActive(true);
        m_BackgroundImage.gameObject.SetActive(true);
        m_EditCompleteButton.gameObject.SetActive(true);

        if(m_RoomManager.SelectedObject.IsPictureSet)
        {
            m_InputField.gameObject.SetActive(true);
            m_InputField.onEndEdit.AddListener((text) =>
            {
                m_RoomManager.SelectedObject.ItemText = text;
            });
        }
        else
        {
            m_InputField.gameObject.SetActive(false);
        }
    }

    public override void OnExitState()
    {
        m_RoomObjectCollider.enabled = true;

        m_SelectedCamera.gameObject.SetActive(false);
        m_PictureCanvas.gameObject.SetActive(false);
        m_BackgroundImage.gameObject.SetActive(false);
        m_InputField.gameObject.SetActive(false);
        m_EditCompleteButton.gameObject.SetActive(false);

        base.OnExitState();
    }

    private void Update()
    {
        if (m_RoomManager.SelectedObject != null)
        {
            int layerMask = (1 << 11);

            float rawMinWidth = Screen.width / 2f - 256f;
            float rawMaxWidth = Screen.width / 2f + 256f;
            float rawMinHeight = Screen.height / 2f + Screen.height / 5f - 256f;
            float rawMaxHeight = Screen.height / 2f + Screen.height / 5f + 256f;

            if (Input.mousePosition.x >= rawMinWidth && Input.mousePosition.x <= rawMaxWidth)
            {
                if (Input.mousePosition.y >= rawMinHeight && Input.mousePosition.y <= rawMaxHeight)
                {
                    Vector3 tapPos = Input.mousePosition - new Vector3(rawMinWidth, rawMinHeight, 0);

                    Ray ray = m_SelectedCamera.ScreenPointToRay(tapPos);

#if UNITY_EDITOR
                    float distance = 100f;
                    float duration = 5f;
                    Debug.DrawRay(ray.origin, ray.direction * distance, Color.red, duration, false);
#endif

                    TapSelectValues values = RoomSingleton.Instance.ObjectSelector.TapSelect(ray, layerMask);
                    
                    if (values.SelectState == TapSelectState.Tap || values.SelectState == TapSelectState.Hold)
                    {
                        ITappable tappable = values.TappableObject;
                        if (tappable.GetGameObject().tag == "ImageSpace")
                        {
                            //m_RoomManager.SelectedPictureObject = tappable.GetGameObject();
                            m_PickerController.OnPressShowPicker(m_BackgroundImage);
                        }
                    }
                }
            }
        }

        if(Input.GetMouseButtonDown(0))
        {
            m_BefInputPos = Input.mousePosition;
        }

        if(Input.GetMouseButton(0))
        {
            Vector3 offset = Input.mousePosition - m_BefInputPos;
            m_Phi += offset.x * 0.01f;
            m_Theta += offset.y * 0.01f;

            m_Phi = Mathf.Clamp(m_Phi, -Mathf.PI, Mathf.PI);
            m_Theta = Mathf.Clamp(m_Theta, 0f, Mathf.PI);
           
            if(m_RoomManager.SelectedObject.Data.PosType == PositionType.WALL)
            {
                m_SelectedCamera.transform.position = m_RotateCenter;
                m_SelectedCamera.transform.position += m_RotateRadius * Mathf.Sin(m_Theta) * (m_RoomManager.SelectedObject.transform.right * Mathf.Cos(m_Phi) + m_RoomManager.SelectedObject.transform.forward * (-Mathf.Sin(m_Phi)));
                m_SelectedCamera.transform.position += m_RotateRadius * Mathf.Cos(m_Theta) * Vector3.up;
            }
            else
            {
                /*if (m_RoomManager.SelectedObject.Data.Tags.Contains(Tag.Book))
                {
                    m_SelectedCamera.transform.position = m_RotateCenter;
                    m_SelectedCamera.transform.position += m_RotateRadius * Mathf.Sin(m_Theta) * (m_RoomManager.SelectedObject.transform.up * Mathf.Cos(m_Phi) + m_RoomManager.SelectedObject.transform.right * (-Mathf.Sin(m_Phi)));
                    m_SelectedCamera.transform.position += m_RotateRadius * Mathf.Cos(m_Theta) * Vector3.forward;
                }*/
                
                m_SelectedCamera.transform.position = m_RotateCenter;
                m_SelectedCamera.transform.position += m_RotateRadius * Mathf.Sin(m_Theta) * ((-1f) * m_RoomManager.SelectedObject.transform.forward * Mathf.Cos(m_Phi) + (-1f) * m_RoomManager.SelectedObject.transform.right * Mathf.Sin(m_Phi));
                m_SelectedCamera.transform.position += m_RotateRadius * Mathf.Cos(m_Theta) * Vector3.up;
            }

            m_SelectedCamera.transform.LookAt(m_RotateCenter);

            m_BefInputPos = Input.mousePosition;
        }
    }

    protected override void PublishUIEvent(RoomUIEvent roomUIEvent)
    {
        base.PublishUIEvent(roomUIEvent);
    }
}
