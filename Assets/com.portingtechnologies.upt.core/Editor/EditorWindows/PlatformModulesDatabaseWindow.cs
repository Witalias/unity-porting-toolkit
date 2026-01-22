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

        private Vector2 m_scrollPosition;
        private IReadOnlyList<PackageData> m_packageDatabase;

        private readonly List<PackageData> installationQueue = new();
        private AddRequest addRequest;
        private string currentPackageName;
        private bool isInstalling;

        private ListRequest listRequest;
        private bool isGettingPackageList;

        private GUIStyle m_titleStyle;
        private GUIStyle m_grayStyle;
        private GUIStyle m_greenStyle;
        private GUIStyle m_redStyle;

        [MenuItem("Tools/Porting Toolkit/Modules Database")]
        public static void ShowWindow()
        {
            GetWindow<PlatformModulesDatabaseWindow>("Modules Database");
        }

        private void OnEnable()
        {
            InitializeStyles();
            LoadPackageData();
            EditorApplication.update += OnEditorUpdate;

            if (m_packageDatabase == null)
                return;

            foreach (var package in m_packageDatabase)
                package.Status = PackageStatus.NotInstalled;

            RefreshPackageStatus();
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(Constants.EditorGaps.Padding);

            m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);

            DrawPackageTable();
            DrawStatusBar();

            EditorGUILayout.EndScrollView();

            GUILayout.Space(Constants.EditorGaps.Padding);
            EditorGUILayout.EndHorizontal();
        }

        private void InitializeStyles()
        {
            m_titleStyle = new();
            m_titleStyle.fontSize = 16;
            m_titleStyle.fontStyle = FontStyle.Bold;

            m_grayStyle = new(EditorStyles.label);
            m_grayStyle.normal.textColor = Color.gray;

            m_greenStyle = new(EditorStyles.label);
            m_greenStyle.normal.textColor = Color.darkGreen;

            m_redStyle = new(EditorStyles.label);
            m_redStyle.normal.textColor = Color.darkRed;
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
            EditorGUILayout.Space(20);
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            if (isInstalling)
            {
                EditorGUILayout.LabelField("Status: Installing packages...");
            }
            else if (installationQueue.Count == 0)
            {
                EditorGUILayout.LabelField("Status: Ready");
            }

            if (installationQueue.Count > 0)
            {
                EditorGUILayout.LabelField($"Queue: {installationQueue.Count} remaining");
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawPackageTable()
        {
            // Table headers
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("UPT Module", EditorStyles.boldLabel, GUILayout.Width(CELL_WIDTH));
            GUILayout.Label("External Package", EditorStyles.boldLabel, GUILayout.Width(CELL_WIDTH));
            GUILayout.Label("", EditorStyles.boldLabel, GUILayout.Width(100.0f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            for (int i = 0; i < m_packageDatabase.Count; i += 2)
            {
                EditorGUILayout.BeginHorizontal();

                DrawPackageColumn(i);

                if (i + 1 < m_packageDatabase.Count)
                    DrawPackageColumn(i + 1);
                else
                    GUILayout.Label("", GUILayout.Width(CELL_WIDTH));

                DrawInstallButtonPair(i);

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(5);
            }
        }

        private void DrawPackageColumn(int index)
        {
            if (index >= m_packageDatabase.Count)
                return;

            var package = m_packageDatabase[index];

            EditorGUILayout.BeginVertical(GUILayout.Width(CELL_WIDTH));

            EditorGUILayout.LabelField(package.DisplayName, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(package.Name, m_grayStyle);
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
                    style = m_grayStyle;
                    break;
                case PackageStatus.Installing:
                    statusText = "Installing...";
                    break;
                case PackageStatus.Installed:
                    statusText = "Installed";
                    style = m_greenStyle;
                    break;
                case PackageStatus.Failed:
                    statusText = $"Failed: {package.StatusMessage}";
                    style = m_redStyle;
                    break;
            }

            EditorGUILayout.LabelField(statusText, style);
        }

        private void DrawInstallButtonPair(int firstIndex)
        {
            if (firstIndex + 1 >= m_packageDatabase.Count)
                return;

            EditorGUILayout.BeginVertical(GUILayout.Width(100.0f));

            var package1 = m_packageDatabase[firstIndex];
            var package2 = m_packageDatabase[firstIndex + 1];

            var isThisPairInstalling = false;
            if (isInstalling && currentPackageName != null)
            {
                isThisPairInstalling =  currentPackageName == package1.Name ||
                                        currentPackageName == package2.Name;
            }

            if (package1.Status is PackageStatus.Installed && package2.Status is PackageStatus.Installed)
            {
                EditorGUILayout.LabelField("Installed");
            }
            else if (package1.Status is PackageStatus.Installing || package2.Status is PackageStatus.Installing)
            {
                EditorGUILayout.LabelField("Installing...");
                GUILayoutRunningBar.Draw();
            }
            else
            {
                if (GUILayout.Button("Install", GUILayout.Height(20)))
                    InstallPackagePair(firstIndex);
                    
            }

            EditorGUILayout.EndVertical();
        }

        private void InstallPackagePair(int firstIndex)
        {
            if (firstIndex + 1 >= m_packageDatabase.Count)
                return;

            installationQueue.Clear();

            if (m_packageDatabase[firstIndex].Status is PackageStatus.NotInstalled)
            {
                installationQueue.Add(m_packageDatabase[firstIndex]);
                m_packageDatabase[firstIndex].Status = PackageStatus.Installing;
            }

            if (m_packageDatabase[firstIndex + 1].Status is PackageStatus.NotInstalled)
            {
                installationQueue.Add(m_packageDatabase[firstIndex + 1]);
                m_packageDatabase[firstIndex + 1].Status = PackageStatus.Installing;
            }

            StartInstallation();
        }

        private void StartInstallation()
        {
            if (installationQueue.Count > 0 && !isInstalling)
            {
                var url = installationQueue[0].Url;
                if (string.IsNullOrEmpty(url))
                {
                    UptLogger.Error($"Package {installationQueue[0].Name}: Url is null");
                    return;
                }    

                currentPackageName = installationQueue[0].Name;

                UptLogger.Info($"Starting installation: {currentPackageName}");

                addRequest = Client.Add(url);
                isInstalling = true;

                Repaint();
            }
        }

        private void OnEditorUpdate()
        {
            if (isInstalling && addRequest != null && addRequest.IsCompleted)
                HandleInstallationResult();

            if (isGettingPackageList && listRequest != null && listRequest.IsCompleted)
                HandlePackageListResult();
        }

        private void HandleInstallationResult()
        {
            PackageData package = m_packageDatabase.FirstOrDefault(p => p.Name == currentPackageName);

            if (addRequest.Status == StatusCode.Success)
            {
                UptLogger.Info($"Successfully installed: {currentPackageName}");

                if (package != null)
                {
                    package.Status = PackageStatus.Installed;
                    package.StatusMessage = "Success";
                }

                ShowNotification(new GUIContent($"{currentPackageName} installed successfully!"), 2);
            }
            else
            {
                UptLogger.Error($"Failed to install {currentPackageName}: {addRequest.Error.message}");

                if (package != null)
                {
                    package.Status = PackageStatus.Failed;
                    package.StatusMessage = addRequest.Error.message;
                }

                ShowNotification(new GUIContent($"Failed to install {currentPackageName}"), 2);
            }

            if (installationQueue.Count > 0)
                installationQueue.RemoveAt(0);

            addRequest = null;
            currentPackageName = null;

            if (installationQueue.Count > 0)
            {
                StartInstallation();
            }
            else
            {
                isInstalling = false;
                RefreshPackageStatus();
            }

            Repaint();
        }

        private void HandlePackageListResult()
        {
            if (listRequest.Status == StatusCode.Success)
            {
                foreach (var installedPackage in listRequest.Result)
                {
                    var package = m_packageDatabase.FirstOrDefault(p => p.Name == installedPackage.name);
                    if (package != null)
                        package.Status = PackageStatus.Installed;
                }
            }
            else
            {
                UptLogger.Error($"Failed to get package list");
            }
        }

        private void RefreshPackageStatus()
        {
            listRequest = Client.List();
            isGettingPackageList = true;
        }
    }
}
