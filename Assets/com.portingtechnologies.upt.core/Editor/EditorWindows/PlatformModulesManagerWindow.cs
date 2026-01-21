#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UPT.Core;

namespace UPT.Editor
{
    public class PlatformModulesManagerWindow : EditorWindow
    {
        private const string NONE_OPTION = "None";
        private const string RENAME_COLLECTION_FOCUS = "RenameCollection";

        private PlatformServiceCollection m_currentServiceCollection;
        private PlatformServiceCollection m_editingCollection;
        private List<PlatformServiceCollection> m_serviceCollections = new();
        private List<IPlatformModule> m_availableModules = new();

        private Vector2 m_scrollPosition;
        private bool m_showModules;
        private bool m_showServices;
        private bool m_showCollections;
        private bool m_updatePreprocessorDefinitions;
        private string m_GUIFocusControl;
        private string m_collectionNameBuffer;

        private GUIStyle m_titleStyle;

        private Texture2D m_trashIcon;
        private Texture2D m_refreshIcon;
        private Texture2D m_plusIcon;
        private Texture2D m_infoIcon;
        private Texture2D m_saveIcon;
        private Texture2D m_toolsIcon;
        private Texture2D m_settingsIcon;
        private Texture2D m_editIcon;

        [MenuItem("Tools/Porting Toolkit/Modules Manager", priority = 1)]
        public static void ShowWindow()
        {
            GetWindow<PlatformModulesManagerWindow>("Modules Manager");
        }

        void OnEnable()
        {
            InitializeStyles();
            InitializeIcons();
            RefreshAvailableModules();
            LoadCollections();
        }

        private void OnDisable()
        {
            UpdatePreprocessorDefinitions();
        }

        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(Constants.EditorGaps.Padding);

            m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);

            DrawHeader();
            DrawCollectionsSection();
            DrawModulesSection();
            DrawServicesSection();
            DrawFooter();

            EditorGUILayout.EndScrollView();

