using UnityEngine;
using UnityEngine.UI;

namespace UPT.Core.Samples
{
    public class GlobalManager : MonoBehaviour
    {
        public static GlobalManager Instance { get; private set; }

        [SerializeField] private GameObject m_spinnerScreen;
        [SerializeField] private RectTransform m_rootUITransform;

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

        public void RebuildLayout()
        {
            if (!m_rootUITransform)
                return;

            foreach (var child in m_rootUITransform.GetComponentsInChildren<LayoutGroup>())
                LayoutRebuilder.ForceRebuildLayoutImmediate(child.transform as RectTransform);

            LayoutRebuilder.ForceRebuildLayoutImmediate(m_rootUITransform);
        }
    }
}
