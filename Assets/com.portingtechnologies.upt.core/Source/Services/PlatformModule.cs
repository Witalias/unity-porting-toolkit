using System;
using System.Collections.Generic;

namespace UPT.Core
{
    public abstract class PlatformModule : IPlatformModule
    {
        private readonly Dictionary<Type, object> m_services = new();
        private readonly Dictionary<Type, Func<object>> m_serviceFactories = new();

        public abstract string DisplayName { get; }
        public abstract string Version { get; }
        public abstract IReadOnlyCollection<Type> ProvidedServiceTypes { get; }

        public virtual bool Initialize()
        {
            RegisterServiceFactories();
            CreateServices();

            if (m_services.Count > 0)
            {
                UptLogger.Info($"{DisplayName} module initialized with {m_services.Count} services", this);
                return true;
            }
            return false;
        }

        public virtual void Shutdown()
        {
            foreach (var serviceType in m_services.Keys)
                ServiceContainer.Unregister(serviceType);

            m_services.Clear();
            UptLogger.Info($"{DisplayName} module shutdown completed", this);
        }

        public abstract bool IsAvailable();

        public T GetService<T>() where T : class
        {
            var serviceType = typeof(T);
            if (m_services.TryGetValue(serviceType, out var service))
                return service as T;

            UptLogger.Warning($"Service not found: {serviceType.Name}", this);
            return null;
        }

        public bool IsServiceSupported<T>() where T : class
        {
            return m_services.ContainsKey(typeof(T));
        }

        protected void RegisterServiceFactory<T>(Func<T> factory) where T : class
        {
            m_serviceFactories[typeof(T)] = () => factory();
        }

        protected void UnregisterService<T>() where T : class
        {
            ServiceContainer.Unregister<T>();
            var serviceType = typeof(T);
            if (m_services.Remove(serviceType))
                UptLogger.Info($"Service unregistered: {serviceType.Name}", this);
        }

        protected abstract void RegisterServiceFactories();

        private void CreateServices()
        {
            foreach (var factory in m_serviceFactories)
            {
                var serviceType = factory.Key;
                var createService = factory.Value;

                if (createService == null)
                    continue;

                if (ShouldCreateService(serviceType))
                {
                    var service = createService();
                    if (service != null)
                    {
                        if (ServiceContainer.Register(serviceType, service))
                        {
                            m_services.Add(serviceType, service);
                            UptLogger.Info($"Service registered: {serviceType.Name}", this);
                        }
                        else
                        {
                            UptLogger.Warning($"Service was already registered: {serviceType.Name}", this);
                        }
                    }
                }
            }
        }

        private bool ShouldCreateService(Type serviceType)
        {
            if (serviceType == null || m_services.ContainsKey(serviceType))
                return false;

            var serviceCollection = ResourceManager.LoadCurrentServiceCollection();
            if (serviceCollection == null)
            {
                UptLogger.Error($"Failed to load active service collection. Check Platform Module Manager for details");
                return false;
            }

            var serviceConfig = serviceCollection.GetServiceConfig(serviceType);
            if (serviceConfig == null)
            {
                UptLogger.Error($"Service config of type {serviceType.Name} wasn't found. " +
                    $"Perhaps you haven't added the service type to the ProvidedServiceTypes of the platform? ");
                return false;
            }

            return serviceConfig.PreferredPlatform == DisplayName;
        }
    }
}
