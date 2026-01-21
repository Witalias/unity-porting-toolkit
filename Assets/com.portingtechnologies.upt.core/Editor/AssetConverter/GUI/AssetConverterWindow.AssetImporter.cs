using System.IO;
using UnityEditor;
using UnityEngine;

namespace UPT.Core.AssetConverter
{
    public partial class AssetConverterWindow
    {
        private IImporterSectionEditor m_importerSection;
        private AssetTreeView m_assetTreeView;
        private bool m_applied;
        private bool m_wasModifiedTextures;

        private void AssetImporterModule()
        {
            if (m_importerSection.PlatformOverrides)
                DrawBuildTargets(UpdateLayout);

            var storage = UserSettings.Instance;
            storage.FoldoutTextureDefaultPresets = EditorGUILayout.Foldout(storage.FoldoutTextureDefaultPresets, "Default Presets", "FoldoutHeader");
            if (storage.FoldoutTextureDefaultPresets)
            {
                EditorGUILayout.Space(10.0f);

                if (m_importerSection.HasFilters)
                {
                    EditorGUILayout.LabelField("Filters", m_subTitleStyle);
                    DrawSeparator();

                    m_importerSection.DrawFilters(UpdateLayout);
                    EditorGUILayout.Space(20.0f);
                }

                EditorGUILayout.LabelField("Tree View", m_subTitleStyle);
                DrawSeparator();

                m_assetTreeView ??= m_importerSection.TreeView;

                var message = "No assets found with the selected filters! Try changing filters.";
                if (m_assetTreeView.AssetsFound)
                {
                    if (m_importerSection.HasFilters)
                        message = "All found assets with the given filters are listed below.\nSelect the ones you want to apply the settings to.";
                    else
                        message = "All found assets are listed below.\nSelect the ones you want to apply the settings to.";
                }
                else if (!m_importerSection.HasFilters)
                {
                    message = "No assets found!";
                }

                EditorGUILayout.HelpBox(message, m_assetTreeView.AssetsFound ? MessageType.Info : MessageType.Warning);
                m_assetTreeView.Draw();
                EditorGUILayout.Space(20.0f);

                EditorGUILayout.LabelField("Settings Preset", m_subTitleStyle);
                DrawSeparator();

                m_importerSection.DrawSettingsPreset(modified =>
                {
                    m_wasModifiedTextures = modified;
                    m_applied = true;
                });

                if (m_applied)
                {
                    EditorGUILayout.HelpBox(m_wasModifiedTextures ?
                        "Success! Check the editor log for details." :
                        "No assets modification is required! Either no assets are specified to override in Tree View, or all settings are already up to date.",
                        m_wasModifiedTextures ? MessageType.Info : MessageType.Warning);
                }

                EditorGUILayout.Space(20.0f);
            }
            else
            {
                EditorGUILayout.Space();
            }

            storage.FoldoutTextureOverrideByDirectories = EditorGUILayout.Foldout(storage.FoldoutTextureOverrideByDirectories, "Override By Directories", "FoldoutHeader");
            if (storage.FoldoutTextureOverrideByDirectories)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Add relative asset directory paths below to set custom non-default presets for them.", MessageType.Info);
                EditorGUILayout.Space();

                var datas = storage.GetPresetsByDirectories(m_importerSection.AssetType);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(Utils.GetUnityIcon("Toolbar Plus"), GUILayout.Width(50.0f)))
                {
                    var presetByDirectory = new AssetPresetByDirectory(m_importerSection.SettingsPreset)
                    {
                        BuildTargetGroup = m_importerSection.PlatformOverrides ? storage.SelectedPlatform.BuildTargetGroup : BuildTargetGroup.Standalone
                    };
                    storage.AddSettingsPresetByDirectory(presetByDirectory);
                }
                EditorGUILayout.EndHorizontal();

                m_scrollPosition2 = EditorGUILayout.BeginScrollView(m_scrollPosition2, "FrameBox", GUILayout.MinHeight(600.0f));
                var oldColor = GUI.backgroundColor;

