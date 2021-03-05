using System;
using System.Collections.Generic;
using System.Linq;
using TowerTag;
using TowerTagSOES;
using UnityEngine;
#if !UNITY_ANDROID
using UnityEngine.Windows.Speech;
#endif

namespace AI {
    /// <summary>
    /// Handles Voice Commands recognition from real players to bot players. Uses the build in Windows speech recognition with keywords.
    /// </summary>
    public class BotVoiceCommandsManager : MonoBehaviour {
        [SerializeField] private bool _useVoiceCommands;
        [SerializeField] private string _attackCommand = "attack";
        [SerializeField] private string _defendCommand = "defend";
        [SerializeField] private string _claimCommand = "claim";

        private IPlayer[] _botPlayers;
#if !UNITY_ANDROID
        private KeywordRecognizer _keywordRecognizer;
#endif
        private readonly Dictionary<string, Action> _keywords = new Dictionary<string, Action>();

        public enum VoiceCommands {
            Attack,
            Defend,
            Claim
        }

        private VoiceCommands _currentCommand;
        private PlayerNetworkEventHandler _playerNetworkEventHandler;

        private void Start() {
            //don't use voice recognition on client if option "voice commands" is unchecked
            if (!_useVoiceCommands || SharedControllerType.IsAdmin) return;

            IPlayer ownPlayer = PlayerManager.Instance.GetOwnPlayer();
            if (ownPlayer == null) {
                Debug.LogError("No local player found to initialize BotVoiceCommandsManager");
                return;
            }
            if (ownPlayer.GameObject != null)
                _playerNetworkEventHandler = ownPlayer.GameObject.GetComponent<PlayerNetworkEventHandler>();

            //Create keywords for keyword recognizer
            _keywords.Add(_attackCommand, () => {
                // action to be performed when this keyword is spoken
                if (_currentCommand == VoiceCommands.Attack)
                    return; //don't trigger same command multiple times directly after each other
                Debug.Log("### Keyword Recognizer ATTACK");
                _currentCommand = VoiceCommands.Attack;
                _playerNetworkEventHandler.BroadcastBotVoiceCommand(ownPlayer.TeamID, _currentCommand);
            });

            _keywords.Add(_defendCommand, () => {
                if (_currentCommand == VoiceCommands.Defend) return;
                Debug.Log("### Keyword Recognizer DEFEND");
                _currentCommand = VoiceCommands.Defend;
                _playerNetworkEventHandler.BroadcastBotVoiceCommand(ownPlayer.TeamID, _currentCommand);
            });

            _keywords.Add(_claimCommand, () => {
                if (_currentCommand == VoiceCommands.Claim) return;
                Debug.Log("### Keyword Recognizer CLAIM");
                _currentCommand = VoiceCommands.Claim;
                _playerNetworkEventHandler.BroadcastBotVoiceCommand(ownPlayer.TeamID, _currentCommand);
            });

#if !UNITY_ANDROID
            _keywordRecognizer = new KeywordRecognizer(_keywords.Keys.ToArray());
            _keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnCommandRecognized;
            _keywordRecognizer.Start();
#endif
            Debug.Log("### Keyword Recognizer started");
        }

#if !UNITY_ANDROID
        private void KeywordRecognizer_OnCommandRecognized(PhraseRecognizedEventArgs args) {
            // if the keyword recognized is in our dictionary, call that Action.
            if (_keywords.TryGetValue(args.text, out Action keywordAction)) {
                keywordAction.Invoke();
            }
        }
#endif
    }
}