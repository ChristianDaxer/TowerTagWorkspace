using System.Collections;
using TowerTag;
using TowerTagSOES;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VolumetricLines;

/// <summary>
/// Controls the visual representation of a shot. Manages the shot length to compensate for latency.
/// <br/><br/>
/// <b>Explanation:</b> <br/>
/// Say a shot has a speed of 35 m/s and is fired by some remote player. When the message reaches the local client,
/// the shot will have a spawn time in the past, depending on the latency. This can easily be 200 ms, which
/// corresponds to 7m. If the shot is just 1m long, it will appear midair, or worse not at all, because it is
/// already behind the camera. In order to give the illusion that the shot was created at the gun muzzle, it is
/// stretched to 7m length and shrinks to its usual length in a certain amount of time after some delay.
/// </summary>
/// <author>Ole Jürgensen (ole@vrnerds.de)</author>
public class ShotModel : MonoBehaviour {
    [SerializeField] private Shot _shot;
    [FormerlySerializedAs("_setpointLength")] [SerializeField] private float _setPointLength;
    [SerializeField] private float _shrinkTime;
    [SerializeField] private float _shrinkDelay;
    [SerializeField] private VolumetricLineBehavior _volumetricLineBehavior;

    [SerializeField, Tooltip("Renderer the colors of which are adapted to the team color.")]
    private Renderer[] _renderers;

    [SerializeField, Tooltip("Lights the colors of which are adapted to the team color.")]
    private Light[] _lights;

    [SerializeField, Tooltip("Images the colors of which are adapted to the team color.")]
    private Image[] _images;

    private int _tintColorPropertyId;

    private void Awake() {
        _tintColorPropertyId = Shader.PropertyToID("_Color");
    }

    private void OnEnable() {
        if (_shot != null) _shot.Fired += OnShotFired;
    }

    private void OnDisable() {
        if (_shot != null) _shot.Fired -= OnShotFired;
        StopAllCoroutines();
    }

    private void OnShotFired() {
        ChangeProjectileColor();
        float length = Vector3.Distance(transform.position, _shot.SpawnPosition);
        StopAllCoroutines();
        StartCoroutine(AdaptLength(length));
    }

    private void ChangeProjectileColor() {
        Color teamColor = Color.red;
        if (_shot.Player != null) {
            ITeam team = TeamManager.Singleton.Get(_shot.Player.TeamID);
            if (team != null) teamColor = team.Colors.Effect;
        }

        ChangeProjectileColor(teamColor);
    }

    private void ChangeProjectileColor(Color teamColor) {
        ColorChanger.ChangeColorInRendererComponents(_renderers, teamColor, _tintColorPropertyId, true);
        ColorChanger.ChangeColorInLightComponents(_lights, teamColor);
        if(SharedControllerType.IsAdmin)
            _images.ForEach(img => img.color = teamColor);
    }

    private IEnumerator AdaptLength(float length) {
        float currentLength = length;
        SetLength(currentLength);

        while (currentLength < _setPointLength) {
            currentLength = Mathf.Min(Vector3.Distance(transform.position, _shot.SpawnPosition), _setPointLength);
            SetLength(currentLength);
            yield return null;
        }

        yield return new WaitForSeconds(_shrinkDelay);

        while (currentLength > _setPointLength) {
            currentLength = Mathf.Lerp(currentLength, _setPointLength, Time.deltaTime / _shrinkTime);
            SetLength(currentLength);
            yield return null;
        }
    }

    private void SetLength(float length) {
        _volumetricLineBehavior.StartPos = -length * Vector3.forward;
        _volumetricLineBehavior.EndPos = Vector3.zero;
    }
}