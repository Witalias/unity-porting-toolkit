using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UPT.Core;

namespace UPT.Editor
{
    public class PackageDependencyManager
    {
        private static ListRequest m_listRequest;

        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            m_listRequest = Client.List();
            EditorApplication.update += CheckForPackageList;
        }

        private static void CheckForPackageList()
        {
            if (m_listRequest.IsCompleted)
            {
                EditorApplication.update -= CheckForPackageList;

                if (m_listRequest.Status == StatusCode.Success)
                {
                    var platformServicePackages = m_listRequest.Result
                        .Where(p => p.name.StartsWith(Constants.PackageNamePrefix))
                        .ToList();

                    ValidateDependencies(platformServicePackages);
                }
            }
        }

        private static void ValidateDependencies(IList<UnityEditor.PackageManager.PackageInfo> packages)
        {
            // Check if the core is installed.
            var hasCoreDependency = packages.Any(p => p.name == $"{Constants.PackageNamePrefix}core");

            foreach (var package in packages)
            {
                // If it is a module (not a core)
                if (package.name != $"{Constants.PackageNamePrefix}.core")
                {
                    if (!hasCoreDependency)
                        Debug.LogError($"Platform module {package.name} requires Core package!");
                }
            }
        }
    }
}
