using UnityEngine;

public class CurvedTeleport : TeleportAlgorithm {
    private Vector3 _p0, _p1, _p2;

    public override void Init(Vector3 startPosition, Pillar target, float minDistanceToTarget, Transform gunTransform) {
        float strength = BalancingConfiguration.Singleton.TeleportCurveStrength;

        Vector3 gunOffset = startPosition;
        gunOffset.y = gunTransform.position.y;
        Vector3 direction = (target.AnchorTransform.position - gunOffset) * strength;

        _p0 = startPosition;
        _p1 = startPosition + direction;
        _p2 = target.TeleportTransform.position;
    }

    public override Vector3 GetPositionAt(float delta) {
        return BezierCurves.EvaluateQuadraticBezier(_p0, _p1, _p2, delta);
    }
}