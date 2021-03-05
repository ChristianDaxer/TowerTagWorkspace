using System.Collections.Generic;
using GameManagement;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Action/Manager")]
public class GameActionManager : ScriptableObject {
    private readonly List<GameAction> _gameActions = new List<GameAction>();
    private readonly List<byte> _eventCodes = new List<byte>();
    private readonly List<byte> _typeCodes = new List<byte>();

    public void Register(GameAction gameAction, byte eventCode, byte denyEventCode, byte parameterTypeCode) {
        if (_gameActions.Contains(gameAction)) return;
        if (_eventCodes.Contains(eventCode) || _eventCodes.Contains(denyEventCode)) {
            Debug.LogWarning($"Cannot register GameAction {gameAction.name}, because its event codes are taken. " +
                             "Each GameAction must define unique event and parameter-type codes.");
            return;
        }

        if (_typeCodes.Contains(parameterTypeCode)) {
            Debug.LogWarning($"Cannot register GameAction {gameAction.name}," +
                             "because the parameter-type code is taken. " +
                             "Each GameAction must define unique event and parameter-type codes.");
            return;
        }

        _gameActions.Add(gameAction);
        _eventCodes.Add(eventCode);
        _eventCodes.Add(denyEventCode);
        _typeCodes.Add(parameterTypeCode);
    }

    public void Unregister(GameAction gameAction, byte eventCode, byte denyEventCode, byte parameterTypeCode) {
        if (!_gameActions.Contains(gameAction)) return;
        _gameActions.Remove(gameAction);
        _eventCodes.Remove(eventCode);
        _eventCodes.Remove(denyEventCode);
        _typeCodes.Remove(parameterTypeCode);
    }

    public void Init() {
        _gameActions.ForEach(gameAction =>
            gameAction.Init(
                ServiceProvider.Get<IPhotonService>(),
                GameManager.Instance,
                PlayerManager.Instance,
                ServiceProvider.Get<ISceneService>()));
    }
}