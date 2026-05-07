using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UPT.Core
{
    public class InputFieldElement : MonoBehaviour
    {
        [field: SerializeField] public TMP_InputField InputField { get; private set; }
        [field: SerializeField] public Button RemoveButton { get; private set; }
    }
}
