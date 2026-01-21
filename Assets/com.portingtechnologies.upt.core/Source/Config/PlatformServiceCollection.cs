using System;
using System.Collections.Generic;
using UnityEngine;

namespace UPT.Core
{
    public class PlatformServiceCollection : ScriptableObject
    {
        [Serializable]
        public class ServiceBackendConfig
        {
            public string ServiceType;
            public string PreferredPlatform;
        }

        public string Name;
        public bool IsActive = false;

        [SerializeField] private List<ServiceBackendConfig> m_serviceConfigs = new List<ServiceBackendConfig>();

        public List<ServiceBackendConfig> GetServiceConfigs() => m_serviceConfigs;

        public ServiceBackendConfig GetServiceConfig(Type serviceType)
        {
            return m_serviceConfigs.Find(c => c.ServiceType == serviceType.Name);
        }

        public ServiceBackendConfig CreateConfig(Type serviceType)
        {
            var config = new ServiceBackendConfig
            {
                ServiceType = serviceType.Name
            };
            m_serviceConfigs.Add(config);

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif

            return config;
        }

        public void SetServicePlatform(Type serviceType, string platformName)
        {
            var config = m_serviceConfigs.Find(c => c.ServiceType == serviceType.Name);
            if (config == null)
            {
                UptLogger.Warning($"The service config {serviceType} wasn't found");
                return;
            }

            config.PreferredPlatform = platformName;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
