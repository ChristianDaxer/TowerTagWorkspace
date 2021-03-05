using System.Collections;
using JetBrains.Annotations;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class LobbySpectatorUiController : MonoBehaviour {

    [SerializeField] private Button _joinButton;
    [SerializeField] private GameObject _connectLobby;
    [SerializeField] private GameObject _findMatchLayout;

    private void OnEnable() {
        if(!PhotonNetwork.InLobby)
            StartCoroutine(WaitForLobby());
        else
        {
            ToggleSpectatorPanel(true);
        }
    }

    private void OnDisable() {
        StopAllCoroutines();
    }

    private IEnumerator WaitForLobby() {
        while (!PhotonNetwork.InLobby)
        {
            yield return null;
        }

        JoinedLobby();
    }

    [UsedImplicitly]
    public void OnRegionDropDownPressed() {
        ToggleSpectatorPanel(PhotonNetwork.InLobby);
        StartCoroutine(WaitForLobby());
    }

    private void JoinedLobby() {
        ToggleSpectatorPanel(PhotonNetwork.InLobby);
    }

    private void ToggleSpectatorPanel(bool isInLobby) {
        _connectLobby.SetActive(!isInLobby);
        _findMatchLayout.SetActive(isInLobby);
        if(!isInLobby)
            _joinButton.interactable = false;
    }
}
