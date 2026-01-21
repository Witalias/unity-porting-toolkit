using System;
using UnityEditor;
using UnityEngine;

namespace UPT.Core.AssetConverter
{
    [Serializable]
    public class AudioSettingsPreset : IImporterSettingsPreset
    {
        public AssetType AssetType => AssetType.Audio;
        public bool AutomaticallyApplyForNew { get; set; }

        public bool OverrideLoadType = false;
        public AudioClipLoadType LoadType = AudioClipLoadType.DecompressOnLoad;

        public bool OverrideCompressionFormat = false;
        public AudioCompressionFormat CompressionFormat = AudioCompressionFormat.Vorbis;

        public bool OverrideQuality = false;
        public float Quality = 100.0f;

        public bool OverrideSampleRateSetting = false;
        public AudioSampleRateSetting SampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
        public int SampleRate = 44100;
    }
}