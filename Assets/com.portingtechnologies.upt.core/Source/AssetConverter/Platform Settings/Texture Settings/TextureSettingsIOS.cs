using UnityEditor;

namespace UPT.Core.AssetConverter
{
    public class TextureSettingsIOS : IPlatformTextureSettings
    {
        private readonly TextureImporterFormat[] m_textureFormats = new[]
        {
        TextureImporterFormat.ASTC_12x12,
        TextureImporterFormat.ASTC_10x10,
        TextureImporterFormat.ASTC_8x8,
        TextureImporterFormat.ASTC_6x6,
        TextureImporterFormat.ASTC_5x5,
        TextureImporterFormat.ASTC_4x4,
        TextureImporterFormat.ASTC_HDR_12x12,
        TextureImporterFormat.ASTC_HDR_10x10,
        TextureImporterFormat.ASTC_HDR_8x8,
        TextureImporterFormat.ASTC_HDR_6x6,
        TextureImporterFormat.ASTC_HDR_5x5,
        TextureImporterFormat.ASTC_HDR_4x4,
        TextureImporterFormat.ETC2_RGBA8,
        TextureImporterFormat.ETC2_RGBA8Crunched,
        TextureImporterFormat.ETC2_RGB4,
        TextureImporterFormat.ETC2_RGB4_PUNCHTHROUGH_ALPHA,
        TextureImporterFormat.ETC_RGB4,
        TextureImporterFormat.ETC_RGB4Crunched,
        TextureImporterFormat.PVRTC_RGBA4,
        TextureImporterFormat.PVRTC_RGBA2,
        TextureImporterFormat.PVRTC_RGB4,
        TextureImporterFormat.PVRTC_RGB2,
        TextureImporterFormat.RGBA64,
        TextureImporterFormat.RGBA32,
        TextureImporterFormat.RGBA16,
        TextureImporterFormat.RGB48,
        TextureImporterFormat.RGB24,
        TextureImporterFormat.RGB16,
        TextureImporterFormat.RG32,
        TextureImporterFormat.R8,
        TextureImporterFormat.R16,
        TextureImporterFormat.Alpha8,
        TextureImporterFormat.RGBAFloat,
        TextureImporterFormat.RGBAHalf,
        TextureImporterFormat.RGFloat,
        TextureImporterFormat.RFloat,
        TextureImporterFormat.BC6H,
        TextureImporterFormat.RGB9E5,
        TextureImporterFormat.EAC_RG,
        TextureImporterFormat.EAC_R,
    };

        public TextureImporterFormat[] TextureFormats => m_textureFormats;

        public AndroidETC2FallbackOverride[] AndroidETC2FallbackOverrides => null;
    }

}