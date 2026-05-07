using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UPT.Core.Samples;

namespace UPT.Core
{
    public class DynamicInputList : MonoBehaviour
    {
        [SerializeField] private Button m_addButton;
        [SerializeField] private InputFieldElement m_itemPrefab;
        [SerializeField] private Transform m_contentParent;

        private readonly List<InputFieldElement> m_items = new();

        public IReadOnlyList<InputFieldElement> Items => m_items;

        private void Start()
        {
            m_addButton.onClick.AddListener(AddItem);
        }

        public string[] GetStringValues()
        {
            var result = new string[m_items.Count];
            for (var i = 0; i < m_items.Count; i++)
            {
                var value = m_items[i].InputField.text;
                if (string.IsNullOrEmpty(value))
                    return null;
                result[i] = value;
            }
            return result;
        }

        public int[] GetIntValues()
        {
            var result = new int[m_items.Count];
            var stringValues = GetStringValues();
            for (var i = 0; i < stringValues.Length; i++)
            {
                if (!int.TryParse(stringValues[i], out var intValue))
                    return null;
                result[i] = intValue;
            }
            return result;
        }

        private void AddItem()
        {
            var item = Instantiate(m_itemPrefab, m_contentParent);
            var removeButton = item.RemoveButton;
            if (removeButton)
            {
                var index = m_items.Count;
                removeButton.onClick.AddListener(() => RemoveItem(item));
            }
            m_items.Add(item);
            GlobalManager.Instance.RebuildLayout();
        }

        private void RemoveItem(InputFieldElement item)
        {
            m_items.Remove(item);
            Destroy(item.gameObject);
        }
    }
}
