using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotonDispatchBehaviour : MonoBehaviour
{
    private void Awake()
    {
        PhotonDispatchThreadManager.StartBackgroundDispatch();
    }

    private void OnDestroy()
    {
        PhotonDispatchThreadManager.StopBackgroundDispatch();
    }
}
