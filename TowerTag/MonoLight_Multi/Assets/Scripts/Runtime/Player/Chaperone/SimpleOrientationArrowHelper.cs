using TowerTag;
using UnityEngine;

public class SimpleOrientationArrowHelper : MonoBehaviour {
    [SerializeField, Tooltip("Players head (Transform which forward Vector corresponds to lookDirection of player).")]
    private Transform _lookTransform;

    [Header("Arrows")]
    [SerializeField, Tooltip("Transform of the ArrowObject (used for position/rotate the arrows).")]
    private Transform _arrowObjectTransform;

    [SerializeField,
     Tooltip("Min & Max Angle from the players lookDirection where the arrows are positioned in the view.")]
    public Vector2 _minMaxClampedAngle = new Vector2(15, 25);

    [SerializeField, Tooltip("If the Angle between the lookDirection of the player and the target direction " +
                             "is smaller than this angle, the ArrowObject will be disabled.")]
    public float _maxAngleDistanceToActivate = 15; // nico wants 45

    [SerializeField, Tooltip("Distance from the head to position the arrowObject.")]
    private float _radius = 2;

    [Header("Transparency")]
    [SerializeField, Tooltip("Material to set the transparency of the MainColor")]
    private Material _material;

    [SerializeField, Tooltip("TransparencyCurve to manipulate transparency fall-off dependent on the " +
                             "angle [0..180] -> [0..1].")]
    private AnimationCurve _transparencyCurve;

    [Header("AnimationSpeed")]
    [SerializeField, Tooltip("Animator to set speed of the animation.")]
    private Animator _animator;

    [SerializeField, Tooltip("Animators parameter name to set speed of the animation.")]
    private string _speedParameterName = "speed";

    [SerializeField, Tooltip("Min & Max value to set to the speed parameter in the animator.")]
    private Vector2 _minMaxAnimationSpeed = new Vector2(0, 4);

    private Pillar _pillar;
    private IPlayer _owner;

    private void Awake() {
        _owner = GetComponentInParent<IPlayer>();
    }

    private void OnEnable()
    {
        if (_owner != null)
            _owner.TeleportHandler.PlayerTeleporting += OnPlayerTeleporting;
        if (_pillar != null)
            _pillar = _owner.CurrentPillar;
    }

    private void OnDisable()
    {
        if (_owner != null)
            _owner.TeleportHandler.PlayerTeleporting -= OnPlayerTeleporting;
    }

    private void OnPlayerTeleporting(TeleportHandler sender, Pillar origin, Pillar target, float timeToTeleport) {
        _pillar = target;
    }

    private void Update() {
        if (_lookTransform == null) {
            Debug.LogWarning("SimpleOrientationArrowHelper.Update: lookTransform is null!");
            enabled = false;
            return;
        }


        if (_pillar == null || !_pillar.enabled || !_pillar.ShowOrientationHelp) {
            ShowArrows(false);
            return;
        }

        Vector3 pillarDirection = _pillar.TeleportTransform.forward;
        float angle = CalculateAngleOnXZPlane(pillarDirection, _lookTransform.forward);

        if (Mathf.Abs(angle) < _maxAngleDistanceToActivate) {
            ShowArrows(false);
        } else {
            ShowArrows(true);
            // update position/rotation/transparency and animationSpeed of arrows
            UpdateArrows(angle);
        }
    }

    // update position/rotation/transparency and animationSpeed of arrows
    private void UpdateArrows(float angle) {
        // cache normalized amount of rotation [0..+-180] -> [0..1]
        float delta = Mathf.Abs(angle / 180);

        // clamp rotationAngle (from -180, 180 to minMaxClampAngle)
        float clampedAngle = Mathf.Clamp(Mathf.Abs(angle), _minMaxClampedAngle.x, _minMaxClampedAngle.y) *
                             Mathf.Sign(angle);
        // rotate lookVector with the above clamped angle to set position for arrowObject
        Vector3 forward = _lookTransform.forward;
        Vector3 arrowDirection = Quaternion.Euler(0, clampedAngle, 0)
                                 * new Vector3(forward.x, 0, forward.z);
        // position Arrow on a (horizontal circle) around my head
        Vector3 lookTransformPosition = _lookTransform.position;
        _arrowObjectTransform.position = lookTransformPosition + arrowDirection.normalized * _radius;
        // new Rotation of arrows -> sprite looks at me & is oriented left or right (rotate around z-Axis: 0 is left, 180 is right)
        _arrowObjectTransform.rotation =
            Quaternion.LookRotation(lookTransformPosition - _arrowObjectTransform.position, Vector3.up)
            * Quaternion.Euler(0, 0, 90 + 90 * Mathf.Sign(angle));

        _animator.SetFloat(_speedParameterName,
            Mathf.Lerp(_minMaxAnimationSpeed.x, _minMaxAnimationSpeed.y, delta));

        Color c = _material.color;
        c.a = _transparencyCurve.Evaluate(delta);
        _material.color = c;
    }

    // calculate angle between targetDirection and lookDirection
    // - positive angle means i have to rotate right,
    // - negative angle means rotate left (-90 -> rotate 90 ° left)
    private static float CalculateAngleOnXZPlane(Vector3 targetDirection, Vector3 lookDirection) {
        // targetDirection
        var tDir = new Vector2(targetDirection.x, targetDirection.z);

        // targetDirection rotated around 90 degrees (counter clockwise)
        var tDirPlus90Deg = new Vector2(-targetDirection.z, targetDirection.x);

        // lookDirection
        var lDir = new Vector2(lookDirection.x, lookDirection.z);

        // calculate rotationDirection
        float sign = Mathf.Sign(Vector2.Dot(tDirPlus90Deg, lDir));

        // calculate angle between target and lookDirection and multiply with rotationDirection
        // - positive angle means i have to rotate right,
        // - negative angle means rotate left (-90 -> rotate 90 ° left)
        //   if targetDirection = (0,1)
        //           0
        //           ^
        //   90    <-|->  -90
        //           |
        //      180     -180
        return Vector2.Angle(tDir, lDir) * sign;
    }

    private void ShowArrows(bool doShow) {
        if (doShow != _arrowObjectTransform.gameObject.activeSelf)
            _arrowObjectTransform.gameObject.SetActive(doShow);
    }
}