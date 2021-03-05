using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Home.UI
{
    public class KeyboardReferences : MonoBehaviour
    {
        [SerializeField] private Button _enterButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private TMP_Text _title;

        public Button EnterButton => _enterButton;
        public TMP_InputField InputField => _inputField;

        public TMP_Text Title => _title;

        public Button CloseButton => _closeButton;
    }
}