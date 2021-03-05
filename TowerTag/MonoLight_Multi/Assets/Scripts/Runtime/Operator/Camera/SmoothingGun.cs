using UnityEngine;
using Photon.Pun;

/// <summary>
/// This script lerps the position of the gun in the ego view to the real position.
/// </summary>
public class SmoothingGun : MonoBehaviourPun {
    [SerializeField] private GameObject _laserGun;

    private Vector3 _correctPlayerPos = Vector3.zero; // We lerp towards this
    private Quaternion _correctPlayerRot = Quaternion.identity;  // We lerp towards this

    void Update()
    {
        if (!photonView.IsMine) {
            _laserGun.transform.position = Vector3.Lerp(_laserGun.transform.position, _correctPlayerPos, Time.deltaTime * 5);
            _laserGun.transform.rotation = Quaternion.Lerp(_laserGun.transform.rotation, _correctPlayerRot, Time.deltaTime * 5);
        }
    }

    // ReSharper disable once UnusedMember.Local
    void OnPhotonSerializeView(PhotonStream stream)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(_laserGun.transform.position);
            stream.SendNext(_laserGun.transform.rotation);
        }
        else
        {
            // Network player, receive data
            _correctPlayerPos = _laserGun.transform.position;
            _correctPlayerRot = _laserGun.transform.rotation;
        }
    }
}