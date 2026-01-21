using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UPT.Core.AssetConverter
{
    public class AudioProcessorEditor
    {
        public bool ApplySettingsPreset(AudioSettingsPreset preset)
        {
            if (preset == null)
            {
                UptLogger.Error("Preset is NULL");
                return false;
            }

            var storage = UserSettings.Instance;
            var modified = false;
            var log = $"Changed audio clips for build target <b><color={UserSettings.DEBUG_HIGHLIGHT_COLOR}>{storage.SelectedPlatform.BuildTargetGroup}</color></b>:";
            var guids = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets" });
            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var importer = AssetImporter.GetAtPath(path) as AudioImporter;
                if (importer == null)
                    continue;

                var assetData = storage.GetAssetData(path);
                if (assetData == null || !assetData.OverrideSettings)
                    continue;

                var platform = storage.SelectedPlatform.Name;
                var innerSampleSettings = importer.GetOverrideSampleSettings(platform);
                if (AudioProcessor.ApplyPresetToAudioSampleSettings(preset, innerSampleSettings, out var settings, out var resultLog))
                {
                    importer.SetOverrideSampleSettings(platform, settings);
                    importer.SaveAndReimport();
                    modified = true;
                    log += $"\n<b>{path}</b>. {resultLog}";
                }
            }

            if (modified)
                UptLogger.Info(log + '\n');

            return modified;
        }

        public bool ApplySettingsPreset(IReadOnlyCollection<string> directoryPaths, AudioSettingsPreset preset)
        {
            if (preset == null)
            {
                UptLogger.Error("Preset is NULL");
                return false;
            }

            var storage = UserSettings.Instance;
            var modified = false;
            var log = $"Changed audio clips for build target <b><color={UserSettings.DEBUG_HIGHLIGHT_COLOR}>{storage.SelectedPlatform.BuildTargetGroup}</color></b>:";

            foreach (var directoryPath in directoryPaths)
            {
                if (!Directory.Exists(directoryPath))
                    continue;

                var assetPaths = Directory.GetFiles(directoryPath);
                foreach (var assetPath in assetPaths)
                {
                    var importer = AssetImporter.GetAtPath(assetPath) as AudioImporter;
                    if (importer == null)
                        continue;

                    var platform = storage.SelectedPlatform.Name;
                    var innerSampleSettings = importer.GetOverrideSampleSettings(platform);
                    if (AudioProcessor.ApplyPresetToAudioSampleSettings(preset, innerSampleSettings, out var settings, out var resultLog))
                    {
                        importer.SetOverrideSampleSettings(platform, settings);
                        importer.SaveAndReimport();
                        modified = true;
                        log += $"\n<b>{assetPath}</b>. {resultLog}";
                    }
                }
            }

            if (modified)
                UptLogger.Info(log + '\n');

            return modified;
        }
    }
}