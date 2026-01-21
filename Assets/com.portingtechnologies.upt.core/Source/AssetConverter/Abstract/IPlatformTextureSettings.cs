using UnityEditor;

namespace UPT.Core.AssetConverter
{
    public interface IPlatformTextureSettings
    {
        public TextureImporterFormat[] TextureFormats { get; }
        public AndroidETC2FallbackOverride[] AndroidETC2FallbackOverrides { get; }
    }
}