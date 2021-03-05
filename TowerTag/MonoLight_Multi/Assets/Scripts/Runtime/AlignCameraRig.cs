using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class AlignCameraRig : MonoBehaviour
{
    private PlayerRigBase _playerRigBase;
    private void Start()
    {
        if (_playerRigBase == null)
        {
            if (!PlayerRigBase.GetInstance(out _playerRigBase)) {
                return;   
            }
        }
        Debug.Log($"In {GetType().Name}: Set {_playerRigBase.gameObject.name} to object {gameObject.name} position {transform.position} rotation {transform.rotation}");
        
        _playerRigBase.transform.position = transform.position;
        _playerRigBase.transform.rotation = transform.rotation;
    }
}
