using UnityEngine;

namespace UPT.Core.Samples
{
    [CreateAssetMenu]
    public class SampleServiceContainer : ScriptableObject
    {
        [SerializeField] private GameObject m_prefab;

        public GameObject Prefab => m_prefab;
    }
}
