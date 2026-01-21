using UnityEditor;
using UnityEngine;

namespace UPT.Core.AssetConverter
{
    public class StreamingAudioTreeView : AudioTreeView
    {
        public StreamingAudioTreeView() : base(false) { }

        protected override bool IsValidAsset(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as AudioImporter;
            if (importer == null)
                return false;

            var settings = importer.GetOverrideSampleSettings(UserSettings.Instance.SelectedPlatform.Name);
            return settings.loadType == AudioClipLoadType.Streaming;
        }
    }
}
