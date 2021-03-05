using UnityEngine;

public class AvatarMovement : MonoBehaviour {

    [SerializeField] private Transform _headTargetTransform;
    [SerializeField] private Transform _bodyTargetTransform;

    [SerializeField] private Transform _headSourceTransform;

    [SerializeField] private Vector2 _minMaxAngleX = new Vector2(-70, 50);
    [SerializeField] private Vector2 _minMaxAngleZ = new Vector2(-25, 25);
    [SerializeField] private float _speed = 0.025f;
    [SerializeField] private float _dampingX = 0.25f;
    [SerializeField] private float _dampingZ = 0.75f;

    public bool alignToPlayer = false;

    public Transform BodyTargetTransform {
        get => _bodyTargetTransform;
        set => _bodyTargetTransform = value;
    }

    public Transform HeadTargetTransform {
        get => _headTargetTransform;
        set => _headTargetTransform = value;
    }

    private PlayerHeadBase PlayerHeadInstance
    {
        get
        {
            if (playerHeadBase == null)
            {
                if (!PlayerHeadBase.GetInstance(out playerHeadBase))
                    return null;
            }

            return playerHeadBase;
        }
    }

    private PlayerHeadBase playerHeadBase;

    public Transform HeadSourceTransform
    {
        get
        {
            if (alignToPlayer)
                return PlayerHeadInstance != null ? PlayerHeadInstance.transform : null;
            return _headSourceTransform;
        }
    }

    private void LateUpdate() {

        if (HeadSourceTransform == null ||
            HeadTargetTransform == null ||
            BodyTargetTransform == null)
            return;

        Transform headSource = HeadSourceTransform;
        Vector3 position = headSource.position;

        Quaternion q = Quaternion.Lerp(_bodyTargetTransform.localRotation, headSource.localRotation, _speed);
        Vector3 eulerAngles = headSource.localEulerAngles;
        float clampedEulerX = Mathf.Clamp(Mathf.DeltaAngle(0, eulerAngles.x) * _dampingX,
            _minMaxAngleX.x, _minMaxAngleX.y);
        float clampedEulerZ = Mathf.Clamp(Mathf.DeltaAngle(0, eulerAngles.z) * _dampingZ,
            _minMaxAngleZ.x, _minMaxAngleZ.y);

        Vector3 localEulerAngles = _bodyTargetTransform.localEulerAngles;
        float smoothXRotation = Mathf.LerpAngle(localEulerAngles.x, clampedEulerX, _speed);
        float smoothZRotation = Mathf.LerpAngle(localEulerAngles.z, clampedEulerZ, _speed);
        Quaternion localRotation = Quaternion.Euler(smoothXRotation, q.eulerAngles.y, smoothZRotation);

        _headTargetTransform.position = position;
        _headTargetTransform.localRotation = headSource.rotation;

        _bodyTargetTransform.position = position;
        _bodyTargetTransform.localRotation = localRotation;
    }
}