using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace UPT.Core.AssetConverter
{
    public class AssetProcessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            var storage = UserSettings.Instance;

            foreach (var path in deletedAssets)
                storage.RemoveAssetData(path);

            var presets = new List<AssetPresetByDirectory>();
            var assetTypes = Enum.GetValues(typeof(AssetType));
            foreach (var platform in PlatformData.Platforms)
            {
                foreach (var type in assetTypes)
                    presets.AddRange(storage.GetPresetsByDirectories(platform.BuildTargetGroup, (AssetType)type));
            }

            if (movedAssets.Length > 0)
            {
                foreach (var preset in presets)
                {
                    if (!preset.AutomationAssistant)
                        continue;

                    for (var i = 0; i < movedFromAssetPaths.Length; i++)
                    {
                        for (var j = 0; j < preset.DirectoryPaths.Count; j++)
                        {
                            if (preset.DirectoryPaths[j].Equals(movedFromAssetPaths[i], System.StringComparison.OrdinalIgnoreCase))
                                preset.DirectoryPaths[j] = movedAssets[i];
                        }
                    }
                }
            }

            importedAssets = importedAssets.Where(asset => System.IO.Directory.Exists(asset)).Except(movedAssets).ToArray();

            if (importedAssets.Length > 0)
            {
                foreach (var preset in presets)
                {
                    if (!preset.AutoNewSubdirectories)
                        continue;

                    foreach (var asset in importedAssets)
                    {
                        for (var i = 0; i < preset.DirectoryPaths.Count; i++)
                        {
                            if (asset.StartsWith(preset.DirectoryPaths[i], System.StringComparison.OrdinalIgnoreCase) && !preset.DirectoryPaths.Contains(asset, Utils.s_pathComparer))
                            {
                                preset.DirectoryPaths.Add(asset);
                                break;
                            }
                        }
                    }
                }
            }

            if (deletedAssets.Length > 0)
            {
                foreach (var preset in presets)
                {
                    if (preset.AutomationAssistant)
                        preset.DirectoryPaths = Utils.RemoveSubdirectories(preset.DirectoryPaths, deletedAssets);
                }
            }
        }
    }
}
