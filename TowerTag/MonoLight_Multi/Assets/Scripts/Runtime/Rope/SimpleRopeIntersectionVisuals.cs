using System.Linq;
using Rope;
using TowerTag;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;


// TODO:    check positions (projector lighting differs)
//          check Particle On/Off (sometimes doesn't spawn), inner
//          
public class SimpleRopeIntersectionVisuals : MonoBehaviour {
    #region Member

    [Header("Collision Detection")] [SerializeField, Tooltip("Layers the rope should collide with.")]
    private LayerMask _collisionMask;

    [SerializeField, Tooltip("Number of sample points per Curve.")]
    private int _curveSampleCount = 10;

    [SerializeField, Tooltip("Factor multiplied with curveIndex and curveSampleCount to create more points on curves " +
                             "near the Player and less on curves away from the Player.")]
    private float _curveSampleCountFallOffFactor = 1f;

    [SerializeField, Tooltip("Radius of the Rope used for collision detection " +
                             "(use a value which is a little bit bigger than the size of the Projectors Mask-Texture).")]
    private float _ropeRadius = .1f;
	
	public Transform _spawnBeamAnchor;
	//[FormerlySerializedAs("rope")] [SerializeField, Tooltip("The Rope used by the Player.")]
	private ChargerRopeRenderer _rope;

    [SerializeField, Tooltip("Should we use the simple algorithm to place decals?")]
    private bool _useSimpleCollisionCalculations;

    [SerializeField, Tooltip("How much should ropeDirection and particle direction align to assume " +
                             "the rope is parallel to the collider?")]
    private float _directionThreshold = 0.75f;


    [Header("Decals")] [SerializeField, Tooltip("Size of the decal pool (maximum number of decals to show at once).")]
    private int _decalCount;

    [ReadOnly, Tooltip("Number of currently used decal objects.")]
    private int _activeDecalCount;


    [Header("Projector Decals")] [SerializeField, Tooltip("Prefab used for projector decals.")]
    private GameObject _projectorDecalPrefab;

    [SerializeField, Tooltip("Name of the shader property to set the color ramp texture to.")]
    private string _decalMaterialColorRampPropertyName = "_ColorRamp";

    [SerializeField, Tooltip("Material used for the Projector decals " +
                             "(we make a copy on the run so we can change properties for Teams).")]
    private Material _decalPrefabMaterial;

    /// <summary>
    /// Copy of decalPrefabMaterial, so we can change properties dependent of players Team.
    /// </summary>
    private Material _decalMaterial;

    /// <summary>
    /// Cached projector decals spawned in CreateDecals.
    /// </summary>
    private Transform[] _projectorDecals;

    [SerializeField] private GameObject ropePrefab;


    [Header("Particles")]
    [SerializeField, Tooltip("Prefab used to create Particles (the prefab has to have a " +
                             "SimpleRopeIntersectionParticleWrapper-Script on it!)")]
    private GameObject _particlePrefab;

    [SerializeField, Tooltip("Factor multiplied with ropeRadius to define " +
                             "max distance to collider to spawn Particles.")]
    public float _ropeRadiusFactorForParticleStrength = 0.5f;

    [SerializeField, Tooltip("Minimum distance between two decals.")]
    private float _minDistanceBetweenDecals = 0.024f;

    [SerializeField, Tooltip("Offset used to detect intersection with hull (used to offset point in line cast).")]
    private float _hullCollisionOffset = 0.001f;

    /// <summary>
    /// Cached particle decals spawned in CreateDecals.
    /// </summary>
    private SimpleRopeIntersectionParticlesWrapper[] _particles;

    // ***** cache some variables we use only in AdvancedCollisionHandling function *****
    /// <summary>
    /// holds collider returned from OverlapCapsuleNonAlloc
    /// </summary>
    private Collider[] _collider;

    /// <summary>
    /// holds the last line cast hit result
    /// </summary>
    private RaycastHit _hitInfo;

    /// <summary>
    /// quick and dirty: factor to make the capsule a little bit smaller to prevent overshooting
    ///      used as fraction of the ropeRadius
    /// </summary>
    private const float CollisionCapsuleRadiusOffsetMultiplier = 1.1f;

	#endregion


	#region Init & Cleanup

