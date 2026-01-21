using UnityEngine;
using UPT.Services;

namespace UPT.Core.Samples
{
    public class GlobalManager : MonoBehaviour
    {
        public static GlobalManager Instance { get; private set; }

        [SerializeField] private GameObject m_spinnerScreen;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public void SetSpinnerActive(bool value)
        {
            if (m_spinnerScreen != null)
                m_spinnerScreen.SetActive(value);
        }
    }
}
