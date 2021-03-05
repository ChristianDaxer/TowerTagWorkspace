using PopUpMenu;
using TMPro;
using TowerTag;
using UnityEngine;
using UnityEngine.UI;

public class PlayerOptionButton : MonoBehaviour {
    public IPlayer Player { get; set; }

    private Color _teamColor;
    public Color TeamColor
    {
        set
        {
            _button.gameObject.GetComponentInChildren<TMP_Text>().color = value;
            _teamColor = value;
        }
    }

    private PlayerOption _playerOption;
    public PlayerOption PlayerOption {
        set {
            value.UpdateButtonText(Player);
            _button.gameObject.GetComponentInChildren<TMP_Text>().text = value.ButtonText;
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() =>
                value.OptionOnClick(Player));
            _playerOption = value;
        }
    }

    private Button _button;

    void OnEnable() {
        _button = GetComponent<Button>();

    }

    void OnDisable() {
        _button.onClick.RemoveAllListeners();
    }
}
