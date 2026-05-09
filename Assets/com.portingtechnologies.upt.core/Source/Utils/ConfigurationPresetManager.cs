using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UPT.Editor;

namespace UPT.Core
{
    public static class ConfigurationPresetManager
    {
        public static void Switch(string presetName)
        {
            var allPresets = ResourceManager.LoadServiceCollections();
            var preset = allPresets.Where(p => p.Name == presetName).FirstOrDefault();

            if (preset == null)
            {
                Debug.LogWarning($"Not found the configuration preset {presetName}");
                return;
            }

            var availableModules = ModuleDiscovery.FindAvailableModules();
            ApplyPreset(preset, allPresets, availableModules);
            UpdatePreprocessorDefinitions(preset, availableModules);
        }

        public static void ApplyPreset(PlatformServiceCollection preset, IList<PlatformServiceCollection> allPresets, IList<IPlatformModule> availableModules)
        {
            foreach (var col in allPresets)
            {
                col.IsActive = false;
                EditorUtility.SetDirty(col);
            }

            preset.IsActive = true;

            EditorUtility.SetDirty(preset);
            AssetDatabase.SaveAssets();
        }

        public static void UpdatePreprocessorDefinitions(PlatformServiceCollection preset, IList<IPlatformModule> availableModules)
        {
            foreach (var module in availableModules)
            {
                var attribute = module.GetType().GetCustomAttribute<PlatformModuleAttribute>();

                if (string.IsNullOrEmpty(attribute.ProprocessorDefinition))
                    continue;

                var enableDefinition = ModuleUtility.IsModuleActive(module, preset, availableModules);
                if (attribute.ProprocessorDefinitionInverted)
                    enableDefinition = !enableDefinition;

                PreprocessorDefinitionManager.Set(NamedBuildTarget.Standalone, attribute.ProprocessorDefinition, enableDefinition);
            }
        }
    }
}
