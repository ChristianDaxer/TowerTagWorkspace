using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OperatorUIManager : MonoBehaviour {
    [SerializeField] private Toggle _autoStartToggle;
    [SerializeField] private Toggle _allowTeamChangeToggle;
    [SerializeField] private TMP_Dropdown _matchUp;
    [SerializeField] private TMP_Dropdown _maps;

    public void ToggleUserVoteMode(bool value) {
        _autoStartToggle.isOn = false;
        _autoStartToggle.interactable = !value;
        _allowTeamChangeToggle.isOn = false;
        _allowTeamChangeToggle.interactable = value;
        _matchUp.interactable = !value;
        _maps.interactable = !value;
        _maps.value = 0;
    }
}