using UnityEditor;
using UnityEngine;

namespace UPT.Core.AssetConverter
{
    public class TextureTreeView : AssetTreeView
    {
        private TextureImporterType m_importerType;
        private TextureImporterShape m_importerShape;
        private AlphaSourcePresence? m_alphaSource;

        public TextureTreeView(TextureType textureType, AlphaSourcePresence? alphaSource, bool overrideOption) : base(typeof(Texture), overrideOption)
        {
            (m_importerType, m_importerShape) = Utils.ConvertToTextureImporter(textureType);
            m_alphaSource = alphaSource;
            Reload();
            ExpandAll();
        }

        protected override string GetAssetIconName()
        {
            switch (m_importerType)
            {
                case TextureImporterType.Default:
                {
                    switch (m_importerShape)
                    {
                        case TextureImporterShape.Texture2D: return "Texture Icon";
                        case TextureImporterShape.TextureCube: return "Cubemap Icon";
                        default: return null;
                    }
                }
                case TextureImporterType.Sprite: return "Sprite Icon";
                case TextureImporterType.Lightmap: return "Texture Icon";
                default: return null;
            }
        }

        protected override bool IsValidAsset(string path)
        {
            var textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            if (textureImporter == null)
                return false;

            if (textureImporter.textureType != m_importerType)
                return false;

            if (textureImporter.textureType is TextureImporterType.Default && textureImporter.textureShape != m_importerShape)
                return false;

            if (m_alphaSource != null)
            {
                var alphaSource = Utils.ConvertToAlphaSourcePresence(textureImporter.alphaSource);
                if (m_alphaSource.GetValueOrDefault() != alphaSource)
                    return false;
            }

            return true;
        }
    }
}
