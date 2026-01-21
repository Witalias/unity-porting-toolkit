using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UPT.Core.AssetConverter
{
    public class TextureImporterEditorGUI : IImporterSectionEditor
    {
        private readonly TextureImporterType[] m_textureImporterTypes = new[]
        {
            TextureImporterType.Default,
            TextureImporterType.Lightmap,
            TextureImporterType.Sprite
        };

        private readonly TextureImporterShape[] m_textureImporterShapes = new[]
        {
            TextureImporterShape.Texture2D,
            TextureImporterShape.TextureCube
        };

        private readonly int[] m_textureMaxSizes = new[] { 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384 };

        private readonly AlphaSourcePresence[] m_alphaSourcePresences = new[]
        {
            AlphaSourcePresence.None,
            AlphaSourcePresence.Assigned
        };

        private readonly TextureProcessorEditor m_textureProcessor = new();

        public AssetTreeView TreeView
        {
            get
            {
                (var textureType, var preset) = GetSettingsPreset();
                return preset == null ? null : new TextureTreeView(textureType, preset.FilterByAlphaSource ? UserSettings.Instance.SelectedAlphaSource : null, true);
            }
        }

        public IImporterSettingsPreset SettingsPreset => new TextureSettingsPreset();

        public AssetType AssetType => AssetType.Texture;

        public bool HasFilters => true;

        public bool PlatformOverrides => true;

        public void DrawFilters(Action updateLayout)
        {
            var storage = UserSettings.Instance;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Texture Type");
            Utils.DrawDropdownMenu(m_textureImporterTypes, storage.SelectedTextureType, OnImporterTypeFilterClick);
            EditorGUILayout.EndHorizontal();

            if (storage.SelectedTextureType is TextureImporterType.Default)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Texture Shape");
                Utils.DrawDropdownMenu(m_textureImporterShapes, storage.SelectedTextureShape, OnImporterShapeFilterClick);
                EditorGUILayout.EndHorizontal();
            }

            (var textureType, var preset) = GetSettingsPreset();
            if (preset == null)
            {
                Debug.LogError($"Texture settings preset for {textureType} was not found");
                return;
            }

            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            var filterByAlphaSource = EditorGUILayout.BeginToggleGroup("Filter By Alpha Source", preset.FilterByAlphaSource);
            if (EditorGUI.EndChangeCheck())
            {
                if (!filterByAlphaSource && storage.SelectedAlphaSource is not AlphaSourcePresence.None)
                {
                    if (EditorUtility.DisplayDialog("Disable alpha source filtering",
                        "When you disable alpha source filtering, a preset with an alpha source value of \"None\" is applied to all textures of the selected type (regardless of their alpha source).",
                        "Disable", "Cancel"))
                    {
                        storage.SelectedAlphaSource = AlphaSourcePresence.None;
                        preset.FilterByAlphaSource = filterByAlphaSource;
                    }
                }
                else
                {
                    preset.FilterByAlphaSource = filterByAlphaSource;
                }
                storage.OnFilterByAlphaSourceChanged(preset.FilterByAlphaSource);
                updateLayout?.Invoke();
            }
            if (!preset.FilterByAlphaSource)
            {
                storage.SelectedAlphaSource = AlphaSourcePresence.None;
            }
            Utils.DrawDropdownMenu(m_alphaSourcePresences, storage.SelectedAlphaSource, OnAlphaSourceFilterClick);
            EditorGUILayout.EndToggleGroup();

            void OnImporterTypeFilterClick(object type)
            {
                storage.SelectedTextureType = (TextureImporterType)type;
                updateLayout?.Invoke();
            }

            void OnImporterShapeFilterClick(object shape)
            {
                storage.SelectedTextureShape = (TextureImporterShape)shape;
                updateLayout?.Invoke();
            }

            void OnAlphaSourceFilterClick(object alphaSource)
            {
                storage.SelectedAlphaSource = (AlphaSourcePresence)alphaSource;
                updateLayout?.Invoke();
            }
        }

        public void DrawSettingsPreset(Action<bool> onApplied)
        {
            (var _, var preset) = GetSettingsPreset();
            DrawSettingsPreset_Internal(preset, onApplied);
        }

        public void DrawSettingsPreset(AssetPresetByDirectory presetByDictionary, Action<bool> onApplied)
        {
            if (presetByDictionary.Preset is TextureSettingsPreset texturePreset)
                DrawSettingsPreset_Internal(texturePreset, onApplied, presetByDictionary.DirectoryPaths);
        }

        private void DrawSettingsPreset_Internal(TextureSettingsPreset preset, Action<bool> onApplied, List<string> dictionaryPaths = null)
        {
            var storage = UserSettings.Instance;

            EditorGUILayout.BeginHorizontal();
            preset.AutomaticallyApplyForNew = EditorGUILayout.Toggle("Apply For New Textures", preset.AutomaticallyApplyForNew);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            preset.MaxSizeOverride = EditorGUILayout.BeginToggleGroup("Max Size", preset.MaxSizeOverride);
            Utils.DrawDropdownMenu(m_textureMaxSizes, preset.MaxSize, OnMaxSizeClick);
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.Space();

            preset.FormatOverride = EditorGUILayout.BeginToggleGroup("Format", preset.FormatOverride);
            Utils.DrawDropdownMenu(storage.SelectedPlatform.TextureSettings.TextureFormats, preset.Format, OnFormatClick);
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.Space();

            preset.CompressorQualityOverride = EditorGUILayout.BeginToggleGroup("Compressor Quality", preset.CompressorQualityOverride);
            preset.CompressorQuality = EditorGUILayout.IntSlider(preset.CompressorQuality, 0, 100);
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.Space();

            if (storage.SelectedPlatform.Name == "Android")
            {
                preset.OverrideETC2FallbackOverride = EditorGUILayout.BeginToggleGroup("Override ETC2 Fallback", preset.OverrideETC2FallbackOverride);
                Utils.DrawDropdownMenu(storage.SelectedPlatform.TextureSettings.AndroidETC2FallbackOverrides, preset.OverrideETS2Fallback, OnOverrideETC2FallbackClick);
                EditorGUILayout.EndToggleGroup();
                EditorGUILayout.Space();
            }

            if (GUILayout.Button(new GUIContent("Apply Settings Now", "Apply preset settings to marked textures if possible")))
            {
                var modified = dictionaryPaths == null
                    ? m_textureProcessor.ApplySettingsPreset(storage.SelectedTextureType, storage.SelectedTextureShape, storage.SelectedAlphaSource, preset)
                    : m_textureProcessor.ApplySettingsPreset(dictionaryPaths, preset);
                onApplied?.Invoke(modified);
            }

            void OnMaxSizeClick(object size)
            {
                preset.MaxSize = (int)size;
            }

            void OnOverrideETC2FallbackClick(object overrideETC2Callback)
            {
                preset.OverrideETS2Fallback = (AndroidETC2FallbackOverride)overrideETC2Callback;
            }

            void OnFormatClick(object format)
            {
                preset.Format = (TextureImporterFormat)format;
            }
        }

        private (TextureType, TextureSettingsPreset) GetSettingsPreset()
        {
            var storage = UserSettings.Instance;
            var textureType = Utils.ConvertToTextureType(storage.SelectedTextureType, storage.SelectedTextureShape);
            var preset = UserSettings.Instance.GetTextureSettings(textureType, storage.SelectedAlphaSource);
            return (textureType, preset);
        }
    }
}
