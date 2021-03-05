using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class MatchHostUpdater : MonoBehaviour
{
    private void Start()
    {
        if (!ConnectionManager.Instance)
        {
            Debug.LogError("There is no connection manager! Can't start Match Host Updater!");
            return;
        }
        
        ConnectionManager.Instance.MasterClientSwitched += OnMasterClientSwitched;
    }

    private void OnMasterClientSwitched(ConnectionManager connectionManager, Player player)
    {
        if (player == null
            || !player.IsMasterClient
            || !player.IsLocal
            || PhotonNetwork.CurrentRoom == null) return;

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(RoomPropertyKeys.HostName))
        {
            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable
            {
                [RoomPropertyKeys.HostName] = PlayerProfileManager.CurrentPlayerProfile.PlayerName
            });
        }
    }

    private void OnDestroy()
    {
        if(ConnectionManager.Instance != null)
            ConnectionManager.Instance.MasterClientSwitched -= OnMasterClientSwitched;
    }
}