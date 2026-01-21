using System;
using UnityEngine;

namespace UPT.Core
{
    public class UptBootstrapper : MonoBehaviour
    {
        private PlatformModuleManager m_moduleManager;
        private bool m_initialized;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeOnLoad()
        {
            var bootstrapObject = new GameObject("[UPT Bootstrapper]");
            bootstrapObject.AddComponent<UptBootstrapper>();
            DontDestroyOnLoad(bootstrapObject);
        }

        void Awake()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            Shutdown();
        }

        private void Initialize()
        {
            if (m_initialized)
                return;

            try
            {
                m_moduleManager = new PlatformModuleManager();
                m_moduleManager.LoadModules();
            }
            catch (Exception e)
            {
                UptLogger.Error($"UPT Bootstrap failed: {e.Message}");
                throw;
            }

            m_initialized = true;
        }

        private void Shutdown()
        {
            if (!m_initialized)
                return;

            try
            {
                m_moduleManager?.UnloadAllModules();
            }
            catch (Exception e)
            {
                UptLogger.Error($"Error during shutdown: {e.Message}");
            }

            m_initialized = false;
        }

        //private void FillMockServices()
        //{
        //    var loadedModules = m_moduleManager.LoadedModules;
        //    var availableServiceTypes = ModuleUtility.GetAllAvailableServiceTypes(loadedModules);

        //    foreach (var serviceType in availableServiceTypes)
        //    {
        //        if (!ServiceContainer.IsRegistered(serviceType))
        //        {
        //            var 
        //        }
        //    }
        //}

        private void Update()
        {
            if (!m_initialized)
                return;

            var servicesEnumerator = ServiceContainer.GetEnumerator();
            while (servicesEnumerator.MoveNext())
            {
                if (servicesEnumerator.Current is IUpdatableService updatable)
                    updatable.Update();
            }
        }
    }
}
