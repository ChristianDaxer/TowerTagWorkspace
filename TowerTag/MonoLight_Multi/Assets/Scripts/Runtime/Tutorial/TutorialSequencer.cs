using System;
using JetBrains.Annotations;
using TowerTag;
using UnityEngine;
using UnityEngine.Serialization;

namespace Tutorial {
    public class TutorialSequencer : MonoBehaviour {
        [SerializeField] private TutorialSequence[] _sequences;
        
        [Header("SequenceReferences")] 
        
        [FormerlySerializedAs("_hubWallDoor")]
        [SerializeField] private Animator _startHubWallDoor;

        [SerializeField] private Animator _endHubWallDoor;

        [SerializeField] private AudioSource _source;

        [SerializeField] private Pillar[] _pillarOrder;
        private TutorialSequence CurrentSequence => _sequences[_currentIndex];

        public Animator StartHubWallDoor => _startHubWallDoor;
        public Animator EndHubWallDoor => _endHubWallDoor;
        
        public AudioSource Source => _source;

        private int _currentIndex;
        private static readonly int StartOpen = Animator.StringToHash("startOpen");
        private IPlayer _ownPlayer;

        private void Awake() {
            _currentIndex = 0;
            _source = GetComponent<AudioSource>();
            EndHubWallDoor.SetTrigger(StartOpen);
        }

        private void Start() {
            CurrentSequence.Init();
        }

        private void OnEnable() {
            _ownPlayer = PlayerManager.Instance.GetOwnPlayer();
            if (_ownPlayer != null)
                _ownPlayer.PlayerHealth.PlayerDied += HubSceneBehaviour.RespawnOnCurrentPillar;
        }

        private void OnDisable() {
            if (_ownPlayer != null)
                _ownPlayer.PlayerHealth.PlayerDied += HubSceneBehaviour.RespawnOnCurrentPillar;
        }

        private void Update() {
            if (!CurrentSequence.IsRunning) return;
            CurrentSequence.Update();
            if (CurrentSequence.IsCompleted() && _currentIndex < _sequences.Length) {
                CurrentSequence.Finish(_currentIndex + 1 < _sequences.Length ? _sequences[++_currentIndex] : null);
            }
        }

        [CanBeNull]
        public Pillar PillarGetNextPillar(Pillar currentPillar) {
            int index = Array.IndexOf(_pillarOrder, currentPillar);
            return index + 1 < _pillarOrder.Length ? _pillarOrder[index + 1] : null;
        }
    }
}