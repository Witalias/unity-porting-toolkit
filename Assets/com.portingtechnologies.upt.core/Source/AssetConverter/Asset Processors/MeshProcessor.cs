using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UPT.Core.AssetConverter
{
    public class MeshProcessor : AssetPostprocessor
    {
        private void OnPreprocessModel()
        {
            var importer = assetImporter as ModelImporter;
            var directory = Path.GetDirectoryName(importer.assetPath).Replace('\\', '/');
            var storage = UserSettings.Instance;
            var note = "\n\n<b>NOTE:</b> Follow <i>Tool —> Porting Tool —> Mesh Importer</i> to change the settings.\n";

            var presetsByDirectories = storage.GetPresetsByDirectories(BuildTargetGroup.Standalone, AssetType.Mesh);
            foreach (var preset in presetsByDirectories)
            {
                if (preset.Preset is MeshSettingsPreset meshSettingsPreset)
                {
                    if (meshSettingsPreset.AutomaticallyApplyForNew && preset.DirectoryPaths.Any(path => directory.Equals(path, System.StringComparison.OrdinalIgnoreCase)))
                    {
                        if (ApplyPresetToMeshImporter(importer, meshSettingsPreset, out var outLog))
                        {
                            var log = $"The settings for the model <b>{importer.assetPath}</b> are set automatically | <b>Override By Directories</b>:\n{outLog}";
                            log += note;
                            UptLogger.Info(log);
                        }
                        return;
                    }
                }
            }

            var meshSettings = storage.GetMeshSettings();
            if (!meshSettings.AutomaticallyApplyForNew)
                return;

            if (ApplyPresetToMeshImporter(importer, meshSettings, out var resultLog))
            {
                var log = $"The settings for the model <b>{importer.assetPath}</b> are set automatically | <b>Default Preset</b>:\n{resultLog}";
                UptLogger.Info(log);
            }
        }

        public static bool ApplyPresetToMeshImporter(ModelImporter importer, MeshSettingsPreset preset, [CanBeNull] out string log)
        {
            var modified = false;
            log = string.Empty;

            if (preset.OverrideCompression && importer.meshCompression != preset.Compression)
            {
                log += $"Mesh Compression: {importer.meshCompression} —> {preset.Compression}. ";
                importer.meshCompression = preset.Compression;
                modified = true;
            }

            if (!modified)
                log = null;

            return modified;
        }
    }
}
