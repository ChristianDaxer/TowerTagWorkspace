using JetBrains.Annotations;
using TowerTag;
using UnityEngine;

namespace Home.UI {
    public class TutorialPanel : HomeMenuPanel {
        [UsedImplicitly]
        public void OnTutorialButtonPressed() {
            GameManager.Instance.StartTutorial(false);
        }

        [UsedImplicitly]
        public void OnSkipButtonPressed() {
            ConnectionManager.Instance.LeaveRoom();
            PlayerPrefs.SetInt(PlayerPrefKeys.Tutorial, 1);
        }
    }
}