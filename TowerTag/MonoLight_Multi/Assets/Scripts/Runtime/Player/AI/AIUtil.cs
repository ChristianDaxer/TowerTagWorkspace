using UnityEngine;

namespace AI {
    public static class AIUtil {
        public static int RandomSign() {
            return Random.value < 0.5 ? 1 : -1;
        }

        /// <summary>
        /// Performs a linear spherical interpolation between <see cref="from"/> and <see cref="to"/>.
        /// The vertical component is interpolated linearly.
        /// The horizontal components are interpolated spherically around the origin.
        /// </summary>
        /// <param name="from">Start position of interpolation</param>
        /// <param name="to">End position of interpolation</param>
        /// <param name="t">Interpolation value between 0 and 1</param>
        /// <returns>Position between from and to on a circular curve</returns>
        public static Vector3 HorizontalSlerp(Vector3 from, Vector3 to, float t) {
            return HorizontalSlerp(from, to, t, Vector3.zero, Vector3.up);
        }

        /// <summary>
        /// Performs a linear spherical interpolation between <see cref="from"/> and <see cref="to"/>.
        /// The component parallel to <see cref="up"/> are interpolated linearly.
        /// The components perpendicular to <see cref="up"/> are interpolated spherically around <see cref="pivot"/>.
        /// </summary>
        /// <param name="from">Start position of interpolation</param>
        /// <param name="to">End position of interpolation</param>
        /// <param name="t">Interpolation value between 0 and 1</param>
        /// <param name="pivot">Pivot around which the spherical interpolation is performed</param>
        /// <param name="up">Component that is interpolated linearly</param>
        /// <returns>Position between from and to on a circular curve</returns>
        public static Vector3 HorizontalSlerp(Vector3 from, Vector3 to, float t, Vector3 pivot, Vector3 up) {
            Vector3 localFrom = from - pivot;
            Vector3 localTo = to - pivot;
            Vector3 normUp = up.normalized;
            float upComponent = Mathf.Lerp(Vector3.Dot(localFrom, normUp), Vector3.Dot(localTo, normUp), t);
            return Vector3.Slerp(Vector3.ProjectOnPlane(localFrom, normUp), Vector3.ProjectOnPlane(localTo, normUp), t)
                   + upComponent * normUp + pivot;
        }
    }
}