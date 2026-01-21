using System;
using UnityEditor;

namespace UPT.Core.AssetConverter
{
    [Serializable]
    public class MeshSettingsPreset : IImporterSettingsPreset
    {
        public AssetType AssetType => AssetType.Mesh;
        public bool AutomaticallyApplyForNew { get; set; }

        public bool OverrideCompression;
        public ModelImporterMeshCompression Compression = ModelImporterMeshCompression.Off;
    }
}