                for (var i = 0; i < datas.Count; i++)
                {
                    var data = datas[i];

                    EditorGUILayout.BeginVertical("GroupBox");
                    EditorGUILayout.BeginHorizontal();

                    if (!data.IsCorrectDirectories)
                        GUI.backgroundColor = m_highlightBackgroundColor2;
                    data.EditorFoldoutDirectoryPaths = EditorGUILayout.Foldout(data.EditorFoldoutDirectoryPaths, "Directory Paths", true, "FoldoutHeader");
                    GUI.backgroundColor = oldColor;

                    if (GUILayout.Button(new GUIContent(Utils.GetUnityIcon("TreeEditor.Trash"), "Remove block"), GUILayout.Width(30.0f)))
                    {
                        storage.RemoveSettingsPresetByDirectory(data);
                        i--;
                        continue;
                    }
                    EditorGUILayout.EndHorizontal();

                    if (data.EditorFoldoutDirectoryPaths)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.BeginHorizontal();
                        data.AutomationAssistant = EditorGUILayout.Toggle(new GUIContent("Automation Assistant",
                            "If enabled, we will monitor changes to the specified directories (renaming, moving and removing) and automatically update the list"), 
                            data.AutomationAssistant);
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUI.BeginChangeCheck();
                        data.AutoNewSubdirectories = EditorGUILayout.Toggle(new GUIContent("Auto New Subdirectories",
                            "Automatically add new subdirectories of the specified directories to the list"), data.AutoNewSubdirectories);
                        if (EditorGUI.EndChangeCheck() && data.AutoNewSubdirectories)
                            Utils.AddSubdirectories(data.DirectoryPaths);
                        if (data.AutoNewSubdirectories && GUILayout.Button(
                            new GUIContent(Utils.GetUnityIcon("RotateTool"), "Check and add missing subdirectories"), GUILayout.Width(30.0f)))
                            Utils.AddSubdirectories(data.DirectoryPaths);
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.Space();

                        if (GUILayout.Button(Utils.GetUnityIcon("Toolbar Plus"), GUILayout.Width(50.0f)))
                            data.DirectoryPaths.Add("Assets/");

                        data.IsCorrectDirectories = true;
                        var paths = data.DirectoryPaths;
                        for (var j = 0; j < paths.Count; j++)
                        {
                            EditorGUILayout.BeginHorizontal();

                            var path = $"{Application.dataPath[..Application.dataPath.IndexOf("Assets")]}/{paths[j]}";
                            var directoryExists = paths[j] != string.Empty && Directory.Exists(path);
                            GUI.backgroundColor = directoryExists ? oldColor : m_highlightBackgroundColor2;
                            paths[j] = EditorGUILayout.TextField(paths[j]);
                            GUI.backgroundColor = oldColor;

                            var markImageName = directoryExists ? "TestPassed" : "Error";
                            GUILayout.Box(new GUIContent(Utils.GetUnityIcon(markImageName),
                                directoryExists ? "This directory path is correct" : "This directory at the specified path was not found"), m_iconStyle);

                            if (GUILayout.Button(new GUIContent(Utils.GetUnityIcon("TreeEditor.Trash"), "Remove field"), GUILayout.Width(30.0f)))
                            {
                                paths.RemoveAt(j);
                                j--;
                            }

                            if (!directoryExists)
                                data.IsCorrectDirectories = false;

                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUILayout.Space(20.0f);
                    }
                    else
                    {
                        EditorGUILayout.Space();
                    }
                    data.EditorFoldoutPreset = EditorGUILayout.Foldout(data.EditorFoldoutPreset, "Settings Preset", true, "FoldoutHeader");
                    if (data.EditorFoldoutPreset)
                    {
                        EditorGUILayout.Space();
                        m_importerSection.DrawSettingsPreset(data, modified =>
                        {
                            data.EditorWasModifiedTextures = modified;
                            data.EditorSettingsApplied = true;
                        });

                        if (!data.IsCorrectDirectories)
                        {
                            EditorGUILayout.HelpBox("Incorrect directory paths found. Please correct them before applying the settings.", MessageType.Warning);
                        }
                        else if (data.EditorSettingsApplied)
                        {
                            EditorGUILayout.HelpBox(data.EditorWasModifiedTextures ?
                                "Success! Check the editor log for details." :
                                "No assets modification is required! Either the specified directories do not contain assets of the appropriate type, or all settings are already up to date.",
                                data.EditorWasModifiedTextures ? MessageType.Info : MessageType.Warning);
                        }
                    }

                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.EndScrollView();
            }
            else
            {
                EditorGUILayout.Space();
            }

            void UpdateLayout()
            {
                m_assetTreeView = null;
                m_applied = false;
            }
        }
    }
}