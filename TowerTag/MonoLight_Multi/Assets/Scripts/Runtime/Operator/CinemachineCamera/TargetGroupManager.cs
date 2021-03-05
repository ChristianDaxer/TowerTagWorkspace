using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using OperatorCamera;
using TowerTag;
using UnityEngine;
using Target = Cinemachine.CinemachineTargetGroup.Target;

public class TargetGroupManager : MonoBehaviour {
    [SerializeField] private CinemachineTargetGroup _targetGroup;
    private CinemachineTargetGroup TargetGroup => _targetGroup;

    public bool GoalPillarGettingClaimed => _pillarsGettingClaimed.Count > 0;

    private IPlayer[] _players;
    private readonly List<Pillar> _pillarsGettingClaimed = new List<Pillar>();

    public void Init() {
        UpdatePlayerAndTargets();
        PlayerManager.Instance.PlayerAdded += OnPlayerAdded;
        PlayerManager.Instance.PlayerRemoved += OnPlayerRemoved;
        CameraManager.Instance.GoalPillarGettingClaimed += OnGoalPillarClaimed;
        CameraManager.Instance.GoalPillarClaimAborted += OnGoalPillarClaimAborted;
        GameManager.Instance.MatchHasFinishedLoading += OnMatchFinishedLoading;

        if (_players != null && _players.Length > 0)
            _players.ForEach(player => player.PlayerHealth.HealthChanged += ChangeWeightOfPlayer);
    }

    private void OnPlayerAdded(IPlayer player) {
        UpdatePlayerAndTargets();
        player.PlayerHealth.HealthChanged += ChangeWeightOfPlayer;
        player.PlayerHealth.PlayerDied += OnPlayerDied;
    }

    private void OnPlayerRemoved(IPlayer player) {
        UpdatePlayerAndTargets();
        player.PlayerHealth.HealthChanged -= ChangeWeightOfPlayer;
        player.PlayerHealth.PlayerDied -= OnPlayerDied;
    }

    private void OnMatchFinishedLoading(IMatch obj) {
        _pillarsGettingClaimed.Clear();
    }

    private void OnPlayerDied(PlayerHealth playerHealth, IPlayer enemyWhoAppliedDamage, byte colliderType) {
        if (playerHealth.Player.GameObject != null)
            StartCoroutine(LerpWeightToValue(playerHealth.Player.GameObject.transform, 0, 0.2f));
    }

    public void UpdatePlayerAndTargets() {
        if (GoalPillarGettingClaimed) return;
        PlayerManager.Instance.GetParticipatingPlayers(out var players, out var count);
        _players = players.Take(count).ToArray();
        if (_players != null)
            TargetGroup.m_Targets = AddAllPlayerToTargetGroup(_players);
    }

    /// <summary>
    /// Converts the given Player array to Targets.
    /// </summary>
    /// <param name="players">The Players you want to get focused</param>
    /// <returns></returns>
    private Target[] AddAllPlayerToTargetGroup(IPlayer[] players) {
        return players
            .Where(player => player.GameObject != null)
            .Select(player => new Target {
                target = player.GameObject.transform,
                weight = player.IsAlive ? 1 : 0,
                radius = 5
            }).ToArray();
    }

    private void OnGoalPillarClaimed(Pillar pillar) {
        if(!_pillarsGettingClaimed.Contains(pillar))
            _pillarsGettingClaimed.Add(pillar);
        TargetGroup.m_Targets = AddPillarToTargetGroup(_pillarsGettingClaimed.ToArray());
    }

    private void OnGoalPillarClaimAborted(Pillar pillar) {
        if (_pillarsGettingClaimed.Contains(pillar))
            _pillarsGettingClaimed.Remove(pillar);

        if (!GoalPillarGettingClaimed) {
            _pillarsGettingClaimed.Clear();
            return;
        }

        TargetGroup.m_Targets = AddPillarToTargetGroup(_pillarsGettingClaimed.ToArray());
    }

    private Target[] AddPillarToTargetGroup(Pillar[] pillars) {
        return pillars
            .Where(pillar => pillar != null && pillar.gameObject != null)
            .Select(pillar => new Target {
                target = pillar.transform,
                weight = 1,
                radius = 5
            }).ToArray();
    }

    private void ChangeWeightOfPlayer(PlayerHealth playerHealth, int newHealth, IPlayer other, byte colliderType) {
        if (GameManager.Instance.CurrentMatch != null && !GameManager.Instance.CurrentMatch.IsActive) return;
        if (newHealth == playerHealth.MaxHealth) {
            if (playerHealth.Player.GameObject != null) {
                StartCoroutine(LerpWeightToValue(playerHealth.Player.GameObject.transform,
                    1));
            }
        }
    }

    private IEnumerator LerpWeightToValue(Transform target, float lerpToValue, float duration = 1) {
        int index = TargetGroup.FindMember(target);
        if (index == -1) yield break;
        float startWeight = TargetGroup.m_Targets[index].weight;
        var time = 0f;
        while (time <= 1) {
            time += Time.deltaTime / duration;
            index = TargetGroup.FindMember(target);
            if (index == -1) yield break;
            TargetGroup.m_Targets[index].weight = Mathf.Lerp(startWeight, lerpToValue, time);
            yield return null;
        }

        index = TargetGroup.FindMember(target);
        if (index == -1) yield break;
        TargetGroup.m_Targets[index].weight = lerpToValue;
    }

    private void OnDestroy() {
        PlayerManager.Instance.PlayerAdded -= OnPlayerAdded;
        PlayerManager.Instance.PlayerRemoved -= OnPlayerRemoved;
        if (CameraManager.Instance != null) {
            CameraManager.Instance.GoalPillarGettingClaimed -= OnGoalPillarClaimed;
            CameraManager.Instance.GoalPillarClaimAborted -= OnGoalPillarClaimAborted;
        }

        if (_players != null && _players.Length > 0)
            _players.ForEach(player => player.PlayerHealth.HealthChanged -= ChangeWeightOfPlayer);
    }
}