using System;
using System.Collections;
using System.Linq;
using UI;
using UnityEngine;
using UnityEngine.UI;
using CameraMode = OperatorCamera.CameraManager.CameraMode;

namespace OperatorCamera {
    public class CameraUIManager : MonoBehaviour {
        [Serializable]
        private struct CameraModeImagePair {
            public CameraMode Mode;
            public Image Image;
        }

        [SerializeField] private ToggleOperatorUI _toggleUI;
        [SerializeField] private RectTransform _cameraIconsParent;
        [SerializeField, Tooltip("The material for a activated mode")] private Material _activeMaterial;
        [SerializeField, Tooltip("The material for a deactivated mode")] private Material _inactiveMaterial;

        [SerializeField] private CameraModeImagePair[] _modeImagePairs;
        [SerializeField, Tooltip("Image to visualize if the follow mode is locked to a player")]
        private Image _followPlayerLockedImage;
        [SerializeField, Tooltip("Image to visualize if the ego mode is locked to a player")]
        private Image _egoPlayerLockedImage;

        private Coroutine _coru;

        private void Start() {
            CameraManager.Instance.CameraModeChanged += OnCameraModeChanged;
            CameraManager.Instance.HardFocusOnPlayerChanged += OnHardFocusOnPlayerChanged;
            _toggleUI.SpectatorUIToggled += OnSpectatorModeToggle;
        }

        private void OnSpectatorModeToggle(bool value) {
            if(_coru != null)
                StopCoroutine(_coru);

            _coru = StartCoroutine(ToggleSymbolVisibility(value));
        }

        private IEnumerator ToggleSymbolVisibility(bool value) {
            var anchoredPosition = _cameraIconsParent.anchoredPosition;
            Vector2 finishPosition = value
                ? new Vector2(0, anchoredPosition.y)
                : new Vector2(anchoredPosition.x + _cameraIconsParent.sizeDelta.x
                    , anchoredPosition.y);
            while (Vector2.Distance(_cameraIconsParent.anchoredPosition, finishPosition) > 0.1f) {
                _cameraIconsParent.anchoredPosition = Vector2.Lerp(_cameraIconsParent.anchoredPosition, finishPosition, 0.2f);
                yield return null;
            }

            _cameraIconsParent.anchoredPosition = finishPosition;
        }

        private void OnCameraModeChanged(CameraManager sender, CameraMode oldMode, CameraMode newMode) {
            CameraModeImagePair oldPair = _modeImagePairs.FirstOrDefault(pair => pair.Mode == oldMode);
            CameraModeImagePair newPair = _modeImagePairs.FirstOrDefault(pair => pair.Mode == newMode);

            if(oldPair.Image != null) oldPair.Image.material = _inactiveMaterial;
            if(newPair.Image != null) newPair.Image.material = _activeMaterial;

            if (sender.HardFocusOnPlayer) {
                if(newMode == CameraMode.Ego) _egoPlayerLockedImage.material = _activeMaterial;
                else if(newMode == CameraMode.Follow) _followPlayerLockedImage.material = _activeMaterial;
                if (oldMode == CameraMode.Ego)
                    _egoPlayerLockedImage.material = _inactiveMaterial;
                else if (oldMode == CameraMode.Follow) _followPlayerLockedImage.material = _inactiveMaterial;
            }
            else {
                if (oldMode == CameraMode.Ego)
                    _egoPlayerLockedImage.material = _inactiveMaterial;
                else if (oldMode == CameraMode.Follow) _followPlayerLockedImage.material = _inactiveMaterial;
            }
        }

        private void OnHardFocusOnPlayerChanged(CameraManager sender, bool value) {
            switch (sender.CurrentCameraMode) {
                case CameraMode.Ego:
                    _egoPlayerLockedImage.material = value ? _activeMaterial : _inactiveMaterial;
                    break;
                case CameraMode.Follow:
                    _followPlayerLockedImage.material = value ? _activeMaterial : _inactiveMaterial;
                    break;
            }
        }

        void OnDestroy() {
            if (CameraManager.Instance != null) {
                CameraManager.Instance.CameraModeChanged -= OnCameraModeChanged;
                CameraManager.Instance.HardFocusOnPlayerChanged -= OnHardFocusOnPlayerChanged;
            }

            _toggleUI.SpectatorUIToggled -= OnSpectatorModeToggle;
        }
    }
}