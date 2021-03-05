using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

namespace Tutorial {
    [CreateAssetMenu(menuName = "TutorialSequences/TeleportSequence", fileName = nameof(TeleportSequence))]
    public class TeleportSequence : TutorialSequence {
        [SerializeField] private GameObject _videoPrefab;
        [SerializeField] private float _secondsUntilVideoSpawn = 1.0f;
        [SerializeField] private AudioClip _captureHint;
        [SerializeField] private AudioClip _usageHint;
        [SerializeField] private AudioMixerGroup _hintMixer;
        private GameObject _video;
        private Animator _door;

        private static readonly int Open = Animator.StringToHash("open");
        private static readonly int Close = Animator.StringToHash("close");
        private bool _hasPlayerTeleported;
        
        
        protected override void ResetValues() {
            _hasPlayerTeleported = false;
            _video = null;
            _door = null;
        }

        protected override IEnumerator StartSequence() {
            _ownPlayer.TeleportHandler.TeleportRequested += OnPlayerTeleporting;
            _door = FindObjectOfType<TutorialSequencer>().StartHubWallDoor;
            _door.SetTrigger(Open);

            AudioSource src = _door.gameObject.AddComponent<AudioSource>();
            src.outputAudioMixerGroup = _hintMixer;
            src.clip = _captureHint;
            src.Play();

            yield return new WaitForSeconds(_secondsUntilVideoSpawn);

            InitializeIngameExplainer();
        }

        private void InitializeIngameExplainer() {
            _video = InstantiateWrapper.InstantiateWithMessage(_videoPrefab, _ownPlayer.GameObject.CheckForNull()?.transform);
            _video.transform.parent = null;

            StaticCoroutine.StartStaticCoroutine(PlayDescription());
        }

        private IEnumerator PlayDescription() {
            yield return new WaitForSeconds(2f);
            AudioSource src = _video.gameObject.AddComponent<AudioSource>();
            src.outputAudioMixerGroup = _hintMixer;
            src.clip = _usageHint;
            src.Play();
        }

        private void OnPlayerTeleporting(Pillar pillarThePlayerIsCurrentlyOn, Pillar origin, int timestampTheTeleportWasRequested) {
            _hasPlayerTeleported = true;
        }

        protected override IEnumerator EndSequence(TutorialSequence nextSequence) {
            yield return null;
            _ownPlayer.TeleportHandler.TeleportRequested -= OnPlayerTeleporting;
            _door.SetTrigger(Close);
            Destroy(_video);
            if (nextSequence != null) nextSequence.Init();
        }

        public override void Update() {
        }

        public override bool IsCompleted() {
            return _hasPlayerTeleported;
        }
    }
}