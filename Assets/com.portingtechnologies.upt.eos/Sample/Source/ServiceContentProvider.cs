using UnityEngine;
using UPT.Core.Samples;

namespace UPT.EOS.Samples
{
    public class ServiceContentProvider
    {
        [SampleContentProvider("EOS Authentication")]
        public static GameObject GetPrefab()
        {
            var provider = Resources.Load<SampleServiceContainer>("EOSAuthenticationContainer");
            return provider.Prefab;
        }
    }
}
