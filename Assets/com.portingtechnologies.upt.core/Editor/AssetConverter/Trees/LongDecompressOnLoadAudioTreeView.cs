using UnityEditor;
using UnityEngine;

namespace UPT.Core.AssetConverter
{
    public class LongDecompressOnLoadAudioTreeView : AudioTreeView
    {
        public LongDecompressOnLoadAudioTreeView() : base(false) { }

        protected override bool IsValidAsset(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as AudioImporter;
            if (importer == null)
                return false;

            var audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (audioClip == null)
                return false;

            var settings = importer.GetOverrideSampleSettings(UserSettings.Instance.SelectedPlatform.Name);
            return settings.loadType == AudioClipLoadType.DecompressOnLoad && audioClip.length >= 30.0f;
        }
    }
}
