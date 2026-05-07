using System.Collections.Generic;

namespace UPT.Core
{
    public static class Constants
    {
        public static string PackageNamePrefix = "com.portingtechnologies.upt.";
        public static string ResourcesFolderRoot = "UPTFramework";
        public static string ServiceCollectionsResourceFolderRoot = "ServiceCollections";
        public static string AssetConverterUserDataFileName = "ACUserData";

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

        public static class Locale
        {
            public static string Arabic = "ar";
            public static string Bulgarian = "bg";
            public static string SimplifedChinise = "zh-hans";
            public static string TraditionalChinise = "zh-hant";
            public static string Czech = "cs";
            public static string Danish = "da";
            public static string Dutch = "nl";
            public static string English = "en";
            public static string Filipino = "fil";
            public static string Finnish = "fi";
            public static string French = "fr";
            public static string German = "de";
            public static string Greek = "el";
            public static string Hindi = "hi";
            public static string Hungarian = "hu";
            public static string Indonesian = "id";
            public static string Italian = "it";
            public static string Japanese = "ja";
            public static string Korean = "ru";
            public static string Malay = "ms";
            public static string Norwegian = "no";
            public static string Polish = "pl";
            public static string Portuguese = "pt";
            public static string BrazilianPortuguese = "pt-br";
            public static string Romanian = "ro";
            public static string Russian = "ru";
            public static string Spanish = "es";
            public static string LatinSpanish = "es-419";
            public static string Swedish = "sv";
            public static string Thai = "th";
            public static string Turkish = "tr";
            public static string Ukrainian = "uk";
            public static string Vietnamese = "vi";
        }
    }
}
