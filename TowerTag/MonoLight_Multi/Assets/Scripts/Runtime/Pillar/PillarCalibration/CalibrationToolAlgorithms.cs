using UnityEngine;

public static class CalibrationToolAlgorithms
{
    /// <summary>
    /// Calculate midPoint of a number of given points
    /// </summary>
    /// <param name="points">Array with points to calculate the center from.</param>
    /// <returns>Center of the given points.</returns>
    public static Vector3 GetCenter(Vector3[] points)
    {
        if (points == null || points.Length == 0) {
            Debug.LogWarning("Cannot get center to calibrate: points are missing. Returning (0,0,0)");
            return Vector3.zero;
        }

        Vector3 center = points[0];
        for (var i = 1; i < points.Length; i++)
        {
            center += points[i];
        }

        return center / points.Length;
    }

    /// <summary>
    /// Calculates an offsetAngle between a quad that is axis-oriented (edges are parallel to coordinate axes) to a quad represented by the given edge points.
    /// Works only for rotationAngles between [-44.9 .. 45]
    /// </summary>
    /// <param name="points">Array of 4 points representing a quad.</param>
    /// <param name="center">Center of the quad.</param>
    /// <returns>OffsetAngle of the given quad to an axis oriented quad.</returns>
    public static float GetRotationOffsetAroundYAxis(Vector3[] points, Vector3 center)
    {
        if (points == null || points.Length == 0) {
            Debug.LogWarning("Cannot get rotation to calibrate: points are missing. Returning 0");
            return 0;
        }

        int pointCount = points.Length;
        float angle = 0;
        for (var i = 0; i < pointCount; i++)
        {
            Vector3 point = points[i];
            float currentAngle = GetAngleFromDirectionInDegrees(point - center) % 90;
            angle += currentAngle;
        }
        float offsetRotationAngle = 45 - angle/pointCount;
        return offsetRotationAngle;
    }

    /// <summary>
    /// Calculates full Angle [0..360] of a direction in UnitCircle in X/Z-Plane (f^-1(angle) = direction).
    /// Vector(1,0,0) means 0°, (1,0,1) -> 45°, (0,0,1) -> 90°, (-1, 0, -1) -> 315° and so on
    /// </summary>
    /// <param name="direction">Given direction to calculate the angle to the X-Axis from.</param>
    /// <returns>Full Angle [0..360] (Vector(1,0,0) means 0°, (1,0,1) -> 45°, (0,0,1) -> 90°, (-1, 0, -1) -> 315° and so on)</returns>
    private static float GetAngleFromDirectionInDegrees(Vector3 direction)
    {
        direction.y = 0;
        direction.Normalize();
        return (Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg + 360f) % 360;
    }
}
