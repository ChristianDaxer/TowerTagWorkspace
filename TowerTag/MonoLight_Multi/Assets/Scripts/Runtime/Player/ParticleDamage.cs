using TowerTag;
using UnityEngine;
using UnityEngine.Serialization;

public class ParticleDamage : FloatVisuals {
    [FormerlySerializedAs("_main")] [SerializeField]
    private ParticleSystem _particleSystem;

    [FormerlySerializedAs("minMaxEmission")] [SerializeField]
    private Vector2 _minMaxEmission;

    [SerializeField]
    private AnimationCurve _emissionCurve;

    [FormerlySerializedAs("minMaxStartSize")] [SerializeField]
    private Vector2 _minMaxStartSize;

    [SerializeField] private AnimationCurve _startSizeCurve;

    [SerializeField] private ParticleSystem _addOn;

    public override void SetValue(float newValue) {
        if (_particleSystem != null) {
            if (newValue < 0.9f) {
                if (!_particleSystem.isPlaying)
                    _particleSystem.Play();

                ParticleSystem.EmissionModule emission = _particleSystem.emission;
                ParticleSystem.MainModule particleSystemMain = _particleSystem.main;

                emission.rateOverTime = Mathf.Lerp(_minMaxEmission.x, _minMaxEmission.y,
                    _emissionCurve.Evaluate(1f - newValue));
                particleSystemMain.startSizeMultiplier = Mathf.Lerp(_minMaxStartSize.x, _minMaxStartSize.y,
                    _startSizeCurve.Evaluate(1f - newValue));
                if(newValue <= 0) {
                    if (_particleSystem.isPlaying)
                        _particleSystem.Stop();
                }
            }
            else {
                if (_particleSystem.isPlaying)
                    _particleSystem.Stop();
            }

            if (_addOn != null) {
                if (newValue < 0.3f) {
                    if (!_addOn.isPlaying)
                        _addOn.Play();
                }
                else {
                    if (_addOn.isPlaying)
                        _addOn.Stop();
                }
            }
        }
    }

    public void SetTeamColors(ITeam team) {
        if (_particleSystem != null) {
            ColorChanger.ChangeColorInChildRendererComponents(gameObject, team.Colors.Main, true);
            ColorChanger.ChangeColorInChildLightComponents(gameObject, team.Colors.Main);
        }
    }
}