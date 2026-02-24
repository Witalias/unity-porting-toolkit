using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UPT.Core;

namespace UPT.Editor
{
    public class PlatformModulesDatabaseWindow : EditorWindow
    {
        private const float CELL_WIDTH = 250.0f;
        private const float CELL_GAP = 5.0f;
        private const float INSTALL_BUTTON_WIDTH = 100.0f;

        private Vector2 m_scrollPosition;
        private GUILoading m_loadingIndicator;

        private IReadOnlyList<PackagePair> m_packageDatabase;
        private readonly List<PackageData> m_installationQueue = new();
        private string m_currentPackageName;

        private bool m_isInstalling;
        private bool m_isGettingPackageList;

        private AddRequest m_addRequest;
        private ListRequest m_listRequest;

        [MenuItem("Tools/Porting Toolkit/Modules Database", priority = 1)]
        public static void ShowWindow()
        {
            var window = GetWindow<PlatformModulesDatabaseWindow>("Modules Database");
            window.minSize = new Vector2(650, 100);
            window.Show();
        }

        private void OnEnable()
        {
            m_loadingIndicator = new();

            LoadPackageData();
            EditorApplication.update += OnEditorUpdate;

            if (m_packageDatabase == null)
                return;

            foreach (var packagePair in m_packageDatabase)
            {
                if (string.IsNullOrEmpty(packagePair.ModulePackage.Name))
                    packagePair.ModulePackage.Status = PackageStatus.Installed;

                if (string.IsNullOrEmpty(packagePair.ExternalPackage.Name))
                    packagePair.ExternalPackage.Status = PackageStatus.Installed;
            }    

            RefreshPackageStatus();
        }

        private void OnDisable()
        {
            m_loadingIndicator = null;
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(GUIConstants.Gaps.Padding);
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical();

            DrawStatusBar();
            DrawPackageTable();

            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.Space(GUIConstants.Gaps.Padding);
            EditorGUILayout.EndHorizontal();
        }

        private void LoadPackageData()
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(PackageDatabaseData).Name}");

            if (guids.Length == 0)
            {
                UptLogger.Error($"No {typeof(PackageDatabaseData).Name} found in project!");
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var asset = AssetDatabase.LoadAssetAtPath<PackageDatabaseData>(path);
            m_packageDatabase = asset.Packages;
        }

