using UnityEngine;

namespace UPT.Core.AssetConverter
{
    public class AudioSettingsStandalone : IPlatformAudioSettings
    {
        private readonly AudioCompressionFormat[] m_compressionFormats = new[]
        {
            AudioCompressionFormat.PCM,
            AudioCompressionFormat.Vorbis,
            AudioCompressionFormat.ADPCM
        };

        public AudioCompressionFormat[] CompressionFormats => m_compressionFormats;
    }
}
