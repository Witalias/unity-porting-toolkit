using UnityEngine;

namespace UPT.Core.AssetConverter
{
    public class AudioSettingsSwitch : IPlatformAudioSettings
    {
        private readonly AudioCompressionFormat[] m_compressionFormats = new[]
        {
            AudioCompressionFormat.PCM,
            AudioCompressionFormat.Vorbis,
            AudioCompressionFormat.ADPCM,
            AudioCompressionFormat.MP3,
        };

        public AudioCompressionFormat[] CompressionFormats => m_compressionFormats;
    }
}
