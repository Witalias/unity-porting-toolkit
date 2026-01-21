using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UPT.Core.AssetConverter
{
    public class MeshImporterEditorGUI : IImporterSectionEditor
    {
        private readonly MeshProcessorEditor m_meshProcessor = new();

        public AssetTreeView TreeView => new MeshTreeView(true);

        public IImporterSettingsPreset SettingsPreset => new MeshSettingsPreset();

        public AssetType AssetType => AssetType.Mesh;

        public bool HasFilters => false;

        public bool PlatformOverrides => false;

        public void DrawFilters(Action updateLayout) { }

        public void DrawSettingsPreset(Action<bool> onApplied)
        {
            var preset = UserSettings.Instance.GetMeshSettings();
            DrawSettingsPreset_Internal(preset, onApplied);
        }

        public void DrawSettingsPreset(AssetPresetByDirectory presetByDirectory, Action<bool> onApplied)
        {
            if (presetByDirectory.Preset is MeshSettingsPreset meshPreset)
                DrawSettingsPreset_Internal(meshPreset, onApplied, presetByDirectory.DirectoryPaths);
        }

        private void DrawSettingsPreset_Internal(MeshSettingsPreset preset, Action<bool> onApplied, List<string> directoryPaths = null)
        {
            var storage = UserSettings.Instance;

            EditorGUILayout.BeginHorizontal();
            preset.AutomaticallyApplyForNew = EditorGUILayout.Toggle("Apply For New Models", preset.AutomaticallyApplyForNew);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            preset.OverrideCompression = EditorGUILayout.BeginToggleGroup("Mesh Compression", preset.OverrideCompression);
            Utils.DrawDropdownMenu(Enum.GetValues(typeof(ModelImporterMeshCompression)), preset.Compression, OnCompressionChanged);
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.Space();

            if (GUILayout.Button(new GUIContent("Apply Settings Now", "Apply preset settings to marked audio if possible")))
            {
                var modified = directoryPaths == null
                    ? m_meshProcessor.ApplySettingsPreset(preset)
                    : m_meshProcessor.ApplySettingsPreset(directoryPaths, preset);
                onApplied?.Invoke(modified);
            }

            void OnCompressionChanged(object compression)
            {
                preset.Compression = (ModelImporterMeshCompression)compression;
            }
        }
    }
}
