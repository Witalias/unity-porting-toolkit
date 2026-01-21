using UnityEditor;
using UnityEngine;

namespace UPT.Core.AssetConverter
{
    public class MeshTreeView : AssetTreeView
    {
        public MeshTreeView(bool overrideOption) : base(typeof(Mesh), overrideOption)
        {
            Reload();
            ExpandAll();
        }

        protected override string GetAssetIconName()
        {
            return "MeshFilter Icon";
        }

        protected override bool IsValidAsset(string path)
        {
            return AssetImporter.GetAtPath(path) as ModelImporter != null;
        }
    }
}
