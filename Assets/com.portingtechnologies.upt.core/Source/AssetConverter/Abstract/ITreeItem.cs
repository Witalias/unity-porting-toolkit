using UnityEngine;

namespace UPT.Core.AssetConverter
{
    public interface ITreeItem
    {
        bool Override { get; set; }
        public Object Source { get; }
    }
}