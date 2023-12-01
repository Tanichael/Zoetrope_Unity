using UnityEngine;
using UnityEngine.UI;
using CameraControl;

public class TestCameraSceneController : MonoBehaviour
{
    [SerializeField] Text m_DebugRotateText;
    [SerializeField] Text m_DebugZoomText;
    [SerializeField] GameObject m_Cube;
    [SerializeField] Camera m_Camera;

    private IMockInputProvider m_InputProvider;
    private IRotationInputProvider m_RotationInputProvider;
    private IZoomInputProvider m_ZoomInputProvider;

    // Start is called before the first frame update
    void Start()
    {
#if UNITY_EDITOR
        m_InputProvider = new EditorInputProvider();
        m_RotationInputProvider = new EditorRotationInputProvider(m_InputProvider);
        m_ZoomInputProvider = new EditorZoomInputProvider(m_InputProvider);
#else
        m_InputProvider = new MobileInputProvider();
        m_RotationInputProvider = new MobileRotationInputProvider(m_InputProvider);
        m_ZoomInputProvider = new MobileZoomInputProvider(m_InputProvider);
#endif
    }

    // Update is called once per frame
    void Update()
    {
        m_RotationInputProvider.OnUpdate();
        m_ZoomInputProvider.OnUpdate();

        string debugRotateText = m_DebugRotateText.text;
        string debugZoomText = m_DebugZoomText.text;

        if (m_RotationInputProvider.RotationStarted)
        {
            debugRotateText = "RotationStarted";
        }
        else if(m_RotationInputProvider.RotationEnded)
        {
            debugRotateText = "RotationEnded";
        }
        else if(m_RotationInputProvider.IsRotating)
        {
            debugRotateText = "RotationDelta: " + m_RotationInputProvider.RotationDelta.ToString("F4");
            Vector2 delta = m_RotationInputProvider.RotationDelta;
            m_Cube.transform.position += new Vector3(delta.x * 7f, delta.y * 7f, 0f);
            //Debug.Log(debugText);
        }

        if(m_ZoomInputProvider.ZoomStarted)
        {
            debugZoomText = "ZoomStarted";
        }
        else if (m_ZoomInputProvider.ZoomEnded)
        {
            debugZoomText = "ZoomEnded";
        }
        else if(m_ZoomInputProvider.IsZooming)
        {
            debugZoomText = "ZoomMagnitude: " + m_ZoomInputProvider.ZoomMagnitude.ToString("F4");
            float delta = m_ZoomInputProvider.ZoomMagnitude;
            m_Camera.fieldOfView += delta;
        }

        m_DebugRotateText.text = debugRotateText;
        m_DebugZoomText.text = debugZoomText;
    }
}
