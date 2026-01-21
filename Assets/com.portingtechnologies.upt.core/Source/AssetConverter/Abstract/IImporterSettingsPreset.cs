namespace UPT.Core.AssetConverter
{
    public interface IImporterSettingsPreset
    {
        public AssetType AssetType { get; }
        public bool AutomaticallyApplyForNew { get; set; }
    }
}