using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UPT.Core.AssetConverter
{
    public partial class AssetConverterWindow : EditorWindow
    {
        private GUIStyle m_generalTextStyle;
        private GUIStyle m_generalBigTextStyle;
        private GUIStyle m_titleStyle;
        private GUIStyle m_subTitleStyle;
        private GUIStyle m_tabButtonStyle;
        private GUIStyle m_separatorStyle;
        private GUIStyle m_iconStyle;

        private Color m_highlightBackgroundColor1;
        private Color m_highlightBackgroundColor2;
        private Color m_highlightBackgroundColor3;

        private Vector2 m_scrollPosition;
        private Vector2 m_scrollPosition2;

        private readonly TabData[] m_tabs = new[]
        {
            new TabData { Tab = Tab.TextureImporter, Name = "Texture Importer" },
            new TabData { Tab = Tab.AudioImporter, Name = "Audio Importer" },
            new TabData { Tab = Tab.ModelImporeter, Name = "Model Importer" },
            //new TabData { Tab = Tab.LicensePacker, Name = "License Packer" },
            //new TabData { Tab = Tab.Optimization, Name = "Optimization" },
        };
        private readonly List<string> m_tabNames = new();

        private List<PlatformData> m_supportedPlatforms;

        [MenuItem("Tools/Porting Toolkit/Asset Converter")]
        public static void ShowWindow()
        {
            GetWindow<AssetConverterWindow>("Asset Converter");
        }

        private void OnEnable()
        {
            UserSettings.Instance.LoadFromDisk();

            #region styles
            var colorCode = EditorGUIUtility.isProSkin ? "#b8e0dc" : "#baf7f1";
            ColorUtility.TryParseHtmlString(colorCode, out m_highlightBackgroundColor1);

            colorCode = EditorGUIUtility.isProSkin ? "#f7c5c1" : "#f7c5c1";
            ColorUtility.TryParseHtmlString(colorCode, out m_highlightBackgroundColor2);

            colorCode = EditorGUIUtility.isProSkin ? "#9afcaa" : "#9afcaa";
            ColorUtility.TryParseHtmlString(colorCode, out m_highlightBackgroundColor3);

            m_generalTextStyle = new GUIStyle
            {
                richText = true,
                wordWrap = true
            };
            m_generalTextStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;

            m_generalBigTextStyle = new GUIStyle(m_generalTextStyle)
            {
                fontSize = 16
            };

            m_titleStyle = new GUIStyle(m_generalTextStyle)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold
            };

            m_subTitleStyle = new GUIStyle(m_titleStyle)
            {
                fontSize = 16
            };

            m_tabButtonStyle = new GUIStyle(m_generalTextStyle)
            {
                alignment = TextAnchor.MiddleCenter,
            };

            m_separatorStyle = new GUIStyle
            {
                margin = new RectOffset(0, 0, 4, 4),
                fixedHeight = 1
            };
            m_separatorStyle.normal.background = Texture2D.whiteTexture;

            m_iconStyle = new GUIStyle
            {
                stretchWidth = false
            };
            m_iconStyle.padding.top = 3;
            #endregion

            if (m_tabNames.Count == 0)
            {
                m_tabNames.Capacity = m_tabs.Length;
                foreach (var tab in m_tabs)
                    m_tabNames.Add(tab.Name);
            }

            if (m_supportedPlatforms == null)
            {
                m_supportedPlatforms = new List<PlatformData>();
                foreach (var platform in PlatformData.Platforms)
                {
                    if (BuildPipeline.IsBuildTargetSupported(platform.BuildTargetGroup, platform.BuildTarget))
                        m_supportedPlatforms.Add(platform);
                }
            }

            var storage = UserSettings.Instance;
            storage.SelectedPlatform ??= m_supportedPlatforms[0];
            ChangeTab(storage.SelectedTab);
        }

        private void OnDisable()
        {
            m_assetTreeView = null;
            UserSettings.Instance.OnDisableTool();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();

            LeftMenu();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();
            m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition, GUILayout.ExpandHeight(true));

            switch (UserSettings.Instance.SelectedTab)
            {
                case Tab.TextureImporter:
                case Tab.AudioImporter:
                case Tab.ModelImporeter:
                    AssetImporterModule();
                    break;

                case Tab.LicensePacker: break;
                case Tab.Optimization: OptimizationModule(); break;
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private void LeftMenu()
        {
            EditorGUILayout.BeginVertical("box", GUILayout.Width(130.0f), GUILayout.ExpandHeight(true));
            EditorGUILayout.Space();

            var storage = UserSettings.Instance;
            var oldColor = GUI.backgroundColor;

            for (var i = 0; i < m_tabNames.Count; i++)
            {
                var isSelectedTab = i == (int)storage.SelectedTab;
                GUI.backgroundColor = isSelectedTab ? m_highlightBackgroundColor1 : oldColor;
                m_tabButtonStyle.fontStyle = isSelectedTab ? FontStyle.Bold : FontStyle.Normal;

                EditorGUILayout.BeginHorizontal("button", GUILayout.Height(22.0f));

                if (GUILayout.Button(m_tabNames[i], m_tabButtonStyle))
                    ChangeTab((Tab)i);

                EditorGUILayout.EndHorizontal();
            }
            m_tabButtonStyle.fontStyle = FontStyle.Normal;

            EditorGUILayout.Space(20.0f);

            GUI.backgroundColor = oldColor;
            EditorGUILayout.BeginHorizontal("Button", GUILayout.Height(22.0f));
            if (GUILayout.Button("Save", m_tabButtonStyle))
                storage.WriteOnDisk();
            EditorGUILayout.EndHorizontal();

            GUI.backgroundColor = m_highlightBackgroundColor2;
            EditorGUILayout.BeginHorizontal("Button", GUILayout.Height(22.0f));
            if (GUILayout.Button("Reset", m_tabButtonStyle))
            {
                if (EditorUtility.DisplayDialog("Reset", "Are you sure you want to reset all settings to default?", "Reset", "Cancel"))
                    ResetSettings();
            }
            GUI.backgroundColor = oldColor;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void ChangeTab(Tab tab)
        {
            var storage = UserSettings.Instance;
            storage.SelectedTab = tab;
            m_importerSection = storage.SelectedTab switch
            {
                Tab.TextureImporter => new TextureImporterEditorGUI(),
                Tab.AudioImporter => new AudioImporterEditorGUI(),
                Tab.ModelImporeter => new MeshImporterEditorGUI(),
                _ => null,
            };
            m_assetTreeView = null;
        }

        private void DrawSeparator()
        {
            var oldColor = GUI.color;
            GUI.color = EditorGUIUtility.isProSkin ? Color.white : Color.gray;
            GUILayout.Box(GUIContent.none, m_separatorStyle);
            GUI.color = oldColor;
        }

        private void DrawBuildTargets(Action onChange = null)
        {
            var storage = UserSettings.Instance;
            EditorGUILayout.LabelField("Build Targets", m_subTitleStyle);
            EditorGUILayout.Space(5.0f);
            EditorGUILayout.BeginHorizontal();

            var oldColor = GUI.backgroundColor;
            for (var i = 0; i < m_supportedPlatforms.Count; i++)
            {
                var isSelectedPlatform = m_supportedPlatforms[i].BuildTargetGroup == storage.SelectedPlatform.BuildTargetGroup;
                GUI.backgroundColor = isSelectedPlatform ? m_highlightBackgroundColor1 : oldColor;

                if (GUILayout.Button(new GUIContent(Utils.GetUnityIcon(m_supportedPlatforms[i].IconName), m_supportedPlatforms[i].Tooltip)))
                {
                    storage.SelectedPlatform = m_supportedPlatforms[i];
                    onChange?.Invoke();
                }
            }
            GUI.backgroundColor = oldColor;

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(20.0f);
        }

        private void ResetSettings()
        {
            UserSettings.Instance.ResetSettings();
            Repaint();
        }
    }
}
