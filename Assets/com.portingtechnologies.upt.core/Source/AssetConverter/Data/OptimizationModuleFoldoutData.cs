using System;

namespace UPT.Core.AssetConverter
{
    [Serializable]
    public class OptimizationModuleFoldoutData
    {
        public bool Opened = false;
        public bool Completed = false;
    }

    public enum OptimizationSection
    {
        StreamingAudio,
        LongDecompressOnLoadAudio,
    }
}
