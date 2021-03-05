using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace Tutorial {
    public class FootPrintInfoPin : MonoBehaviour {
        [FormerlySerializedAs("_footPrints")] [SerializeField] private Transform _followPoint;
        private Camera _mainCamera;

        [SerializeField] private Animator _animator;
        [SerializeField] private AudioSource _audioSource;
        private static readonly int End = Animator.StringToHash("End");
        private static readonly int _start = Animator.StringToHash("Start");
        public bool IsAnimatorInDefaultState => _animator.GetCurrentAnimatorStateInfo(0).IsName("DefaultState");


        // Start is called before the first frame update
        void Start() {
            if (PlayerHeadBase.GetInstance(out var playerHeadBase))
                _mainCamera = playerHeadBase.HeadCamera;
        }

        // Update is called once per frame
        void Update() {
            if (_mainCamera != null && _followPoint != null) {
                _followPoint.position = _mainCamera.transform.position + _mainCamera.transform.forward;
            }
            else {
                Debug.LogError($"_cam is {_mainCamera} and _footprints is {_followPoint}. Disabling Script");
                enabled = false;
            }
        }

        public IEnumerator EndAnimation(int waitDuration) {
            yield return new WaitForSeconds(waitDuration);
            if (_animator != null) {
                _audioSource.PlayDelayed(0);
                _animator.SetTrigger(End);
            }
        }

        public void StartAnimation() {
            if (_animator != null)
                _animator.SetTrigger(_start);
            if (_audioSource != null)
                _audioSource.PlayDelayed(1);
        }
    }
}