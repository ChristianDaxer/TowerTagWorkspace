using UnityEngine;

public class LinearTeleport : TeleportAlgorithm {
    private Vector3 _startPosition;
    private Vector3 _endPosition;
    private Vector3 _direction;
    private float _startHeight;

    public override void Init(Vector3 startPosition, Pillar target, float minDistanceToTarget, Transform gunTransform) {
        _endPosition = target.TeleportTransform.position;

        _startPosition = startPosition;
        _direction = _endPosition - _startPosition;
        _direction = _direction.normalized * (_direction.magnitude - minDistanceToTarget);
    }

    public override Vector3 GetPositionAt(float delta) {
        return Vector3.Lerp(_startPosition, _startPosition + _direction, delta);
    }
}