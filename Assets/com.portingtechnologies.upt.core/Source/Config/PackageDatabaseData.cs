using System;
using System.Collections.Generic;
using UnityEngine;

namespace UPT.Core
{
    public class PackageDatabaseData : ScriptableObject
    {
        [SerializeField] private List<PackagePair> m_packages;

        public IReadOnlyList<PackagePair> Packages => m_packages;
    }

    [Serializable]
    public class PackageData
    {
        public string Name;
        public string DisplayName;
        public string Url;

        [NonSerialized] public string Version;
        [NonSerialized] public PackageStatus Status;
        [NonSerialized] public string StatusMessage;
    }

    [Serializable]
    public class PackagePair
    {
        public PackageData ModulePackage;
        public PackageData ExternalPackage;
    }

    public enum PackageStatus
    {
        NotInstalled,
        Installing,
        Installed,
        Failed,
    }
}
