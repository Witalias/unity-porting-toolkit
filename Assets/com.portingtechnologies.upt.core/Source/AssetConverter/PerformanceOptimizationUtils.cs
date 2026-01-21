using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UPT.Core.AssetConverter
{
    public static class PerformanceOptimizationUtils
    {
        /// <summary>
        /// Returns all AudioClip assets in the project with LoadType == Streaming.
        /// </summary>
        public static List<AudioClip> GetStreamingAudioClips()
        {
            var result = new List<AudioClip>();
            string[] guids = AssetDatabase.FindAssets("t:AudioClip");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (audioClip == null)
                    continue;

                var importer = AssetImporter.GetAtPath(path) as AudioImporter;
                if (importer == null)
                    continue;

                var settings = importer.defaultSampleSettings;
                if (settings.loadType == AudioClipLoadType.Streaming)
                {
                    result.Add(audioClip);
                }
            }
            return result;
        }
    }
}