using UnityEditor;
using UnityEngine;

namespace UPT.Core.AssetConverter
{
    public class AudioTreeView : AssetTreeView
    {
        public AudioTreeView(bool overrideOption) : base(typeof(AudioClip), overrideOption)
        {
            Reload();
            ExpandAll();
        }

        protected override string GetAssetIconName()
        {
            return "AudioImporter Icon";
        }

        protected override bool IsValidAsset(string path)
        {
            return AssetImporter.GetAtPath(path) as AudioImporter != null;
        }
    }
}
