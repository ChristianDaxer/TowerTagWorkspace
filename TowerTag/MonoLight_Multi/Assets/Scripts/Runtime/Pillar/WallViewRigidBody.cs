using System.Collections;
using TowerTag;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Class to visualize damage and hits by Bullets using the WallDamageHandler_Base (its events).
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(HingeJoint))]
public class WallViewRigidBody : PillarWallView, IIndexedMesh
{
    #region Member
    /// <summary>
    /// Gets multiplied with moving direction of Bullets (which hit a wall) to apply a force (as ForceVector) to the wall
    /// (bigger force multiplier cause walls to wobble more).
    /// Gets overriden by BalancingConfig.bulletWallHitForceMultiplier in Awake.
    /// </summary>
    [Header("Physical Animation")]
    [SerializeField, Tooltip("Gets multiplied with moving direction of Bullets to apply a force to the wall. Gets overriden by BalancingConfig.bulletWallHitForceMultiplier in Awake.")]
    private float _forceMultiplier = 1;

    /// <summary>
    /// ForceMode to apply force to rigidBody when a Bullet hits the wall
    /// </summary>
    [SerializeField, Tooltip("ForceMode to apply force to rigidBody when a Bullet hits the wall (see UnityEngine.ForceMode).")]
    private ForceMode _forceMode = ForceMode.Force;

    /// <summary>
    /// RigidBody to apply force. Reference is automatically fetched by GetComponent in Awake.
    /// </summary>
    [SerializeField] private Rigidbody _rigid;

    /// <summary>
    /// Joint the wall rotates around when a force is applied. Reference is automatically fetched by GetComponent in Awake.
    /// </summary>
    [SerializeField] private HingeJoint _joint;

    /// <summary>
    /// Animation to change Height of Wall over time (played if wall reached max damage).
    /// </summary>
    [Header("Procedural Animation")]
    [SerializeField, Tooltip("Animation to change Height of Wall over time (played if wall reached max damage).")]
    private AnimationCurve _fallAnimation;
    [SerializeField] private float _valueToLerpTo;


    /// <summary>
    /// Cached localPosition of the wall before Match starts.
    /// </summary>
    private Vector3 _localPositionAtStart;

    /// <summary>
    /// Name key of fallDown sound in Sound database
    /// </summary>
    [Header("Sounds")]
    [SerializeField, Tooltip("Name key of fallDown sound in Sound database")]
    private string _fallDownSoundName = "27_WallFallsDown";

    /// <summary>
    /// AudioSource to play the fallDown sound
    /// </summary>
    [SerializeField] private AudioSource _source;

    public bool FallDownAnimationIsRunning { get; private set; }

    public bool IsDown => transform.localPosition.y <= _valueToLerpTo + 0.01f;

    [SerializeField] private int meshIndex;
    public int MeshIndex { get => meshIndex; set
        {
            meshIndex = value;
            #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif
        }
    }

    public int GameObjectInstanceID => gameObject.GetInstanceID();

    [SerializeField] private WallLocalPositionDistributor wallTransformDistributor;
    public void ApplyDistributor(MaterialDataDistributor materialDataDistributor)
    {
        if (wallTransformDistributor != null)
            return;

        wallTransformDistributor = materialDataDistributor as WallLocalPositionDistributor;
        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
    }

#endregion

    private void OnValidate()
    {
        ApplyDistributor(GetComponentInParent<WallLocalPositionDistributor>());

        _rigid = GetComponent<Rigidbody>();
        _joint = GetComponent<HingeJoint>();

        _source = gameObject.GetComponent<AudioSource>();
        if (_source == null)
            _source = gameObject.AddComponent<AudioSource>();
    }

    #region Core
    /// <summary>
    /// Initialize Components
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        _localPositionAtStart = transform.localPosition;
        _forceMultiplier = BalancingConfiguration.Singleton.BulletWallHitForceMultiplier;
        ChangeHeight(_localPositionAtStart.y);
    }

/*
#if UNITY_EDITOR
    private void Update()
    {
        ChangeHeight(Mathf.Cos(Time.time + meshIndex * 0.1f));
    }
#endif
*/

    /// <summary>
    /// Update Wall Height (Transform & HingeJoint).
    /// </summary>
    /// <param name="localHeight">The local Height (localPosition relative to it's parent) of the wall.</param>
    private void ChangeHeight(float localHeight)
    {
        // update height (local position of wall transform)
        Vector3 position = _localPositionAtStart;
        position.y = localHeight;
        transform.localPosition = position;

        if (wallTransformDistributor != null)
            wallTransformDistributor.SetLocalPosition(meshIndex, transform.position - transform.parent.position);

        if (_rigid != null) {
            // update anchor of wall so it rotates around its mount (around the anchorPoint)
            var anchorPoint = new Vector3(0, -localHeight, 0);
            _joint.anchor = anchorPoint;
        } else
            Debug.LogError("WallView_RigidBody.ChangeHeight: Can't apply new anchorPoint to HingeJoint because the joint reference is null!");
    }
    #endregion

    #region Event Handler Functions for OnDamageChanged-, OnReachedMaxDamage- and OnForceWasAplied- events from WallDamageHandler
    /// <summary>
    /// React on WallDamageHandler's OnWallDamageChanged event.
    /// </summary>
    /// <param name="oldDamage">The old damage of the wall before change.</param>
    /// <param name="newDamage">The current damage of the wall (now).</param>
    protected override void OnWallDamageChanged(float oldDamage, float newDamage)
    {
        //Debug.Log("WallView_RigidBody.OnWallDamageChanged: old: " + oldDamage + " new: " + newDamage);
    }

    /// <summary>
    /// React on WallDamageHandler's OnReachedMaxDamage event.
    /// -> Start fallingDown Animation
    /// </summary>
    protected override void OnWallReachedMaxDamage()
    {
        //Debug.Log("WallView_RigidBody.OnWallReachedMaxDamage");
        StartCoroutine(AnimateFall());
    }

    private IEnumerator AnimateFall() {
        float timePlayed = 0;
        float timeToPlay = _fallAnimation.keys[_fallAnimation.length - 1].time;
        float startYPosition = transform.localPosition.y;
        SoundDatabase.Instance.PlaySound(_source, _fallDownSoundName);
        FallDownAnimationIsRunning = true;
        while (timePlayed <= timeToPlay) {
            float newHeight = Mathf.Lerp(startYPosition, _valueToLerpTo,_fallAnimation.Evaluate(timePlayed));
            timePlayed += Time.deltaTime;
            ChangeHeight(newHeight);
            yield return new WaitForEndOfFrame();
        }
        FallDownAnimationIsRunning = false;
        float finalHeight = Mathf.Lerp(_localPositionAtStart.y, _valueToLerpTo, _fallAnimation.Evaluate(timeToPlay));
        ChangeHeight(finalHeight);
    }

    public override void Reset() {
        ChangeHeight(_localPositionAtStart.y);
    }

    protected override void OnOwningTeamChanged(Claimable claimable, TeamID oldTeam, TeamID newTeam, IPlayer[] newOwner) { }

    /// <summary>
    /// React on WallDamageHandler's OnForceWasApplied event.
    /// -> Add force to RigidBody so it wobbles a bit.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="force"></param>
    protected override void OnForceWasApplied(Vector3 position, Vector3 force)
    {
        if (_rigid == null) {
            Debug.LogError("WallView_RigidBody.OnForceWasApplied: Can't apply force to rigid body because it is null!");
            return;
        }

        _rigid.AddForceAtPosition(force * _forceMultiplier, position, _forceMode);
    }

    #endregion
}
