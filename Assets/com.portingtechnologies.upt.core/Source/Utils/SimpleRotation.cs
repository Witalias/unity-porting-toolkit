using UnityEngine;

namespace UPT.Core
{
    public class SimpleRotation : MonoBehaviour
    {
        [SerializeField] private float m_rotationSpeed = 50f;

        void Update()
        {
            transform.Rotate(0.0f, 0.0f, m_rotationSpeed * Time.deltaTime, Space.Self);
        }
    }
}
