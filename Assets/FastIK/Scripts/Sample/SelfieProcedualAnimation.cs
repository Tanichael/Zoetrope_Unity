using UnityEngine;

namespace DitzelGames.FastIK
{
    class SelfieProcedualAnimation :  MonoBehaviour
    {
        public Transform AvatorRoot;
        public Transform Camera;
        public Transform Head;

        [SerializeField]
        Vector3 m_avatorLocalPos;

        private void Start()
        {
            m_avatorLocalPos = AvatorRoot.position - Camera.position;
            m_avatorLocalPos = Quaternion.Inverse(Camera.rotation) * m_avatorLocalPos;
        }

        public void LateUpdate()
        {
            Head.rotation = Quaternion.LookRotation(Camera.position - Head.position);
            var avtPos = Camera.rotation*m_avatorLocalPos+Camera.position;
            avtPos.y = AvatorRoot.position.y;
            AvatorRoot.position=avtPos;
        }

    }
}
