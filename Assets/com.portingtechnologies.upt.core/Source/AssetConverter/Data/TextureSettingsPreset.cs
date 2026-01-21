using System;
using UnityEditor;

namespace UPT.Core.AssetConverter
{
    [Serializable]
    public class TextureSettingsPreset : IImporterSettingsPreset
    {
        public AssetType AssetType => AssetType.Texture;
        public bool AutomaticallyApplyForNew { get; set; }

        public bool FilterByAlphaSource = false;

        public bool MaxSizeOverride = false;
        public int MaxSize = 2048;

        public bool FormatOverride = false;
        public TextureImporterFormat Format = TextureImporterFormat.RGBA32;

        public bool CompressorQualityOverride = false;
        public int CompressorQuality = 50;

        public bool OverrideETC2FallbackOverride = false;
        public AndroidETC2FallbackOverride OverrideETS2Fallback = AndroidETC2FallbackOverride.UseBuildSettings;
    }

    public enum TextureType
    {
        Default2D,
        DefaultCubemap,
        Lightmap,
        Sprite
    }

    public enum AlphaSourcePresence
    {
        None,
        Assigned
    }
}