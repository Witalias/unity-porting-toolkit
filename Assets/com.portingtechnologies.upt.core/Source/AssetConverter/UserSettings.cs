using JetBrains.Annotations;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace UPT.Core.AssetConverter
{
    public class UserSettings
    {
        public const string DEBUG_HIGHLIGHT_COLOR = "#009e37";

        private static UserSettings s_instance;

        public static UserSettings Instance
        {
            get
            {
                s_instance ??= new UserSettings();
                return s_instance;
            }
        }

        [System.Serializable]
        public class TexturePreset
        {
            public TextureType TextureType;
            public BuildTargetGroup BuildTargetGroup;
            public AlphaSourcePresence AlphaSource;
            public TextureSettingsPreset Preset;
        }

        [System.Serializable]
        public class AssetData
        {
            public BuildTargetGroup BuildTargetGroup;
            public bool OverrideSettings = true;
            public string Path;

            [JsonIgnore] public Object Asset { get; }

            public AssetData(BuildTargetGroup platform, Object asset)
            {
                BuildTargetGroup = platform;
                Asset = asset;
                Path = AssetDatabase.GetAssetPath(asset);
            }
        }

        [JsonProperty] private readonly Dictionary<BuildTargetGroup, Dictionary<string, AssetData>> m_assetDataDict = new();
        [JsonProperty] private readonly Dictionary<BuildTargetGroup, Dictionary<TextureType, Dictionary<AlphaSourcePresence, TextureSettingsPreset>>> m_textureSettingsPresetsDict = new();
        [JsonProperty] private readonly Dictionary<BuildTargetGroup, AudioSettingsPreset> m_audioSettingsPresetsDict = new();
        [JsonProperty] private MeshSettingsPreset m_meshSettingsPreset = new();
        [JsonProperty] private readonly Dictionary<BuildTargetGroup, Dictionary<AssetType, List<AssetPresetByDirectory>>> m_settingsPresetsByDirectoriesDict = new();
        [JsonProperty] private readonly Dictionary<BuildTargetGroup, Dictionary<OptimizationSection, OptimizationModuleFoldoutData>> m_optimizationModuleFoldoutDataDict = new();

        [HideInInspector] public Tab SelectedTab;
        [HideInInspector, JsonIgnore] public PlatformData SelectedPlatform;
        [HideInInspector] public TextureImporterType SelectedTextureType;
        [HideInInspector] public TextureImporterShape SelectedTextureShape = TextureImporterShape.Texture2D;
        [HideInInspector] public AlphaSourcePresence SelectedAlphaSource;
        [HideInInspector] public bool FoldoutTextureDefaultPresets;
        [HideInInspector] public bool FoldoutTextureOverrideByDirectories;

        [JsonProperty] private BuildTargetGroup m_selectedTargetGroup;

        [CanBeNull]
        public TextureSettingsPreset GetTextureSettings(TextureType type, AlphaSourcePresence alphaSource, BuildTargetGroup platform)
        {
            if (m_textureSettingsPresetsDict == null || m_textureSettingsPresetsDict.Count == 0)
            {
                var platforms = PlatformData.Platforms;
                var textureTypes = System.Enum.GetValues(typeof(TextureType));
                var alphaSourceTypes = System.Enum.GetValues(typeof(AlphaSourcePresence));

                var presets = new List<TexturePreset>(textureTypes.Length * alphaSourceTypes.Length * platforms.Length);

                for (var i = 0; i < platforms.Length; i++)
                {
                    var buildGroup = platforms[i].BuildTargetGroup;
                    for (var j = 0; j < textureTypes.Length; j++)
                    {
                        var textureType = (TextureType)textureTypes.GetValue(j);
                        for (var k = 0; k < alphaSourceTypes.Length; k++)
                        {
                            var alphaSourcePresence = (AlphaSourcePresence)alphaSourceTypes.GetValue(k);
                            var texturePreset = new TexturePreset
                            {
                                TextureType = textureType,
                                BuildTargetGroup = buildGroup,
                                AlphaSource = alphaSourcePresence,
                                Preset = new TextureSettingsPreset()
                            };
                            presets.Add(texturePreset);
                        }
                    }
                }

                foreach (var data in presets)
                {
                    m_textureSettingsPresetsDict.TryAdd(data.BuildTargetGroup, new Dictionary<TextureType, Dictionary<AlphaSourcePresence, TextureSettingsPreset>>());
                    m_textureSettingsPresetsDict[data.BuildTargetGroup].TryAdd(data.TextureType, new Dictionary<AlphaSourcePresence, TextureSettingsPreset>());
                    m_textureSettingsPresetsDict[data.BuildTargetGroup][data.TextureType].TryAdd(data.AlphaSource, data.Preset);
                }
            }

            if (m_textureSettingsPresetsDict.TryGetValue(platform, out var textureTypeToDict) &&
                textureTypeToDict.TryGetValue(type, out var alphaSourceToPreset) &&
                alphaSourceToPreset.TryGetValue(alphaSource, out var preset))
            {
                return preset;
            }

            return null;
        }

        [CanBeNull]
        public TextureSettingsPreset GetTextureSettings(TextureType type, AlphaSourcePresence alphaSource) => 
            GetTextureSettings(type, alphaSource, SelectedPlatform.BuildTargetGroup);

        public void OnFilterByAlphaSourceChanged(bool value)
        {
            var textureType = Utils.ConvertToTextureType(SelectedTextureType, SelectedTextureShape);
            foreach (var preset in m_textureSettingsPresetsDict[SelectedPlatform.BuildTargetGroup][textureType].Values)
                preset.FilterByAlphaSource = value;
        }

        [NotNull]
        public AudioSettingsPreset GetAudioSettings(BuildTargetGroup platform)
        {
            if (!m_audioSettingsPresetsDict.TryGetValue(platform, out var preset))
            {
                preset = new();
                m_audioSettingsPresetsDict.Add(platform, preset);
            }
            return preset;
        }

        [NotNull]
        public AudioSettingsPreset GetAudioSettings() => GetAudioSettings(SelectedPlatform.BuildTargetGroup);

        [NotNull]
        public MeshSettingsPreset GetMeshSettings() => m_meshSettingsPreset;

        [NotNull]
        public OptimizationModuleFoldoutData GetOptimizationModuleFoldoutData(OptimizationSection section)
        {
            if (!m_optimizationModuleFoldoutDataDict.TryGetValue(SelectedPlatform.BuildTargetGroup, out var sectionToFoldoutData))
            {
                sectionToFoldoutData = new();
                m_optimizationModuleFoldoutDataDict.Add(SelectedPlatform.BuildTargetGroup, sectionToFoldoutData);
            }
            if (!sectionToFoldoutData.TryGetValue(section, out var foldoutData))
            {
                foldoutData = new();
                sectionToFoldoutData.Add(section, foldoutData);
            }
            return foldoutData;
        }

        [NotNull]
        public List<AssetPresetByDirectory> GetPresetsByDirectories(BuildTargetGroup platform, AssetType assetType)
        {
            if (!m_settingsPresetsByDirectoriesDict.TryGetValue(platform, out var assetTypeToPresets))
            {
                assetTypeToPresets = new();
                m_settingsPresetsByDirectoriesDict.Add(platform, assetTypeToPresets);
            }

            if (!m_settingsPresetsByDirectoriesDict[platform].TryGetValue(assetType, out var presets))
            {
                presets = new();
                m_settingsPresetsByDirectoriesDict[platform].Add(assetType, presets);
            }

            return presets;
        }

        [NotNull]
        public List<AssetPresetByDirectory> GetPresetsByDirectories(AssetType assetType)
        {
            return GetPresetsByDirectories(SelectedPlatform.BuildTargetGroup, assetType);
        }

        public void AddSettingsPresetByDirectory(AssetPresetByDirectory preset)
        {
            m_settingsPresetsByDirectoriesDict.TryAdd(preset.BuildTargetGroup, new());
            m_settingsPresetsByDirectoriesDict[preset.BuildTargetGroup].TryAdd(preset.Preset.AssetType, new());
            m_settingsPresetsByDirectoriesDict[preset.BuildTargetGroup][preset.Preset.AssetType].Add(preset);
        }

        public void RemoveSettingsPresetByDirectory(AssetPresetByDirectory preset)
        {
            if (m_settingsPresetsByDirectoriesDict.TryGetValue(preset.BuildTargetGroup, out var assetTypeToPresets) &&
                assetTypeToPresets.TryGetValue(preset.Preset.AssetType, out var presets) &&
                presets.Contains(preset))
            {
                presets.Remove(preset);
            }
        }

        [CanBeNull]
        public AssetData GetAssetData(string path)
        {
            if (!m_assetDataDict.TryGetValue(SelectedPlatform.BuildTargetGroup, out var pathsToData))
            {
                pathsToData = new Dictionary<string, AssetData>();
                m_assetDataDict.Add(SelectedPlatform.BuildTargetGroup, pathsToData);
            }

            if (!pathsToData.TryGetValue(path, out var data))
            {
                var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (asset != null)
                {
                    data = new AssetData(SelectedPlatform.BuildTargetGroup, asset);
                    pathsToData.Add(path, data);
                }
            }

            return data;
        }

        public void UpdateAssetData(ITreeItem treeItem)
        {
            if (treeItem == null)
                return;

            var path = AssetDatabase.GetAssetPath(treeItem.Source);
            if (path == null)
                return;

            if (!m_assetDataDict.TryGetValue(SelectedPlatform.BuildTargetGroup, out var pathsToData))
            {
                pathsToData = new Dictionary<string, AssetData>();
                m_assetDataDict.Add(SelectedPlatform.BuildTargetGroup, pathsToData);
            }

            if (pathsToData.TryGetValue(path, out var data))
            {
                data.OverrideSettings = treeItem.Override;
            }
            else
            {
                var assetData = new AssetData(SelectedPlatform.BuildTargetGroup, treeItem.Source)
                {
                    OverrideSettings = treeItem.Override,
                };
                pathsToData.Add(path, assetData);
            }
        }

        public void RemoveAssetData(string path)
        {
            foreach (var platform in m_assetDataDict.Keys)
            {
                if (m_assetDataDict[platform].ContainsKey(path))
                    m_assetDataDict[SelectedPlatform.BuildTargetGroup].Remove(path);
            }
        }

        public void ResetSettings()
        {
            m_textureSettingsPresetsDict.Clear();
            m_audioSettingsPresetsDict.Clear();
            m_meshSettingsPreset = new();
            m_assetDataDict.Clear();
            m_settingsPresetsByDirectoriesDict.Clear();
            m_optimizationModuleFoldoutDataDict.Clear();

            var path = $"{Constants.AssetConverterUserDataFileName}.json";
            ResourceManager.CreateJsonFile(path, string.Empty);
        }

        public void OnDisableTool()
        {
            WriteOnDisk();
            foreach (var assetTypeToPrsets in m_settingsPresetsByDirectoriesDict.Values)
            {
                foreach (var presets in assetTypeToPrsets.Values)
                {
                    foreach (var preset in presets)
                    {
                        preset.EditorSettingsApplied = false;
                        preset.EditorWasModifiedTextures = false;
                    }
                }
            }
        }

        public void WriteOnDisk()
        {
            if (SelectedPlatform != null)
                m_selectedTargetGroup = SelectedPlatform.BuildTargetGroup;

            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            var path = $"{Constants.AssetConverterUserDataFileName}.json";

            ResourceManager.CreateJsonFile(path, json);
        }

        public void LoadFromDisk()
        {
            var path = $"{Constants.AssetConverterUserDataFileName}.json";
            var json = ResourceManager.LoadFile(path);

            if (json == null)
            {
                ResetSettings();
                return;
            }

            s_instance = JsonConvert.DeserializeObject<UserSettings>(json);
            SelectedPlatform = PlatformData.Platforms.Where(platform => platform.BuildTargetGroup == m_selectedTargetGroup).FirstOrDefault();
        }
    }

    public enum AssetType
    {
        Texture,
        Audio,
        Mesh
    }
}