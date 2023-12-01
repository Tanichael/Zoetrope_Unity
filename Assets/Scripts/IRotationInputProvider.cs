using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace CameraControl
{
    public interface IRotationInputProvider
    {
        bool RotationStarted { get; }
        bool IsRotating { get; }
        Vector2 RotationDelta { get; }
        bool RotationEnded { get; }

        public void OnUpdate();
    }

    public class MobileRotationInputProvider : IRotationInputProvider
    {
        private IMockInputProvider inputProvider;
        private bool m_WasRotating;
        private bool m_RotationStarted;

        public MobileRotationInputProvider(IMockInputProvider inputProvider)
        {
            this.inputProvider = inputProvider;
        }

        public bool RotationStarted
        {
            get => m_RotationStarted;
        }

        public bool IsRotating => inputProvider.TouchCount == 1 && inputProvider.GetTouch(0).phase == TouchPhase.Moved || inputProvider.GetTouch(0).phase == TouchPhase.Stationary;

        public Vector2 RotationDelta
        {
            get
            {
                if (inputProvider.TouchCount < 1) return Vector2.zero;
                return inputProvider.GetTouch(0).deltaPosition;
            }
        }

        public bool RotationEnded => inputProvider.TouchCount < 1 || inputProvider.GetTouch(0).phase == TouchPhase.Ended;

        public void OnUpdate()
        {
            bool isRotating = inputProvider.TouchCount == 1 && inputProvider.GetTouch(0).phase != TouchPhase.Ended;
            m_RotationStarted = isRotating && !m_WasRotating;
            m_WasRotating = isRotating;
        }
    }

    public class EditorRotationInputProvider : IRotationInputProvider
    {
        private IMockInputProvider m_InputProvider;
        private bool m_IsRotating;
        private bool m_RotationStarted;
        private bool m_RotationEnded;
        private Vector2 m_CurrentVelocity;
        private Vector2 m_DeltaPosition;

        private CancellationTokenSource m_Cts;

        public EditorRotationInputProvider(IMockInputProvider inputProvider)
        {
            m_InputProvider = inputProvider;
            m_IsRotating = false;
            m_RotationStarted = false;
            m_RotationEnded = false;

            m_Cts = new CancellationTokenSource();
            m_DeltaPosition = Vector2.zero;
        }

        public bool RotationStarted
        {
            get
            {
                return m_RotationStarted;
            }
        }

        public bool IsRotating => m_IsRotating;

        public Vector2 RotationDelta
        {
            get
            {
                if (m_InputProvider.TouchCount < 1)
                {
                    return m_DeltaPosition;
                }
                return m_InputProvider.GetTouch(0).deltaPosition;
            }
        }

        public bool RotationEnded
        {
            get
            {
                return m_RotationEnded;
            }
        }

        public void OnUpdate()
        {
            m_InputProvider.OnUpdate();

            m_RotationStarted = false;
            m_RotationEnded = false;

            if (m_InputProvider.TouchCount == 1 && m_InputProvider.GetTouch(0).phase == TouchPhase.Began)
            {
                if(m_IsRotating)
                {
                    m_Cts.Cancel();
                    m_Cts = new CancellationTokenSource();

                    m_CurrentVelocity = Vector2.zero;
                    m_DeltaPosition = Vector2.zero;
                }
                
                m_IsRotating = true;
                m_RotationStarted = true;
            }

            if (m_IsRotating && m_InputProvider.TouchCount == 1 && m_InputProvider.GetTouch(0).phase == TouchPhase.Ended)
            {
                m_CurrentVelocity = Vector2.Scale(m_InputProvider.GetTouch(0).deltaPosition, new Vector2(Screen.width, Screen.height)) / Time.deltaTime;
                RotateInertiaAsync().Forget();
            }

            if(m_IsRotating && m_InputProvider.TouchCount == 1 && m_InputProvider.GetTouch(0).phase == TouchPhase.Moved)
            {
                m_CurrentVelocity = Vector2.Scale(m_InputProvider.GetTouch(0).deltaPosition, new Vector2(Screen.width, Screen.height)) / Time.deltaTime;
            }
        }

        private async UniTask RotateInertiaAsync()
        {
            Vector2 deltaPosition = Vector2.zero;
            m_DeltaPosition = deltaPosition;
            while(m_CurrentVelocity.magnitude > 1f)
            {
                m_CurrentVelocity = Vector3.Lerp(m_CurrentVelocity, Vector2.zero, Time.deltaTime * 5f);
                deltaPosition += m_CurrentVelocity * Time.deltaTime * 0.75f;
                if(deltaPosition.magnitude > 1f)
                {
                    deltaPosition = new Vector2(deltaPosition.x / Screen.width, deltaPosition.y / Screen.height);
                    m_DeltaPosition = deltaPosition;
                    deltaPosition = Vector2.zero;
                }
                else
                {
                    m_DeltaPosition = Vector2.zero;
                }
                await UniTask.DelayFrame(1, cancellationToken: m_Cts.Token);
            }
            Debug.Log("inertia end");
            m_IsRotating = false;
            m_RotationEnded = true;
            m_CurrentVelocity = Vector2.zero;
        }
    }

}