	public void InitVisuals()
	{
		if (!_rope)
		{
        /*
#if UNITY_ANDROID
			var ropePrefab = Resources.Load<GameObject>("FX_LineRendererRope");
#else
			var ropePrefab = Resources.Load<GameObject>("FX_TessellatedRope");
#endif
        */
			if (ropePrefab)
			{
				_rope = InstantiateWrapper.InstantiateWithMessage(ropePrefab, transform, false).GetComponent<ChargerRopeRenderer>();
				if (_rope)
					_rope.SpawnBeamAnchor = _spawnBeamAnchor;
			}
		}
	}

	/// <summary>
	/// Init simple RopeIntersection system
	/// </summary>
	/// <param name="player">Player for whose rope the intersections are visualized</param>
	public void Init(IPlayer player)
	{
		InitVisuals();

		if (_rope == null)
            enabled = false;

		if (_decalCount > 0)
		{
			_collider = new Collider[_decalCount];

			CreateDecals(_decalCount);
		}
		player.PlayerTeamChanged += OnTeamChanged;
        OnTeamChanged(player, player.TeamID);
        DeactivateAllDecals();
    }

    private void OnDestroy() {
        DestroyDecals();
        _collider = null;
    }

    private void Update() {
		if (_rope)
			UpdateDecals();
    }

    /// <summary>
    /// Create decal pool so we don't need runtime Instantiation.
    /// </summary>
    /// <param name="decalCount">Number of decals to create.</param>
    private void CreateDecals(int decalCount) {
        // cleanup before we create new
        DestroyDecals();

        // projector decals
        // create copy of original material so we can tint it with TeamColors
        if (_decalPrefabMaterial != null) {
            _decalMaterial = new Material(_decalPrefabMaterial) { name = _decalPrefabMaterial.name + "_copy" };
        } else
            Debug.LogError("SimpleRopeIntersectionVisuals.CreateDecals: decalPrefabMaterial is null!");

        // create & Init projector decals
        _projectorDecals = new Transform[decalCount];
        for (var i = 0; i < _projectorDecals.Length; i++) {
            // create decal instance
            _projectorDecals[i] =
                InstantiateWrapper.InstantiateWithMessage(_projectorDecalPrefab.transform, EffectDatabase.Instance.transform);

            // apply Material copy
            if (_decalMaterial != null) {
                var proj = _projectorDecals[i].GetComponentInChildren<Projector>(true);
                if (proj != null) {
                    proj.material = _decalMaterial;
                }
            }
        }

        // particle system decals
        // create & Init particle system decals
        _particles = new SimpleRopeIntersectionParticlesWrapper[decalCount];
        for (var i = 0; i < _particles.Length; i++) {
            // create decal instance
            GameObject particleInstance = InstantiateWrapper.InstantiateWithMessage(_particlePrefab, EffectDatabase.Instance.transform);
            _particles[i] = particleInstance.GetComponentInChildren<SimpleRopeIntersectionParticlesWrapper>();
            _particles[i].Init();
        }
    }

    /// <summary>
    /// Cleanup all created decals, material copies etc.
    /// </summary>
    private void DestroyDecals() {
        // decals
        // destroy old decal Material
        if (_decalMaterial != null)
            Destroy(_decalMaterial);

        // destroy old projector decals
        if (_projectorDecals != null) {
            foreach (Transform decal in _projectorDecals) {
                if (decal != null)
                    Destroy(decal.gameObject);
            }
        }

        // particles
        // destroy old particle decals
        if (_particles != null) {
            foreach (SimpleRopeIntersectionParticlesWrapper wrapper in _particles) {
                if (wrapper != null)
                    Destroy(wrapper.gameObject);
            }
        }
    }

#endregion

#region Core (Collision Checks and Placing Decals)

    /// <summary>
    /// Updates decals (heavy use of Physics system to check for collisions of rope with other collider to place decals)
    /// place & activate decals for each collision
    /// deactivate the rest of the decals (the active ones we have no collision for)
    /// </summary>
    private void UpdateDecals() {
        if (_rope == null) {
            Debug.LogError("SimpleRopeIntersectionVisuals.UpdateDecals: can't update decals: Rope is null!");
            enabled = false;
            return;
        }

        // deactivate all if rope is inactive
        if (!_rope.IsConnected) {
            DeactivateAllDecals();
            return;
        }

        // calculate collisions and place decals
        int placedDecals = PlaceDecals();

        // deactivate the rest of the decals (the active ones we have no collision for)
        if (placedDecals < _activeDecalCount) {
            for (int i = placedDecals; i < _activeDecalCount; i++) {
                SetDecalActive(_projectorDecals[i], false);
                SetParticleSystemActive(i, false);
            }
        }

        // remember how many decals we set active
        _activeDecalCount = placedDecals;
    }

