using Home.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Tutorial{
    [RequireComponent(typeof(BadaboomHyperactionPointerNeeded))]
    public class TutorialUIController : HubUIController
    {
        private BadaboomHyperactionPointerNeeded _badaboomHyperactionPointerNeeded;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private Button _startTutorialButton;
        [SerializeField] private Button _gameTipsCompleteButton;
        [SerializeField] private Button _tutorialFinishedButton;

        public Button StartTutorialButton => _startTutorialButton;

        public Button GameTipsCompleteButton => _gameTipsCompleteButton;
        public Button TutorialFinishedButton => _tutorialFinishedButton;

        public bool IsMenuHidden => _animator.GetCurrentAnimatorStateInfo(0).IsName("HiddenMenu");

        private void Awake()
        {
            _badaboomHyperactionPointerNeeded = GetComponent<BadaboomHyperactionPointerNeeded>();
            SwitchPanel(PanelType.GTC);
            _canvas.worldCamera = FindObjectOfType<PhysicsRaycaster>().GetComponent<Camera>();
        }

        protected override void ActivateButtons()
        {
        }

        protected override void DeactivateButtons()
        {
        }

        public override void TogglePointerNeededTag(object sender, bool status, bool immediately) {
            if (_badaboomHyperactionPointerNeeded != null)
                _badaboomHyperactionPointerNeeded.enabled = IngameUIActive;
        }
    }
}