using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UPT.Core
{
    public static class ResourceManager
    {
        public static List<PlatformServiceCollection> LoadServiceCollections()
        {
            return Resources.LoadAll<PlatformServiceCollection>($"{Constants.ResourcesFolderRoot}/{Constants.ServiceCollectionsResourceFolderRoot}").ToList();
        }

        public static PlatformServiceCollection LoadCurrentServiceCollection()
        {
            var collections = LoadServiceCollections();
            return collections.FirstOrDefault(c => c.IsActive);
        }

        public static string LoadFile(string filePath)
        {
            var fullPath = Path.Combine(Application.dataPath, $"Resources/{Constants.ResourcesFolderRoot}", filePath);

            if (!File.Exists(fullPath))
                return null;

            return File.ReadAllText(fullPath);
        }

        public static void CreateResourceAsset(Object asset, string filePath)
        {
#if UNITY_EDITOR
            var fullPath = Path.Combine(GetResourceFrameworkRoot(), filePath);
            CreateDirectoriesForPath(fullPath);

            AssetDatabase.CreateAsset(asset, fullPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif
        }

        public static void CreateJsonFile(string filePath, string jsonContent)
        {
            GetResourceFrameworkRoot();

            var fullPath = Path.Combine(Application.dataPath, $"Resources/{Constants.ResourcesFolderRoot}", filePath);
            var directory = Path.GetDirectoryName(fullPath);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(fullPath, jsonContent);

#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }

        private static string GetResourceFrameworkRoot()
        {
#if UNITY_EDITOR
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");

            if (!AssetDatabase.IsValidFolder($"Assets/Resources/{Constants.ResourcesFolderRoot}"))
                AssetDatabase.CreateFolder("Assets/Resources", Constants.ResourcesFolderRoot);

            return $"Assets/Resources/{Constants.ResourcesFolderRoot}";
#endif
        }

        private static void CreateDirectoriesForPath(string filePath)
        {
#if UNITY_EDITOR
            var directoryPath = Path.GetDirectoryName(filePath);

            if (AssetDatabase.IsValidFolder(directoryPath))
                return;

            var folders = directoryPath.Split('/');
            var currentPath = string.Empty;

            for (int i = 0; i < folders.Length; i++)
            {
                if (string.IsNullOrEmpty(folders[i]))
                    continue;

                var newPath = currentPath + (currentPath.Length > 0 ? "/" : "") + folders[i];

                if (!AssetDatabase.IsValidFolder(newPath))
                {
                    var parentPath = i > 0 ? string.Join("/", folders, 0, i) : "";
                    AssetDatabase.CreateFolder(parentPath, folders[i]);
                }

                currentPath = newPath;
            }
#endif
        }
    }
}