    /// <summary>
    /// Iterates over all curve segments of all curves to check for collisions and places decals.
    /// </summary>
    /// <returns></returns>
    private int PlaceDecals() {
        // remember how many decals we placed
        var placedDecals = 0;

        // get current rope curves
        Vector3[][] curves = _rope.Conf.GetPatches(_rope.RPI).ToArray();

        // iterate over all curves
        for (int i = curves.Length - 1; i >= 0; i--) {
            // calculate number of points per curve
            int numberOfSamplePointsPerCurve = _curveSampleCount * ((int) (i * _curveSampleCountFallOffFactor) + 1);
            // grab current curves sample points with given point sampling (points per curve)
            Vector3[] cubicCurve = BezierUtil.AsPointSequence(curves[i], numberOfSamplePointsPerCurve).ToArray();

            // iterate over all curve segments of the current curve
            for (int k = cubicCurve.Length - 1; k >= 1; k--) {
                Vector3 startPoint = cubicCurve[k];
                Vector3 direction = cubicCurve[k - 1] - cubicCurve[k];

                placedDecals = _useSimpleCollisionCalculations
                    ? SimpleCollisionHandling(startPoint, direction, placedDecals)
                    : AdvancedCollisionHandling(startPoint, direction, placedDecals);

                if (placedDecals >= _decalCount)
                    return _decalCount;
            }
        }

        return placedDecals;
    }

#region old

    private int SimpleCollisionHandling(Vector3 startPoint, Vector3 direction, int placedDecals)
	{
		if (_collider == null)
			return 0;

		Profiler.BeginSample("SimpleRopeIntersectionVisuals.AdvancedCollisionHandling");

        // first check if the current line segment collides with any collider
        int hitCount = Physics.OverlapCapsuleNonAlloc(startPoint, startPoint + direction, _ropeRadius, _collider,
            _collisionMask);
        if (hitCount > 0) {
            // iterate over all collider we intersect with current curve segment
            for (var h = 0; h < hitCount; h++) {
                if (placedDecals < _decalCount) {
                    // calculate nearest point on Collider/BoundingBox
                    Vector3 nearestPoint = GetNearestPointOnBoundsDependentOfColliderType(_collider[h], startPoint);
                    // calculate distance to nearest point ( distance > 0 means we are on the outside of the object, distance 0 means we are on the hull of or inside of the object)
                    float distance = Vector3.Distance(nearestPoint, startPoint);

                    // are we near enough to activate decals?
                    if (distance <= _ropeRadius) {
                        // projector decals
                        direction.Normalize();
                        // set position of projector decal
                        Vector3 pointToPlaceProjectorDecal = startPoint;

                        // particles
                        // direction particleSystem is facing
                        Vector3 particleDirection = direction;

                        // distance to handle particles emission rate
                        float distanceToCollider = distance;
                        var activateParticles = true;

                        // distance > 0 means we are on the outside of the object, distance 0 means we are on the hull of or inside of the object
                        if (distance > 0) {
                            // direction the particles should face
                            particleDirection = (startPoint - nearestPoint).normalized;
                            float dot = Vector3.Dot(particleDirection, -direction);

                            // check if the direction from nearest point is aligned with rope direction
                            // if it is not aligned we assume the rope is parallel to the object
                            if (Mathf.Abs(dot) > _directionThreshold) {
                                // place projector on the hull of the object if aligned with rope (it enters on the front or back of an object)
                                pointToPlaceProjectorDecal = nearestPoint;
                                distanceToCollider = 0f;
                            }
                        }
                        else {
                            activateParticles = false;
                        }

                        PlaceProjectorDecal(placedDecals, pointToPlaceProjectorDecal, -direction);
                        PlaceParticleDecal(placedDecals, nearestPoint, particleDirection, distanceToCollider,
                            activateParticles);
                        placedDecals++;
                    }
                }
                else {
                    Profiler.EndSample();
                    return _decalCount;
                }
            }
        }

        Profiler.EndSample();
        return placedDecals;
    }

#endregion


