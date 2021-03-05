using UnityEngine;

public class CurvedTeleportInverse : TeleportAlgorithm {
    private Vector3 _p0, _p1, _p2;

    public override void Init(Vector3 startPosition, Pillar target, float minDistanceToTarget, Transform gunTransform) {
        var strength = 0.75f;
        var maxDist = 15f;
        var heightOffset = 1f;
        if (BalancingConfiguration.Singleton != null) {
            strength = BalancingConfiguration.Singleton.TeleportCurveStrength;
            maxDist = Mathf.Max(1f, BalancingConfiguration.Singleton.ChargerBeamLength);
            heightOffset = BalancingConfiguration.Singleton.TeleportHeightFactor;
        }

        Vector3 targetPosition = target.TeleportTransform.position;
        float factor = Mathf.Clamp01(Vector3.Distance(startPosition, targetPosition) / maxDist);

        Vector3 gunOffset = startPosition;
        gunOffset.y = gunTransform.position.y;
        Vector3 direction = factor * strength * (target.AnchorTransform.position + new Vector3(0f, heightOffset * factor, 0f) - gunOffset);

        _p2 = startPosition;
        direction.y = -direction.y;
        _p1 = targetPosition - direction;
        _p0 = targetPosition;
    }

    public override Vector3 GetPositionAt(float delta) {
        return BezierCurves.EvaluateQuadraticBezier(_p0, _p1, _p2, 1f - delta);
    }
}