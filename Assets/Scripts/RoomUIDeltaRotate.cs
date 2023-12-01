using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomUIDeltaRotate : RoomUIBase
{
    [SerializeField] private Slider m_DeltaRotateSliderHorizontal;
    [SerializeField] private Slider m_DeltaRotateSliderVertical;
    [SerializeField] private Button m_CompleteDeltaRotateButton;

    public override RoomPhase GetRoomPhase()
    {
        return RoomPhase.DeltaRotate;
    }

    public override void Init(RoomUIMachine machine, MockRoomManager roomManager, IInputProvider inputProvider)
    {
        base.Init(machine, roomManager, inputProvider);
        m_DeltaRotateSliderHorizontal.gameObject.SetActive(false);
        m_DeltaRotateSliderVertical.gameObject.SetActive(false);
        m_CompleteDeltaRotateButton.gameObject.SetActive(false);

        RectTransform sliderHorizontalRectTransform = m_DeltaRotateSliderHorizontal.transform as RectTransform;
        if (sliderHorizontalRectTransform != null)
        {
            sliderHorizontalRectTransform.sizeDelta = new Vector2(Screen.width * 0.65f, Screen.width * 0.04f);
            float dx = 0f;
            //float dy = Screen.width / 2.5f + Screen.height * 0.5f;
            float dy = Screen.height * 0.62f;
            sliderHorizontalRectTransform.anchoredPosition = new Vector2(dx, dy);
        }
        
        RectTransform sliderVerticalRectTransform = m_DeltaRotateSliderVertical.transform as RectTransform;
        if (sliderVerticalRectTransform != null)
        {
            sliderVerticalRectTransform.sizeDelta = new Vector2(Screen.width * 0.5f, Screen.width * 0.04f);
            sliderVerticalRectTransform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            float dx = Screen.width / 7f;
            float dy = Screen.height * 0.28f;
            sliderVerticalRectTransform.anchoredPosition = new Vector2(dx, dy);
        }

        RectTransform completeRectTransform = m_CompleteDeltaRotateButton.transform as RectTransform;
        if (completeRectTransform != null)
        {
            completeRectTransform.sizeDelta = new Vector2(Screen.width / 2.5f, Screen.width / 2.5f / 3f);
            float dx = 0f;
            float dy = Screen.width * 0.06f + Screen.height * 0.5f;
            completeRectTransform.anchoredPosition = new Vector2(dx, dy);
        }

        m_DeltaRotateSliderHorizontal.onValueChanged.AddListener((deltaAngle) =>
        {
            PublishUIEvent(new DetaRoteteValueChangeEvent(deltaAngle));
        });

        m_CompleteDeltaRotateButton.onClick.AddListener(() =>
        {
            PublishUIEvent(new CompleteDeltaRotateButtoClickEvent(m_DeltaRotateSliderHorizontal.value));
        });
    }

    public override void OnEnterState()
    {
        base.OnEnterState();
        m_DeltaRotateSliderHorizontal.value = m_RoomManager.SelectedObject.GetDeltaAngleHorizontal();
        m_DeltaRotateSliderVertical.value = m_RoomManager.SelectedObject.GetDeltaAngleVertical();

        m_Machine.FurniturePanel.SetActive(true);
        m_Machine.SelectedCanvas.gameObject.SetActive(true);
        m_DeltaRotateSliderHorizontal.gameObject.SetActive(true);
        Debug.Log("is both rotate; " + m_RoomManager.SelectedObject.IsBothRotate);
        if(m_RoomManager.SelectedObject.IsBothRotate)
        {
            m_DeltaRotateSliderVertical.gameObject.SetActive(true);
        }
        m_CompleteDeltaRotateButton.gameObject.SetActive(true);
    }

    public override void OnExitState()
    {
        m_DeltaRotateSliderHorizontal.gameObject.SetActive(false);
        m_DeltaRotateSliderVertical.gameObject.SetActive(false);
        m_CompleteDeltaRotateButton.gameObject.SetActive(false);
        m_Machine.FurniturePanel.SetActive(false);
        m_Machine.SelectedCanvas.gameObject.SetActive(false);
        base.OnExitState();
    }

    private void Update()
    {
        //コマンド経由になっていない
        m_RoomManager.SelectedObject.DeltaRotate(m_DeltaRotateSliderHorizontal.value, m_DeltaRotateSliderVertical.value);
    }

    protected override void PublishUIEvent(RoomUIEvent roomUIEvent)
    {
        base.PublishUIEvent(roomUIEvent);
    }
}
