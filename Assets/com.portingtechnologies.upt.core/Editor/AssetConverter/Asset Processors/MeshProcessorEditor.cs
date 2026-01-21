using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UPT.Core.AssetConverter
{
    public class MeshProcessorEditor
    {
        public bool ApplySettingsPreset(MeshSettingsPreset preset)
        {
            if (preset == null)
            {
                UptLogger.Error("Preset is NULL");
                return false;
            }

            var storage = UserSettings.Instance;
            var modified = false;
            var log = "Changed models:";
            var guids = AssetDatabase.FindAssets("t:Model", new[] { "Assets" });
            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null)
                    continue;

                var assetData = storage.GetAssetData(path);
                if (assetData == null || !assetData.OverrideSettings)
                    continue;

                if (MeshProcessor.ApplyPresetToMeshImporter(importer, preset, out var resultLog))
                {
                    importer.SaveAndReimport();
                    modified = true;
                    log += $"\n<b>{path}</b>. {resultLog}";
                }
            }

            if (modified)
                UptLogger.Info(log + '\n');

            return modified;
        }

        public bool ApplySettingsPreset(IReadOnlyCollection<string> directoryPaths, MeshSettingsPreset preset)
        {
            if (preset == null)
            {
                UptLogger.Error("Preset is NULL");
                return false;
            }

            var modified = false;
            var log = "Changed models:";

            foreach (var directoryPath in directoryPaths)
            {
                if (!Directory.Exists(directoryPath))
                    continue;

                var assetPaths = Directory.GetFiles(directoryPath);
                foreach (var assetPath in assetPaths)
                {
                    var importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
                    if (importer == null)
                        continue;

                    if (MeshProcessor.ApplyPresetToMeshImporter(importer, preset, out var resultLog))
                    {
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