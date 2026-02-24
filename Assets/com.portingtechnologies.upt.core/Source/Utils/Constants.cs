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
    }
}
