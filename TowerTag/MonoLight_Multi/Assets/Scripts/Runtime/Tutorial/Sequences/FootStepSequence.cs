using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace Tutorial {
    [CreateAssetMenu(menuName = "TutorialSequences/FootStepSequence", fileName = nameof(FootStepSequence))]
    public class FootStepSequence : TutorialSequence {
        [SerializeField] private Transform _footPrintsPrefab;
        [SerializeField] private float _timeUntilInfoPinSpawns;
        [SerializeField] private LayerMask _collisionLayer;
        [SerializeField] private AudioClip _footprintsHint;
        [SerializeField] private AudioMixerGroup _hintMixer;

        private bool IsPlayerOnFootPrints =>
            Physics.SphereCast(new Ray(_footPrints.position, Vector3.up), 0.2f, 10,
                _collisionLayer);

        private bool IsPlayerLookingForwards =>
            Vector3.Angle(_playerHead.forward, Vector3.forward) < 30;

        private FootPrintInfoPin _infoPin;
        private Transform _playerHead;

        private Transform _footPrints;

        protected override void ResetValues() {
            _footPrints = null;
            _infoPin = null;
            _playerHead = null;
        }

        protected override IEnumerator StartSequence() {
            InstantiatePrefabs();
            InitGlobalVariables();
            yield return new WaitForSeconds(_timeUntilInfoPinSpawns);
            if (_infoPin != null)
                _infoPin.StartAnimation();
        }

        private void InstantiatePrefabs() {
            _footPrints = InstantiateWrapper.InstantiateWithMessage(_footPrintsPrefab, _ownPlayer.GameObject.CheckForNull()?.transform);
            AudioSource src = _footPrints.gameObject.AddComponent<AudioSource>();
            src.outputAudioMixerGroup = _hintMixer;
            src.clip = _footprintsHint;
            src.Play();
        }

        private void InitGlobalVariables() {
            _playerHead = _ownPlayer.PlayerAvatar.AvatarMovement.HeadSourceTransform;
            _infoPin = _footPrints.GetComponentInChildren<FootPrintInfoPin>(true);
        }

        protected override IEnumerator EndSequence(TutorialSequence nextSequence) {
            StaticCoroutine.StartStaticCoroutine(_infoPin.EndAnimation(1));
            yield return new WaitUntil(() => _infoPin.IsAnimatorInDefaultState);
            Destroy(_footPrints.gameObject);
            if (nextSequence != null) nextSequence.Init();
        }

        public override void Update() {
        }

        public override bool IsCompleted() {
            return _playerHead != null && _footPrints != null
                                       && IsPlayerOnFootPrints
                                       && IsPlayerLookingForwards;
        }
    }
}