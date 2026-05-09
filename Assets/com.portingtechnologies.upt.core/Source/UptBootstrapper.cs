using System;
using UnityEngine;

namespace UPT.Core
{
    public class UptBootstrapper : MonoBehaviour
    {
#if !DISABLE_UPT
        private PlatformModuleManager m_moduleManager;
        private bool m_initialized;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeOnLoad()
        {
            var bootstrapObject = new GameObject("[UPT Bootstrapper]");
            bootstrapObject.AddComponent<UptBootstrapper>();
            DontDestroyOnLoad(bootstrapObject);
        }

        private void Awake()
        {
            Initialize();
        }

        private void Start()
        {
            PostInitialize();
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

        private void PostInitialize()
        {
            m_moduleManager?.PostInitialize();
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
#endif
    }
}
