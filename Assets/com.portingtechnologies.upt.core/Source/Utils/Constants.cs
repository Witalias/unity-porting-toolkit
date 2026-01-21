using System.Collections.Generic;

namespace UPT.Core
{
    public static class Constants
    {
        public static string PackageNamePrefix => "com.portingtechnologies.upt.";
        public static string ResourcesFolderRoot => "UPTFramework";
        public static string ServiceCollectionsResourceFolderRoot => "ServiceCollections";
        public static string AssetConverterUserDataFileName => "ACUserData";

        public static IReadOnlyList<string> SystemAssemblies = new[]
        {
            "mscorlib",
            "System",
            "UnityEngine",
            "UnityEditor",
            "netstandard",
            "Microsoft",
            "nunit"
        };

        public static class Icons
        {
            public static string Trash => "TreeEditor.Trash";
            public static string Refresh => "Refresh";
            public static string Plus => "Toolbar Plus";
            public static string Info => "UnityEditor.InspectorWindow";
            public static string Save => "SaveAs";
            public static string Tools => "CustomTool";
            public static string Settings => "Settings";
            public static string Edit => "editicon.sml";
        }

        public static class EditorGaps
        {
            public static float Padding => 10.0f;
            public static float SectionGap => 10.0f;
            public static float IconButtonWidth => 30.0f;
        }
    }
}
