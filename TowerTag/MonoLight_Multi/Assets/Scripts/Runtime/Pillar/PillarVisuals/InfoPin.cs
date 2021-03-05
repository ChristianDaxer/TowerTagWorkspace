using System.Collections;
using System.Collections.Generic;
using TMPro;
using TowerTag;
using TowerTagSOES;
using UnityEngine;

public class InfoPin : MonoBehaviour {
    [SerializeField] private Animator _animator;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private TextMeshPro _frontText;
    [SerializeField] private TextMeshPro _backText;
    [SerializeField] private LineRenderer[] _lineRenderer;
    [SerializeField] private MeshRenderer[] _meshRenderer;
    private Transform _cameraTransform;

    private readonly Dictionary<GameMode, (string yourTeamText, string enemyTeamText)> _infoPinText = new Dictionary<GameMode, (string yourTeamText, string enemyTeamText)> {
        {GameMode.GoalTower, ("Your Goal Tower", "Enemy Goal Tower")},
        {GameMode.Elimination, ("Your Team Base", "Enemy Team Base")},
        {GameMode.DeathMatch, ("Your Team Base", "Enemy Team Base")},
    };

    private static readonly int _end = Animator.StringToHash("End");
    private static readonly int _start = Animator.StringToHash("Start");
    private static readonly int _property = Shader.PropertyToID("_FaceColor");

    /// <summary>
    /// Initializes the info pins
    /// </summary>
    /// <param name="pillar">The Pillar the info pin is according to</param>
    /// <param name="mode">The game mode of the current match</param>
    public void Init(Pillar pillar, GameMode mode) {
        switch (mode) {
            case GameMode.GoalTower:
                IPlayer ownPlayer = PlayerManager.Instance.GetOwnPlayer();
                if (ownPlayer == null) {
                    Debug.LogError("Cannot initialize InfoPin: Own player not found");
                    return;
                }
                ITeam team = TeamManager.Singleton.Get(ownPlayer.TeamID);
                bool accordingToOwnTeam = team.ID == pillar.OwningTeamID;
                SetTexts(accordingToOwnTeam
                    ? _infoPinText[mode].yourTeamText
                    : _infoPinText[mode].enemyTeamText);
                ColorizeComponents(pillar.OwningTeam);
                break;
            case GameMode.Elimination:
            case GameMode.DeathMatch:
                SetTexts(_infoPinText[mode].enemyTeamText);
                break;
        }

        if (Camera.main != null) {
            _cameraTransform = Camera.main.transform;
            SetPinRotation();
        }
        _frontText.enableCulling = true;
        _backText.enableCulling = true;
    }

    private void ColorizeComponents(ITeam pillarOwningTeam) {
        Material material = TeamMaterialManager.Singleton.GetFlatUI(pillarOwningTeam.ID);
        _lineRenderer.ForEach(lr => lr.material = material);
        _meshRenderer.ForEach(mr => mr.material = material);
        SetTextColor(_frontText, material.color);
        SetTextColor(_backText, material.color);
    }

    private void SetTexts(string text) {
        _frontText.text = text;
        _backText.text = text;
    }

    private void SetTextColor(TextMeshPro text, Color color) {
        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        text.renderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor(_property, color);
        text.renderer.SetPropertyBlock(propertyBlock);
    }

    /// <summary>
    /// Setting the rotation of the info pin to the players position with x = 0
    /// Depending on the pins position it rotates the pin 180° in the y-axis to make it spawn from left or right
    /// </summary>
    private void SetPinRotation() {
        if (!SharedControllerType.IsAdmin) {
            Transform pinTransform = transform;
            var camPosition = _cameraTransform.position;
            var pinPosition = pinTransform.position;
            Vector3 cameraSidePos = new Vector3(pinPosition.x, pinPosition.y, camPosition.z);
            pinTransform.rotation = Quaternion.LookRotation(pinPosition - cameraSidePos);
            Vector3 direction = pinPosition - camPosition;
            if (direction.z < 0 && direction.x >= 0 || direction.z >= 0 && direction.x < 0)
                    pinTransform.localEulerAngles = new Vector3(0, pinTransform.localEulerAngles.y + 180, 0);
        }
    }

    public void StartAnimation() {
        _animator.SetTrigger(_start);
        _audioSource.PlayDelayed(1);
    }

    public IEnumerator EndAnimation(float waitDuration) {
        yield return new WaitForSeconds(waitDuration);
        if(_animator != null)
            _animator.SetTrigger(_end);
    }
}
