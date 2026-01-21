using System;

namespace UPT.Core.AssetConverter
{
    public interface IImporterSectionEditor
    {
        public AssetTreeView TreeView { get; }
        public IImporterSettingsPreset SettingsPreset { get; }
        public AssetType AssetType { get; }
        public bool HasFilters { get; }
        public bool PlatformOverrides { get; }
        public void DrawFilters(Action updateLayout);
        public void DrawSettingsPreset(Action<bool> onApplied);
        public void DrawSettingsPreset(AssetPresetByDirectory presetByDirectory, Action<bool> onApplied);
    }
}
