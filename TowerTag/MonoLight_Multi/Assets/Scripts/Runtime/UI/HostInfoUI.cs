using System;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.UI
{
    public class HostInfoUI : MonoBehaviour
    {
        [SerializeField] private Text _displayText;

        private void OnEnable()
        {
            StartCoroutine(UpdateTag());
            ConnectionManager.Instance.MasterClientSwitched += OnMasterClientSwitched;
        }

        private void OnDisable()
        {
            StartCoroutine(UpdateTag());
            if (ConnectionManager.Instance != null)
                ConnectionManager.Instance.MasterClientSwitched -= OnMasterClientSwitched;
        }

        private void OnMasterClientSwitched(ConnectionManager connectionManager, Photon.Realtime.Player player)
        {
            StartCoroutine(UpdateTag());
        }

        private IEnumerator UpdateTag()
        {
            yield return new WaitForSeconds(1);
            _displayText.text = PhotonNetwork.IsMasterClient
                ? "YOU'RE HOST. YOU CAN ADD & KICK BOTS!"
                : $"{PhotonNetwork.CurrentRoom.CustomProperties[RoomPropertyKeys.HostName].ToString().ToUpperInvariant()} IS HOST!";
        }
    }
}