using JetBrains.Annotations;
using Toornament.Store.Model;
using TowerTag;
using UnityEngine;
using UnityEngine.UI;

namespace Home.UI {
    public class GameTipsPanel : HomeMenuPanel {
        public static HubUIController.PanelType PanelTypeToLoadIn = HubUIController.PanelType.FinishTutorial;
        
        [SerializeField] private Sprite[] _gameTips;

        private int _currentIndex = 0;

        [SerializeField] private Transform _imageContainer;
        
        [Space]
        [SerializeField] private Button _previousButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _readyButton;

        private new void Awake()
        {
            base.Awake();
        }

        public override void OnEnable()
        {
            base.OnEnable();
            ResetPanel();
        }

        private void ResetPanel()
        {
            _currentIndex = 0;
            
            _previousButton.gameObject.SetActive(false);
            _nextButton.gameObject.SetActive(true);
            _readyButton.gameObject.SetActive(false);
            
            _imageContainer.GetComponent<Image>().sprite = _gameTips[_currentIndex];
        }

        [UsedImplicitly]
        public void OnPreviousButtonPressed()
        {
            _currentIndex--;
            Debug.Log($"Game Tips INDEX: {_currentIndex}");
            
            if (_currentIndex <= 1) _previousButton.gameObject.SetActive(false);

            if (_currentIndex == _gameTips.Length-2)
            {
                _nextButton.gameObject.SetActive(true);
                _readyButton.gameObject.SetActive(false);
            }

            _imageContainer.GetComponent<Image>().sprite = _gameTips[_currentIndex];
        }

        [UsedImplicitly]
        public void OnNextButtonPressed()
        {
            _currentIndex++;
            Debug.Log($"Game Tips INDEX: {_currentIndex}");
            
            if (_currentIndex >= 1) _previousButton.gameObject.SetActive(true);
            
            if (_currentIndex == _gameTips.Length-1)
            {
                _nextButton.gameObject.SetActive(false);
                _readyButton.gameObject.SetActive(true);
            }
            
            _imageContainer.GetComponent<Image>().sprite = _gameTips[_currentIndex];
        }
        
        protected void OnDisable()
        {
            ResetPanel();
        }
    }
}