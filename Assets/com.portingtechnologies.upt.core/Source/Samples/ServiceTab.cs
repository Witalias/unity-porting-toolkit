using UnityEngine;
using UnityEngine.UI;

namespace UPT.Core.Samples
{
    [RequireComponent(typeof(Toggle))]
    public class ServiceTab : MonoBehaviour
    {
        private const string SELECTED_COLOR_HEX = "#B0B0B0";

        [SerializeField] private GameObject m_content;

        private Toggle m_toggle;
        private Color m_normalColor = Color.white;
        private Color m_selectedColor = Color.white;

        private void Awake()
        {
            m_toggle = GetComponent<Toggle>();
            m_normalColor = m_toggle.colors.normalColor;
            ColorUtility.TryParseHtmlString(SELECTED_COLOR_HEX, out m_selectedColor);
        }

        private void OnEnable()
        {
            m_toggle.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnDisable()
        {
            m_toggle.onValueChanged.RemoveListener(OnValueChanged);
        }

        private void OnValueChanged(bool isOn)
        {
            var colors = m_toggle.colors;
            colors.normalColor = isOn ? m_selectedColor : m_normalColor;
            m_toggle.colors = colors;

            if (m_content != null)
                m_content.SetActive(isOn);
        }
    }
}
