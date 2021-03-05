using System;
using System.Linq;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;
using UnityEngine.UI;

namespace Home.UI {
    public abstract class HubUIController : MonoBehaviour {
        public delegate void IngameUIControllerAction(object sender, bool status, bool immediately);

        public static event IngameUIControllerAction VRIngameUIToggled;
        
        [Serializable]
        public struct PanelHeadlinePair {
            public RectTransform Panel;
            public string HeadlineText;
        }

        [Serializable]
        public class PanelDictionary : SerializableDictionaryBase<PanelType, PanelHeadlinePair> {
        }

        [SerializeField] protected Text _headlineText;
        [SerializeField] protected PanelDictionary _panelsDictionary;
        [SerializeField] protected BadaboomHyperactionPointer _pointerPrefab;
        [SerializeField] protected IngameMenuAnimationEventHandler _menuAnimationEventHandler;

        [Header("Animation")] [SerializeField] protected Animator _animator;
        protected const string SpawnedState = "MainMenuSpawn";
        protected const string DespawnState = "MainMenuDespawn";
        protected const string Spawn = "spawn";
        private const string Despawn = "despawn";
        
        [Header("Audio")] [SerializeField] protected AudioSource _source;
        [SerializeField] protected AudioClip _panelSwitchSound;
        protected OverlayCanvasModel OverlayCanvasModel;
		protected BadaboomHyperactionPointer _pointer;

		protected bool IngameUIActive { get; set; }


        [Serializable]
        public enum PanelType {
            MainMenu,
            FindMatch,
            Settings,
            CreateMatch,
            Training,
            Statistics,
            Loading,
            GTC,
            StartTutorial,
            FinishTutorial,
            Friends,
            RegionSetting,
            Setup,
            KickReport,
            TutorialGameTips
        }

        protected abstract void ActivateButtons();
        protected abstract void DeactivateButtons();
        public abstract void TogglePointerNeededTag(object sender, bool status, bool immediately);


        protected void CheckOverlayCanvas() {
            if (OverlayCanvasModel.Canvas.enabled)
                DeactivateButtons();
            else
                ActivateButtons();
        }

        protected void OnEnable() {
            VRIngameUIToggled += TogglePointerNeededTag;
        }

        protected void OnDisable() {
            VRIngameUIToggled -= TogglePointerNeededTag;
        }

        public void ToggleIngameUI(bool status, bool immediately = false) {
            if (status == IngameUIActive) return;
            _animator.SetTrigger(status ? Spawn : Despawn);
            IngameUIActive = status;
            if (status && !BadaboomHyperactionPointer.GetInstance(out _pointer)) {
                _pointer = InstantiateWrapper.InstantiateWithMessage(_pointerPrefab);
            }

            VRIngameUIToggled?.Invoke(this, IngameUIActive, immediately);
        }

        public HomeMenuPanel GetPanelByType(PanelType panelType) {
            return _panelsDictionary[panelType].Panel.GetComponent<HomeMenuPanel>();
        }

        public void SwitchPanel(PanelType panelType, bool withSound = true) {
            _headlineText.text = _panelsDictionary[panelType].HeadlineText;
            _panelsDictionary.Where(panel => panel.Key != panelType)
                .ForEach(panel => panel.Value.Panel.gameObject.SetActive(false));
            _panelsDictionary[panelType].Panel.gameObject.SetActive(true);

            //Don't play the sound at the start of the application! Sound level not loaded!
            if (withSound)
                PlayAudioSound(_panelSwitchSound);
        }

        public void PlayAudioSound(AudioClip clip) {
            _source.clip = clip;
            _source.Play();
        }
    }
}