    /// <summary>
    /// Calculates collision points for a line segment.
    /// </summary>
    /// <param name="startPoint">Start point of the line.</param>
    /// <param name="direction">Direction of the line (EndPoint := startPoint + direction).</param>
    /// <param name="placedDecals">Number of decals we placed already.</param>
    /// <returns>Number of placed decals after collision check and spawning of new decals.</returns>
    private int AdvancedCollisionHandling(Vector3 startPoint, Vector3 direction, int placedDecals) {
		if (_collider == null)
			return 0;

        Profiler.BeginSample("SimpleRopeIntersectionVisuals.AdvancedCollisionHandling");

        // first check if the current line segment collides with any collider
        Vector3 radiusOffset = CollisionCapsuleRadiusOffsetMultiplier * _ropeRadius * direction.normalized;
        int hitCount = Physics.OverlapCapsuleNonAlloc(startPoint + radiusOffset, startPoint + direction - radiusOffset,
            _ropeRadius, _collider, _collisionMask);

        // if we hit a collider -> do some more checks to evaluate if we are near the collider, in the collider or on its hull
        if (hitCount > 0) {
            // iterate over all collider we intersect with current curve segment
            for (var h = 0; h < hitCount; h++) {
                // in one case we spawn two decals so we have to leave one extra for headroom
                if (placedDecals < _decalCount - 1) {
                    Vector3 nearestPoint = GetNearestPointOnBoundsDependentOfColliderType(_collider[h], startPoint);
                    Vector3 directionToNearest = nearestPoint - startPoint;
                    float distanceToNearest = directionToNearest.magnitude;

                    // are we near the collider but still outside?
                    if (distanceToNearest > 0) {
                        // do we intersect the hull of the collider -> Attention: the direction of the LineCast(from, to) is important (does not check if we cast from inside to outside)!!!!!!
                        bool doesThisLineSegmentCutHull = Physics.Linecast(startPoint, startPoint + direction,
                            out _hitInfo, _collisionMask);

                        // we intersect -> so use position on hull (hitPoint.point)
                        if (doesThisLineSegmentCutHull) {
                            // projector looks in rope direction
                            PlaceProjectorDecal(placedDecals, _hitInfo.point, direction);
                            // particles look away from object in direction of the rope
                            PlaceParticleDecal(placedDecals, _hitInfo.point, -direction, 0, true);
                            placedDecals++;
                        }
                        // the line segment is "parallel" (not intersecting) to collider hull
                        //      -> place projector (at its own position) & particles (at the hull or at least the nearest point on the bounds -> see IsColliderValidForClosestPointCalculation)
                        else {
                            PlaceProjectorDecal(placedDecals, startPoint, direction);
                            PlaceParticleDecal(placedDecals, nearestPoint, -directionToNearest, distanceToNearest,
                                true);
                            placedDecals++;
                        }
                    }
                    // on hull
                    // inner
                    else {
                        // do we intersect the hull of the collider -> Attention: the direction of the LineCast(from, to) is important (does not check if we cast from inside to outside)!!!!!!
                        //      -> so we cast in negative direction, also move little bit backwards to check distance to hitPoint (because we inside the bounds of the collider or on its hull so the cast would not hit)
                        Vector3 offsetStartPoint =
                            (startPoint + direction) + direction.normalized * _hullCollisionOffset;
                        bool doesThisLineSegmentCutHull =
                            Physics.Linecast(offsetStartPoint, startPoint, out _hitInfo, _collisionMask);

                        // we cut object -> so we are on the objects hull
                        if (doesThisLineSegmentCutHull &&
                            Vector3.Distance(_hitInfo.point, offsetStartPoint) >= _hullCollisionOffset) {
                            // ensure we don't spawn objects to close to each other when we create a new point on the hull
                            if (Vector3.Distance(startPoint, _hitInfo.point) > _minDistanceBetweenDecals) {
                                PlaceProjectorDecal(placedDecals, startPoint, direction);
                                PlaceParticleDecal(placedDecals, startPoint, direction, 0, false);
                                placedDecals++;
                            }

                            // create new decal on the hull
                            PlaceProjectorDecal(placedDecals, _hitInfo.point, direction);
                            PlaceParticleDecal(placedDecals, _hitInfo.point, direction, 0, true);
                            placedDecals++;
                        }
                        // we are in the object
                        else {
                            PlaceProjectorDecal(placedDecals, startPoint, direction);
                            PlaceParticleDecal(placedDecals, _hitInfo.point, direction, 0, false);
                            placedDecals++;
                        }
                    }
                }
                else {
                    Profiler.EndSample();
                    return _decalCount;
                }
            }
        }

        Profiler.EndSample();
        return placedDecals;
    }

