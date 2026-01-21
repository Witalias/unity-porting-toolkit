namespace UPT.Core.AssetConverter
{
    public class TabData
    {
        public Tab Tab;
        public string Name;
    }

    public enum Tab
    {
        TextureImporter,
        AudioImporter,
        ModelImporeter,
        LicensePacker,
        Optimization,
    }
}