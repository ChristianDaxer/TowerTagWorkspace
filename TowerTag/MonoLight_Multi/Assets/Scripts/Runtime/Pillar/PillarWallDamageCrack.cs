using System.Collections;
using SOEventSystem.References;
using TowerTag;
using UnityEngine;

/// <summary>
/// Controls the visual cracks on the wall, according to it's damage value. To be placed on a single Wall.
/// </summary>
/// <author>Sebastian Krebs (sebastian.krebs@vrnerds.de)</author>
[RequireComponent(typeof(MeshRenderer), typeof(WallViewRigidBody))]
public class PillarWallDamageCrack : PillarWallView {
    [SerializeField] private Vector3Reference _threshold;
    [SerializeField] private Vector3Reference _intensity;

    private MeshRenderer _meshRenderer;
    private MeshRenderer MeshRenderer => _meshRenderer != null
        ? _meshRenderer
        : _meshRenderer = GetComponent<MeshRenderer>();

    private WallViewRigidBody _wallViewRigidBody;

    private MaterialPropertyBlock _propertyBlock;

    private Vector2 _uvOffset;
    private static readonly int _layerBlendFactors = Shader.PropertyToID("_LayerBlendFactors");
    private static readonly int _tintColor = Shader.PropertyToID("_TintColor");
    private static readonly int _offset = Shader.PropertyToID("_UVOffset");

    /// <inheritdoc />
    /// <summary>
    /// Initialize Component & check MeshRenderer
    /// </summary>
    protected override void Awake() {
        base.Awake();

        _uvOffset = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f));
        _wallViewRigidBody = GetComponent<WallViewRigidBody>();
        Debug.Assert(_wallViewRigidBody,
            "No WallView_RigidBody found. This instance of PillarWallDamageCrack uses the default Reset Value.");
        enabled = false;
    }

    /// <inheritdoc />
    /// <summary>
    /// Override inherited Method and Update Damage Cracks on Wall Damage has Changed.
    /// </summary>
    /// <param name="oldDamage">old suffered Wall Damage</param>
    /// <param name="newDamage">new suffered Wall Damage</param>
    protected override void OnWallDamageChanged(float oldDamage, float newDamage) {
        UpdateWallCracks(newDamage);
    }


    /// <inheritdoc />
    /// <summary>
    /// Override inherited Method and Reset Wall-Cracks View on reached Max suffered Wall Damage
    /// </summary>
    protected override void OnWallReachedMaxDamage() {
        if (!_wallViewRigidBody.IsDown) StartCoroutine(ResetWallCracks());
    }

    public override void Reset() {
        OnWallDamageChanged(PillarWall.Damage, PillarWall.Damage);
    }

    /// <inheritdoc />
    /// <summary>
    /// Override inherited Method with empty Method
    /// </summary>
    /// <param name="position"></param>
    /// <param name="force"></param>
    protected override void OnForceWasApplied(Vector3 position, Vector3 force) { }

    /// <summary>
    /// Create or Update Wall Material Property Block and Set Wall View Damage Cracks depends on Wall condition
    /// </summary>
    /// <param name="value">Inverse Lerp Value between start and end</param>
    private void UpdateWallCracks(float value) {
        if (_propertyBlock == null) {
            _propertyBlock = new MaterialPropertyBlock();
        }

        // gather previously set properties
        MeshRenderer.GetPropertyBlock(_propertyBlock);

        // calculate layer intensity in [0,1]
        Vector3 layerIntensity;
        layerIntensity.x = Mathf.InverseLerp(_threshold.Value.x, 1f, value);
        layerIntensity.y = Mathf.InverseLerp(_threshold.Value.y, 1f, value);
        layerIntensity.z = Mathf.InverseLerp(_threshold.Value.z, 1f, value);

        // scale layer intensity by user defined intensity
        layerIntensity.Scale(_intensity);

        // set values to property block
        _propertyBlock.SetVector(_layerBlendFactors, _wallViewRigidBody.IsDown ? Vector3.zero : layerIntensity);
        _propertyBlock.SetVector(_offset, _uvOffset);

        // set property block
        MeshRenderer.SetPropertyBlock(_propertyBlock);
    }

    protected override void OnOwningTeamChanged(Claimable claimable, TeamID oldTeam, TeamID newTeam, IPlayer[] newOwner) {
        if (_propertyBlock == null) {
            _propertyBlock = new MaterialPropertyBlock();
        }

        MeshRenderer.GetPropertyBlock(_propertyBlock);

        if (newTeam != TeamID.Neutral)
            _propertyBlock.SetColor(_tintColor, TeamManager.Singleton.Get(newTeam).Colors.WallCracks);

        MeshRenderer.SetPropertyBlock(_propertyBlock);
    }

    /// <summary>
    /// Co-Routine to Fade-Out Cracks while falling Animation is running & Clear Wall on reached max Damage
    /// </summary>
    IEnumerator ResetWallCracks() {
        var running = true;

        //No WallView_RigidBody found. Can´t use Wall falling Animation as time dependent trigger.
        if (_wallViewRigidBody == null) {
            running = false;
            yield return new WaitForSeconds(0.5f);
        }

        //Wait for end of Wall falling Animation when no WallView_RigidBody init
        while (running) {
            yield return new WaitForEndOfFrame();
            if (!_wallViewRigidBody.FallDownAnimationIsRunning) running = false;
        }

        StartCoroutine(FadeOutWallCracks());
    }


    private IEnumerator FadeOutWallCracks() {
        var timeRest = PillarWall.Damage;
        MeshRenderer.GetPropertyBlock(_propertyBlock);
        while (timeRest >= 0f) {
            timeRest -= Time.deltaTime * 2;
            UpdateWallCracks(timeRest);
            yield return null;
        }
        _propertyBlock.SetVector(_layerBlendFactors, Vector3.zero);
        if (MeshRenderer != null) MeshRenderer.SetPropertyBlock(_propertyBlock);
    }

//Use SO shared float and Test the Wall Damage Crack View manually
#if DEBUG_SET_DAMAGE_BY_USER
    public SharedFloat DebugDamage;

    private void Update()
    {
        UpdateWallCracks( DebugDamage.Value);
    }
#endif
}