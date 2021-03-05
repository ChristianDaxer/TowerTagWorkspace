using System.Collections;
using Home.UI;
using TowerTag;
using UnityEngine;

namespace Tutorial {
    [CreateAssetMenu(menuName = "TutorialSequences/FinishTutorialSequence", fileName = nameof(FinishTutorialSequence))]
    public class FinishTutorialSequence : TutorialSequence {
        [SerializeField] private TutorialUIController _tutorialUIPrefab;
        [SerializeField] private AudioClip _winJingle;
        private TutorialUIController _tutorialUIController;
        private bool _isTutorialFinishConfirmed;
        private TutorialSequencer _sequencer;
        private static readonly int Close = Animator.StringToHash("close");

        // Update is called once per frame
        protected override IEnumerator StartSequence() {
            _sequencer = FindObjectOfType<TutorialSequencer>();
            InitializeTutorialUI();
            _sequencer.Source.clip = _winJingle;
            _sequencer.Source.Play();
            _sequencer.EndHubWallDoor.SetTrigger(Close);
            yield return null;
        }

        private void InitializeTutorialUI() {
            _tutorialUIController = InstantiateWrapper.InstantiateWithMessage(_tutorialUIPrefab, _ownPlayer.GameObject.CheckForNull()?.transform);
            _tutorialUIController.ToggleIngameUI(true);
            
            _tutorialUIController.SwitchPanel(HubUIController.PanelType.TutorialGameTips);
            
            _tutorialUIController.GameTipsCompleteButton.onClick.AddListener(OnGameTipsCompleteButtonPressed);
            _tutorialUIController.TutorialFinishedButton.onClick.AddListener(OnTutorialFinishedButtonPressed);
        }

        private void OnGameTipsCompleteButtonPressed()
        {
            _tutorialUIController.SwitchPanel(HubUIController.PanelType.FinishTutorial);
        }

        private void OnTutorialFinishedButtonPressed() {
            _isTutorialFinishConfirmed = true;
        }

        protected override IEnumerator EndSequence(TutorialSequence nextSequence) {
            _tutorialUIController.ToggleIngameUI(false);
            yield return new WaitUntil(() => _tutorialUIController.IsMenuHidden);
            Destroy(_tutorialUIController);
            _tutorialUIController.GameTipsCompleteButton.onClick.RemoveAllListeners();
            _tutorialUIController.TutorialFinishedButton.onClick.RemoveAllListeners();
            ConnectionManager.Instance.LeaveRoom();
            PlayerPrefs.SetInt(PlayerPrefKeys.Tutorial, 1);
            if (TowerTagSettings.Home)
                if (Achievements.GetInstance(out var achievements))
                    AchievementManager.SetAchievement(achievements.Keys.TutorialDone);
            yield return null;
            if (nextSequence != null) nextSequence.Init();
        }

        public override void Update() {
        }

        public override bool IsCompleted() {
            return _isTutorialFinishConfirmed;
        }

        protected override void ResetValues()
        {
            _isTutorialFinishConfirmed = false;
            _tutorialUIController = null;
            _sequencer = null;
        }
    }
}