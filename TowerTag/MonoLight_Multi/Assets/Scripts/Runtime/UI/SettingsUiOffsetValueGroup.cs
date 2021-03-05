using Runtime.Pillar.PillarCalibration;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.UI
{
    public class SettingsUiOffsetValueGroup : MonoBehaviour
    {
        public delegate void SettingsUiOffsetValueButtonAction(object sender, bool direction);

        public event SettingsUiOffsetValueButtonAction SettingsUiOffsetValueButtonPressed;

        [SerializeField] private PillarOffsetManager.PillarOffsetCalibrationMode _pillarOffsetCalibrationMode;
        [SerializeField] private Text _offsetValueText;

        [SerializeField] private Button _offsetValueButtonDown;
        [SerializeField] private Button _offsetValueButtonUp;

        private float _customOffset;

        public float CustomOffset
        {
            get => _customOffset;
            set
            {
                _customOffset = value;
                if (_offsetValueText != null && _pillarOffsetCalibrationMode ==
                    PillarOffsetManager.PillarOffsetCalibrationMode.Position)
                    _offsetValueText.text = (_customOffset * -1).ToString("F2");
                if (_offsetValueText != null && _pillarOffsetCalibrationMode ==
                    PillarOffsetManager.PillarOffsetCalibrationMode.Rotation)
                    _offsetValueText.text = (_customOffset % 180).ToString("F0");
            }
        }

        private void Start()
        {
            if (_offsetValueButtonUp != null)
            {
                _offsetValueButtonUp.onClick.AddListener(OnUpButtonClicked);
            }

            if (_offsetValueButtonDown != null)
            {
                _offsetValueButtonDown.onClick.AddListener(OnDownButtonClicked);
            }
        }

        private void OnDestroy()
        {
            if (_offsetValueButtonUp != null)
            {
                _offsetValueButtonUp.onClick.RemoveListener(OnUpButtonClicked);
            }

            if (_offsetValueButtonDown != null)
            {
                _offsetValueButtonDown.onClick.RemoveListener(OnDownButtonClicked);
            }
        }

        private void OnUpButtonClicked()
        {
            SettingsUiOffsetValueButtonPressed?.Invoke(this, true);
        }

        private void OnDownButtonClicked()
        {
            SettingsUiOffsetValueButtonPressed?.Invoke(this, false);
        }

        public void ToggleButtonInteraction(bool status)
        {
            _offsetValueButtonUp.interactable = status;
            _offsetValueButtonDown.interactable = status;
            _offsetValueText.color = status ? Color.white : Color.gray;
        }
    }
}