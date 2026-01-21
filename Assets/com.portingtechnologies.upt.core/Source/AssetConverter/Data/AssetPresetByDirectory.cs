using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace UPT.Core.AssetConverter
{
    [Serializable]
    public class AssetPresetByDirectory
    {
        public BuildTargetGroup BuildTargetGroup;
        public List<string> DirectoryPaths = new() { "Assets/" };

        [JsonConverter(typeof(ImporterSettingsPresetJsonConverter))]
        public IImporterSettingsPreset Preset;

        public bool AutoNewSubdirectories = false;
        public bool AutomationAssistant = true;
        public bool IsCorrectDirectories = true;

        public bool EditorFoldoutDirectoryPaths = true;
        public bool EditorFoldoutPreset = true;
        [JsonIgnore] public bool EditorSettingsApplied = false;
        [JsonIgnore] public bool EditorWasModifiedTextures = false;

        public AssetPresetByDirectory(IImporterSettingsPreset preset)
        {
            Preset = preset;
        }
    }

    public class ImporterSettingsPresetJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(IImporterSettingsPreset).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = Newtonsoft.Json.Linq.JObject.Load(reader);
            var assetType = (AssetType)Enum.Parse(typeof(AssetType), jsonObject["AssetType"].ToString());
            Type concreteType = assetType switch
            {
                AssetType.Texture => typeof(TextureSettingsPreset),
                AssetType.Audio => typeof(AudioSettingsPreset),
                AssetType.Mesh => typeof(MeshSettingsPreset),
                _ => throw new NotSupportedException($"Unknown AssetType: {assetType}")
            };
            return jsonObject.ToObject(concreteType, serializer);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
