using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TowerTag;
using TowerTagSOES;
using UnityEngine;

public class InfoPinManager : MonoBehaviour {
    [SerializeField] private InfoPin _infoPinPrefab;
    private int _infoPinDefaultHeight = 6;
    private int _ownGTPinDelay = 2; //Duration in seconds after MatchHasFinishedLoading to spawn own goal tower info pin
    private int _enemyGTPinDelay = 3; //Duration in seconds after MatchHasFinishedLoading to spawn enemy goal tower info pin
    private int _hideDelay = 30; //Duration in seconds after match has been started to hide animation for the info pins
    private readonly List<InfoPin> _infoPins = new List<InfoPin>();

    private void Start() {
        GameManager.Instance.MatchHasFinishedLoading += OnMatchHasFinishedLoading;
    }

    private void OnDestroy() {
        GameManager.Instance.MatchHasFinishedLoading -= OnMatchHasFinishedLoading;
        StopAllCoroutines();
    }

    private void OnMatchHasFinishedLoading(IMatch match) {
        GameManager.Instance.CurrentMatch.Started += OnMatchStarted;
        switch (match.GameMode) {
            case GameMode.GoalTower:
                StartCoroutine(SetUpGoalTowerInfoPins());
                break;
            case GameMode.DeathMatch:
            case GameMode.Elimination:
                break;
        }
    }

    private void OnMatchStarted(IMatch match) {
        _infoPins.ForEach(infoPin => StartCoroutine(infoPin.EndAnimation(_hideDelay)));
        _infoPins.Clear();
    }

    private IEnumerator SetUpGoalTowerInfoPins() {
        Pillar[] pillars = PillarManager.Instance.GetAllGoalPillarsInScene();
        if (!SharedControllerType.IsAdmin && !SharedControllerType.Spectator) {
            IPlayer ownPlayer = PlayerManager.Instance.GetOwnPlayer();
            if (ownPlayer == null) yield break;

            Pillar localPlayerGoalTower = pillars.FirstOrDefault(pillar => pillar.OwningTeamID == ownPlayer.TeamID);
            if (localPlayerGoalTower == null) yield break;

            yield return new WaitForSeconds(_ownGTPinDelay);
            InfoPin ownInfoPin =InstantiateWrapper.InstantiateWithMessage(_infoPinPrefab, localPlayerGoalTower.transform);
            ownInfoPin.Init(localPlayerGoalTower, GameMode.GoalTower);
            ownInfoPin.transform.localPosition = new Vector3(0,_infoPinDefaultHeight,0);
            ownInfoPin.StartAnimation();

            Pillar enemyPlayerGoalTower = pillars.FirstOrDefault(pillar => pillar.OwningTeamID != ownPlayer.TeamID);
            if (enemyPlayerGoalTower == null) yield break;

            yield return new WaitForSeconds(_enemyGTPinDelay - _ownGTPinDelay);
            InfoPin enemyInfoPin =InstantiateWrapper.InstantiateWithMessage(_infoPinPrefab, enemyPlayerGoalTower.transform);
            enemyInfoPin.Init(enemyPlayerGoalTower, GameMode.GoalTower);
            enemyInfoPin.transform.localPosition = new Vector3(0, _infoPinDefaultHeight+1, 0);
            enemyInfoPin.StartAnimation();

            _infoPins.Add(ownInfoPin);
            _infoPins.Add(enemyInfoPin);
        }
    }
}
