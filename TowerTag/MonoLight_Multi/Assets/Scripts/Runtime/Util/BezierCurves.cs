using UnityEngine;

public static class BezierCurves
{
    public static Vector3 EvaluateQuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float twoTimesT = 2f * t;
        float tSquared = t * t;
        return (1 - twoTimesT + tSquared) * p0 + (twoTimesT - 2f * tSquared) * p1 + tSquared * p2;
    }
}
