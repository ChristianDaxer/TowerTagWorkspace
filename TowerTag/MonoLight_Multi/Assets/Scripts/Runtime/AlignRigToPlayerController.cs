using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlignRigToPlayerController : MonoBehaviour {

    [SerializeField] private bool alignRotation = true;
    [SerializeField] private bool alignPosition = true;

    private void Awake() {
    }

    private void LateUpdate()
    {
        
        if (PlayerManager.Instance == null)
        {
            Debug.LogErrorFormat("Cannot align camera rig to player, there is no instance of {0} in the scene.", typeof(PlayerManager).FullName);
            return;
        }

        TowerTag.IPlayer player = PlayerManager.Instance.GetOwnPlayer();
        if (player == null)
        {
            // Debug.LogWarning("Cannot align camera rig to player, there is no local instance of player.");
            return;
        }

        AlignmentTarget target = player.PlayerAlignmentTarget;
        if (target == null)
        {
            // Debug.LogErrorFormat("Cannot align camera rig to player, unable to find {0} in the scene.", typeof(AlignmentTarget).FullName);
            return;
        }

        if (GameObject.Find("MenuPlayer")) {
            Debug.Log("MenuPlayer found. No AlignRigToPlayer.");
            return;
        }

        if (alignPosition) {
            transform.position = target.transform.position;
        }

        if (alignRotation) {
            transform.rotation = target.transform.rotation;
        }
        
    }
    
}
