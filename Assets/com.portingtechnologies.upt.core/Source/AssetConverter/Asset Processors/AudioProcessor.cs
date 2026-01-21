using JetBrains.Annotations;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UPT.Core.AssetConverter
{
    public class AudioProcessor : AssetPostprocessor
    {
        private void OnPreprocessAudio()
        {
            var importer = assetImporter as AudioImporter;
            var directory = Path.GetDirectoryName(importer.assetPath).Replace('\\', '/');
            var storage = UserSettings.Instance;
            var log = $"The settings for the audio clip <b>{importer.assetPath}</b> are set automatically:";
            var modified = false;

            foreach (var platform in PlatformData.Platforms)
            {
                var innerSampleSettings = importer.GetOverrideSampleSettings(platform.Name);
                var presetsByDirectories = storage.GetPresetsByDirectories(platform.BuildTargetGroup, AssetType.Audio);
                var overridedByDirectory = false;
                foreach (var preset in presetsByDirectories)
                {
                    if (preset.Preset is AudioSettingsPreset audioSettingsPreset)
                    {
                        if (audioSettingsPreset.AutomaticallyApplyForNew && preset.DirectoryPaths.Any(path => directory.Equals(path, System.StringComparison.OrdinalIgnoreCase)))
                        {
                            if (ApplyPresetToAudioSampleSettings(audioSettingsPreset, innerSampleSettings, out var outerSampleSettings, out var outLog))
                            {
                                modified = true;
                                importer.SetOverrideSampleSettings(platform.Name, outerSampleSettings);
                                log += $"\n<b><color={UserSettings.DEBUG_HIGHLIGHT_COLOR}>{platform.Name}</color></b> | <b>Override By Directories</b>: {outLog}";
                            }
                            overridedByDirectory = true;
                        }
                    }
                }

                if (overridedByDirectory)
                    continue;

                var audioSettings = storage.GetAudioSettings(platform.BuildTargetGroup);
                if (!audioSettings.AutomaticallyApplyForNew)
                    continue;

                if (ApplyPresetToAudioSampleSettings(audioSettings, innerSampleSettings, out var sampleSettings, out var resultLog))
                {
                    modified = true;
                    importer.SetOverrideSampleSettings(platform.Name, sampleSettings);
                    log += $"\n<b><color={UserSettings.DEBUG_HIGHLIGHT_COLOR}>{platform.Name}</color></b> | <b>Default Preset</b>: {resultLog}";
                }
            }

            if (modified)
            {
                log += "\n\n<b>NOTE:</b> Follow <i>Tool —> Porting Tool —> Audio Importer</i> to change the settings.\n";
                UptLogger.Info(log);
            }
        }

        public static bool ApplyPresetToAudioSampleSettings(AudioSettingsPreset preset, AudioImporterSampleSettings innerSampleSettings, out AudioImporterSampleSettings outerSampleSettings, [CanBeNull] out string log)
        {
            var modified = false;
            log = string.Empty;
            outerSampleSettings = innerSampleSettings;

            if (preset.OverrideLoadType && outerSampleSettings.loadType != preset.LoadType)
            {
                log += $"Load Type: {outerSampleSettings.loadType} —> {preset.LoadType}. ";
                outerSampleSettings.loadType = preset.LoadType;
                modified = true;
            }

            if (preset.OverrideCompressionFormat && outerSampleSettings.compressionFormat != preset.CompressionFormat)
            {
                log += $"Compression Format: {outerSampleSettings.compressionFormat} —> {preset.CompressionFormat}. ";
                outerSampleSettings.compressionFormat = preset.CompressionFormat;
                modified = true;
            }

            if (preset.OverrideQuality && outerSampleSettings.quality != preset.Quality)
            {
                log += $"Quality: {outerSampleSettings.quality} —> {preset.Quality}. ";
                outerSampleSettings.quality = preset.Quality;
                modified = true;
            }

            if (preset.OverrideSampleRateSetting && outerSampleSettings.sampleRateSetting != preset.SampleRateSetting)
            {
                log += $"Sample Rate Settings: {outerSampleSettings.sampleRateSetting} —> {preset.SampleRateSetting}. ";
                outerSampleSettings.sampleRateSetting = preset.SampleRateSetting;

                if (preset.SampleRateSetting == AudioSampleRateSetting.OverrideSampleRate && outerSampleSettings.sampleRateOverride != preset.SampleRate)
                {
                    log += $"Sample Rate: {outerSampleSettings.sampleRateOverride} —> {preset.SampleRate}. ";
                    outerSampleSettings.sampleRateOverride = (uint)preset.SampleRate;
                }

                modified = true;
            }

            if (!modified)
                log = null;

            return modified;
        }
    }
}