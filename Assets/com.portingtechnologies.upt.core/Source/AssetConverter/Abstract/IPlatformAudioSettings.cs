using UnityEngine;

namespace UPT.Core.AssetConverter
{
    public interface IPlatformAudioSettings
    {
        public AudioCompressionFormat[] CompressionFormats { get; }
    }
}