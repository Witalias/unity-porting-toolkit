using System;
using UnityEngine;

namespace UPT.Core
{
    public static class UptLogger
    {
        public static void Info(string message, IPlatformModule module = null)
        {
            Debug.Log($"{GetPrefix(module)} {message}");
        }

        public static void Warning(string message, IPlatformModule module = null)
        {
            Debug.LogWarning($"{GetPrefix(module)} {message}");
        }

        public static void Error(string message, IPlatformModule module = null)
        {
            Debug.LogError($"{GetPrefix(module)} {message}");
        }

        public static Exception Exception(string message, IPlatformModule module = null)
        {
            return new Exception($"{GetPrefix(module)} {message}");
        }

        private static string GetPrefix(IPlatformModule module)
        {
            if (module != null)
                return $"[UPT {module.DisplayName}]";

            return "[UPT Core]";
        }
    }
}