    /// <summary>
    /// Set position and direction of the projector decal with given index.
    /// </summary>
    /// <param name="decalIndex">Index of the decal to set.</param>
    /// <param name="position">Position of the decal.</param>
    /// <param name="direction">Direction of the decal (look direction).</param>
    private void PlaceProjectorDecal(int decalIndex, Vector3 position, Vector3 direction) {
        if (_projectorDecals == null) {
            Debug.LogError("decals array is null");
            return;
        }

        if (decalIndex < 0 || decalIndex > _projectorDecals.Length) {
            Debug.LogError("decal index out of range");
            return;
        }

        if (_projectorDecals[decalIndex] == null) {
            Debug.LogError("decal is null");
            return;
        }

        Profiler.BeginSample("SimpleRopeIntersectionVisuals.PlaceDecal");

        // set position and orientation
        Transform decal = _projectorDecals[decalIndex];
        decal.position = position;
        decal.rotation = Quaternion.LookRotation(direction, Vector3.up);

        if (!decal.gameObject.activeSelf) {
            // if decal is not active yet, we randomize orientation around its z-Axis
            decal.rotation = Quaternion.AngleAxis(Random.Range(0, 360), decal.forward) * decal.rotation;
            // activate decal
            SetDecalActive(decal, true);
        }

        Profiler.EndSample();
    }

    /// <summary>
    /// Set position and direction of the particle decal with given index.
    /// </summary>
    /// <param name="decalIndex">Index of the decal to set.</param>
    /// <param name="position">Position of the decal.</param>
    /// <param name="direction">Direction of the decal (look direction).</param>
    /// <param name="distanceToBounds">Distance to the collider of an object to calculate strength of ParticleEffect.</param>
    /// <param name="setActive">Should the decal get activated or deactivated (so we can have more or less active particleSystems then ProjectorDecals without the stress of handling more indices).</param>
    private void PlaceParticleDecal(int decalIndex, Vector3 position, Vector3 direction, float distanceToBounds,
        bool setActive) {
        if (_particles == null) {
            Debug.LogError("particles are null");
            return;
        }

        if (decalIndex < 0 || decalIndex > _particles.Length) {
            Debug.LogError("decal index out of range");
            return;
        }

        if (_particles[decalIndex] == null) {
            Debug.LogError("decal is null");
            return;
        }

        Profiler.BeginSample("SimpleRopeIntersectionVisuals.PlaceParticleSystem");
        if (setActive) {
            // calculate strength of the particles dependent of the distance to the collider (max strength at distance 0, min strength at at max distance (_ropeRadius * _ropeRadiusFactorForParticleStrength))
            float strength = 1 - distanceToBounds / (_ropeRadius * _ropeRadiusFactorForParticleStrength);
            _particles[decalIndex].SetStrength(strength);

            // set position & orientation
            Transform decal = _particles[decalIndex].transform;
            decal.position = position;
            decal.rotation = Quaternion.LookRotation(direction, Vector3.up);

            if (!IsParticleSystemActive(decalIndex)) {
                // if decal is not active yet, we randomize orientation around its z-Axis
                decal.rotation = Quaternion.AngleAxis(Random.Range(0, 360), decal.forward) * decal.rotation;
                // activate decal
                SetParticleSystemActive(decalIndex, true);
            }
        }
        else {
            SetParticleSystemActive(decalIndex, false);
        }

        Profiler.EndSample();
    }


    /// <summary>
    /// Change Shader properties dependent of which team the player chooses
    /// </summary>
    /// <param name="player"></param>
    /// <param name="teamID">Id of the team the player has chosen.</param>
    public void OnTeamChanged(IPlayer player, TeamID teamID) {
        if (_projectorDecals == null) {
            Debug.LogError("decals are null");
            return;
        }

        ITeam team = TeamManager.Singleton.Get(teamID);
        if (team != null) {
            if (_decalMaterial != null) {
                // set color rampTexture from team (to tint the projector from center to outer radius)
                Debug.Log("Creating Texture from gradient");
                Texture2D tex = ColorRampToTexture.ConvertColorRampToTexture(128, 1, TextureFormat.RGBA32,
                    team.Colors.RopeIntersectionProjector, FilterMode.Bilinear);

                if (_decalMaterial.HasProperty(_decalMaterialColorRampPropertyName))
                    _decalMaterial.SetTexture(_decalMaterialColorRampPropertyName, tex);
            }

            // call OnTeamChanged to tint particle effects
            for (var k = 0; k < _projectorDecals.Length; k++) {
                if (_particles[k] != null)
                    _particles[k].OnTeamChanged(teamID);
            }
        }
    }

#endregion

#region Helper Functions