            GUILayout.Space(Constants.EditorGaps.Padding);
            EditorGUILayout.EndHorizontal();
        }

        private void InitializeStyles()
        {
            m_titleStyle = new();
            m_titleStyle.fontSize = 16;
            m_titleStyle.fontStyle = FontStyle.Bold;
        }

        private void InitializeIcons()
        {
            m_trashIcon = RetrieveIcon(Constants.Icons.Trash);
            m_refreshIcon = RetrieveIcon(Constants.Icons.Refresh);
            m_plusIcon = RetrieveIcon(Constants.Icons.Plus);
            m_infoIcon = RetrieveIcon(Constants.Icons.Info);
            m_saveIcon = RetrieveIcon(Constants.Icons.Save);
            m_toolsIcon = RetrieveIcon(Constants.Icons.Tools);
            m_settingsIcon = RetrieveIcon(Constants.Icons.Settings);
            m_editIcon = RetrieveIcon(Constants.Icons.Edit);
        }

        private Texture2D RetrieveIcon(string name)
        {
            return (Texture2D)EditorGUIUtility.IconContent(name).image;
        }

        private void RefreshAvailableModules()
        {
            m_availableModules = ModuleDiscovery.FindAvailableModules();
        }

        private void LoadCollections()
        {
            m_serviceCollections = ResourceManager.LoadServiceCollections();
            m_currentServiceCollection = ResourceManager.LoadCurrentServiceCollection();

            if (m_serviceCollections.Count == 0)
                CreateDefaultCollection();

            if (m_currentServiceCollection == null)
            {
                m_currentServiceCollection = m_serviceCollections[0];
                m_currentServiceCollection.IsActive = true;
            }
        }

        private void DrawHeader() 
        {
            GUILayout.Space(Constants.EditorGaps.SectionGap);

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label($"Active Preset:", EditorStyles.boldLabel);

            // Options popup
            var currentIndex = m_serviceCollections.IndexOf(m_currentServiceCollection);
            var options = m_serviceCollections.Select(c => c.Name).ToArray();
            var newIndex = EditorGUILayout.Popup(currentIndex, options);
            var newCollection = options[newIndex];

            if (newCollection != m_currentServiceCollection.Name)
            {
                m_currentServiceCollection = m_serviceCollections.Find(c => c.Name == newCollection);
                SwitchCollection(m_currentServiceCollection);
            }

            EditorGUILayout.EndHorizontal();

            if (m_updatePreprocessorDefinitions)
            {
                GUILayout.Space(Constants.EditorGaps.Padding);

                EditorGUILayout.HelpBox(
                    "The current configuration preset requires changing the Scripting Definition Symbols and reloading domain. " +
                    "This will be done automatically when the window is closed, or you can do it yourself by clicking the button below.",
                    MessageType.Warning);

                if (GUILayout.Button("Apply Scripting Definition Symbols"))
                {
                    m_updatePreprocessorDefinitions = false;
                    UpdatePreprocessorDefinitions();
                }
            }

            GUILayout.Space(Constants.EditorGaps.SectionGap);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Space(Constants.EditorGaps.SectionGap);
        }

        private void DrawModulesSection()
        {
            m_showModules = EditorGUILayout.BeginFoldoutHeaderGroup(m_showModules, new GUIContent("Platform Modules", m_toolsIcon));

            if (m_showModules)
            {
                EditorGUILayout.HelpBox(
                    "Active modules are determined by service configuration. " +
                    "A module is active if at least one service uses it as backend. " +
                    "The system automatically detects the installed modules. " +
                    "To find out how to install a particular module, refer to the documentation.",
                    MessageType.Info);

                EditorGUILayout.BeginVertical("Box");

                if (m_availableModules.Count > 0)
                {
                    foreach (var module in m_availableModules)
                        DrawModuleItem(module);
                }
                else
                {
                    EditorGUILayout.LabelField("No platform modules found", EditorStyles.centeredGreyMiniLabel);
                }

                GUILayout.Space(Constants.EditorGaps.Padding);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            GUILayout.Space(Constants.EditorGaps.SectionGap);
        }

        private void DrawModuleItem(IPlatformModule module)
        {
            var isActive = IsModuleActive(module, m_currentServiceCollection);
            var servicesCount = module.ProvidedServiceTypes.Count;

            //EditorGUILayout.BeginVertical("Box");

            GUILayout.Space(Constants.EditorGaps.Padding);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(Constants.EditorGaps.Padding);

            //var statusIcon = isActive ? "✅" : "❌";
            GUILayout.Label($"{module.DisplayName}", EditorStyles.boldLabel, GUILayout.Width(200));

            GUILayout.FlexibleSpace();
            
            GUILayout.Label($"Provided services: {servicesCount}", EditorStyles.label, GUILayout.Width(160));

            var statusText = isActive ? "✓ Using" : "✕ Not using";
            GUILayout.Label(statusText, GUILayout.Width(120));

            if (GUILayout.Button(new GUIContent(m_infoIcon, "Info"), GUILayout.Width(30)))
                ShowModuleInfo(module);

            GUILayout.Space(Constants.EditorGaps.Padding);
            EditorGUILayout.EndHorizontal();

            //EditorGUILayout.EndVertical();
        }

        private void DrawServicesSection()
        {
            m_showServices = EditorGUILayout.BeginFoldoutHeaderGroup(m_showServices, new GUIContent("Services Configuration", m_settingsIcon));

            if (m_showServices)
            {
                EditorGUILayout.HelpBox(
                    "Select the desired platform backend for each service. " +
                    "If the backend is not selected, a backend stub will be used for the service, so you can safely use it in your code.",
                    MessageType.Info);

                EditorGUILayout.BeginVertical("Box");

                var availableServiceTypes = ModuleUtility.GetAllAvailableServiceTypes(m_availableModules);

                foreach (var serviceType in availableServiceTypes)
                    DrawServiceConfiguration(serviceType);

                if (availableServiceTypes.Count == 0)
                {
                    EditorGUILayout.LabelField("No services available in loaded modules",
                        EditorStyles.centeredGreyMiniLabel);
                }

                GUILayout.Space(Constants.EditorGaps.Padding);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            GUILayout.Space(Constants.EditorGaps.SectionGap);
        }

        private void DrawServiceConfiguration(Type serviceType)
        {
            var serviceConfig = m_currentServiceCollection.GetServiceConfig(serviceType);
            serviceConfig ??= m_currentServiceCollection.CreateConfig(serviceType);

            var serviceName = serviceConfig.ServiceType.ToString();
            var currentModule = serviceConfig.PreferredPlatform;
            var hasBackend = !string.IsNullOrEmpty(currentModule) && currentModule != NONE_OPTION;

            var availableModules = m_availableModules
                .Where(module => module.ProvidedServiceTypes.Contains(serviceType))
                .Select(module => module.DisplayName)
                .ToList();

            GUILayout.Space(Constants.EditorGaps.Padding);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(Constants.EditorGaps.Padding);

            GUILayout.Label(serviceName, EditorStyles.boldLabel, GUILayout.Width(150));
            GUILayout.FlexibleSpace();

            if (hasBackend)
            {
                var statusColor = availableModules.Contains(currentModule) ? Color.green : Color.yellow;
                var statusText = availableModules.Contains(currentModule) ?
                $"✓ Compatible" : $"⚠️ {currentModule} isn't compatible";

                var originalColor = GUI.color;
                GUI.color = statusColor;
                GUILayout.Label(statusText);
                GUI.color = originalColor;
            }
            else
            {
                GUILayout.Label("✕ No backend selected");
            }

            GUILayout.Space(60);

            // Choose module popup
            var popupOptions = new List<string> { "None" };
            popupOptions.AddRange(availableModules);

            var currentIndex = popupOptions.IndexOf(currentModule ?? NONE_OPTION);
            var newIndex = EditorGUILayout.Popup(currentIndex, popupOptions.ToArray(), GUILayout.Width(200));

            var newModule = popupOptions[newIndex];

            if (newModule != currentModule)
            {
                m_currentServiceCollection.SetServicePlatform(serviceType, newModule);
                UpdatePreprocessorDefinitionsStatusForUI();
            }

            GUILayout.Space(Constants.EditorGaps.Padding);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCollectionsSection()
        {
            //EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            //GUILayout.Space(SECTION_GAP);

            m_showCollections = EditorGUILayout.BeginFoldoutHeaderGroup(m_showCollections, new GUIContent("Configuration Presets", m_saveIcon));

            if (m_showCollections)
            {
                EditorGUILayout.HelpBox(
                    "You can switch between different configuration presets. " +
                    "Please note that you create configuration presets separately for each build platform.",
                    MessageType.Info);

                EditorGUILayout.BeginVertical("Box");

                var totalCollections = m_serviceCollections.Count;
                for (var i = 0; i < totalCollections; i++)
                {
                    // The collection could have been deleted during the iteration
                    if (m_serviceCollections.Count != totalCollections)
                    {
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndFoldoutHeaderGroup();
                        return;
                    }

                    DrawCollectionItem(m_serviceCollections[i]);
                }

                GUILayout.Space(Constants.EditorGaps.Padding);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(Constants.EditorGaps.Padding);

                if (GUILayout.Button(new GUIContent(m_plusIcon, "Create new collection"), GUILayout.Width(Constants.EditorGaps.IconButtonWidth)))
                    CreateNewCollection();

                if (GUILayout.Button(new GUIContent(m_refreshIcon, "Refresh collections"), GUILayout.Width(Constants.EditorGaps.IconButtonWidth)))
                {
                    LoadCollections();
                    Repaint();
                }

                EditorGUILayout.EndHorizontal();

                GUILayout.Space(Constants.EditorGaps.Padding);
                EditorGUILayout.EndVertical();
            }
            else
            {
                m_editingCollection = null;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            GUILayout.Space(Constants.EditorGaps.SectionGap);
        }

        private void DrawCollectionItem(PlatformServiceCollection collection)
        {
            GUILayout.Space(Constants.EditorGaps.Padding);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(Constants.EditorGaps.Padding);

            var nameWidth = 200.0f;

            if (collection == m_editingCollection)
            {
                GUI.SetNextControlName(RENAME_COLLECTION_FOCUS);

                m_collectionNameBuffer = EditorGUILayout.TextArea(m_collectionNameBuffer, GUILayout.Width(nameWidth));

                if (m_GUIFocusControl == RENAME_COLLECTION_FOCUS)
                {
                    GUI.FocusControl(RENAME_COLLECTION_FOCUS);
                    m_GUIFocusControl = string.Empty;
                }

                EditorGUILayout.Space();
                if (GUILayout.Button("Apply", GUILayout.Width(60.0f)))
                {
                    collection.Name = m_collectionNameBuffer;
                    m_editingCollection = null;
                    EditorUtility.SetDirty(collection);
                    AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(collection), collection.Name);
                    AssetDatabase.SaveAssets();
                }
            }
            else
            {
                var nameStyle = collection.IsActive ? EditorStyles.boldLabel : EditorStyles.label;
                GUILayout.Label(collection.Name, nameStyle, GUILayout.Width(nameWidth));
            }

            GUILayout.FlexibleSpace();

            if (collection.IsActive)
                GUILayout.Label("✓ Active");

            var activeModulesCount = m_availableModules
                .Where(module => IsModuleActive(module, collection))
                .Count();

            var moduleNames = m_availableModules
                .Select(module => module.DisplayName);

            var activeServicesCount = collection
                .GetServiceConfigs()
                .Where(config => moduleNames.Contains(config.PreferredPlatform))
                .Count();

            var gap = 20.0f;
            GUILayout.Space(gap);
            GUILayout.Label(new GUIContent(activeModulesCount.ToString(), m_toolsIcon, "Number of modules used"));
            GUILayout.Space(gap);
            GUILayout.Label(new GUIContent(activeServicesCount.ToString(), m_settingsIcon, "Number of services used"));
            GUILayout.Space(gap);

            if (collection == m_editingCollection)
            {
                if (GUILayout.Button("✕", GUILayout.Width(Constants.EditorGaps.IconButtonWidth)))
                    m_editingCollection = null;
            }
            else
            {
                if (GUILayout.Button(new GUIContent(m_editIcon, "Rename collection"), GUILayout.Width(Constants.EditorGaps.IconButtonWidth)))
                    SwitchEditModeCollection(collection);
            }

            if (GUILayout.Button(new GUIContent(m_trashIcon, "Delete collection"), GUILayout.Width(Constants.EditorGaps.IconButtonWidth)))
                DeleteCollection(collection);

            GUILayout.Space(Constants.EditorGaps.Padding);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);
        }

        private void DrawFooter()
        {
            //EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            //GUILayout.Space(SECTION_GAP);
        }

        /// <summary>
        /// The module is active if at least one service uses it as a backend.
        /// </summary>
        private bool IsModuleActive(IPlatformModule module, PlatformServiceCollection forCollection)
        {
            var availableServiceTypes = ModuleUtility.GetAllAvailableServiceTypes(m_availableModules);

            foreach (var serviceType in availableServiceTypes)
            {
                var service = forCollection.GetServiceConfig(serviceType);
                var currentPlatform = service?.PreferredPlatform ?? NONE_OPTION;
                if (currentPlatform == module.DisplayName)
                    return true;
            }

            return false;
        }

        private void ShowModuleInfo(IPlatformModule module)
        {
            var moduleType = module.GetType();
            var attribute = moduleType.GetCustomAttribute<PlatformModuleAttribute>();

            var info = $"Module: {module.DisplayName}\n" +
                      $"Version: {module.Version}\n" +
                      $"Package: {attribute.PackageName}\n" +
                      $"Active: {IsModuleActive(module, m_currentServiceCollection)}\n" +
                      $"Services: {module.ProvidedServiceTypes.Count}";

            EditorUtility.DisplayDialog("Module Information", info, "OK");
        }

        private void CreateDefaultCollection()
        {
            var collection = CreateInstance<PlatformServiceCollection>();
            collection.Name = "ConfigurationPreset1";
            collection.IsActive = true;

            ResourceManager.CreateResourceAsset(collection, $"{Constants.ServiceCollectionsResourceFolderRoot}/{collection.Name}.asset");

            m_serviceCollections.Add(collection);
            m_currentServiceCollection = collection;

            UpdatePreprocessorDefinitionsStatusForUI();
        }

        private void CreateNewCollection()
        {
            var collection = CreateInstance<PlatformServiceCollection>();
            collection.Name = $"ConfigurationPreset{m_serviceCollections.Count + 1}";

            var path = EditorUtility.SaveFilePanel("Create Service Collection", 
                $"Assets/Resources/{Constants.ResourcesFolderRoot}/{Constants.ServiceCollectionsResourceFolderRoot}",
                $"{collection.Name}", "asset");
            
            if (!string.IsNullOrEmpty(path))
            {
                path = "Assets" + path[Application.dataPath.Length..];
                collection.Name = System.IO.Path.GetFileNameWithoutExtension(path);
                AssetDatabase.CreateAsset(collection, path);
                AssetDatabase.SaveAssets();

                m_serviceCollections.Add(collection);
                SwitchCollection(collection);
            }
        }

        private void SwitchCollection(PlatformServiceCollection collection)
        {
            foreach (var col in m_serviceCollections)
            {
                col.IsActive = false;
                EditorUtility.SetDirty(col);
            }

            collection.IsActive = true;
            m_currentServiceCollection = collection;

            EditorUtility.SetDirty(collection);
            AssetDatabase.SaveAssets();

            UpdatePreprocessorDefinitionsStatusForUI();
        }

        private void DeleteCollection(PlatformServiceCollection collection)
        {
            if (EditorUtility.DisplayDialog("Delete Collection",
                $"Delete collection '{collection.Name}'? You will not be able to cancel the action.", "Yes", "No"))
            {
                m_serviceCollections.Remove(collection);
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(collection));

                if (collection.IsActive)
                {
                    m_currentServiceCollection = m_serviceCollections.FirstOrDefault();
                    if (m_currentServiceCollection == null)
                        CreateDefaultCollection();
                    else
                        m_currentServiceCollection.IsActive = true;
                }
            }

            UpdatePreprocessorDefinitionsStatusForUI();
        }

        private void SwitchEditModeCollection(PlatformServiceCollection collection)
        {
            GUI.FocusControl(null);
            m_editingCollection = collection;
            m_collectionNameBuffer = collection.Name;
            m_GUIFocusControl = RENAME_COLLECTION_FOCUS;
        }

        private void UpdatePreprocessorDefinitionsStatusForUI()
        {
            foreach (var module in m_availableModules)
            {
                var attribute = module.GetType().GetCustomAttribute<PlatformModuleAttribute>();

                if (string.IsNullOrEmpty(attribute.ProprocessorDefinition))
                    continue;

                var enableDefinition = IsModuleActive(module, m_currentServiceCollection);
                if (attribute.ProprocessorDefinitionInverted)
                    enableDefinition = !enableDefinition;

                if (PreprocessorDefinitionManager.HasDefinition(NamedBuildTarget.Standalone, attribute.ProprocessorDefinition) != enableDefinition)
                {
                    m_updatePreprocessorDefinitions = true;
                    return;
                }
            }
            m_updatePreprocessorDefinitions = false;
        }

        private void UpdatePreprocessorDefinitions()
        {
            foreach (var module in m_availableModules)
            {
                var attribute = module.GetType().GetCustomAttribute<PlatformModuleAttribute>();

                if (string.IsNullOrEmpty(attribute.ProprocessorDefinition))
                    continue;

                var enableDefinition = IsModuleActive(module, m_currentServiceCollection);
                if (attribute.ProprocessorDefinitionInverted)
                    enableDefinition = !enableDefinition;

                PreprocessorDefinitionManager.Set(NamedBuildTarget.Standalone, attribute.ProprocessorDefinition, enableDefinition);
            }
        }
    }
}

#endif