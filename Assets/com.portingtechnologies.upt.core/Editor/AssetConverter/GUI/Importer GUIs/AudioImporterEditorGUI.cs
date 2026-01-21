using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UPT.Core.AssetConverter
{
    public class AudioImporterEditorGUI : IImporterSectionEditor
    {
        private readonly int[] m_sampleRates = new[] { 8000, 11025, 22050, 44100, 48000, 96000, 192000 };
        private readonly AudioProcessorEditor m_audioProcessor = new();

        public AssetTreeView TreeView => new AudioTreeView(true);

        public IImporterSettingsPreset SettingsPreset => new AudioSettingsPreset();

        public AssetType AssetType => AssetType.Audio;

        public bool HasFilters => false;

        public bool PlatformOverrides => true;

        public void DrawFilters(Action updateLayout) { }

        public void DrawSettingsPreset(Action<bool> onApplied)
        {
            DrawSettingsPreset_Internal(UserSettings.Instance.GetAudioSettings(), onApplied);
        }

        public void DrawSettingsPreset(AssetPresetByDirectory presetByDirectory, Action<bool> onApplied)
        {
            if (presetByDirectory.Preset is AudioSettingsPreset audioPreset)
                DrawSettingsPreset_Internal(audioPreset, onApplied, presetByDirectory.DirectoryPaths);
        }

        private void DrawSettingsPreset_Internal(AudioSettingsPreset preset, Action<bool> onApplied, List<string> directoryPaths = null)
        {
            var storage = UserSettings.Instance;

            EditorGUILayout.BeginHorizontal();
            preset.AutomaticallyApplyForNew = EditorGUILayout.Toggle("Apply For New Audio", preset.AutomaticallyApplyForNew);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            preset.OverrideLoadType = EditorGUILayout.BeginToggleGroup("Load Type", preset.OverrideLoadType);
            Utils.DrawDropdownMenu(Enum.GetValues(typeof(AudioClipLoadType)), preset.LoadType, OnLoadTypeChanged);
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.Space();

            preset.OverrideCompressionFormat = EditorGUILayout.BeginToggleGroup("Compression Format", preset.OverrideCompressionFormat);
            Utils.DrawDropdownMenu(storage.SelectedPlatform.AudioSettings.CompressionFormats, preset.CompressionFormat, OnCompressionFormatChanged);
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.Space();

            if (preset.OverrideCompressionFormat && (preset.CompressionFormat is AudioCompressionFormat.Vorbis || preset.CompressionFormat is AudioCompressionFormat.MP3))
            {
                preset.OverrideQuality = EditorGUILayout.BeginToggleGroup("Quality", preset.OverrideQuality);
                preset.Quality = EditorGUILayout.Slider(preset.Quality, 0.0f, 1.0f);
                EditorGUILayout.EndToggleGroup();
                EditorGUILayout.Space();
            }
            else
            {
                preset.OverrideQuality = false;
            }

            preset.OverrideSampleRateSetting = EditorGUILayout.BeginToggleGroup("Sample Rate Setting", preset.OverrideSampleRateSetting);
            Utils.DrawDropdownMenu(Enum.GetValues(typeof(AudioSampleRateSetting)), preset.SampleRateSetting, OnSampleRateSettingChanged);
            if (preset.SampleRateSetting is AudioSampleRateSetting.OverrideSampleRate)
                Utils.DrawDropdownMenu(m_sampleRates, preset.SampleRate, OnSampleRateChanged);
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.Space();

            if (GUILayout.Button(new GUIContent("Apply Settings Now", "Apply preset settings to marked audio if possible")))
            {
                var modified = directoryPaths == null
                    ? m_audioProcessor.ApplySettingsPreset(preset)
                    : m_audioProcessor.ApplySettingsPreset(directoryPaths, preset);
                onApplied?.Invoke(modified);
            }

            void OnLoadTypeChanged(object loadType)
            {
                preset.LoadType = (AudioClipLoadType)loadType;
            }

            void OnCompressionFormatChanged(object compressionFormat)
            {
                preset.CompressionFormat = (AudioCompressionFormat)compressionFormat;
            }

            void OnSampleRateSettingChanged(object sampleRateSetting)
            {
                preset.SampleRateSetting = (AudioSampleRateSetting)sampleRateSetting;
            }

            void OnSampleRateChanged(object sampleRate)
            {
                preset.SampleRate = (int)sampleRate;
            }
        }
    }
}
