using TMPro;
using TowerTag;
using TowerTagSOES;
using UnityEngine;

namespace Commendations {
    /// <summary>
    /// Visual representation of a <see cref="Commendation"/> awarded to some <see cref="IPlayer"/>.
    ///
    /// <author>Ole Jürgensen (ole@vrnerds.de)</author>
    /// </summary>
    public class CommendationView : MonoBehaviour {
        [Header("Data to display")] [SerializeField, Tooltip("The awarded commendation")]
        private Commendation _commendation;

        [SerializeField, Tooltip("The name of the player awarded with the commendation")]
        private string _playerName;

        [SerializeField, Tooltip("The team defines the color of the UI elements")]
        private TeamID _teamID;

        [Header("UI components")] [SerializeField, Tooltip("The UI component to display the commendation name")]
        private TextMeshPro _commendationNameText;

        [SerializeField, Tooltip("The UI component to display the commendation description")]
        private TextMeshPro _commendationDescriptionText;

        [SerializeField, Tooltip("The UI component to display the player name")]
        private TextMeshPro _playerNameText;

        [SerializeField, Tooltip("The UI component to render the commendation icon")]
        private SpriteRenderer _iconRenderer;

        [SerializeField, Tooltip("The UI component to render the separator sprite")]
        private SpriteRenderer _separatorRenderer;

        [Header("Team-specific Materials")] [SerializeField, Tooltip("Icon Material for team ice")]
        private Material _iceIconMaterial;

        [SerializeField, Tooltip("Icon Material for team ice")]
        private Material _fireIconMaterial;

        private IPlayer _player;

        private void OnValidate() {
            if (gameObject.scene.buildIndex == -1) return;
            Refresh();
        }

        private void OnEnable() {
            if (_player != null) _player.TeleportHandler.PlayerTeleporting += OnPlayerTeleporting;
        }

        private void OnDisable() {
            if (_player != null) _player.TeleportHandler.PlayerTeleporting -= OnPlayerTeleporting;
        }

        /// <summary>
        /// Display a <see cref="Commendation"/> awarded to a <see cref="IPlayer"/>.
        /// </summary>
        /// <param name="commendation">The awarded commendation</param>
        /// <param name="player">The player that is awarded</param>
        public void Set(Commendation commendation, IPlayer player) {
            if (_player != null) _player.TeleportHandler.PlayerTeleporting -= OnPlayerTeleporting;
            _commendation = commendation;
            _player = player;
            _playerName = player.PlayerName;
            _teamID = player.TeamID;
            _player.TeleportHandler.PlayerTeleporting += OnPlayerTeleporting;
            if (player.CurrentPillar != null) {
                OnPlayerTeleporting(player.TeleportHandler, null, player.CurrentPillar, 0);
                SetPillar(player.CurrentPillar);
            }

            Refresh();
        }

        private void OnPlayerTeleporting(TeleportHandler sender, Pillar origin, Pillar pillar, float teleportTime) {
            SetPillar(pillar);
        }

        private void SetPillar(Pillar pillar) {
            Transform pillarTransform = pillar.transform;
            Transform thisTransform = transform;
            thisTransform.position = pillarTransform.position;
            thisTransform.rotation = pillarTransform.rotation;
            thisTransform.localScale =
                new Vector3(SharedControllerType.IsAdmin || SharedControllerType.Spectator ? 1 : -1, 1, 1);
        }

        private void Refresh() {
            Material material = _teamID == TeamID.Fire ? _fireIconMaterial : _iceIconMaterial;
            if (_commendation == null) {
                _commendationNameText.text = "Awesome!";
                _commendationDescriptionText.text = "You Are Awesome!";
                _iconRenderer.sprite = null;
                _iconRenderer.material = material;
                _separatorRenderer.material = material;
            }
            else {
                _commendationNameText.text = _commendation.DisplayName;
                _commendationDescriptionText.text = _commendation.Description;
                _iconRenderer.sprite = _commendation.Icon;
                _iconRenderer.material = material;
                _separatorRenderer.material = material;
            }

            _separatorRenderer.gameObject.SetActive(_commendationNameText.text != "");
            _playerNameText.text = _playerName;
            _playerNameText.color = material.color;
        }
    }
}