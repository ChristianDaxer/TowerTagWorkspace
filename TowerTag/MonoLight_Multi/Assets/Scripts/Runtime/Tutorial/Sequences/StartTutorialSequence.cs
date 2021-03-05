using System.Collections;
using Home.UI;
using TowerTag;
using UnityEngine;

namespace Tutorial
{
    [CreateAssetMenu(menuName = "TutorialSequences/StartTutorialSequence", fileName = nameof(StartTutorialSequence))]
    public class StartTutorialSequence : TutorialSequence
    {
        [SerializeField] private TutorialUIController _tutorialUIPrefab;

        private bool IsGTCAccepted => PlayerPrefs.GetInt(PlayerPrefKeys.GTC) == _gtcPanel.GTCVersion;
        private bool IsStartTutorialButtonPressed { get; set; }

        private TutorialUIController _tutorialUIController;
        private GTCPanel _gtcPanel;


        protected override void ResetValues()
        {
            IsStartTutorialButtonPressed = false;
            _tutorialUIController = null;
            _gtcPanel = null;
        }

        protected override IEnumerator StartSequence()
        {
            _tutorialUIController = InstantiateWrapper.InstantiateWithMessage(_tutorialUIPrefab, _ownPlayer.GameObject.CheckForNull()?.transform);
            _gtcPanel = (GTCPanel) _tutorialUIController.GetPanelByType(HubUIController.PanelType.GTC);
            _tutorialUIController.ToggleIngameUI(true);
            _tutorialUIController.StartTutorialButton.onClick.AddListener(OnStartButtonClicked);
            yield return null;
        }

        private void OnStartButtonClicked()
        {
            IsStartTutorialButtonPressed = true;
        }

        protected override IEnumerator EndSequence(TutorialSequence nextSequence)
        {
            _tutorialUIController.ToggleIngameUI(false);
            yield return new WaitUntil(() => _tutorialUIController.IsMenuHidden);
            _tutorialUIController.StartTutorialButton.onClick.RemoveListener(OnStartButtonClicked);
            Destroy(_tutorialUIController.gameObject);
            if (nextSequence != null) nextSequence.Init();
        }

        public override void Update()
        {
        }

        public override bool IsCompleted()
        {
            return _gtcPanel != null && (IsGTCAccepted && IsStartTutorialButtonPressed);
        }
    }
}