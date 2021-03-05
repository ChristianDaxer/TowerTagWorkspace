using TowerTag;
using UnityEngine;

public class SimpleRopeIntersectionParticlesWrapper : MonoBehaviour {
    [Header("Sparks")] [SerializeField] private ParticleSystem _particles;

    [SerializeField] private float _particlesEmissionRateFactor = 20;

    [SerializeField] private string _particleMaterialColor1PropertyName = "_TintColor";

    [SerializeField] private string _particleMaterialColor2PropertyName = "_FresnelColor";

    private int _particleMaterialColor1PropertyID;
    private int _particleMaterialColor2PropertyID;

    [Header("Smoke")] [SerializeField] private ParticleSystem _smoke;

    [SerializeField] private float _smokeEmissionRateFactor = 20;

    public bool IsActive { get; private set; }

    public void Init() {
        _particleMaterialColor1PropertyID = Shader.PropertyToID(_particleMaterialColor1PropertyName);
        _particleMaterialColor2PropertyID = Shader.PropertyToID(_particleMaterialColor2PropertyName);
    }

    // Set strength of ParticleEffects
    public void SetStrength(float strength) {
        if (_particles != null) {
            ParticleSystem.EmissionModule emission = _particles.emission;
            emission.rateOverTimeMultiplier = strength * _particlesEmissionRateFactor;
        }
        else {
            Debug.LogWarning("Cannot set strength of particles: particles are null");
        }

        if (_smoke != null) {
            ParticleSystem.EmissionModule emission = _smoke.emission;
            emission.rateOverTimeMultiplier = strength * _smokeEmissionRateFactor;
        }
        else {
            Debug.LogWarning("Cannot set smoke strength: smoke is null");
        }
    }

    public void SetActive(bool setActive) {
        if (IsActive != setActive) {
            if (_particles != null) {
                if (setActive)
                    _particles.Play();
                else
                    _particles.Stop();
            }
            else {
                Debug.LogWarning("Failed to toggle particles");
            }

            if (_smoke != null) {
                if (setActive)
                    _smoke.Play();
                else
                    _smoke.Stop();
            }
            else {
                Debug.LogWarning("Failed to toggle smoke");
            }
        }

        IsActive = setActive;
    }

    // Change Material properties if Team changed
    public void OnTeamChanged(TeamID teamID) {
        if (_particles == null) {
            Debug.LogError("Particles are null");
            return;
        }

        ColorChanger.ChangeColorInChildRendererComponents(_particles.gameObject,
            TeamManager.Singleton.Get(teamID).Colors.Main,
            _particleMaterialColor1PropertyID, true);
        ColorChanger.ChangeColorInChildRendererComponents(_particles.gameObject,
            TeamManager.Singleton.Get(teamID).Colors.Emissive,
            _particleMaterialColor2PropertyID, true);
    }
}