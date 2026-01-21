using UnityEditor;

namespace UPT.Core.AssetConverter
{
    public class TextureSettingsStandalone : IPlatformTextureSettings
    {
        private TextureImporterFormat[] m_textureFormats = new[]
        {
        TextureImporterFormat.BC7,
        TextureImporterFormat.BC4,
        TextureImporterFormat.BC6H,
        TextureImporterFormat.DXT5,
        TextureImporterFormat.DXT5Crunched,
        TextureImporterFormat.DXT1,
        TextureImporterFormat.DXT1Crunched,
        TextureImporterFormat.RGBA64,
        TextureImporterFormat.RGBA32,
        TextureImporterFormat.ARGB16,
        TextureImporterFormat.RGB48,
        TextureImporterFormat.RGB16,
        TextureImporterFormat.RGB24,
        TextureImporterFormat.RG32,
        TextureImporterFormat.R8,
        TextureImporterFormat.R16,
        TextureImporterFormat.Alpha8,
        TextureImporterFormat.RGBAFloat,
        TextureImporterFormat.RGBAHalf,
        TextureImporterFormat.RGFloat,
        TextureImporterFormat.RFloat,
        TextureImporterFormat.RGB9E5
    };

        public TextureImporterFormat[] TextureFormats => m_textureFormats;

        public AndroidETC2FallbackOverride[] AndroidETC2FallbackOverrides => null;
    }
}
