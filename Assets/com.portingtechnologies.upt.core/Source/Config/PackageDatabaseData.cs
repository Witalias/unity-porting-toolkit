using System;
using System.Collections.Generic;
using UnityEngine;

namespace UPT.Core
{
    [CreateAssetMenu(fileName = "PackageDatabaseData", menuName = "Scriptable Objects/PackageDatabaseData")]
    public class PackageDatabaseData : ScriptableObject
    {
        [SerializeField] private List<PackageData> m_packages;

        public IReadOnlyList<PackageData> Packages => m_packages;
    }

    [Serializable]
    public class PackageData
    {
        public string Name;
        public string DisplayName;
        public string Url;
        public string Version = "main";

        [NonSerialized] public PackageStatus Status;
        [NonSerialized] public string StatusMessage;
    }

    public enum PackageStatus
    {
        NotInstalled,
        Installing,
        Installed,
        Failed,
    }
}
