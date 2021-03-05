using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Linq;
using TowerTag;

/// <summary>
/// This is for the LookAtTarget object of the RemoteClient. It is important for the orbital Transposer of the follow cam, so it will stay behind the player
/// </summary>
public class LookToNearestTarget : MonoBehaviour {
    [SerializeField] private TransitionObject _transitionObject;
    public Transform Target => _transitionObject.transform;

    public IPlayer CurrentlyLookingAtPlayer {
        get => _transitionObject.CurrentlyLookingAt;
        set => _transitionObject.CurrentlyLookingAt = value;
    }

    [SerializeField] private IPlayer _currentlyFollowingPlayer;
    public IPlayer CurrentlyFollowingPlayer {
        get => _currentlyFollowingPlayer;
        set => _currentlyFollowingPlayer = value;
    }

    private Coroutine _nearestEnemyCoru;
    private Coroutine _rotatingCoru;
    private float _waitingTime = 3f;

    private void OnEnable() {
        if(_nearestEnemyCoru == null)
            _nearestEnemyCoru = StartCoroutine(CheckNearestTarget());
    }

    private void OnDisable() {
        if(_nearestEnemyCoru != null)
            StopCoroutine(_nearestEnemyCoru);
        _nearestEnemyCoru = null;
        CurrentlyFollowingPlayer = null;
        CurrentlyLookingAtPlayer = null;
    }

    void Update()
    {
        if (Target != null && CurrentlyFollowingPlayer != null) {
            if (CurrentlyFollowingPlayer.GameObject != null) transform.position = CurrentlyFollowingPlayer.GameObject.transform.position;
            transform.LookAt(Target, Vector3.up);
        }
    }

    private IEnumerator CheckNearestTarget() {
        IPlayer nearestEnemy = GetNearestEnemyPlayer();
        if (nearestEnemy != null && nearestEnemy != CurrentlyLookingAtPlayer)
            _transitionObject.StartRotateToNextTarget(nearestEnemy);
        float time = 0;
        while (time < _waitingTime) {
            time += Time.deltaTime;
            if (CurrentlyLookingAtPlayer?.GameObject != null && CurrentlyLookingAtPlayer.PlayerHealth != null
                                                             && !CurrentlyLookingAtPlayer.IsAlive) time = _waitingTime;
            yield return null;
        }

        _nearestEnemyCoru = StartCoroutine(CheckNearestTarget());
    }

    /// <summary>
    /// returns nearest player. Prefers alive players.
    /// </summary>
    private IPlayer GetNearestEnemyPlayer() {
        IPlayer targetPlayer = null;
        float minDistance = float.MaxValue;
        if (CurrentlyFollowingPlayer?.GameObject == null)
            return null;

        PlayerManager.Instance.GetParticipatingPlayers(out var players, out var count);
        IPlayer[] enemyPlayersAlive = players
            .Where(player => player != null && player.TeamID != CurrentlyFollowingPlayer.TeamID && player.PlayerHealth.IsAlive).ToArray();

        if (enemyPlayersAlive.Length <= 0)
            return null;

        foreach (IPlayer player in enemyPlayersAlive) {
            if (player == null)
                continue;

            float distance =
                    Vector3.Magnitude(CurrentlyFollowingPlayer.PlayerAvatar.AvatarMovement.HeadSourceTransform.position -
                                      player.PlayerAvatar.AvatarMovement.HeadSourceTransform.position);
            if (distance <= minDistance) {
                minDistance = distance;
                targetPlayer = player;
            }
        }
        return targetPlayer;
    }
}
