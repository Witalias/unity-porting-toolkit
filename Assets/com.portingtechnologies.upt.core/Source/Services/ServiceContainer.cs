using System;
using System.Collections;
using System.Collections.Generic;

namespace UPT.Core
{
    public static class ServiceContainer
    {
        private static readonly Dictionary<Type, object> m_services = new();

        public static bool Register<T>() where T : class
        {
            if (!m_services.TryAdd(typeof(T), Activator.CreateInstance<T>()))
            {
                UptLogger.Warning($"The service '{typeof(T).Name}' has been skipped, which is already registered.");
                return false;
            }
            return true;
        }

        public static bool Register<T>(object service) where T : class
        {
            if (!m_services.TryAdd(typeof(T), service))
            {
                UptLogger.Warning($"The service '{typeof(T).Name}' has been skipped, which is already registered.");
                return false;
            }
            return true;
        }

        public static bool Register(Type type, object service)
        {
            if (!m_services.TryAdd(type, service))
            {
                UptLogger.Warning($"The service '{type.Name}' has been skipped, which is already registered.");
                return false;
            }
            return true;
        }

        public static void Unregister<T>() where T : class
        {
            if (m_services.ContainsKey(typeof(T)))
                m_services.Remove(typeof(T));
        }

        public static void Unregister(Type type)
        {
            if (m_services.ContainsKey(type))
                m_services.Remove(type);
        }

        public static T Get<T>() where T : class
        {
            if (m_services.TryGetValue(typeof(T), out var service))
                return service as T;

            return null;
        }

        public static IEnumerator GetEnumerator() => m_services.Values.GetEnumerator();

        public static bool IsRegistered<T>() => m_services.ContainsKey(typeof(T));

        public static bool IsRegistered(Type type) => m_services.ContainsKey(type);
    }
}