    /// <summary>
    /// Checks if we can use ClosestPoint on the given collider (is only valid for Box-, Sphere-, Capsule- and convex Mesh-Collider).
    /// </summary>
    /// <param name="collider"></param>
    /// <returns></returns>
    private static bool IsColliderValidForClosestPointCalculation(Collider collider) {
        switch (collider) {
            case BoxCollider _:
            case SphereCollider _:
            case CapsuleCollider _:
                return true;
        }

        var mCol = collider as MeshCollider;
        if (mCol != null && mCol.convex)
            return true;

        return false;
    }

    /// <summary>
    /// Returns the result of ClosestPoint (closest point from given point on/in collider) or ClosestPointOnBounds
    /// (closest point from given point on/in BoundingBox) dependent of the type of collider (see IsColliderValidForClosestPointCalculation).
    /// </summary>
    /// <param name="collider">Collider we have to check the type of.</param>
    /// <param name="point">Point we want the nearest Point on object from.</param>
    /// <returns>A point on/in the BoundingBox or Collider-Hull dependent of the type of the given collider (see IsColliderValidForClosestPointCalculation).</returns>
    private static Vector3 GetNearestPointOnBoundsDependentOfColliderType(Collider collider, Vector3 point) {
        if (IsColliderValidForClosestPointCalculation(collider))
            return collider.ClosestPoint(point);
        return collider.ClosestPointOnBounds(point);
    }

    /// <summary>
    /// Sets all decals (projectors & particles) inactive.
    /// </summary>
    private void DeactivateAllDecals() {
        if (_projectorDecals == null) return;
        Profiler.BeginSample("SimpleRopeIntersectionVisuals.DeactivateAllDecals");
        for (var i = 0; i < _projectorDecals.Length; i++) {
            Profiler.BeginSample("SimpleRopeIntersectionVisuals.DeactivateAllDecals: Decals");
            SetDecalActive(_projectorDecals[i], false);
            Profiler.EndSample();

            Profiler.BeginSample("SimpleRopeIntersectionVisuals.DeactivateAllDecals: Particles");
            SetParticleSystemActive(i, false);
            Profiler.EndSample();
        }

        _activeDecalCount = 0;
        Profiler.EndSample();
    }

    /// <summary>
    /// Activates or deactivates given decal.
    /// </summary>
    /// <param name="decal">Object to activate/deactivate.</param>
    /// <param name="setActive">Sets decal active if true, deactivates it when false.</param>
    private static void SetDecalActive(Transform decal, bool setActive) {
        if (decal == null) {
            Debug.LogError("decal is null");
            return;
        }

        if (decal.gameObject.activeSelf != setActive)
            decal.gameObject.SetActive(setActive);
    }

    /// <summary>
    /// Activates or deactivates particles decal with given index.
    /// </summary>
    /// <param name="index">Index of the particle decal in _particles array.</param>
    /// <param name="setActive">Sets decal active if true, deactivates it when false.</param>
    private void SetParticleSystemActive(int index, bool setActive) {
        if (_particles == null) {
            Debug.LogError("particles are null");
            return;
        }

        if (index < 0 || index > _particles.Length) {
            Debug.LogError("decal index out of range");
            return;
        }

        if (_particles[index] == null) {
            Debug.LogError("decal is null");
            return;
        }

        _particles[index].SetActive(setActive);
    }

    /// <summary>
    /// Returns if ParticleSystem with given index is currently activated or not.
    /// </summary>
    /// <param name="index">Index of the particle decal in _particles array.</param>
    /// <returns>True if particle system was activated, false otherwise.</returns>
    private bool IsParticleSystemActive(int index) {
        if (_particles == null) {
            Debug.LogError("particles are null");
            return false;
        }

        if (index < 0 || index > _particles.Length) {
            Debug.LogError("decal index out of range");
            return false;
        }

        if (_particles[index] == null) {
            Debug.LogError("decal is null");
            return false;
        }

        return _particles[index].IsActive;
    }

#endregion
}