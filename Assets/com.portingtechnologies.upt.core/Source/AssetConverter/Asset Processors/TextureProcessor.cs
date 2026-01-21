using JetBrains.Annotations;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UPT.Core.AssetConverter
{
    public class TextureProcessor : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            var importer = assetImporter as TextureImporter;
            var directory = Path.GetDirectoryName(importer.assetPath).Replace('\\', '/');
            var textureType = Utils.ConvertToTextureType(importer.textureType, importer.textureShape);
            var alphaSource = Utils.ConvertToAlphaSourcePresence(importer.alphaSource);
            var storage = UserSettings.Instance;
            var log = $"The settings for the texture <b>{importer.assetPath}</b> are set automatically:";
            var modified = false;

            foreach (var platform in PlatformData.Platforms)
            {
                var platformSettings = importer.GetPlatformTextureSettings(platform.Name);
                var presetsByDirectories = storage.GetPresetsByDirectories(platform.BuildTargetGroup, AssetType.Texture);
                var overridedByDirectory = false;
                foreach (var preset in presetsByDirectories)
                {
                    if (preset.Preset is TextureSettingsPreset textureSettingsPreset)
                    {
                        if (textureSettingsPreset.AutomaticallyApplyForNew && preset.DirectoryPaths.Any(path => directory.Equals(path, System.StringComparison.OrdinalIgnoreCase)))
                        {
                            if (ApplyPresetToPlatformTextureSettings(platformSettings, textureSettingsPreset, out var outLog))
                            {
                                modified = true;
                                importer.SetPlatformTextureSettings(platformSettings);
                                log += $"\n<b><color={UserSettings.DEBUG_HIGHLIGHT_COLOR}>{platform.Name}</color></b> | <b>Override By Directories</b>: {outLog}";
                            }
                            overridedByDirectory = true;
                        }
                    }
                }

                if (overridedByDirectory)
                    continue;

                var textureSettings = storage.GetTextureSettings(textureType, alphaSource, platform.BuildTargetGroup);

                if (textureSettings == null)
                {
                    Debug.LogError($"Texture settings for type {textureType} was not found!");
                    continue;
                }

                if (!textureSettings.AutomaticallyApplyForNew)
                    continue;

                if (ApplyPresetToPlatformTextureSettings(platformSettings, textureSettings, out var resultLog))
                {
                    modified = true;
                    importer.SetPlatformTextureSettings(platformSettings);
                    log += $"\n<b><color={UserSettings.DEBUG_HIGHLIGHT_COLOR}>{platform.Name}</color></b> | <b>Default Preset</b>: {resultLog}";
                }
            }

            if (modified)
            {
                log += "\n\n<b>NOTE:</b> Follow <i>Tool —> Porting Tool —> Texture Importer</i> to change the settings.\n";
                UptLogger.Info(log);
            }    
        }

        public static bool ApplyPresetToPlatformTextureSettings(TextureImporterPlatformSettings platformSettings, TextureSettingsPreset preset, [CanBeNull] out string log)
        {
            var modified = false;
            log = string.Empty;
            platformSettings.overridden = true;

            if (preset.MaxSizeOverride && platformSettings.maxTextureSize != preset.MaxSize)
            {
                log += $"MaxSize: {platformSettings.maxTextureSize} —> {preset.MaxSize}. ";
                platformSettings.maxTextureSize = preset.MaxSize;
                modified = true;
            }

            if (preset.FormatOverride && platformSettings.format != preset.Format)
            {
                log += $"Format: {platformSettings.format} —> {preset.Format}. ";
                platformSettings.format = preset.Format;
                modified = true;
            }

            if (preset.CompressorQualityOverride && platformSettings.compressionQuality != preset.CompressorQuality)
            {
                log += $"Compressor Quality: {platformSettings.compressionQuality} —> {preset.CompressorQuality}. ";
                platformSettings.compressionQuality = preset.CompressorQuality;
                modified = true;
            }

            if (platformSettings.name == "Android" && preset.OverrideETC2FallbackOverride && platformSettings.androidETC2FallbackOverride != preset.OverrideETS2Fallback)
            {
                log += $"Android ETC2 Fallback Override: {platformSettings.androidETC2FallbackOverride} —> {preset.OverrideETS2Fallback}. ";
                platformSettings.androidETC2FallbackOverride = preset.OverrideETS2Fallback;
                modified = true;
            }

            if (!modified)
                log = null;

            return modified;
        }
    }
}
