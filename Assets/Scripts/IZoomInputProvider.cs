using UnityEngine;

namespace CameraControl
{
    public interface IZoomInputProvider
    {
        bool ZoomStarted { get; }
        bool IsZooming { get; }
        float ZoomMagnitude { get; }
        bool ZoomEnded { get; }
        public void OnUpdate();
    }

    public class MobileZoomInputProvider : IZoomInputProvider
    {
        private IMockInputProvider inputProvider;
        private float previousDistance;

        public MobileZoomInputProvider(IMockInputProvider inputProvider)
        {
            this.inputProvider = inputProvider;
        }

        public bool ZoomStarted => inputProvider.TouchCount == 2 && inputProvider.GetTouch(0).phase == TouchPhase.Began && inputProvider.GetTouch(1).phase == TouchPhase.Began;

        public bool IsZooming => inputProvider.TouchCount == 2 && ZoomStarted == false;

        public float ZoomMagnitude
        {
            get
            {
                if (inputProvider.TouchCount < 2) return 0f;
                Vector2 touch0Pos = inputProvider.GetTouch(0).position;
                Vector2 touch1Pos = inputProvider.GetTouch(1).position;
                float distance = Vector2.Distance(touch0Pos, touch1Pos);
                float magnitude = previousDistance - distance;
                previousDistance = distance;
                return magnitude;
            }
        }

        public bool ZoomEnded => inputProvider.TouchCount < 2 || (inputProvider.GetTouch(0).phase == TouchPhase.Ended || inputProvider.GetTouch(1).phase == TouchPhase.Ended);

        public void OnUpdate()
        {

        }
    }

    public class EditorZoomInputProvider : IZoomInputProvider
    {
        private bool m_IsZooming;
        private bool m_ZoomStarted;
        private bool m_ZoomEnded;

        public EditorZoomInputProvider(IMockInputProvider inputProvider)
        {
            m_IsZooming = false;
            m_ZoomStarted = false;
            m_ZoomEnded = false;
        }

        public bool ZoomStarted
        {
            get
            {
                return m_ZoomStarted;
            }
        }

        public bool IsZooming => m_IsZooming;

        public float ZoomMagnitude => -Input.mouseScrollDelta.y;

        public bool ZoomEnded
        {
            get
            {
                return m_ZoomEnded;
            }
        }

        public void OnUpdate()
        {
            m_ZoomStarted = false;
            m_ZoomEnded = false;

            if (!m_IsZooming && Input.mouseScrollDelta.y != 0f)
            {
                m_ZoomStarted = true;
                m_IsZooming = true;
            }
        }
    }


}
