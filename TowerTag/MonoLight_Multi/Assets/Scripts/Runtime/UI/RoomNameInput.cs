using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class RoomNameInput : MonoBehaviour {
    private InputField _inputField;
    private Configuration _configuration;

    private void Awake() {
        _configuration = ConfigurationManager.Configuration;
        _inputField = GetComponent<InputField>();
        _inputField.text = _configuration.Room;
    }

    private void OnEnable() {
        _inputField.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnDisable() {
        _inputField.onValueChanged.RemoveListener(OnValueChanged);
    }

    private void OnValueChanged(string roomName) {
        _configuration.Room = roomName;
        ConfigurationManager.WriteConfigToFile();
    }
}