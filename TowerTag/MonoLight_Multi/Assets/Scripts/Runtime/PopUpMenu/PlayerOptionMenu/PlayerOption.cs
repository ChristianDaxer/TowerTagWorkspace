using TowerTag;
using UnityEngine;
using UnityEngine.Serialization;

namespace PopUpMenu {
    public abstract class PlayerOption : ScriptableObject {

        [FormerlySerializedAs("ButtonText")] [SerializeField] private string _buttonText;
        public string ButtonText {
            get => _buttonText;
            protected set => _buttonText = value;
        }

        public abstract void OptionOnClick(IPlayer player);

        public virtual void UpdateButtonText(IPlayer player) {

        }
    }
}
