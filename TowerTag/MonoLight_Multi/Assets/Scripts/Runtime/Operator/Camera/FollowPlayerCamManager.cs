using Cinemachine;
using TowerTag;
using UnityEngine;

public class FollowPlayerCamManager : MonoBehaviour {
    [SerializeField] private CinemachineVirtualCamera _vCam;
    [SerializeField] private LookToNearestTarget _lookAt;
    [SerializeField] private IPlayer _followingPlayer;

    public IPlayer FollowingPlayer {
        get => _followingPlayer;
        set => SetFollowingPlayer(value);
    }

    /// <summary>
    /// Follow cameras have to be active all the time to avoid strange camera jumps!
    /// </summary>
    /// <param name="active"></param>
    public void SetActive(bool active) {
        _vCam.Priority = active ? 100 : 0;
    }

    private void SetFollowingPlayer(IPlayer player) {
        _followingPlayer = player;
        _lookAt.CurrentlyFollowingPlayer = player;
    }
}
