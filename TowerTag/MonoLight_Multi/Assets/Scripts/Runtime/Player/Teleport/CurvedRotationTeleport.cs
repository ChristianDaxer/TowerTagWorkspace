using UnityEngine;

public class CurvedRotationTeleport {
    private Vector3 _p0, _p1, _p2;

    public void Init(Vector3 startPosition, Quaternion startRotation, Pillar target, float minDistanceToTarget, Transform gunTransform) {
    }

    public Vector3 GetPositionAt(float delta) {
        return BezierCurves.EvaluateQuadraticBezier(_p0, _p1, _p2, delta);
    }
}