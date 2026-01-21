using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UPT.Core
{
    public static class MockServiceFactory
    {
        private static readonly Dictionary<Type, Type> m_mockTypeCache = new();

        public static object Create(Type serviceType)
        {
            if (serviceType == null || !serviceType.IsInterface)
            {
                UptLogger.Error($"Cannot create mock for null or non-interface type");
                return null;
            }

            var mockType = FindMockType(serviceType);
            if (mockType != null)
            {
                try
                {
                    var instance = Activator.CreateInstance(mockType);
                    UptLogger.Info($"Created existing mock: {mockType.Name} for {serviceType.Name}");
                    return instance;
                }
                catch (Exception e)
                {
                    UptLogger.Warning($"Failed to create existing mock for {serviceType.Name}: {e.Message}");
                }
            }

            return null;
        }

        private static Type FindMockType(Type serviceType)
        {
            if (m_mockTypeCache.TryGetValue(serviceType, out var cachedType))
                return cachedType;

            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in allAssemblies)
            {
                if (ModuleUtility.IsSystemAssembly(assembly))
                    continue;

                try
                {
                    var mockTypes = assembly.GetTypes()
                        .Where(t => !t.IsAbstract && !t.IsInterface)
                        .Where(t => t.GetCustomAttribute<MockServiceAttribute>() != null)
                        .Where(t => serviceType.IsAssignableFrom(t) ||
                                   (t.BaseType?.IsGenericType == true &&
                                    t.BaseType.GetGenericArguments().FirstOrDefault() == serviceType));

                    var mockType = mockTypes.FirstOrDefault();
                    if (mockType != null)
                    {
                        m_mockTypeCache[serviceType] = mockType;
                        return mockType;
                    }
                }
                catch (Exception e)
                {
                    UptLogger.Warning($"Failed to scan assembly {assembly.GetName().Name} for mock types: {e.Message}");
                }
            }

            return null;
        }
    }
}
