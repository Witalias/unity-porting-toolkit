using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UPT.Core.AssetConverter
{
    public class TextureProcessorEditor
    {
        public bool ApplySettingsPreset(TextureImporterType importerType, TextureImporterShape importerShape, AlphaSourcePresence alphaSource, TextureSettingsPreset preset)
        {
            if (preset == null)
            {
                UptLogger.Error("Preset is NULL");
                return false;
            }

            var storage = UserSettings.Instance;
            var modified = false;
            var log = $"Changed textures for build target <b><color={UserSettings.DEBUG_HIGHLIGHT_COLOR}>{storage.SelectedPlatform.BuildTargetGroup}</color></b>:";
            var guids = AssetDatabase.FindAssets("t:Texture", new[] { "Assets" });
            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                    continue;

                if (importer.textureType != importerType)
                    continue;

                if (importer.textureType is TextureImporterType.Default && importer.textureShape != importerShape)
                    continue;

                if (preset.FilterByAlphaSource && Utils.ConvertToAlphaSourcePresence(importer.alphaSource) != alphaSource)
                    continue;

                var assetData = storage.GetAssetData(path);
                if (assetData == null || !assetData.OverrideSettings)
                    continue;

                var platform = storage.SelectedPlatform.Name;
                var settings = importer.GetPlatformTextureSettings(platform);
                if (TextureProcessor.ApplyPresetToPlatformTextureSettings(settings, preset, out var resultLog))
                {
                    importer.SetPlatformTextureSettings(settings);
                    importer.SaveAndReimport();
                    modified = true;

                    if (resultLog != null)
                        log += $"\n<b>{path}</b>. {resultLog}";
                }
            }

            if (modified)
                UptLogger.Info(log + '\n');

            return modified;
        }

        public bool ApplySettingsPreset(IReadOnlyCollection<string> directoryPaths, TextureSettingsPreset preset)
        {
            if (preset == null)
            {
                UptLogger.Error("Preset is NULL");
                return false;
            }

            var storage = UserSettings.Instance;
            var modified = false;
            var log = $"Changed textures for build target <b><color={UserSettings.DEBUG_HIGHLIGHT_COLOR}>{storage.SelectedPlatform.BuildTargetGroup}</color></b>:";

            foreach (var directoryPath in directoryPaths)
            {
                if (!Directory.Exists(directoryPath))
                    continue;

                var assetPaths = Directory.GetFiles(directoryPath);
                foreach (var assetPath in assetPaths)
                {
                    var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                    if (importer == null)
                        continue;

                    var settings = importer.GetPlatformTextureSettings(storage.SelectedPlatform.Name);
                    if (TextureProcessor.ApplyPresetToPlatformTextureSettings(settings, preset, out var resultLog))
                    {
                        importer.SetPlatformTextureSettings(settings);
                        importer.SaveAndReimport();
                        modified = true;

                        if (resultLog != null)
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