        private void DrawStatusBar()
        {
            EditorGUILayout.Space(GUIConstants.Gaps.SectionGap);
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            if (m_isInstalling)
            {
                EditorGUILayout.LabelField("Status: Installing packages...");
            }
            else if (m_installationQueue.Count == 0)
            {
                EditorGUILayout.LabelField("Status: Ready");
            }

            if (m_installationQueue.Count > 0)
            {
                EditorGUILayout.LabelField($"Queue: {m_installationQueue.Count} remaining");
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawPackageTable()
        {
            GUILayout.Space(GUIConstants.Gaps.SectionGap);

            // Table headers
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("UPT Module", EditorStyles.boldLabel, GUILayout.Width(CELL_WIDTH));
            GUILayout.Label("External Package", EditorStyles.boldLabel, GUILayout.Width(CELL_WIDTH));
            GUILayout.Label("", EditorStyles.boldLabel, GUILayout.Width(INSTALL_BUTTON_WIDTH));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(CELL_GAP);

            if (m_isGettingPackageList)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                m_loadingIndicator?.Draw();
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                return;
            }

            m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);

            foreach (var packagePair in m_packageDatabase)
            {
                if (string.IsNullOrEmpty(packagePair.ModulePackage.Name) && string.IsNullOrEmpty(packagePair.ExternalPackage.Name))
                    continue;

                EditorGUILayout.BeginHorizontal();

                DrawPackageCell(packagePair.ModulePackage);
                DrawPackageCell(packagePair.ExternalPackage);
                DrawInstallButtonPair(packagePair);

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(CELL_GAP);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawPackageCell(PackageData package)
        {
            if (package == null || string.IsNullOrEmpty(package.Name))
            {
                GUILayout.Label("", GUILayout.Width(CELL_WIDTH));
                return;
            }

            EditorGUILayout.BeginVertical(GUILayout.Width(CELL_WIDTH));

            EditorGUILayout.LabelField(package.DisplayName, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(package.Name, GUIConstants.Styles.GrayLabel);
            if (!string.IsNullOrEmpty(package.Version))
                EditorGUILayout.LabelField($"Version: {package.Version}", GUIConstants.Styles.GrayLabel);
            DrawPackageStatus(package);

            EditorGUILayout.EndVertical();
        }

        private void DrawPackageStatus(PackageData package)
        {
            string statusText = "";
            GUIStyle style = EditorStyles.label;

            switch (package.Status)
            {
                case PackageStatus.NotInstalled:
                    statusText = "Not Installed";
                    style = GUIConstants.Styles.GrayLabel;
                    break;
                case PackageStatus.Installing:
                    statusText = "Installing...";
                    break;
                case PackageStatus.Installed:
                    statusText = "Installed";
                    style = GUIConstants.Styles.GreenLabel;
                    break;
                case PackageStatus.Failed:
                    statusText = $"Failed: {package.StatusMessage}";
                    style = GUIConstants.Styles.RedLabel;
                    break;
            }

            EditorGUILayout.LabelField(statusText, style);
        }

        private void DrawInstallButtonPair(PackagePair packagePair)
        {
            if (packagePair == null)
                return;

            EditorGUILayout.BeginVertical(GUILayout.Width(INSTALL_BUTTON_WIDTH));

            if (packagePair.ModulePackage.Status is PackageStatus.Installed && packagePair.ExternalPackage.Status is PackageStatus.Installed)
            {
                EditorGUILayout.LabelField("Installed");
            }
            else if (packagePair.ModulePackage.Status is PackageStatus.Installing || packagePair.ModulePackage.Status is PackageStatus.Installing)
            {
                EditorGUILayout.LabelField("Installing...");
                m_loadingIndicator?.Draw();
            }
            else
            {
                if (GUILayout.Button("Install", GUILayout.Height(20)))
                    InstallPackagePair(packagePair);
                    
            }

            EditorGUILayout.EndVertical();
        }

        private void InstallPackagePair(PackagePair packagePair)
        {
            if (packagePair == null)
                return;

            m_installationQueue.Clear();

            if (packagePair.ModulePackage.Status is PackageStatus.NotInstalled && !string.IsNullOrEmpty(packagePair.ModulePackage.Url))
            {
                m_installationQueue.Add(packagePair.ModulePackage);
                packagePair.ModulePackage.Status = PackageStatus.Installing;
            }

            if (packagePair.ExternalPackage.Status is PackageStatus.NotInstalled && !string.IsNullOrEmpty(packagePair.ExternalPackage.Url))
            {
                m_installationQueue.Add(packagePair.ExternalPackage);
                packagePair.ExternalPackage.Status = PackageStatus.Installing;
            }

            StartInstallation();
        }

        private void StartInstallation()
        {
            if (m_installationQueue.Count > 0 && !m_isInstalling)
            {
                var url = m_installationQueue[0].Url;
                if (string.IsNullOrEmpty(url))
                {
                    UptLogger.Error($"Package {m_installationQueue[0].Name}: Url is null");
                    return;
                }    

                m_currentPackageName = m_installationQueue[0].Name;

                UptLogger.Info($"Starting installation: {m_currentPackageName}");

                m_addRequest = Client.Add(url);
                m_isInstalling = true;

                Repaint();
            }
        }

        private void OnEditorUpdate()
        {
            if (m_isInstalling && m_addRequest != null && m_addRequest.IsCompleted)
                HandleInstallationResult();

            if (m_isGettingPackageList && m_listRequest != null && m_listRequest.IsCompleted)
                HandlePackageListResult();
        }

        private void HandleInstallationResult()
        {
            var package = m_packageDatabase
                .SelectMany(packagePair => new PackageData[] { packagePair.ModulePackage, packagePair.ExternalPackage })
                .FirstOrDefault(p => p.Name == m_currentPackageName);

            if (m_addRequest.Status == StatusCode.Success)
            {
                UptLogger.Info($"Successfully installed: {m_currentPackageName}");

                if (package != null)
                {
                    package.Status = PackageStatus.Installed;
                    package.StatusMessage = "Success";
                }

                ShowNotification(new GUIContent($"{m_currentPackageName} installed successfully!"), 2);
            }
            else
            {
                UptLogger.Error($"Failed to install {m_currentPackageName}: {m_addRequest.Error.message}");

                if (package != null)
                {
                    package.Status = PackageStatus.Failed;
                    package.StatusMessage = m_addRequest.Error.message;
                }

                ShowNotification(new GUIContent($"Failed to install {m_currentPackageName}"), 2);
            }

            if (m_installationQueue.Count > 0)
                m_installationQueue.RemoveAt(0);

            m_addRequest = null;
            m_currentPackageName = null;

            if (m_installationQueue.Count > 0)
            {
                StartInstallation();
            }
            else
            {
                m_isInstalling = false;
                RefreshPackageStatus();
            }

            Repaint();
        }

        private void HandlePackageListResult()
        {
            if (m_listRequest.Status == StatusCode.Success)
            {
                foreach (var installedPackage in m_listRequest.Result)
                {
                    var package = m_packageDatabase
                        .SelectMany(packagePair => new PackageData[] { packagePair.ModulePackage, packagePair.ExternalPackage })
                        .FirstOrDefault(p => p.Name == installedPackage.name);

                    if (package != null)
                    {
                        package.Status = PackageStatus.Installed;
                        package.Version = installedPackage.version;
                    }
                }
            }
            else
            {
                UptLogger.Error($"Failed to get package list");
            }

            m_isGettingPackageList = false;
        }

        private void RefreshPackageStatus()
        {
            m_listRequest = Client.List();
            m_isGettingPackageList = true;
        }
    }
}
