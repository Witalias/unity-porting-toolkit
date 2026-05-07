using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace UPT.Core.Samples
{
    public class SampleServicesLoader : MonoBehaviour
    {
        [SerializeField] private Transform m_serviceTabsContainer;
        [SerializeField] private Transform m_serviceContentsContainer;
        [SerializeField] private ToggleGroup m_serviceTabsToggleGroup;
        [SerializeField] private GameObject m_platformSpecificHeader;
        [SerializeField] private ServiceTab m_serviceTabPrefab;

        private void Start()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var wasAdded = false;

            foreach (var assembly in assemblies)
            {
                if (ModuleUtility.IsSystemAssembly(assembly))
                    continue;

                var providerMethods = assembly.GetTypes()
                    .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public))
                    .Where(m => m.GetCustomAttribute<SampleContentProviderAttribute>() != null)
                    .ToArray();

                foreach (var method in providerMethods)
                {
                    var prefab = method.Invoke(null, null) as GameObject;
                    if (prefab != null)
                    {
                        var serviceTab = Instantiate(m_serviceTabPrefab, m_serviceTabsContainer);
                        var attribute = method.GetCustomAttribute<SampleContentProviderAttribute>();
                        serviceTab.SetText(attribute.ServiceName);
                        serviceTab.gameObject.name = attribute.ServiceName.Replace(" ", "") + "Tab";

                        var serviceContent = Instantiate(prefab, m_serviceContentsContainer);
                        serviceContent.SetActive(false);
                        serviceContent.name = attribute.ServiceName.Replace(" ", "") + "Content";
                        serviceTab.SetContent(serviceContent);

                        if (serviceTab.TryGetComponent<Toggle>(out var toggle))
                            toggle.group = m_serviceTabsToggleGroup;

                        wasAdded = true;
                    }
                }
            }

            m_platformSpecificHeader.SetActive(wasAdded);
        }
    }
}
