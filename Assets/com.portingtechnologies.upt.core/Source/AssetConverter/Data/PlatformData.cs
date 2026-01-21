using UnityEditor;

namespace UPT.Core.AssetConverter
{
    public class PlatformData
    {
        public string Name;
        public string IconName;
        public string Tooltip;
        public BuildTargetGroup BuildTargetGroup;
        public BuildTarget BuildTarget;
        public IPlatformTextureSettings TextureSettings;
        public IPlatformAudioSettings AudioSettings;

        public static readonly PlatformData[] Platforms = new[]
        {
            new PlatformData
            {
                Name = "Standalone",
                IconName = "BuildSettings.Standalone",
                Tooltip = "Standalone",
                BuildTargetGroup = BuildTargetGroup.Standalone,
                BuildTarget = BuildTarget.StandaloneWindows,
                TextureSettings = new TextureSettingsStandalone(),
                AudioSettings = new AudioSettingsStandalone(),
            },
            new PlatformData 
            { 
                Name = "Android", 
                IconName = "BuildSettings.Android",
                Tooltip = "Android",
                BuildTargetGroup = BuildTargetGroup.Android, 
                BuildTarget = BuildTarget.Android,
                TextureSettings = new TextureSettingsAndroid(),
                AudioSettings = new AudioSettingsAndroid(),
            },
            new PlatformData 
            { 
                Name = "iPhone", 
                IconName = "BuildSettings.iPhone",
                Tooltip = "IOS",
                BuildTargetGroup = BuildTargetGroup.iOS, 
                BuildTarget = BuildTarget.iOS,
                TextureSettings = new TextureSettingsIOS(),
                AudioSettings = new AudioSettingsIOS(),
            },
            new PlatformData 
            { 
                Name = "Switch", 
                IconName = "BuildSettings.Switch",
                Tooltip = "Nintendo Switch",
                BuildTargetGroup = BuildTargetGroup.Switch, 
                BuildTarget = BuildTarget.Switch,
                TextureSettings = new TextureSettingsSwitch(),
                AudioSettings = new AudioSettingsSwitch(),
            },
        };
    }
}